using System;

namespace SniffExplorer.Core.Attributes
{
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
}
