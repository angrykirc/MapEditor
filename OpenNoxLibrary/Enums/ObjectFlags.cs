using System;

namespace OpenNoxLibrary.Enums
{
    [Flags]
    public enum ObjectFlag : uint
    {
        // These are guaranteed to correlate with values used by Nox internally
        NULL = 0,
        BELOW = 0x1,
        NO_UPDATE = 0x2,
        ACTIVE = 0x4,
        ALLOW_OVERLAP = 0x8,
        SHORT = 0x10,
        DESTROYED = 0x20,
        NO_COLLIDE = 0x40,
        MISSILE_HIT = 0x80,
        EQUIPPED = 0x100,
        PARTITIONED = 0x200,
        NO_COLLIDE_OWNER = 0x400,
        OWNER_VISIBLE = 0x800,
        EDIT_VISIBLE = 0x1000,
        NO_PUSH_CHARACTERS = 0x2000,
        AIRBORNE = 0x4000,
        DEAD = 0x8000,
        SHADOW = 0x10000,
        FALLING = 0x20000,
        IN_HOLE = 0x40000,
        RESPAWN = 0x80000,
        ON_OBJECT = 0x100000,
        SIGHT_DESTROY = 0x200000,
        TRANSIENT = 0x400000,
        BOUNCY = 0x800000,
        ENABLED = 0x1000000,
        PENDING = 0x2000000,
        TRANSLUCENT = 0x4000000,
        STILL = 0x8000000,
        NO_AUTO_DROP = 0x10000000,
        FLICKER = 0x20000000,
        SELECTED = 0x40000000,
        MARKED = 0x80000000
    }
}
