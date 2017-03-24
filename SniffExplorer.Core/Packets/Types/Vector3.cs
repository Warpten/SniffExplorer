using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SniffExplorer.Core.Packets.Types
{
    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public override string ToString() => $"{{ X: {X} Y: {Y} Z: {Z} }}";
    }
}
