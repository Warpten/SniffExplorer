using System;

namespace SniffExplorer.Core.Packets.Types
{
    /// <summary>
    /// This enumeration describes various data types.
    /// 
    /// <list type="bullet">
    ///   <item>
    ///     <term>Spell</term>
    ///     <description>Describes auras and spells.</description>
    ///   </item>
    /// </list>
    /// </summary>
    public enum MetaDataType : byte
    {
        Spell,
    }

    /// <summary>
    /// This class is used to identify specific elements in various packet structures
    /// and provides a way to accurately track the various occurrences of a specific
    /// value.
    /// 
    /// See <see cref="MetaDataType"/> for more informations about the different types
    /// of data values that are handled.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MetaAttribute : Attribute
    {
        public MetaDataType Type { get; }

        public MetaAttribute(MetaDataType type)
        {
            Type = type;
        }
    }
}
