using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SniffExplorer.Core.Attributes
{
    /// <summary>
    /// Due to the nature of Cataclysm's streamed GUID mess,
    /// you should consider only using this attribute for guids
    /// that are streamed in one single go.
    ///
    /// To be perfectly clear: as of right now, this attribute is <b>not</b>
    /// suited for interleaved GUIDs.
    ///
    /// Should you need interleaved GUIDs, you will have to do the deserialization yourself.
    /// </summary>
    /// <remarks>
    /// In the future, it may be possible to declare new attributes,
    /// respectively for the bit and byte streams, to be assigned to
    /// random properties. These would act as pseudo-properties of the
    /// structure used in serializing the GUIDs - akin to local variables,
    /// but hidden away by the deserialization processor.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class StreamedGuidAttribute : Attribute
    {
        public byte[] BitStream { get; set; }
        public byte[] ByteStream { get; set; }

    }
}
