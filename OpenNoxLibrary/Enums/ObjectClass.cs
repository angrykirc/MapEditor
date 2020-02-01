using System;

namespace OpenNoxLibrary.Enums
{
    [Flags]
    public enum ObjectClass : uint
    {
        // These are guaranteed to correlate with values used by Nox internally
        NULL = 0x0,
        MISSILE = 0x1,
        MONSTER = 0x2,
        PLAYER = 0x4,
        OBSTACLE = 0x8,
        FOOD = 0x10,
        EXIT = 0x20,
        KEY = 0x40,
        DOOR = 0x80,
        INFO_BOOK = 0x100,
        TRIGGER = 0x200,
        TRANSPORTER = 0x400,
        HOLE = 0x800,
        WAND = 0x1000,
        FIRE = 0x2000,
        ELEVATOR = 0x4000,
        ELEVATOR_SHAFT = 0x8000,
        DANGEROUS = 0x10000,
        MONSTERGENERATOR = 0x20000,
        READABLE = 0x40000,
        LIGHT = 0x80000,
        SIMPLE = 0x100000,
        COMPLEX = 0x200000,
        IMMOBILE = 0x400000,
        VISIBLE_ENABLE = 0x800000,
        WEAPON = 0x1000000,
        ARMOR = 0x2000000,
        NOT_STACKABLE = 0x4000000,
        TREASURE = 0x8000000,
        FLAG = 0x10000000,
        CLIENT_PERSIST = 0x20000000,
        CLIENT_PREDICT = 0x40000000,
        PICKUP = 0x80000000
    }
}
