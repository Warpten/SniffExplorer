using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SniffExplorer.Enums;
using System.Linq.Expressions;
using System.Text;
using SniffExplorer.Packets.Types;
using SniffExplorer.Utils;

namespace SniffExplorer.Packets.Parsing
{
    public static class BinaryProcessor
    {
        public static uint Build { get; private set; }
        public static string Locale { get; private set; }

        private static Dictionary<Either<OpcodeClient, OpcodeServer>, Type> _opcodeStructs;

        public static event Action<string> OnOpcodeParsed;
        public static event Action OnSniffLoaded;

        static BinaryProcessor()
        {
            _opcodeStructs = new Dictionary<Either<OpcodeClient, OpcodeServer>, Type>();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsClass)
                    continue;

                var opcodeAttrs = type.GetCustomAttributes<PacketAttribute>();
                foreach (var opcodeAttribute in opcodeAttrs)
                    _opcodeStructs[opcodeAttribute.Opcode] = type;
            }
        }

        public static void Process(string filePath)
        {
            using (var strm = File.OpenRead(filePath))
                Process(strm);
        }

        public static void Process(Stream strm)
        {
            using (var sniffStream = new BinaryReader(strm))
            {
                sniffStream.BaseStream.Position += 3 + 2 + 1;
                Build = sniffStream.ReadUInt32();
                Locale = System.Text.Encoding.UTF8.GetString(sniffStream.ReadBytes(4));
                sniffStream.BaseStream.Position += 40 + 4 + 4;
                var optDataLength = sniffStream.ReadInt32();
                sniffStream.BaseStream.Position += optDataLength;

                while (sniffStream.BaseStream.Position < sniffStream.BaseStream.Length)
                {
                    var direction = sniffStream.ReadUInt32();
                    var connectionID = sniffStream.ReadUInt32();
                    var timeStamp = new DateTime(1970, 1, 1).AddSeconds(sniffStream.ReadUInt32());
                    var optionalHeaderLength = sniffStream.ReadUInt32();
                    var fullSize = sniffStream.ReadInt32() - 4;
                    sniffStream.BaseStream.Position += optionalHeaderLength;
                    var opcode = sniffStream.ReadUInt32();

                    // Yuck.
                    using (var memoryStream = new MemoryStream(sniffStream.ReadBytes(fullSize), false))
                    using (var packetReader = new PacketReader(memoryStream, fullSize))
                    {
                        var opcodeEnum = new Either<OpcodeClient, OpcodeServer>();

                        switch (direction)
                        {
                            case 0x47534D43u: // CMSG
                                opcodeEnum.LeftValue = (OpcodeClient) opcode;
                                break;
                            case 0x47534D53u: // SMSG
                                opcodeEnum.RightValue = (OpcodeServer) opcode;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        if (!_opcodeStructs.ContainsKey(opcodeEnum))
                            continue;

                        var targetType = _opcodeStructs[opcodeEnum];
                        OnOpcodeParsed?.Invoke(opcodeEnum.ToString());

                        if (!TypeReadersStore<Func<PacketReader, ValueType>>.ContainsKey(targetType))
                            GeneratePacketReader(targetType);

                        Store.Insert(opcodeEnum, TypeReadersStore<Func<PacketReader, ValueType>>.Get(targetType)(packetReader), connectionID, timeStamp);

                        if (memoryStream.Position != memoryStream.Length)
                        {
                            Console.WriteLine(@"|-------------------------------------------------|---------------------------------|");
                            Console.WriteLine(@"| 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F | 0 1 2 3 4 5 6 7 8 9 A B C D E F |");
                            Console.WriteLine(@"|-------------------------------------------------|---------------------------------|");

                            for (var i = memoryStream.Position; i < memoryStream.Length; i += 16)
                            {
                                var hexBuffer = new StringBuilder();
                                var asciiBuffer = new StringBuilder();
                                for (var j = 0; j < 16; ++j)
                                {
                                    if (i + j < memoryStream.Length)
                                    {
                                        var value = packetReader.ReadByte();
                                        hexBuffer.Append($"{value:X2} ");
                                        if (value >= 32 && value <= 127)
                                            asciiBuffer.Append($"{(char) value} ");
                                        else
                                            asciiBuffer.Append(". ");
                                    }
                                    else
                                    {
                                        hexBuffer.Append("   ");
                                        asciiBuffer.Append("  ");
                                    }
                                }

                                Console.WriteLine($"| {hexBuffer}| {asciiBuffer} |");
                            }

                            Console.WriteLine(@"|-------------------------------------------------|---------------------------------|");
                        }
                    }
                }

                OnSniffLoaded?.Invoke();
            }
        }

        private static void GeneratePacketReader(Type structureType)
        {
            var packetReaderExpr = Expression.Parameter(typeof(PacketReader));
            var structureExpr = Expression.Variable(structureType);
            var bodyExpressions = new List<Expression> {
                Expression.Assign(structureExpr, Expression.New(structureType))
            };

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var prop in structureType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetCustomAttribute(typeof(IgnoreAttribute)) != null)
                    continue;

                bodyExpressions.Add(prop.PropertyType.IsArray ?
                    GenerateArrayReader(structureType, prop, packetReaderExpr, structureExpr) :
                    GenerateFlatReader(structureType, prop, packetReaderExpr, structureExpr));
            }

            bodyExpressions.Add(Expression.Convert(structureExpr, typeof(ValueType)));

            var lambda = Expression.Lambda<Func<PacketReader, ValueType>>(
                Expression.Block(new[] { structureExpr }, bodyExpressions),
                packetReaderExpr);
            var compiledExpression = lambda.Compile();

            TypeReadersStore<Func<PacketReader, ValueType>>.Store(structureType, compiledExpression);
        }

        private static BlockExpression GenerateSubStructureReader(Type packetStructType, ParameterExpression argExpr)
        {
            if (TypeReadersStore<BlockExpression>.ContainsKey(packetStructType))
                return TypeReadersStore<BlockExpression>.Get(packetStructType);

            var subStructureExpr = Expression.Variable(packetStructType);
            var bodyExpressions = new List<Expression> {
                Expression.Assign(subStructureExpr, Expression.New(packetStructType))
            };

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var prop in packetStructType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetCustomAttribute(typeof(IgnoreAttribute)) != null)
                    continue;

                bodyExpressions.Add(prop.PropertyType.IsArray ?
                    GenerateArrayReader(packetStructType, prop, argExpr, subStructureExpr) :
                    GenerateFlatReader(packetStructType, prop, argExpr, subStructureExpr));
            }

            bodyExpressions.Add(subStructureExpr);

            var block = Expression.Block(new[] {subStructureExpr}, bodyExpressions);

            TypeReadersStore<BlockExpression>.Store(packetStructType, block);
            return block;
        }

        private static Expression GenerateArrayReader(Type packetStructType, PropertyInfo propInfo, ParameterExpression argExpr, Expression tExpr)
        {
            var propExpression = Expression.MakeMemberAccess(tExpr, propInfo);
            var relativeArraySizeAttr = propInfo.GetCustomAttribute<StreamedSizeAttribute>();
            var absoluteArraySizeAttr = propInfo.GetCustomAttribute<FixedSizeAttribute>();

            Expression arraySizeExpr = null; // Never null, keep compiler happy
            if (relativeArraySizeAttr != null)
            {
                if (absoluteArraySizeAttr != null)
                    throw new InvalidOperationException(
                        $"Property {propInfo.Name} has multiple array size specifications!");

                if (relativeArraySizeAttr.InPlace)
                    arraySizeExpr = propInfo.GetCustomAttribute<BitFieldAttribute>()?.GetCallExpression(argExpr, propInfo.PropertyType.GetElementType()) ??
                        Expression.Call(argExpr, ExpressionUtils.Base[TypeCode.Int32]);
                else
                    arraySizeExpr = Expression.MakeMemberAccess(tExpr,
                        packetStructType.GetProperty(relativeArraySizeAttr.PropertyName));
            }
            if (absoluteArraySizeAttr != null)
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (relativeArraySizeAttr != null)
                    throw new InvalidOperationException(
                        $"Property {propInfo.Name} is missing an array size specification!");

                arraySizeExpr = Expression.Constant(absoluteArraySizeAttr.ArraySize);
            }

            var exitLabelExpr = Expression.Label();
            var itrExpr = Expression.Variable(typeof(int));
            return Expression.Block(new[] { itrExpr },
                // ReSharper disable once AssignNullToNotNullAttribute
                Expression.Assign(propExpression,
                    Expression.New(propInfo.PropertyType.GetConstructor(new[] { typeof(int) }), arraySizeExpr)),
                Expression.Assign(itrExpr, Expression.Constant(0)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(itrExpr, arraySizeExpr),
                        Expression.Assign(
                            Expression.ArrayAccess(propExpression, Expression.PostIncrementAssign(itrExpr)),
                            GenerateValueReader(packetStructType, propInfo, argExpr, tExpr)),
                        Expression.Break(exitLabelExpr)),
                    exitLabelExpr));
        }

        private static Expression GenerateFlatReader(Type packetStructType, PropertyInfo propInfo, ParameterExpression argExpr, Expression tExpr)
        {
            return Expression.Assign(Expression.MakeMemberAccess(tExpr, propInfo),
                GenerateValueReader(packetStructType, propInfo, argExpr, tExpr));
        } 

        private static Expression GenerateValueReader(Type packetStructType, PropertyInfo propInfo, ParameterExpression argExpr, Expression tExpr)
        {
            var propType = propInfo.PropertyType;
            if (propType.IsArray)
                propType = propType.GetElementType();

            if (propType.IsArray)
                throw new NotImplementedException($"Field {propInfo.Name} is a multi-dimensional array");


            var bitReaderExpression = propInfo.GetCustomAttribute<BitFieldAttribute>()?.GetCallExpression(argExpr, propType);

            var packedAttr = propInfo.GetCustomAttribute<PackedFieldAttribute>();
            var typeCode = Type.GetTypeCode(propType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    if (bitReaderExpression != null)
                        return Expression.Call(argExpr, ExpressionUtils.Bit);
                    goto case TypeCode.Int32;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int16:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.Int64:
                    if (bitReaderExpression != null)
                        return bitReaderExpression;
                    goto case TypeCode.Single;
                case TypeCode.Single:
                case TypeCode.Double:
                    return Expression.Call(argExpr, ExpressionUtils.Base[typeCode]);
                case TypeCode.UInt64:
                    return Expression.Call(argExpr, packedAttr != null ?
                        ExpressionUtils.PackedUInt64 :
                        ExpressionUtils.Base[TypeCode.UInt64]);
                case TypeCode.DateTime:
                    return Expression.Call(argExpr, packedAttr != null
                        ? ExpressionUtils.ReadPackedTime
                        : ExpressionUtils.ReadTime,
                        Expression.Call(argExpr, ExpressionUtils.Base[TypeCode.Int32]));
                case TypeCode.String:
                {
                    var stringAttr = propInfo.GetCustomAttribute<StreamedSizeAttribute>();
                    if (stringAttr != null)
                    {
                        if (!stringAttr.InPlace)
                            return Expression.Call(argExpr, ExpressionUtils.String,
                                Expression.MakeMemberAccess(tExpr, packetStructType.GetProperty(stringAttr.PropertyName)));

                        return Expression.Call(argExpr, ExpressionUtils.String,
                            bitReaderExpression ?? Expression.Call(argExpr, ExpressionUtils.Base[TypeCode.Int32]));
                    }
                    return Expression.Call(argExpr, ExpressionUtils.CString);
                }
            }

            if (propInfo.PropertyType.IsAssignableFrom(typeof (ObjectGuid)))
                return Expression.Call(argExpr, ExpressionUtils.ObjectGuid);

            return GenerateSubStructureReader(propType, argExpr);
        }
    }
}
