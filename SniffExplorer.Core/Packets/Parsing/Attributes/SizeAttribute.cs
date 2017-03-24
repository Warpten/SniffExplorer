using System;

namespace SniffExplorer.Core.Packets.Parsing.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SizeAttribute : Attribute
    {
        public int ArraySize { get; }
        public bool Streamed { get; }
        public bool InPlace { get; }
        public string PropertyName { get; }

        /// <summary>
        /// Indicates that the associated array has a fixed size.
        /// </summary>
        /// <param name="arraySize"></param>
        public SizeAttribute(int arraySize)
        {
            ArraySize = arraySize;
            Streamed = false;
            InPlace = false;
        }

        /// <summary>
        /// Indicates that the associated array size has been read in the named property.
        /// </summary>
        /// <param name="propertyName"></param>
        public SizeAttribute(string propertyName)
        {
            PropertyName = propertyName;
            InPlace = false;
            Streamed = true;
        }

        /// <summary>
        /// Indicates that the associated array size is streamed exactly before the array.
        /// </summary>
        public SizeAttribute()
        {
            InPlace = true;
            Streamed = true;
        }
    }
}
