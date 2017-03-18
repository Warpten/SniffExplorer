using System;
using SniffExplorer.Enums;
using SniffExplorer.Packets.Parsing;

namespace SniffExplorer.Packets
{
    public struct ResearchHistory
    {
        public int ProjectID { get; set; }
        public DateTime FirstCompleted { get; set; }
        public int CompletionCount { get; set; }
    }

    [ServerPacket(OpcodeServer.SMSG_SETUP_RESEARCH_HISTORY)]
    public struct ClientSetupResearchHistory
    {
        [StreamedSize]
        public ResearchHistory[] History { get; set; }
    }

    [ServerPacket(OpcodeServer.SMSG_RESEARCH_COMPLETE)]
    public struct ClientResearchComplete
    {
        public ResearchHistory Research { get; set; }
    }

    [ServerPacket(OpcodeServer.SMSG_ARCHAEOLOGY_SURVERY_CAST)]
    public struct ClientArchaeologySurveryCast
    {
        public uint NumFindsCompleted { get; set; }
        public uint TotalFinds { get; set; }
        public int ResearchBranchID { get; set; }
        public bool SuccessfulFind { get; set; }
    }
}
