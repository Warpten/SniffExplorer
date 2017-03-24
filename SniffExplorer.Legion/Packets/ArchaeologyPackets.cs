using System;
using System.ComponentModel;
using SniffExplorer.Core;
using SniffExplorer.Core.Packets.Parsing.Attributes;
using V22996 = SniffExplorer.Legion.Enums.V22996;

namespace SniffExplorer.Legion.Packets
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct ResearchHistory
    {
        public int ProjectID { get; set; }
        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime FirstCompleted { get; set; }
        public int CompletionCount { get; set; }
    }

    [Packet(typeof(V22996.OpcodeServer), "SMSG_SETUP_RESEARCH_HISTORY"), TargetBuild(22996)]
    public struct ClientSetupResearchHistory
    {
        [Size]
        public ResearchHistory[] History { get; set; }
    }

    [Packet(typeof(V22996.OpcodeServer), "SMSG_RESEARCH_COMPLETE"), TargetBuild(22996)]
    public struct ClientResearchComplete
    {
        public ResearchHistory Research { get; set; }
    }

    [Packet(typeof(V22996.OpcodeServer), "SMSG_ARCHAEOLOGY_SURVERY_CAST"), TargetBuild(22996)]
    public struct ClientArchaeologySurveryCast
    {
        public uint NumFindsCompleted { get; set; }
        public uint TotalFinds { get; set; }
        public int ResearchBranchID { get; set; }
        public bool SuccessfulFind { get; set; }
    }
}
