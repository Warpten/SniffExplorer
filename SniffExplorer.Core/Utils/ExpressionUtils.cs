using System;
using System.Collections.Generic;
using System.Reflection;
using SniffExplorer.Core.Packets.Parsing;

namespace SniffExplorer.Core.Utils
{
    public static class ExpressionUtils
    {
        public static readonly MethodInfo ObjectGuid = typeof(PacketReader).GetMethod("ReadObjectGuid",
            Type.EmptyTypes);
        public static readonly MethodInfo String = typeof(PacketReader).GetMethod("ReadString",
            typeof(int));
        public static readonly MethodInfo CString = typeof(PacketReader).GetMethod("ReadCString",
            Type.EmptyTypes);

        public static readonly MethodInfo PackedUInt64 = typeof(PacketReader).GetMethod("ReadPackedUInt64",
            Type.EmptyTypes);

        public static readonly MethodInfo Bit = typeof(PacketReader).GetMethod("ReadBit", Type.EmptyTypes);
        public static readonly MethodInfo Bits = typeof(PacketReader).GetMethod("ReadBits", typeof(int));

        public static readonly MethodInfo ReadTime = typeof(PacketReader).GetMethod("ReadTime", Type.EmptyTypes);
        public static readonly MethodInfo ReadPackedTime = typeof(PacketReader).GetMethod("ReadPackedTime", Type.EmptyTypes);

        public static readonly Dictionary<TypeCode, MethodInfo> Base = new Dictionary<TypeCode, MethodInfo>()
            {
                { TypeCode.Boolean, typeof (PacketReader).GetMethod("ReadBit", Type.EmptyTypes) },
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
