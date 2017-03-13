using System;
using SniffExplorer.Enums;

namespace SniffExplorer.Packets.Parsing
{
    /// <summary>
    /// Use this attribute to decorate structures that act
    /// as CMSG parsers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class ClientPacketAttribute : Attribute
    {
        public OpcodeClient Opcode { get; }

        public ClientPacketAttribute(OpcodeClient clientOpcode)
        {
            Opcode = clientOpcode;
        }
    }

    /// <summary>
    /// Use this attribute to decorate structures that act
    /// as SMSG parsers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class ServerPacketAttribute : Attribute
    {
        public OpcodeServer Opcode { get; }

        public ServerPacketAttribute(OpcodeServer clientOpcode)
        {
            Opcode = clientOpcode;
        }
    }

    /// <summary>
    /// Use this attribute to have the packets deserializer ignore
    /// the associated property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class IgnoreAttribute : Attribute
    {
        public IgnoreAttribute() { }
    }

    /// <summary>
    /// Use this attribute to have 64-bits field be read using
    /// <see cref="Packet{T}.ReadPackedUInt64"/> instead of
    /// <see cref="Packet{T}.ReadUInt64"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class PackedFieldAttribute : Attribute
    {
        public PackedFieldAttribute() { }
    }

    /// <summary>
    /// Use this attribute to decorate strings that have their lengths sent
    /// separately in the packet.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class WowStringAttribute : Attribute
    {
        public WowStringAttribute() { }
    }

    /// <summary>
    /// Use this attribute to indicate that the associated property is
    /// stored on bits.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BitFieldAttribute : Attribute
    {
        public int BitSize { get; }

        public BitFieldAttribute(int bitSize)
        {
            BitSize = bitSize;
        }
    }

    /// <summary>
    /// Use this attribute to bind the size of an array to a property
    /// read earlier during deserialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class StreamedSizeAttribute : Attribute
    {
        public string FieldName { get; }

        public StreamedSizeAttribute(string arraySizeName)
        {
            FieldName = arraySizeName;
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
