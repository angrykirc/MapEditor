﻿using System;

namespace OpenNoxLibrary.Enums
{
    public static class Monsters
    {
        [Flags]
        public enum StatusFlags : uint
        {
            DESTROY_WHEN_DEAD = 0x1,
            CHECK = 0x2,
            CAN_BLOCK = 0x4,
            CAN_DODGE = 0x8,
            unused = 0x10,
            CAN_CAST_SPELLS = 0x20,
            HOLD_YOUR_GROUND = 0x40,
            SUMMONED = 0x80,
            ALERT = 0x100,
            INJURED = 0x200,
            CAN_SEE_FRIENDS = 0x400,
            CAN_HEAL_SELF = 0x800,
            CAN_HEAL_OTHERS = 0x1000,
            CAN_RUN = 0x2000,
            RUNNING = 0x4000,
            ALWAYS_RUN = 0x8000,
            NEVER_RUN = 0x10000,
            BOT = 0x20000,
            MORPHED = 0x40000,
            ON_FIRE = 0x80000,
            STAY_DEAD = 0x100000,
            FRUSTRATED = 0x200000
        }

        [Flags]
        public enum SpellCastFlags : uint
        {
            Reaction = 0x08000000,
            Defensive = 0x10000000,
            Disabling = 0x20000000,
            Offensive = 0x40000000,
            Escape = 0x80000000
        }

        public static string[] ActionStrings =
	    {
		    "ACTION_IDLE",
		    "ACTION_WAIT",
		    "ACTION_WAIT_RELATIVE",
		    "ACTION_ESCORT",
		    "ACTION_GUARD",
		    "ACTION_HUNT",
		    "ACTION_RETREAT",
		    "ACTION_MOVE_TO",
		    "ACTION_FAR_MOVE_TO",
		    "ACTION_DODGE",
		    "ACTION_ROAM",
		    "ACTION_PICKUP_OBJECT",
		    "ACTION_DROP_OBJECT",
		    "ACTION_FIND_OBJECT",
		    "ACTION_RETREAT_TO_MASTER",
		    "ACTION_FIGHT",
		    "ACTION_MELEE_ATTACK",
		    "ACTION_MISSILE_ATTACK",
		    "ACTION_CAST_SPELL_ON_OBJECT",
		    "ACTION_CAST_SPELL_ON_LOCATION",
		    "ACTION_CAST_DURATION_SPELL",
		    "ACTION_BLOCK_ATTACK",
		    "ACTION_BLOCK_FINISH",
		    "ACTION_WEAPON_BLOCK",
		    "ACTION_FLEE",
		    "ACTION_FACE_LOCATION",
		    "ACTION_FACE_OBJECT",
		    "ACTION_FACE_ANGLE",
		    "ACTION_SET_ANGLE",
		    "ACTION_RANDOM_WALK",
		    "ACTION_DYING",
		    "ACTION_DEAD",
		    "ACTION_REPORT",
		    "ACTION_MORPH_INTO_CHEST",
		    "ACTION_MORPH_BACK_TO_SELF",
		    "ACTION_GET_UP",
		    "ACTION_CONFUSED",
		    "ACTION_MOVE_TO_HOME",
		    "ACTION_INVALID"
	    };
    }
}