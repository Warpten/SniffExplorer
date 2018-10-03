using System;

namespace SniffExplorer.Core.Attributes
{
    /// <summary>
    /// Use this attribute to have the packets de-serializer ignore
    /// the associated property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class IgnoreAttribute : Attribute
    {
        public IgnoreAttribute() { }
    }
}
