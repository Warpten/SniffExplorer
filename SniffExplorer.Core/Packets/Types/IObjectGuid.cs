using SniffExplorer.Core.Packets.Parsing;

namespace SniffExplorer.Core.Packets.Types
{
    public interface IObjectGuid
    {
        HighGuidType HighType { get; }
        bool HasEntry();

        bool IsEmpty();

        uint Entry { get; }
        ulong Low { get; }

        void ReadPacked(PacketReader reader);
        void Read(PacketReader reader);
    }
}
