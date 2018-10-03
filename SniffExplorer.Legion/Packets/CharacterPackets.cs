using System;
using System.ComponentModel;
using SniffExplorer.Core;
using SniffExplorer.Core.Packets.Parsing.Attributes;
using SniffExplorer.Core.Packets.Types;
using V22996 = SniffExplorer.Legion.Enums.V22996;

namespace SniffExplorer.Legion.Packets
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct PowerUpdateBlock
    {
        public uint Power { get; set; }
        public PowerType PowerType { get; set; }

        public override string ToString() => $"Power type: {PowerType} Amount: {Power}";
    }

    [Packet(typeof(V22996.OpcodeServer), "SMSG_POWER_UPDATE"), TargetBuild(22996)]
    public struct UserClientPowerUpdate
    {
        public ObjectGuid128 GUID { get; set; }

        [Size]
        public PowerUpdateBlock[] Updates { get; set; }
    }

    [Packet(typeof(V22996.OpcodeServer), "SMSG_LEVEL_UP_INFO"), TargetBuild(22996)]
    public struct ClientLevelUpInfo
    {
        public int Level { get; set; }
        public int HealthDelta { get; set; }

        [Size(Method = SizeMethod.FixedSize, Param = 6)]
        public int[] PowerDelta { get; set; }

        [Size(Method = SizeMethod.FixedSize, Param = 4)]
        public int[] StatDelta { get; set; }

        public int Cp { get; set; }
    }

    [Packet(typeof(V22996.OpcodeServer), "SMSG_HEALTH_UPDATE"), TargetBuild(22996)]
    public struct ClientHealthUpdate
    {
        public ObjectGuid128 GUID { get; set; }
        public int Health { get; set; }
        public int Unk { get; set; } // Huh. Not in WPP ??
    }

    [Packet(typeof(V22996.OpcodeClient), "CMSG_ALTER_APPEARANCE"), TargetBuild(22996)]
    public struct PlayerCliAlterAppearance
    {
        public int NewHairStyle { get; set; }
        public int NewHairColor { get; set; }
        public int NewFacialHair { get; set; }
        public int NewSkinColor { get; set; }
        public int NewFace { get; set; }

        [Size(Method = SizeMethod.FixedSize, Param = 3)]
        public int[] NewCustomDisplay { get; set; }
    }

    [Packet(typeof(V22996.OpcodeServer), "SMSG_INSPECT_PVP"), TargetBuild(22996)]
    public struct UserClientInspectPVP
    {
        public ObjectGuid128 GUID { get; set; }

        [BitField(3)]
        public int BracketCount { get; set; }

        [Size(Method = SizeMethod.StreamedProperty, Param = "BracketCount")]
        public BracketData[] Brackets { get; set; }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct BracketData
    {
        public int Rating { get; set; }
        public int Rank { get; set; }
        public int WeeklyPlayed { get; set; }
        public int WeeklyWon { get; set; }
        public int SeasonPlayed { get; set; }
        public int SeasonWon { get; set; }
        public int WeeklyBestRating { get; set; }
        public int Unk710 { get; set; }
        public int Bracket { get; set; }
    }

    [Packet(typeof(V22996.OpcodeServer), "SMSG_ENUM_CHARACTERS_RESULT"), TargetBuild(22996)]
    public struct ClientEnumCharactersResult
    {
        public bool Success { get; set; }
        public bool IsDeletedCharacters { get; set; }
        public bool IsDemonHunterCreationAllowed { get; set; }
        public bool HasDemonHunterOnRealm { get; set; }
        public bool HasLevel70OnRealm { get; set; }
        public bool Unknown7x { get; set; }
        public bool HasDisabledClassesMask { get; set; }

        [Browsable(false)]
        public int CharacterCount { get; set; }
        [Browsable(false)]
        public int FactionChangeRestrictionCount { get; set; }

        [Conditional("HasDisabledClassesMask", ConditionType.Equal, true)]
        public uint DisabledClassessMask { get; set; }

        [Size(Method = SizeMethod.StreamedProperty, Param = "FactionChangeRestrictionCount")]
        public ClientRestrictedFactionChangeRule[] FactionChangeRestrictions { get; set; }

        [Size(Method = SizeMethod.StreamedProperty, Param = "CharacterCount")]
        public ClientCharacterListEntry[] Characters { get; set; }
    }

    [TypeConverter(typeof (ExpandableObjectConverter))]
    public struct ClientRestrictedFactionChangeRule
    {
        public uint Mask { get; set; }
        public byte RaceID { get; set; }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct ClientCharacterListEntry
    {
        public ObjectGuid128 GUID { get; set; }
        public byte ListPosition { get; set; }
        public byte RaceID { get; set; }
        public byte ClassID { get; set; }
        public byte SexID { get; set; }
        public byte SkinID { get; set; }
        public byte FaceID { get; set; }
        public byte HairStyle { get; set; }
        public byte HairColor { get; set; }
        public byte FacialHairStyle { get; set; }
        [Size(Method = SizeMethod.FixedSize, Param = 3)]
        public int[] NewCustomDisplay { get; set; }

        public byte ExperienceLevel { get; set; }
        public uint ZoneID { get; set; }
        public uint MapID { get; set; }

        public Vector3 PreloadPos { get; set; }
        public ObjectGuid128 Guild { get; set; }
        [Size(Method = SizeMethod.FixedSize, Param = 3)]
        public uint[] Flags { get; set; }

        public uint PetCreatureDisplayID { get; set; }
        public uint PetExperienceLevel { get; set; }
        public uint PetCreatureFamilyID { get; set; }

        [Size(Method = SizeMethod.FixedSize, Param = 2)]
        public uint[] ProfessionIDs { get; set; }

        [Size(Method = SizeMethod.FixedSize, Param = 23)]
        public ClientCharacterListItem[] InventoryItems { get; set; }

        public DateTime LastPlayedTime { get; set; }

        public ushort SpecID { get; set; }
        public uint Unknown703 { get; set; }
        public uint Flags4 { get; set; }

        [Browsable(false), BitField(6)]
        public int NameLength { get; set; }
        public bool FirstLogin { get; set; }
        public bool BoostInProgress { get; set; }
        [BitField(5)]
        public int UnkWod61x { get; set; }

        [Size(Method = SizeMethod.StreamedProperty, Param = "NameLength")]
        public string Name { get; set; }

        public override string ToString() => $"{Name} (Level {ExperienceLevel})";
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct ClientCharacterListItem
    {
        public uint DisplayID { get; set; }
        public uint DisplayEnchantID { get; set; }
        public byte InvType { get; set; }
    }
}
