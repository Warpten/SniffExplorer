using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SniffExplorer.Enums;
using System.Linq.Expressions;

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
            var argExpr = Expression.Parameter(typeof(PacketReader), "reader");
            var resultExpr = Expression.Variable(packetStructType, packetStructType.Name + "Value");
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
                    GenerateFlatReader(prop, argExpr, resultExpr));
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
                arraySizeExpr = Expression.MakeMemberAccess(tExpr, packetStructType.GetProperty(relativeArraySizeAttr.FieldName));
            else
                arraySizeExpr = Expression.Constant(absoluteArraySizeAttr.ArraySize);

            // ReSharper disable once AssignNullToNotNullAttribute
            var arrayInitExpr = Expression.New(propInfo.PropertyType.GetConstructor(new[] { typeof(int) }), arraySizeExpr);

            var exitLabelExpr = Expression.Label();
            var itrExpr = Expression.Variable(typeof(int), "itr");
            return Expression.Block(new[] { itrExpr },
                Expression.Assign(propExpression, arrayInitExpr),
                Expression.Assign(itrExpr, Expression.Constant(0)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(itrExpr, arraySizeExpr),
                        Expression.Assign(
                            Expression.ArrayAccess(propExpression, Expression.PostIncrementAssign(itrExpr)),
                            GenerateValueReader(propInfo, argExpr)),
                        Expression.Break(exitLabelExpr)),
                    exitLabelExpr));
        }

        private static Expression GenerateFlatReader(PropertyInfo propInfo, Expression argExpr, Expression tExpr)
        {
            return Expression.Assign(Expression.MakeMemberAccess(tExpr, propInfo),
                GenerateValueReader(propInfo, argExpr));
        }

        private static Expression GenerateValueReader(PropertyInfo propInfo, Expression argExpr)
        {
            var bitSizeAttr = propInfo.GetCustomAttribute<BitFieldAttribute>();
            if (bitSizeAttr != null)
            {
                if (bitSizeAttr.BitSize == 1)
                    return Expression.Call(argExpr, typeof(PacketReader).GetMethod("ReadBit", Type.EmptyTypes));

                return Expression.Call(argExpr, typeof(PacketReader).GetMethod("ReadBits", new[] { typeof(int) }),
                    Expression.Constant(bitSizeAttr.BitSize));
            }

            var propType = propInfo.PropertyType;
            if (propType.IsArray)
                propType = propType.GetElementType();
            var packedAttr = propInfo.GetCustomAttribute<PackedFieldAttribute>();

            switch (Type.GetTypeCode(propType))
            {
                case TypeCode.Int16:
                    return Expression.Call(argExpr,
                        typeof(PacketReader).GetMethod("ReadInt16", Type.EmptyTypes));
                case TypeCode.Boolean:
                    return Expression.Call(argExpr,
                        typeof(PacketReader).GetMethod("ReadBool", Type.EmptyTypes));
                case TypeCode.SByte:
                    return Expression.Call(argExpr,
                        typeof(PacketReader).GetMethod("ReadSByte", Type.EmptyTypes));
                case TypeCode.Byte:
                    return Expression.Call(argExpr,
                        typeof(PacketReader).GetMethod("ReadByte", Type.EmptyTypes));
                case TypeCode.UInt16:
                    return Expression.Call(argExpr,
                        typeof(PacketReader).GetMethod("ReadUInt16", Type.EmptyTypes));
                case TypeCode.Int32:
                    return Expression.Call(argExpr,
                        typeof(PacketReader).GetMethod("ReadInt32", Type.EmptyTypes));
                case TypeCode.UInt32:
                    return Expression.Call(argExpr,
                        typeof(PacketReader).GetMethod("ReadUInt32", Type.EmptyTypes));
                case TypeCode.Int64:
                    return Expression.Call(argExpr,
                        typeof(PacketReader).GetMethod("ReadInt64", Type.EmptyTypes));
                case TypeCode.UInt64:
                    return Expression.Call(argExpr, packedAttr != null ?
                            typeof(PacketReader).GetMethod("ReadPackedUInt64", Type.EmptyTypes) :
                            typeof(PacketReader).GetMethod("ReadUInt64", Type.EmptyTypes));
                case TypeCode.Single:
                    return Expression.Call(argExpr,
                        typeof(PacketReader).GetMethod("ReadSingle", Type.EmptyTypes));
                case TypeCode.Double:
                    return Expression.Call(argExpr,
                        typeof(PacketReader).GetMethod("ReadDouble", Type.EmptyTypes));
                case TypeCode.DateTime:
                    return Expression.Call(ExpressionUtils.ServerEpoch,
                        typeof(DateTime).GetMethod("AddSeconds", new[] { typeof(int) }),
                        Expression.Call(argExpr, typeof(PacketReader).GetMethod("ReadInt32", Type.EmptyTypes)));
                case TypeCode.String:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static class ExpressionUtils
        {
            public static readonly Expression ServerEpoch = Expression.New(
                    // ReSharper disable once AssignNullToNotNullAttribute
                    typeof(DateTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) }),
                    Expression.Constant(2000), Expression.Constant(1), Expression.Constant(1));
        }
    }
}
