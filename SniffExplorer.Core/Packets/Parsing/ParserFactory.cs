using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using SniffExplorer.Core.Packets.Parsing.Attributes;
using SniffExplorer.Core.Packets.Types;
using SniffExplorer.Core.Utils;

namespace SniffExplorer.Core.Packets.Parsing
{
    public static class ParserFactory
    {
        public static Func<PacketReader, ValueType> GeneratePacketReader(Type structureType)
        {
            var packetReaderExpr = Expression.Parameter(typeof(PacketReader));
            var structureExpr = Expression.Variable(structureType);

            var lambda = Expression.Lambda<Func<PacketReader, ValueType>>(
                // Expression.Block(new[] { structureExpr }, bodyExpressions),
                GenerateStructureReader(packetReaderExpr, structureExpr, true),
                packetReaderExpr);
            var compiledExpression = lambda.Compile();

            return compiledExpression;
        }

        private static BlockExpression GenerateStructureReader(ParameterExpression argExpr, Expression leftAssignmentExpr, bool convertToValueType = false)
        {
            var structureExpr = Expression.Variable(leftAssignmentExpr.Type);

            var bodyExpressions = new List<Expression> {
                Expression.Assign(structureExpr, Expression.New(leftAssignmentExpr.Type))
            };

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var prop in leftAssignmentExpr.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var reader = GenerateMemberReader(prop, structureExpr, argExpr);
                if (reader == null)
                    continue;

                bodyExpressions.Add(reader);
            }

            // converToValueType is only true for the top node.
            if (!convertToValueType)
                bodyExpressions.Add(Expression.Assign(leftAssignmentExpr, structureExpr));

            if (convertToValueType)
                bodyExpressions.Add(Expression.Convert(structureExpr, typeof(ValueType)));

            return Expression.Block(new[] { structureExpr }, bodyExpressions);
        }

        private static Expression GenerateMemberReader(PropertyInfo prop, Expression structureExpr, ParameterExpression packetReaderExpr)
        {
            if (prop.GetCustomAttribute(typeof(IgnoreAttribute)) != null)
                return null;

            var conditionalAttribute = prop.GetCustomAttribute<ConditionalAttribute>();
            var readExpression = prop.PropertyType.IsArray
                ? GenerateArrayReader(structureExpr.Type, prop, packetReaderExpr, structureExpr)
                : GenerateFlatReader(prop, packetReaderExpr, structureExpr);

            if (conditionalAttribute == null)
                return readExpression;

            var referenceProperty = structureExpr.Type.GetProperty(conditionalAttribute.PropertyName);
            if (referenceProperty == null)
                return readExpression;

            var leftExpression = Expression.MakeMemberAccess(structureExpr, referenceProperty);

            if (conditionalAttribute.Right.GetType() == referenceProperty.PropertyType)
                readExpression = Expression.IfThen(conditionalAttribute.GetComparisonExpression(leftExpression),
                    readExpression);
            else
                throw new InvalidOperationException(
                    $@"Property {prop.Name} has a condition where the types of both operands do not match: got {
                        conditionalAttribute.Right.GetType().Name}, expected {prop.PropertyType.Name}");

            return readExpression;
        }

        private static Expression GenerateArrayReader(Type packetStructType, PropertyInfo propInfo, ParameterExpression argExpr, Expression tExpr)
        {
            var propExpression = Expression.MakeMemberAccess(tExpr, propInfo);
            var arraySizeAttr = propInfo.GetCustomAttribute<SizeAttribute>();

            var bitReaderExpr = propInfo.GetCustomAttribute<BitFieldAttribute>()?
                .GetCallExpression(argExpr, propInfo.PropertyType.GetElementType());

            Expression arraySizeExpr;
            if (arraySizeAttr != null)
            {
                if (!arraySizeAttr.Streamed)
                    arraySizeExpr = Expression.Constant(arraySizeAttr.ArraySize);
                else if (arraySizeAttr.InPlace)
                    arraySizeExpr = bitReaderExpr ?? Expression.Call(argExpr, ExpressionUtils.Base[TypeCode.Int32]);
                else
                    arraySizeExpr = Expression.MakeMemberAccess(tExpr,
                        packetStructType.GetProperty(arraySizeAttr.PropertyName));
            }
            else
                throw new InvalidOperationException($"Property {propInfo.Name} is missing an array size specification");

            var exitLabelExpr = Expression.Label();
            var itrExpr = Expression.Variable(typeof (int));
            return Expression.Block(new[] { itrExpr },
                // ReSharper disable once AssignNullToNotNullAttribute
                Expression.Assign(propExpression,
                    Expression.New(propInfo.PropertyType.GetConstructor(new[] { typeof(int) }), arraySizeExpr)),
                Expression.Assign(itrExpr, Expression.Constant(0)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(itrExpr,
                            Expression.MakeMemberAccess(propExpression, propExpression.Type.GetProperty("Length"))),
                        Expression.Block(
                            GenerateArrayMemberWriter(propExpression, itrExpr, propInfo, argExpr, tExpr),
                            Expression.PostIncrementAssign(itrExpr)),
                        Expression.Break(exitLabelExpr)),
                    exitLabelExpr));
        }

        private static Expression GenerateArrayMemberWriter(Expression propExpression, Expression itrExpr,
            PropertyInfo propInfo, ParameterExpression argExpr, Expression tExpr)
        {
            var fieldExpression = Expression.ArrayAccess(propExpression, itrExpr);

            var simpleReadExpression = GenerateSimpleReadExpression(propInfo, argExpr, tExpr);
            if (simpleReadExpression != null)
                return Expression.Assign(fieldExpression, simpleReadExpression);

            var expressionList = new List<Expression>();
            var instanceExpr = Expression.New(propInfo.PropertyType.GetElementType());

            expressionList.Add(Expression.Assign(fieldExpression, instanceExpr));

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var property in propInfo.PropertyType.GetElementType().
                GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var reader = GenerateMemberReader(property, fieldExpression, argExpr);
                if (reader == null)
                    continue;

                expressionList.Add(reader);
            }

            return Expression.Block(expressionList.ToArray());
        }

        private static Expression GenerateSimpleReadExpression(PropertyInfo propInfo, ParameterExpression argExpr, Expression tExpr)
        {
            var propType = propInfo.PropertyType;
            if (propType.IsArray)
                propType = propType.GetElementType();

            if (propType.IsArray)
                throw new NotImplementedException($"Field {propInfo.Name} is a multi-dimensional array");

            var bitReaderExpression = propInfo.GetCustomAttribute<BitFieldAttribute>()?.GetCallExpression(argExpr, propType);

            var packedAttr = propInfo.GetCustomAttribute<PackedFieldAttribute>();
            var typeCode = Type.GetTypeCode(propType.IsEnum ? Enum.GetUnderlyingType(propType) : propType);

            Expression readerExpression = null;
            switch (typeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int16:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.Int64:
                    if (bitReaderExpression != null)
                        readerExpression = bitReaderExpression;
                    else
                        goto case TypeCode.Single;
                    break;
                case TypeCode.Single:
                case TypeCode.Double:
                    readerExpression = Expression.Call(argExpr, ExpressionUtils.Base[typeCode]);
                    break;
                case TypeCode.UInt64:
                    readerExpression = Expression.Call(argExpr, packedAttr != null ?
                        ExpressionUtils.PackedUInt64 :
                        ExpressionUtils.Base[TypeCode.UInt64]);
                    break;
                case TypeCode.DateTime:
                    readerExpression = Expression.Call(argExpr, packedAttr != null
                        ? ExpressionUtils.ReadPackedTime
                        : ExpressionUtils.ReadTime);
                    break;
                case TypeCode.String:
                {
                    var stringAttr = propInfo.GetCustomAttribute<StringSizeAttribute>();
                    if (stringAttr != null)
                    {
                        if (!stringAttr.Streamed)
                            readerExpression = Expression.Call(argExpr, ExpressionUtils.String,
                                Expression.Constant(stringAttr.ArraySize));
                        else if (!stringAttr.InPlace)
                            readerExpression = Expression.Call(argExpr, ExpressionUtils.String,
                                Expression.MakeMemberAccess(tExpr, tExpr.Type.GetProperty(stringAttr.PropertyName)));
                        else
                            readerExpression = Expression.Call(argExpr, ExpressionUtils.String,
                                bitReaderExpression ?? Expression.Call(argExpr, ExpressionUtils.Base[TypeCode.Int32]));
                    }
                    else
                        readerExpression = Expression.Call(argExpr, ExpressionUtils.CString);
                    break;
                }
            }

            if (propType.IsEnum && readerExpression != null)
                readerExpression = Expression.Convert(readerExpression, propType);

            if (propType.IsAssignableFrom(typeof(ObjectGuid)))
                readerExpression = Expression.Call(argExpr, ExpressionUtils.ObjectGuid);

            return readerExpression;
        }

        private static Expression GenerateFlatReader(PropertyInfo propInfo, ParameterExpression argExpr, Expression tExpr)
        {
            var memberAccessExpr = Expression.MakeMemberAccess(tExpr, propInfo);

            var simpleReadExpr = GenerateSimpleReadExpression(propInfo, argExpr, tExpr);
            if (simpleReadExpr != null)
                return Expression.Assign(memberAccessExpr, simpleReadExpr);

            return GenerateStructureReader(argExpr, memberAccessExpr);
        }
    }
}
