using System;

namespace SniffExplorer.Core
{
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = true)]
    public sealed class OpcodeAttribute : Attribute
    {
        public uint Direction { get; }
        public uint[] TargetBuilds { get; }

        public OpcodeAttribute(bool clientOpcode, params uint[] targetBuilds)
        {
            Direction = clientOpcode ? 0x47534D43u : 0x47534D53u;
            TargetBuilds = targetBuilds;
        }
    }
}
