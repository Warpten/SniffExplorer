using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SniffExplorer.Enums;
using System.Linq.Expressions;
using SniffExplorer.Packets.Types;
using SniffExplorer.Utils;

namespace SniffExplorer.Packets.Parsing
{
    public static class BinaryProcessor
    {
        public static uint Build { get; private set; }
        public static string Locale { get; private set; }

        private static Dictionary<OpcodeClient, Type> _clientOpcodeStructs = new Dictionary<OpcodeClient, Type>();
        private static Dictionary<OpcodeServer, Type> _serverOpcodeStructs = new Dictionary<OpcodeServer, Type>();

        public static event Action<string> OnOpcodeParsed;
        public static event Action OnSniffLoaded;

        static BinaryProcessor()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsClass)
                    continue;

                var clientOpcodeAttrs = type.GetCustomAttributes<ClientPacketAttribute>();
                foreach (var opcodeAttribute in clientOpcodeAttrs)
                    _clientOpcodeStructs[opcodeAttribute.Opcode] = type;

                var serverOpcodeAttrs = type.GetCustomAttributes<ServerPacketAttribute>();
                foreach (var opcodeAttribute in serverOpcodeAttrs)
                    _serverOpcodeStructs[opcodeAttribute.Opcode] = type;
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
                        Type targetType;
                        switch (direction)
                        {
                            case 0x47534D43u: // CMSG
                                if (!_clientOpcodeStructs.ContainsKey((OpcodeClient) opcode))
                                    continue;
                                targetType = _clientOpcodeStructs[(OpcodeClient) opcode];
                                OnOpcodeParsed?.Invoke(((OpcodeClient) opcode).ToString());
                                break;
                            case 0x47534D53u: // SMSG
                                if (!_serverOpcodeStructs.ContainsKey((OpcodeServer) opcode))
                                    continue;
                                targetType = _serverOpcodeStructs[(OpcodeServer) opcode];
                                OnOpcodeParsed?.Invoke(((OpcodeServer) opcode).ToString());
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        var instance = (ValueType) Activator.CreateInstance(targetType);
                        switch (direction)
                        {
                            case 0x47534D43u: // CMSG
                            {
                                var opcodeEnum = (OpcodeClient) opcode;
                                if (!PacketTypeReadersStore.ContainsKey(targetType))
                                    GeneratePacketReader(targetType);

                                Store.Insert(opcodeEnum, PacketTypeReadersStore.Get(targetType)(packetReader), connectionID, timeStamp);
                                break;
                            }
                            case 0x47534D53u: // SMSG
                            {
                                var opcodeEnum = (OpcodeServer) opcode;
                                if (!PacketTypeReadersStore.ContainsKey(targetType))
                                        GeneratePacketReader(targetType);

                                Store.Insert(opcodeEnum, PacketTypeReadersStore.Get(targetType)(packetReader), connectionID, timeStamp);
                                break;
                            }
                        }

                        if (memoryStream.Position != memoryStream.Length)
                            Console.WriteLine("Failed to parse a full opcode!");
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

            PacketTypeReadersStore.Store(structureType, compiledExpression);
        }

        private static BlockExpression GenerateSubStructureReader(Type packetStructType, ParameterExpression argExpr)
        {
            if (TypeReadersStore.ContainsKey(packetStructType))
                return TypeReadersStore.Get(packetStructType);

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

            TypeReadersStore.Store(packetStructType, block);
            return block;
        }

        private static Expression GenerateArrayReader(Type packetStructType, PropertyInfo propInfo, ParameterExpression argExpr, Expression tExpr)
        {
            var propExpression = Expression.MakeMemberAccess(tExpr, propInfo);
            var relativeArraySizeAttr = propInfo.GetCustomAttribute<StreamedSizeAttribute>();
            var absoluteArraySizeAttr = propInfo.GetCustomAttribute<FixedSizeAttribute>();

            Expression arraySizeExpr;

            if (relativeArraySizeAttr == null && absoluteArraySizeAttr == null)
                throw new InvalidOperationException(
                    $"Property {propInfo.Name} is missing an array size specification!");

            if (relativeArraySizeAttr != null && absoluteArraySizeAttr != null)
                throw new InvalidOperationException(
                    $"Property {propInfo.Name} has multiple array size specifications!");

            if (relativeArraySizeAttr != null)
                arraySizeExpr = Expression.MakeMemberAccess(tExpr, packetStructType.GetProperty(relativeArraySizeAttr.PropertyName));
            else
                arraySizeExpr = Expression.Constant(absoluteArraySizeAttr.ArraySize);

            // ReSharper disable once AssignNullToNotNullAttribute
            var arrayInitExpr = Expression.New(propInfo.PropertyType.GetConstructor(new[] { typeof(int) }), arraySizeExpr);

            var exitLabelExpr = Expression.Label();
            var itrExpr = Expression.Variable(typeof(int));
            return Expression.Block(new[] { itrExpr },
                Expression.Assign(propExpression, arrayInitExpr),
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
            var bitSizeAttr = propInfo.GetCustomAttribute<BitFieldAttribute>();
            if (bitSizeAttr != null)
            {
                if (bitSizeAttr.BitSize == 1)
                    return Expression.Call(argExpr, ExpressionUtils.Bit);

                return Expression.Call(argExpr, ExpressionUtils.Bits, Expression.Constant(bitSizeAttr.BitSize));
            }

            var propType = propInfo.PropertyType;
            if (propType.IsArray)
                propType = propType.GetElementType();

            if (propType.IsArray)
                throw new NotImplementedException($"Field {propInfo.Name} is a multi-dimensional array");

            var packedAttr = propInfo.GetCustomAttribute<PackedFieldAttribute>();
            var typeCode = Type.GetTypeCode(propType);
            switch (typeCode)
            {
                case TypeCode.Int16:
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                    return Expression.Call(argExpr, ExpressionUtils.Base[typeCode]);
                case TypeCode.UInt64:
                    return Expression.Call(argExpr, packedAttr != null ?
                        ExpressionUtils.PackedUInt64 :
                        ExpressionUtils.Base[TypeCode.UInt64]);
                case TypeCode.DateTime:
                    return Expression.Call(ExpressionUtils.ServerEpoch.GetMethodInfo(),
                        Expression.Call(argExpr, ExpressionUtils.Base[TypeCode.Int32]));
                case TypeCode.String:
                {
                    var stringAttr = propInfo.GetCustomAttribute<WowStringAttribute>();
                    if (stringAttr != null)
                        return Expression.Call(argExpr, ExpressionUtils.String,
                            Expression.MakeMemberAccess(tExpr, packetStructType.GetProperty(stringAttr.PropertyName)));
                    return Expression.Call(argExpr, ExpressionUtils.CString);
                }
            }

            if (propInfo.PropertyType.IsAssignableFrom(typeof (ObjectGuid)))
                return Expression.Call(argExpr, ExpressionUtils.ObjectGuid);

            return GenerateSubStructureReader(propType, argExpr);
        }

        private static class ExpressionUtils
        {
            public static readonly MethodInfo ObjectGuid = typeof (PacketReader).GetMethod("ReadObjectGuid",
                Type.EmptyTypes);
            public static readonly MethodInfo String = typeof (PacketReader).GetMethod("ReadString",
                typeof (int));
            public static readonly MethodInfo CString = typeof (PacketReader).GetMethod("ReadString",
                Type.EmptyTypes);

            public static readonly MethodInfo PackedUInt64 = typeof (PacketReader).GetMethod("ReadPackedUInt64",
                Type.EmptyTypes);

            public static readonly MethodInfo Bit = typeof (PacketReader).GetMethod("ReadBit", Type.EmptyTypes);
            public static readonly MethodInfo Bits = typeof (PacketReader).GetMethod("ReadBits", typeof (int));

            public static readonly Func<int, DateTime> ServerEpoch =
                seconds => new DateTime(2000, 1, 1).AddSeconds(seconds);

            public static readonly Dictionary<TypeCode, MethodInfo> Base = new Dictionary<TypeCode, MethodInfo>()
            {
                { TypeCode.Boolean, typeof (PacketReader).GetMethod("ReadBoolean", Type.EmptyTypes) },
                { TypeCode.SByte,   typeof (PacketReader).GetMethod("ReadSByte", Type.EmptyTypes) },
                { TypeCode.Int16,   typeof (PacketReader).GetMethod("ReadInt16", Type.EmptyTypes) },
                { TypeCode.Int32,   typeof (PacketReader).GetMethod("ReadInt32", Type.EmptyTypes) },
                { TypeCode.Int64,   typeof (PacketReader).GetMethod("ReadInt64", Type.EmptyTypes) },
                { TypeCode.Byte,    typeof (PacketReader).GetMethod("ReadByte", Type.EmptyTypes) },
                { TypeCode.UInt16,  typeof (PacketReader).GetMethod("ReadUInt16", Type.EmptyTypes) },
                { TypeCode.UInt32,  typeof (PacketReader).GetMethod("ReadUInt32", Type.EmptyTypes) },
                { TypeCode.UInt64,  typeof (PacketReader).GetMethod("ReadUInt64", Type.EmptyTypes) },
                { TypeCode.Single,  typeof (PacketReader).GetMethod("ReadSingle", Type.EmptyTypes) },
                { TypeCode.Double,  typeof (PacketReader).GetMethod("ReadDouble", Type.EmptyTypes) },
            };
        }
    }
}
