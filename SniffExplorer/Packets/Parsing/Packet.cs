using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using SniffExplorer.Utils;
using System.Reflection;
using System.Reflection.Emit;

namespace SniffExplorer.Packets.Parsing
{
    //! TODO: Make this class static, and have generic methods
    //! TODO: Right now it all is a gigantic hack where it is
    //! TODO: basically allocated and immediately destroyed after
    //! TODO: parsing. All that remains is _loader.
    public sealed class Packet<T> where T : struct, IPacketStruct
    {
        private BinaryReader _reader;

        private DateTime _date;
        private uint _connectionID;

        private uint _dataSize;

        private byte _bitpos = 8;
        private byte _curbitval;

        private static Func<Packet<T>, T> _loader;

        public Packet(BinaryReader reader, uint timeStamp, uint dataSize, uint connectionID)
        {
            _dataSize = dataSize;

            _connectionID = connectionID;
            _date = new DateTime(1970, 1, 1).AddSeconds(timeStamp);

            if (_loader == null)
                _loader = GenerateReader();

            Read(reader);
        }

        private void Read(BinaryReader reader)
        {
            _reader = reader;
            var instance = _loader(this);
            _reader = null;

            instance.Date = _date;
            instance.ConnectionID = _connectionID;
            Store<T>.Insert(instance);
        }

        private Func<Packet<T>, T> GenerateReader()
        {
#if DEBUG
            var asmName = new AssemblyName("Foo");
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly
                (asmName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = asmBuilder.DefineDynamicModule("Foo", "Foo.exe");

            var typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.Public);
            var methodBuilder = typeBuilder.DefineMethod("Main",
                MethodAttributes.Static, typeof(void), new[] { typeof(string) });
#endif

            var argExpr = Expression.Parameter(typeof (Packet<T>), "packetStruct");
            var resultExpr = Expression.Variable(typeof (T), typeof (T).Name + "Value");
            var bodyExpressions = new List<Expression> {
                Expression.Assign(resultExpr, Expression.New(typeof (T)))
            };

            foreach (var prop in typeof (T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetCustomAttribute(typeof (IgnoreAttribute)) != null)
                    continue;

                bodyExpressions.Add(prop.PropertyType.IsArray ?
                    GenerateArrayReader(prop, argExpr, resultExpr) :
                    GenerateFlatReader(prop, argExpr, resultExpr));
            }

            bodyExpressions.Add(resultExpr);

            var lambda = Expression.Lambda<Func<Packet<T>, T>>(Expression.Block(new[] { resultExpr }, bodyExpressions),
                argExpr);
            var compiledExpression = lambda.Compile();
#if DEBUG
            lambda.CompileToMethod(methodBuilder);
            typeBuilder.CreateType();
            asmBuilder.SetEntryPoint(methodBuilder);
            asmBuilder.Save("Foo.exe");
#endif
            return compiledExpression;
        }

        private Expression GenerateArrayReader(PropertyInfo propInfo, Expression argExpr, Expression tExpr)
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
                arraySizeExpr = Expression.MakeMemberAccess(tExpr, typeof (T).GetProperty(relativeArraySizeAttr.FieldName));
            else
                arraySizeExpr = Expression.Constant(absoluteArraySizeAttr.ArraySize);

            // ReSharper disable once AssignNullToNotNullAttribute
            var arrayInitExpr = Expression.New(propInfo.PropertyType.GetConstructor(new[] {typeof(int)}), arraySizeExpr);

            var exitLabelExpr = Expression.Label();
            var itrExpr = Expression.Variable(typeof(int), "itr");
            return Expression.Block(new[] {itrExpr},
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

        private Expression GenerateFlatReader(PropertyInfo propInfo, Expression argExpr, Expression tExpr)
        {
            return Expression.Assign(Expression.MakeMemberAccess(tExpr, propInfo),
                GenerateValueReader(propInfo, argExpr));
        }

        private Expression GenerateValueReader(PropertyInfo propInfo, Expression argExpr)
        {
            var bitSizeAttr = propInfo.GetCustomAttribute<BitFieldAttribute>();
            if (bitSizeAttr != null)
            {
                if (bitSizeAttr.BitSize == 1)
                    return Expression.Call(argExpr, GetType().GetMethod("ReadBit", Type.EmptyTypes));

                return Expression.Call(argExpr, GetType().GetMethod("ReadBits", new[] { typeof(int) }),
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
                        GetType().GetMethod("ReadInt16", Type.EmptyTypes));
                case TypeCode.Boolean:
                    return Expression.Call(argExpr,
                        GetType().GetMethod("ReadBool", Type.EmptyTypes));
                case TypeCode.SByte:
                    return Expression.Call(argExpr,
                        GetType().GetMethod("ReadSByte", Type.EmptyTypes));
                case TypeCode.Byte:
                    return Expression.Call(argExpr,
                        GetType().GetMethod("ReadByte", Type.EmptyTypes));
                case TypeCode.UInt16:
                    return Expression.Call(argExpr,
                        GetType().GetMethod("ReadUInt16", Type.EmptyTypes));
                case TypeCode.Int32:
                    return Expression.Call(argExpr,
                        GetType().GetMethod("ReadInt32", Type.EmptyTypes));
                case TypeCode.UInt32:
                    return Expression.Call(argExpr,
                        GetType().GetMethod("ReadUInt32", Type.EmptyTypes));
                case TypeCode.Int64:
                    return Expression.Call(argExpr,
                        GetType().GetMethod("ReadInt64", Type.EmptyTypes));
                case TypeCode.UInt64:
                    return Expression.Call(argExpr, packedAttr != null ?
                            GetType().GetMethod("ReadPackedUInt64", Type.EmptyTypes) :
                            GetType().GetMethod("ReadUInt64", Type.EmptyTypes));
                case TypeCode.Single:
                    return Expression.Call(argExpr,
                        GetType().GetMethod("ReadSingle", Type.EmptyTypes));
                case TypeCode.Double:
                    return Expression.Call(argExpr,
                        GetType().GetMethod("ReadDouble", Type.EmptyTypes));
                case TypeCode.DateTime:
                {
                    return Expression.Call(ExpressionUtils.ServerEpoch,
                        typeof (DateTime).GetMethod("AddSeconds", new[] { typeof(int) }),
                        Expression.Call(argExpr, GetType().GetMethod("ReadInt32", Type.EmptyTypes)));
                }
                case TypeCode.String:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CheckValid(int size)
        {
            if (_dataSize < size)
                throw new InvalidOperationException(nameof(size));

            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            _dataSize -= (uint)size;
        }

        #region Readers
        public long ReadInt64()
        {
            ResetBitReader();
            CheckValid(sizeof(long));
            return _reader.ReadInt64();
        }

        public ulong ReadUInt64()
        {
            ResetBitReader();
            CheckValid(sizeof(ulong));
            return _reader.ReadUInt64();
        }

        public int ReadInt32()
        {
            ResetBitReader();
            CheckValid(sizeof (int));
            return _reader.ReadInt32();
        }

        public uint ReadUInt32()
        {
            ResetBitReader();
            CheckValid(sizeof (uint));
            return _reader.ReadUInt32();
        }

        public short ReadInt16()
        {
            CheckValid(sizeof (short));
            return _reader.ReadInt16();
        }

        public ushort ReadUInt16()
        {
            ResetBitReader();
            CheckValid(sizeof (ushort));
            return _reader.ReadUInt16();
        }

        public byte ReadByte()
        {
            ResetBitReader();
            CheckValid(sizeof (byte));
            return _reader.ReadByte();
        }

        public sbyte ReadSByte()
        {
            ResetBitReader();
            CheckValid(sizeof (sbyte));
            return _reader.ReadSByte();
        }

        public float ReadSingle()
        {
            ResetBitReader();
            CheckValid(sizeof (float));
            return _reader.ReadSingle();
        }

        public double ReadDouble()
        {
            ResetBitReader();
            CheckValid(sizeof (double));
            return _reader.ReadDouble();
        }

        public U ReadStruct<U>() where U : struct
        {
            ResetBitReader();
            CheckValid(SizeCache<U>.Size);
            return _reader.ReadStruct<U>();
        }

        public bool ReadBool()
        {
            ResetBitReader();
            CheckValid(sizeof (byte));
            return _reader.ReadByte() != 0;
        }

        public void ResetBitReader()
        {
            _bitpos = 8;
        }

        public bool ReadBit()
        {
            ++_bitpos;

            if (_bitpos > 7)
            {
                _bitpos = 0;
                CheckValid(sizeof(byte));
                _curbitval = _reader.ReadByte();
            }

            return ((_curbitval >> (7 - _bitpos)) & 1) != 0;
        }

        public uint ReadBits(int bits)
        {
            uint value = 0;
            for (var i = bits - 1; i >= 0; --i)
                if (ReadBit())
                    value |= (uint) (1 << i);

            return value;
        }

        public string ReadCString()
        {
            var bytes = new List<byte>();

            byte b;
            while ((b = ReadByte()) != 0)  // CDataStore::GetCString calls CanRead too
                bytes.Add(b);

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public ulong ReadPackedUInt64()
        {
            return ReadPackedUInt64(ReadByte());
        }

        private ulong ReadPackedUInt64(byte mask)
        {
            if (mask == 0)
                return 0;

            ulong res = 0;

            var i = 0;
            while (i < 8)
            {
                if ((mask & 1 << i) != 0)
                    res += (ulong)ReadByte() << (i * 8);

                i++;
            }

            return res;
        }
        #endregion
    }

    internal static class ExpressionUtils
    {
        public static readonly Expression ServerEpoch = Expression.New(
                // ReSharper disable once AssignNullToNotNullAttribute
                typeof(DateTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) }),
                Expression.Constant(2000), Expression.Constant(1), Expression.Constant(1));
    }
}
