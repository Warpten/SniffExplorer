using System;

namespace SniffExplorer.Core.Attributes
{
    public enum SizeMethod
    {
        /// <summary>
        /// The size was read in a property of the structure. The name of that property is provided in <see cref="SizeAttribute.Param"/>
        /// </summary>
        StreamedProperty,

        /// <summary>
        /// When used, define the size, in bits, of the field's length as <see cref="SizeAttribute.Param"/>.
        ///
        /// Alternatively, mark the string with <see cref="BitFieldAttribute"/> for the same effect.
        ///
        /// If neither <see cref="BitFieldAttribute"/> nor <see cref="SizeAttribute.Param"/> are set,
        /// assumes an <b>aligned</b> <pre>32</pre> bits length.
        /// </summary>
        InPlace,

        /// <summary>
        /// Size is hardcoded and provided in <see cref="SizeAttribute.Param"/>
        /// </summary>
        FixedSize
    }

    /// <summary>
    /// This attribute allows the deserializer to be aware of the way an array's size is streamed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SizeAttribute : Attribute
    {
        public SizeMethod Method { get; set; }
        public object Param { get; set; }
    }

    /// <summary>
    /// This attribute allows the deserializer to be aware of the way a string's size is streamed.
    ///
    /// This class is a verbatim copy of <see cref="SizeAttribute"/> to allow correct decoration of string arrays.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class StringSizeAttribute : Attribute
    {
        public SizeMethod Method { get; set; }
        public object Param { get; set; }
    }
}
