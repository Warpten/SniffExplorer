﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SniffExplorer.Packets.Types
{
    public enum HighGuidType
    {
        Null = 0,
        Uniq = 1,
        Player = 2,
        Item = 3,
        WorldTransaction = 4,
        StaticDoor = 5,
        Transport = 6,
        Conversation = 7,
        Creature = 8,
        Vehicle = 9,
        Pet = 10,
        GameObject = 11,
        DynamicObject = 12,
        AreaTrigger = 13,
        Corpse = 14,
        LootObject = 15,
        SceneObject = 16,
        Scenario = 17,
        AIGroup = 18,
        DynamicDoor = 19,
        ClientActor = 20,
        Vignette = 21,
        CallForHelp = 22,
        AIResource = 23,
        AILock = 24,
        AILockTicket = 25,
        ChatChannel = 26,
        Party = 27,
        Guild = 28,
        WowAccount = 29,
        BNetAccount = 30,
        GMTask = 31,
        MobileSession = 32,
        RaidGroup = 33,
        Spell = 34,
        Mail = 35,
        WebObj = 36,
        LFGObject = 37,
        LFGList = 38,
        UserRouter = 39,
        PVPQueueGroup = 40,
        UserClient = 41,
        PetBattle = 42,
        UniqUserClient = 43,
        BattlePet = 44,
        CommerceObj = 45,
        ClientSession = 46,
        Cast = 47
    }
}
