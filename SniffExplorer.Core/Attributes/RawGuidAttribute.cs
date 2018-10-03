using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SniffExplorer.Core.Packets.Types;

namespace SniffExplorer.Core.Attributes
{
    /// <summary>
    /// This attribute, when used on supertypes of <see cref="IObjectGuid"/>,
    /// forces the deserializer to read it as a direct value. See <see cref="IObjectGuid.Read(PacketReader)"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RawGuidAttribute : Attribute
    {
    }
}
