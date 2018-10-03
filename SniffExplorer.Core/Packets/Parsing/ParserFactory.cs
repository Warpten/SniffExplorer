using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using SniffExplorer.Core.Attributes;
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

            var exitLabel = Expression.Label(structureExpr.Type);
            var exitLabelTarget = Expression.Label(exitLabel, Expression.Default(structureExpr.Type));

            var bodyExpressions = new List<Expression> {
                Expression.Assign(structureExpr, Expression.New(leftAssignmentExpr.Type))
            };

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var prop in leftAssignmentExpr.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetCustomAttribute<ResetBitsAttribute>() != null)
                {
                    bodyExpressions.Add(Expression.Call(argExpr, ExpressionUtils.ResetBitReader));
                }

                var stopAttribute = prop.GetCustomAttribute<StopIfAttribute>();

                if (stopAttribute != null)
                {
                    var leftHand = stopAttribute.GetLeftHandExpression(structureExpr);
                    var comparison = stopAttribute.GetComparisonExpression(leftHand);

                    bodyExpressions.Add(Expression.IfThen(comparison, Expression.Return(exitLabel, structureExpr)));
                }

                var reader = GenerateMemberReader(prop, structureExpr, argExpr);
                if (reader == null)
                    continue;

                bodyExpressions.Add(reader);
            }

            bodyExpressions.Add(exitLabelTarget);

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

            var referenceProperty = conditionalAttribute.GetLeftHandExpression(structureExpr);
            if (referenceProperty == null)
                return readExpression;

            readExpression = Expression.IfThen(conditionalAttribute.GetComparisonExpression(referenceProperty), readExpression);
            return readExpression;
        }

        private static Expression GenerateArrayReader(Type packetStructType, PropertyInfo propInfo, ParameterExpression argExpr, Expression tExpr)
        {
            var propExpression = Expression.MakeMemberAccess(tExpr, propInfo);
            var arraySizeAttr = propInfo.GetCustomAttribute<SizeAttribute>();

            var bitReaderExpr = propInfo.GetCustomAttribute<BitFieldAttribute>()?
                .GetCallExpression(argExpr, propInfo.PropertyType.GetElementType());

            Expression arraySizeExpr = null;
            if (arraySizeAttr != null)
            {
                switch (arraySizeAttr.Method)
                {
                    case SizeMethod.FixedSize:
                        arraySizeExpr = Expression.Constant((int)arraySizeAttr.Param);
                        break;
                    case SizeMethod.InPlace:
                        if (arraySizeAttr.Param == null)
                            arraySizeExpr = bitReaderExpr ?? Expression.Call(argExpr, ExpressionUtils.Base[TypeCode.Int32]);
                        else
                            arraySizeExpr = Expression.Call(argExpr, ExpressionUtils.Bits, Expression.Constant((int)arraySizeAttr.Param));
                        break;
                    case SizeMethod.StreamedProperty:
                        arraySizeExpr = Expression.MakeMemberAccess(tExpr, tExpr.Type.GetProperty((string)arraySizeAttr.Param));
                        break;
                }
            }
            else
                throw new InvalidOperationException($"Property '{propInfo.DeclaringType.FullName}.{propInfo.Name}' is missing an array size specification!");

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
                throw new NotImplementedException($"Field {propInfo.Name} is a multi-dimensional array, which are not implemented! Try defining a sub-structure.");

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
                    readerExpression = Expression.Call(argExpr, packedAttr != null ? ExpressionUtils.PackedUInt64 : ExpressionUtils.Base[TypeCode.UInt64]);
                    break;
                case TypeCode.DateTime:
                    readerExpression = Expression.Call(argExpr, packedAttr != null ? ExpressionUtils.ReadPackedTime : ExpressionUtils.ReadTime);
                    break;
                case TypeCode.String:
                {
                    var stringAttr = propInfo.GetCustomAttribute<StringSizeAttribute>();
                    if (stringAttr != null)
                    {
                        switch (stringAttr.Method)
                        {
                            case SizeMethod.FixedSize:
                                readerExpression = Expression.Call(argExpr, ExpressionUtils.String,
                                    Expression.Constant((int)stringAttr.Param));
                                break;
                            case SizeMethod.InPlace:
                                if (stringAttr.Param == null)
                                {
                                    readerExpression = Expression.Call(argExpr, ExpressionUtils.String,
                                        bitReaderExpression ?? Expression.Call(argExpr, ExpressionUtils.Base[TypeCode.Int32]));
                                }
                                else
                                {
                                    readerExpression = Expression.Call(argExpr, ExpressionUtils.String,
                                        Expression.Call(argExpr, ExpressionUtils.Bits, Expression.Constant((int)stringAttr.Param)));
                                }
                                break;
                            case SizeMethod.StreamedProperty:
                                readerExpression = Expression.Call(argExpr, ExpressionUtils.String,
                                    Expression.MakeMemberAccess(tExpr, tExpr.Type.GetProperty((string)stringAttr.Param)));
                                break;
                        }
                    }
                    else
                        readerExpression = Expression.Call(argExpr, ExpressionUtils.CString);
                    break;
                }
            }

            if (propType.IsEnum && readerExpression != null)
                readerExpression = Expression.Convert(readerExpression, propType);

            if (typeof(IObjectGuid).IsAssignableFrom(propType))
            {
                var rawGuidAttr = propInfo.GetCustomAttribute<RawGuidAttribute>() != null;

                var methodInfo = (!rawGuidAttr ? ExpressionUtils.ReadPackedGUID : ExpressionUtils.ReadGUID).MakeGenericMethod(propType);
                readerExpression = Expression.Call(argExpr, methodInfo);
            }
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
