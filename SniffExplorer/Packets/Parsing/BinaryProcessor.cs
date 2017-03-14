using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SniffExplorer.Enums;
using System.Linq.Expressions;
using SniffExplorer.Utils;

namespace SniffExplorer.Packets.Parsing
{
    public static class BinaryProcessor
    {
        public static uint Build { get; private set; }
        public static string Locale { get; private set; }

        private static Dictionary<OpcodeClient, Type> _clientOpcodeStructs = new Dictionary<OpcodeClient, Type>();
        private static Dictionary<OpcodeServer, Type> _serverOpcodeStructs = new Dictionary<OpcodeServer, Type>();

        private static Dictionary<OpcodeClient, Func<PacketReader, IPacketStruct>> _clientReaders = new Dictionary<OpcodeClient, Func<PacketReader, IPacketStruct>>();
        private static Dictionary<OpcodeServer, Func<PacketReader, IPacketStruct>> _serverReaders = new Dictionary<OpcodeServer, Func<PacketReader, IPacketStruct>>();

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
                sniffStream.BaseStream.Position += 40 + 4 + 4 + 4;

                while (sniffStream.BaseStream.Position < sniffStream.BaseStream.Length)
                {
                    var direction = sniffStream.ReadUInt32();
                    var connectionID = sniffStream.ReadUInt32();
                    var timeStamp = sniffStream.ReadUInt32();
                    var optionalHeaderLength = sniffStream.ReadUInt32();
                    var fullSize = sniffStream.ReadInt32() - 4;
                    sniffStream.BaseStream.Position += optionalHeaderLength;
                    var opcode = sniffStream.ReadUInt32();

                    Type targetType;
                    switch (direction)
                    {
                        case 0x47534D43u: // CMSG
                            targetType = _clientOpcodeStructs[(OpcodeClient) opcode];
                            OnOpcodeParsed?.Invoke(((OpcodeClient) opcode).ToString());
                            break;
                        case 0x47534D53u: // SMSG
                            targetType = _serverOpcodeStructs[(OpcodeServer) opcode];
                            OnOpcodeParsed?.Invoke(((OpcodeServer) opcode).ToString());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // Yuck.
                    using (var memoryStream = new MemoryStream(sniffStream.ReadBytes(fullSize), false))
                    using (var packetReader = new PacketReader(memoryStream, fullSize))
                    {
                        var instance = (IPacketStruct) Activator.CreateInstance(targetType);
                        instance.Date = new DateTime(1970, 1, 1).AddSeconds(timeStamp);
                        instance.ConnectionID = connectionID;
                        switch (direction)
                        {
                            case 0x47534D43u: // CMSG
                            {
                                var opcodeEnum = (OpcodeClient) opcode;
                                if (!_clientReaders.ContainsKey(opcodeEnum))
                                    _clientReaders[opcodeEnum] = GeneratePacketReader(targetType);

                                Store.Insert(opcodeEnum, _clientReaders[opcodeEnum](packetReader));
                                break;
                            }
                            case 0x47534D53u: // SMSG
                            {
                                var opcodeEnum = (OpcodeServer) opcode;
                                if (!_serverReaders.ContainsKey(opcodeEnum))
                                    _serverReaders[opcodeEnum] = GeneratePacketReader(targetType);

                                Store.Insert(opcodeEnum, _serverReaders[opcodeEnum](packetReader));
                                break;
                            }
                        }
                    }
                }

                OnSniffLoaded?.Invoke();
            }
        }

        private static Func<PacketReader, IPacketStruct> GeneratePacketReader(Type packetStructType)
        {
            var argExpr = Expression.Parameter(typeof(PacketReader));
            var resultExpr = Expression.Variable(packetStructType);
            var bodyExpressions = new List<Expression> {
                Expression.Assign(resultExpr, Expression.New(packetStructType))
            };

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var prop in packetStructType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetCustomAttribute(typeof(IgnoreAttribute)) != null)
                    continue;

                bodyExpressions.Add(prop.PropertyType.IsArray ?
                    GenerateArrayReader(packetStructType, prop, argExpr, resultExpr) :
                    GenerateFlatReader(packetStructType, prop, argExpr, resultExpr));
            }

            bodyExpressions.Add(Expression.Convert(resultExpr, typeof(IPacketStruct)));

            var lambda = Expression.Lambda<Func<PacketReader, IPacketStruct>>(
                Expression.Block(new[] { resultExpr }, bodyExpressions),
                argExpr);
            var compiledExpression = lambda.Compile();

            return compiledExpression;
        }

        private static Expression GenerateArrayReader(Type packetStructType, PropertyInfo propInfo, Expression argExpr, Expression tExpr)
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

        private static Expression GenerateFlatReader(Type packetStructType, PropertyInfo propInfo, Expression argExpr, Expression tExpr)
        {
            return Expression.Assign(Expression.MakeMemberAccess(tExpr, propInfo),
                GenerateValueReader(packetStructType, propInfo, argExpr, tExpr));
        } 

        private static Expression GenerateValueReader(Type packetStructType, PropertyInfo propInfo, Expression argExpr, Expression tExpr)
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

            // Selecting off typecodes doesn't cut it
            /// Handle specific types here - ObjectGuid, etc

            return null;
        }

        private static class ExpressionUtils
        {
            public static readonly MethodInfo ObjectGuid = typeof (PacketReader).GetMethod("ReadObjectGuid",
                Type.EmptyTypes);

            public static readonly MethodInfo String = typeof (PacketReader).GetMethod("ReadWoWString",
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
                { TypeCode.Int64,   typeof (PacketReader).GetMethod("ReadInt64", Type.EmptyTypes)},
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
