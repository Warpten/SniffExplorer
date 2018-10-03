using SniffExplorer.Core.Attributes;
using SniffExplorer.Core.Packets.Types;
using System.ComponentModel;

namespace SniffExplorer.Cataclysm.Packets
{
    public struct ClientBindPointUpdate // SMSG_BIND_POINT_UPDATE
    {
        public Vector3 Position { get; set; }
        public uint MapID { get; set; }
        public uint AreaID { get; set; }
    }

    public struct ClientPlayerBound // SMSG_PLAYER_BOUND
    {
        public ObjectGuid64 BinderID { get; set; }
        public uint AreaID { get; set; }
    }

    public struct ClientBinderConfirm // SMSG_BINDER_CONFIRM
    {
        public ObjectGuid64 Unit { get; set; }
    }

    public struct ClientInvalidatePlayer // SMSG_INVALIDATE_PLAYER
    {
        public ObjectGuid64 Guid { get; set; }
    }

    public struct ClientSetCurrency // SMSG_UPDATE_CURRENCY, should be SMSG_SET_CURRENCY // UNCHECKED
    {
        [Browsable(false), BitField]
        public bool HasWeeklyQuantity { get; set; }
        [BitField]
        public bool HasSeasonQuantity { get; set; }
        [BitField]
        public bool SuppressChatLog { get; set; }
        [Conditional("HasSeasonQuantity", ConditionType.Equal, true)]
        public int SeasonQuantity { get; set; }
        public int Quantity { get; set; }
        public int Type { get; set; }
        [Conditional("HasSeasonQuantity", ConditionType.Equal, true)]
        public int WeeklyQuantity { get; set; }
    }

    public struct ClientDeathReleaseLoc // SMSG_DEATH_RELEASE_LOC
    {
        public uint MapID { get; set; }
        public Vector3 Position { get; set; }
    }

    public struct ClientSetStandState // SMSG_STANDSTATE_UPDATE, should be SMSG_STAND_STATE_UPDATE
    {
        public byte State { get; set; }
    }

    public struct ClientStartMirrorTimer // SMSG_START_MIRROR_TIMER
    {
        public uint Timer { get; set; }
        public uint Value { get; set; }
        public uint MaxValue { get; set; }
        public uint Scale { get; set; } // ?
        public bool Paused { get; set; }
        public uint SpellID { get; set; }
    }

    public struct ClientPauseMirrorTimer // SMSG_PAUSE_MIRROR_TIMER, unknown structure
    {

    }

    public struct ClientStopMirrorTimer // SMSG_STOP_MIRROR_TIMER
    {
        public uint Timer { get; set; }
    }

    public struct ClientExplorationExperience // SMSG_EXPLORATION_EXPERIENCE
    {
        public uint AreaID { get; set; }
        public uint Experience { get; set; }
    }

    public struct ClientUITime // SMSG_WORLD_STATE_UI_TIMER_UPDATE, should be SMSG_UI_TIME
    {
        public uint Time { get; set; }
    }

    public struct ClientTriggerMovie // SMSG_TRIGGER_MOVIE
    {
        public uint MoveID { get; set; }
    }

    public struct ClientTriggerCinematic // SMSG_TRIGGER_CINEMATIC
    {
        public uint CinematicID { get; set; }
    }

    public struct ClientWorldServerInfo // SMSG_WORLD_SERVER_INFO
    {
        [BitField]
        public bool HasRestrictedAccountMaxLevel { get; set; }
        [BitField]
        public bool HasRestrictedAccountMaxMoney { get; set; }
        [BitField]
        public bool HasInstanceGroupSize { get; set; }

        [ResetBits]

        [Conditional("HasInstanceGroupSize", ConditionType.Equal, true)]
        public uint InstanceGroupSize { get; set; }

        public bool IsTournamentRealm { get; set; }

        [Conditional("HasRestrictedAccountMaxLevel", ConditionType.Equal, true)]
        public uint RestrictedAccountMaxLevel { get; set; }
        [Conditional("HasRestrictedAccountMaxMoney", ConditionType.Equal, true)]
        public uint RestrictedAccountMaxMoney { get; set; }
        public uint LastWeeklyReset { get; set; }
        public uint DifficultyID { get; set; }
    }

    public struct ClientLoginSetTimeSpeed // SMSG_LOGIN_SETTIMESPEED, should be SMSG_LOGIN_SET_TIME_SPEED
    {
        public float NewSpeed { get; set; }
        public uint HolidayOffset { get; set; }
    }

    public struct ClientPhaseShiftChange // SMSG_SET_PHASESHIFT, should be SMSG_PHASE_SHIFT_CHANGE
    {
        // Streamed...
        public ObjectGuid64 Client { get; set; }
        [Size(Method = SizeMethod.InPlace)]
        public uint[] UiWorldMapAreaIDSwaps { get; set; }
        public uint PhaseShiftFlags { get; set; }
        [Size(Method = SizeMethod.InPlace)]
        public uint[] PreloadMapIDs { get; set; }
        // Not finished
    }

    // CMSG_WORLD_STATE_UI_TIMER_UPDATE, should be CMSG_UI_TIMER_UPDATE
    public struct ClientEmptyPacket
    {

    }
}
