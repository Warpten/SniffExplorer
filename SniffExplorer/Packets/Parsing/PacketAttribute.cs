using System;
using System.Linq.Expressions;
using SniffExplorer.Enums;
using SniffExplorer.Utils;

namespace SniffExplorer.Packets.Parsing
{
    /// <summary>
    /// Use this attribute to decorate structures that act
    /// as CMSG parsers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class PacketAttribute : Attribute
    {
        public Either<OpcodeClient, OpcodeServer> Opcode { get; }

        public PacketAttribute(OpcodeClient clientOpcode)
        {
            Opcode.LeftValue = clientOpcode;
        }

        public PacketAttribute(OpcodeServer clientOpcode)
        {
            Opcode.RightValue = clientOpcode;
        }
    }

    /// <summary>
    /// Use this attribute to have the packets de-serializer ignore
    /// the associated property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class IgnoreAttribute : Attribute
    {
        public IgnoreAttribute() { }
    }

    /// <summary>
    /// Use this attribute to have 64-bits field be read using
    /// <see cref="PacketReader.ReadPackedUInt64()"/> instead of
    /// <see cref="PacketReader.ReadUInt64()"/>. Also used to allow
    /// reading packed times.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class PackedFieldAttribute : Attribute
    {
        public PackedFieldAttribute() { }
    }

    /// <summary>
    /// Use this attribute to decorate strings that have their lengths sent
    /// separately in the packet. If the size is preceding the string itself,
    /// use the <see cref="StreamedSizeAttribute(bool)"/> constructor,
    /// and mark the string with a bit size if needed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class StreamedSizeAttribute : Attribute
    {
        public string PropertyName { get; }
        public bool InPlace { get; }

        public StreamedSizeAttribute(string propertyName)
        {
            PropertyName = propertyName;
            InPlace = false;
        }

        public StreamedSizeAttribute(bool inPlace = true)
        {
            InPlace = inPlace;
        }
    }

    /// <summary>
    /// Use this attribute to indicate that the associated property is
    /// stored on bits.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BitFieldAttribute : Attribute
    {
        public int BitSize { get; }

        public BitFieldAttribute(int bitSize = 1)
        {
            BitSize = bitSize;
        }

        internal Expression GetCallExpression(Expression argumentExpression, Type propertyType)
        {
            return Expression.Convert(BitSize == 1 ?
                Expression.Call(argumentExpression, ExpressionUtils.Bit) :
                Expression.Call(argumentExpression, ExpressionUtils.Bits, Expression.Constant(BitSize)), propertyType);
        }
    }

    /// <summary>
    /// Use this attribute to decorate fixed-size arrays.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class FixedSizeAttribute : Attribute
    {
        public int ArraySize { get; }

        public FixedSizeAttribute(int arraySize)
        {
            ArraySize = arraySize;
        }
    }
}
