using System;

namespace OpenNoxLibrary.Enums
{
    [Flags]
    public enum MaterialFlag : uint
    {
        FLESH = 0x1,
        CLOTH = 0x2,
        ANIMAL_HIDE = 0x4,
        WOOD = 0x8,
        METAL = 0x10,
        STONE = 0x20,
        EARTH = 0x40,
        LIQUID = 0x80,
        GLASS = 0x100,
        PAPER = 0x200,
        SNOW = 0x400,
        MAGIC = 0x800,
        DIAMOND = 0x1000,
        NONE = 0x2000,
        NULL = 0x4000,
        NULL2 = 0x8000
    }
}
