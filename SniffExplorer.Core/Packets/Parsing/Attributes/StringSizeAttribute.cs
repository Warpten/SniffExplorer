using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SniffExplorer.Core.Packets.Parsing.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class StringSizeAttribute : Attribute
    {
        public int ArraySize { get; }
        public bool Streamed { get; }
        public bool InPlace { get; }
        public string PropertyName { get; }

        /// <summary>
        /// Indicates that the associated string has a fixed size.
        /// </summary>
        /// <param name="arraySize"></param>
        public StringSizeAttribute(int arraySize)
        {
            ArraySize = arraySize;
            Streamed = false;
            InPlace = false;
        }

        /// <summary>
        /// Indicates that the associated string size has been read in the named property.
        /// </summary>
        /// <param name="propertyName"></param>
        public StringSizeAttribute(string propertyName)
        {
            PropertyName = propertyName;
            InPlace = false;
            Streamed = true;
        }

        /// <summary>
        /// Indicates that the associated string size is streamed exactly before the array.
        /// </summary>
        public StringSizeAttribute()
        {
            InPlace = true;
            Streamed = true;
        }
    }
}
