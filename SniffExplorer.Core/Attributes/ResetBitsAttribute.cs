using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SniffExplorer.Core.Attributes
{
    /// <summary>
    /// Use this attribute to reset the current bit stream before that assigned property is read.
    ///
    /// This attribute effectively inserts a <see cref="PacketReader.ResetBitReader"/> call in the handler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ResetBitsAttribute : Attribute
    {
    }
}
