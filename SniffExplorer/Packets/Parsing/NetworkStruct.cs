namespace SniffExplorer.Packets.Parsing
{
    public interface NetworkStruct<T> where T : struct, IPacketStruct
    {
        void Parse(Packet<T> parser);
    }
}
