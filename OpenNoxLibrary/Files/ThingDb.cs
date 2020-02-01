using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;

using OpenNoxLibrary.Encryption;
using OpenNoxLibrary.Enums;
using OpenNoxLibrary.Log;

// Since I cannot claim myself to be the sole data researcher and author of this particular piece of code,
// Here goes a credit for each person that was known to be taking part in making this particular class,
// re-built from NoxShared project.
// ***************
//    Eric Litak
//   Andrew Wesie
//     *Templar*
//    Joshua Statzer
//  NoxForum.net team
//        ..
// ***************

namespace OpenNoxLibrary.Files
{
	public class ThingDb
	{
		public enum ThingToken : uint
		{
			FLOR = 0x464C4F52,//Floor Tile entry
			EDGE = 0x45444745,//Edge Tile entry
			WALL = 0x57414C4C,//Wall entry
			END = 0x454E4420,//End of entry
			AUD = 0x41554420,//Audio section
			AVNT = 0x41564E54,//???
			SPEL = 0x5350454C, //Spell section
			ABIL = 0x4142494C,//Ability section
			IMAG = 0x494D4147,//Image section
			THNG = 0x54484E47,//474e4854,//Thing entry
			STAT = 0x53544154,//Animation State entry
			SEQU = 0x53455155,//Sequence within a State entry
		}

		public class Tile : IComparable
		{
			public enum TileType : uint
			{
				Floor = ThingToken.FLOR,
				Edge = ThingToken.EDGE
			}

            /// <summary>
            /// Substitution color is used for displaying specified tile/edge type if no floor texture mode is used
            /// </summary>
            public uint SubstColor;

            /// <summary>
            /// Specifies whenether this is an edge or normal tile (their entries are very similar)
            /// </summary>
			public TileType Type;

            /// <summary>
            /// Name of this tile type used by the game internally
            /// </summary>
			public string Name;

            /// <summary>
            /// ImageID's for each tile variation
            /// </summary>
            public uint[] Variations;

            /// <summary>
            /// Determines what kind of sound will be emitted by tile when player steps on it
            /// </summary>
            public MaterialFlag MaterialSoundFlag;

            // How many rows and columns will recreate original image, you can track this on GalavaGateFacade for ex.
			public byte NumRows;
			public byte NumCols;
            public byte NumParts;
            public byte Unknown;

            public uint Flags1;
            public uint Flags2;
             
			public int Id;//must be set as the entries are read in. sorted in this order. (0-n)

			public Tile()
			{
			}

			public void Read(BinaryReader rdr, TileType ttype)
			{
                Type = ttype;
				Name = rdr.ReadString(); // Single byte prefix

                if (Type == TileType.Floor)
                    SubstColor = rdr.ReadUInt32();
                else
                    MaterialSoundFlag = (MaterialFlag)rdr.ReadUInt32();

                Flags1 = rdr.ReadUInt32();
				
                int numVariations;
                if (Type == TileType.Floor)
                {
                    Flags2 = rdr.ReadUInt32();

                    NumRows = rdr.ReadByte();
                    NumCols = rdr.ReadByte();
                    NumParts = rdr.ReadByte();

                    Unknown = rdr.ReadByte();

                    numVariations = NumParts * NumRows * NumCols;
                }
                else // Edges
                {
                    Unknown = rdr.ReadByte(); // If 1, variations are skipped, by Nox.exe
                    NumParts = rdr.ReadByte();
                    
                    Flags2 = rdr.ReadUInt16();

                    NumRows = rdr.ReadByte();
                    NumCols = rdr.ReadByte();

                    numVariations = 2 * NumParts * (NumRows + NumCols);

                    if (Unknown == 1) return;
                }

                Variations = new uint[numVariations];

                for (int i = 0; i < numVariations; i++)
                    Variations[i] = rdr.ReadUInt32();
			}

			public int CompareTo(object obj)
			{
				Tile rhs = obj as Tile;
				if (rhs == null || Id == rhs.Id)
					return 0;
				else
					return (int)(Id - rhs.Id);
			}

			public override string ToString()
			{
				return Name;
			}
		}

		public class Wall
		{
			public string Name;
			public int Id;
			public uint Flags1;
			public MaterialFlag MaterialFlag;
			public uint Flags3;
            public ushort Flags4;
            public byte Unknown;

            /// <summary>
            /// The types of objects created when the wall is broken
            /// </summary>
            public string[] Debris;

            public string AudSecretOpen;
            public string AudSecretClose;
            public string AudBreak;

			public RenderInfo[][] RenderNormal;
			public RenderInfo[][] RenderBreakable;

            public struct RenderInfo
            {
                public int OffX;
                public int OffY;
                public uint SpriteIndex;

                public RenderInfo(int doX, int doY, uint sprite)
                {
                    OffX = doX;
                    OffY = doY;
                    SpriteIndex = sprite;
                }
            }

			public Wall()
			{
				RenderNormal = new RenderInfo[15][];
				RenderBreakable = new RenderInfo[15][];
			}
			
			public void Read(BinaryReader rdr)
			{
				Name = rdr.ReadString();
				Flags1 = rdr.ReadUInt32();
                MaterialFlag = (MaterialFlag)rdr.ReadUInt32();
                Flags3 = rdr.ReadUInt32();
                Flags4 = rdr.ReadUInt16();
				rdr.BaseStream.Seek((8 - rdr.BaseStream.Position % 8) % 8, SeekOrigin.Current);//SkipToNextBoundary

                uint numDebris = rdr.ReadUInt32();
                rdr.ReadUInt32(); // Always zero
                Debris = new string[numDebris];

				for (int i = 0; i < numDebris; i++)
					Debris[i] = rdr.ReadString();

				AudSecretOpen = rdr.ReadString();
				AudSecretClose = rdr.ReadString();
				AudBreak = rdr.ReadString();
				
				List<RenderInfo> spritesNormal;
				List<RenderInfo> spritesSecret;
				Unknown = rdr.ReadByte();
				
				int direction = 0; RenderInfo wri;
				while (direction < 15) // 0x5A9E14 - directions
				{
					byte spriteCount = rdr.ReadByte(); 
					if (spriteCount == 0)
						continue;
					
					spritesNormal = new List<RenderInfo>(4);
					spritesSecret = new List<RenderInfo>(4);
                    rdr.BaseStream.Seek((8 - rdr.BaseStream.Position % 8) % 8, SeekOrigin.Current);//SkipToNextBoundary

					while (spriteCount > 0)
					{
						for (int i = 0; i < 4; i++)
						{
							wri = new RenderInfo(rdr.ReadInt32(), rdr.ReadInt32(), rdr.ReadUInt32());
							if (i > 1)
								spritesSecret.Add(wri);
							else
								spritesNormal.Add(wri);
                            // Nox.exe parseWALL_Client 0x46A010
						}
						spriteCount--;
					}
					RenderNormal[direction] = spritesNormal.ToArray();
					RenderBreakable[direction] = spritesSecret.ToArray();
					direction++;
				}
			}

			public int CompareTo(object obj)
			{
				Wall rhs = obj as Wall;
				if (rhs == null || Id == rhs.Id)
					return 0;
				else
					return (int)(Id - rhs.Id);
			}

			public override string ToString()
			{
				return Name;
			}
		}

		public class AudioMapping
		{
			public string Name;
			public List<string> Sounds = new List<string>();

            // Note: AVNTs and AUDs have this data block shared when loaded by the game
            public ushort Intensity;
            public ushort AIFlags;
            public byte Unknown3, Unknown4, Unknown5;

			public AudioMapping()
			{
			}

			public void Read(BinaryReader rdr)
			{
				Name = rdr.ReadString();
                // Nox.exe 0x502370, exact order
                AIFlags = rdr.ReadUInt16();
                Unknown3 = rdr.ReadByte();
                Intensity = (ushort)(15 * rdr.ReadUInt16());
                Unknown5 = rdr.ReadByte();
                Unknown4 = 2; // Yes it's constant
                rdr.BaseStream.Seek(3, SeekOrigin.Current);

                byte stringLen;
				while ((stringLen = rdr.ReadByte()) != 0)
				{
                    Sounds.Add(Encoding.ASCII.GetString(rdr.ReadBytes(stringLen)));
				}
			}
		}

        // Contains custom configuration data for looping/sustained sounds (Overrides AUD's)
		public class AudioEvent
		{
			public string Name;
            public List<string> Sounds = new List<string>();

            // Note: AVNTs and AUDs have this data block shared when loaded by the game
            public ushort Intensity;
            public ushort AIFlags;
            public byte Unknown3, Unknown4, Unknown5;

			public AudioEvent()
			{
			}

            public void Read(BinaryReader rdr)
			{
				Name = rdr.ReadString();

                while (true) // RLE
                {
                    byte opcode; 
                    switch (opcode = rdr.ReadByte())
                    {
                        case 0:
                            return;
                        case 1:
                        case 5:
                            rdr.BaseStream.Seek(1, SeekOrigin.Current);
                            break;
                        case 2:
                            Unknown4 = rdr.ReadByte();
                            break;
                        case 3:
                            Unknown3 = rdr.ReadByte();
                            break;
                        case 4:
                            Unknown5 = rdr.ReadByte();
                            break;
                        case 6:
                            rdr.BaseStream.Seek(2, SeekOrigin.Current);
                            break;
                        case 9:
                            Intensity = (ushort)(15 * rdr.ReadUInt16());
                            break;
                        case 10:
                            AIFlags = rdr.ReadUInt16();
                            break;
                        case 7:
                            byte stringLen;
                            while ((stringLen = rdr.ReadByte()) != 0)
                            {
                                Sounds.Add(Encoding.ASCII.GetString(rdr.ReadBytes(stringLen)));
                            }
                            break;
                        case 8:
                            rdr.BaseStream.Seek(8, SeekOrigin.Current);
                            break;
                        default:
                            Debug.Fail(String.Format("Unknown AVNT opcode {0}", opcode));
                            return;

                    }
                }
			}
		}

		public class Spell
		{
			public enum Phoneme : byte
			{
				KA = 0,
				UN = 1,
				IN = 2,
				ET = 3,
                PAUSE = 4,
				CHA = 5,
				RO = 6,
				ZO = 7,
				DO = 8
			}

            [Flags]
			public enum SpellFlags : uint
			{
                UNKNOWN_1 = 0x01,
				UNKNOWN_2 = 0x02,
                SPAWN_PROJECTILE = 0x04,
                UNKNOWN_8 = 0x08,
                UNKNOWN_10 = 0x10,
                HOSTILE = 0x20,
                UNKNOWN_40 = 0x40,
                UNKNOWN_80 = 0x80,
                UNKNOWN_100 = 0x100,
                FRIENDLY = 0x200,
                UNKNOWN_400 = 0x400,
                UNKNOWN_800 = 0x800,
                UNKNOWN_1000 = 0x1000,
                UNKNOWN_2000 = 0x2000,
                UNKNOWN_4000 = 0x4000,
                UNKNOWN_8000 = 0x8000,
                UNKNOWN_10000 = 0x10000,
                UNKNOWN_20000 = 0x20000,
                UNKNOWN_40000 = 0x40000,
                UNKNOWN_80000 = 0x80000,
                UNKNOWN_100000 = 0x100000,
                UNKNOWN_200000 = 0x200000,
                UNKNOWN_400000 = 0x400000,
                UNKNOWN_800000 = 0x800000,
                UNKNOWN_1000000 = 0x1000000,
                UNKNOWN_2000000 = 0x2000000,
                UNKNOWN_4000000 = 0x4000000,
                UNKNOWN_8000000 = 0x8000000,
                UNKNOWN_10000000 = 0x10000000,
                UNKNOWN_20000000 = 0x20000000,
                UNKNOWN_40000000 = 0x40000000,
                UNKNOWN_80000000 = 0x80000000
			}

			public string SysName;
			public byte ManaCost;
			public Phoneme[] Phonemes;
			public SpellFlags Flags;

			public string DisplayNameCsf;
			public string DescriptionCsf;
			public string SoundCast;
			public string SoundOn;
			public string SoundOff;

			public uint SpriteIcon, SpriteIconCast;
			public byte b1, b2;

			public Spell()
			{
			}

			public void Read(BinaryReader rdr)
			{
				SysName = rdr.ReadString();
				ManaCost = rdr.ReadByte();
				b1 = rdr.ReadByte();
				b2 = rdr.ReadByte();

				int numPhonemes = rdr.ReadByte();
                Phonemes = new Phoneme[numPhonemes];
                for (int i = 0; i < numPhonemes; i++)
					Phonemes[i] = (Phoneme) rdr.ReadByte();

				SpriteIcon = rdr.ReadUInt32();
				SpriteIconCast = rdr.ReadUInt32();
				Flags = (SpellFlags)rdr.ReadUInt32();

				DisplayNameCsf = rdr.ReadString();
				DescriptionCsf = new string(rdr.ReadChars(rdr.ReadUInt16()));
				SoundCast = rdr.ReadString();
				SoundOn = rdr.ReadString();
				SoundOff = rdr.ReadString();
			}
		}

		public class Ability
		{
			public string Name;
			public string NameString;
			public string DescriptionString;
			public string SoundCast;
			public string SoundOn;
			public string SoundOff;

			public uint SpriteIcon, SpriteIconActive, SpriteIconOff;

			public Ability()
			{
			}

			public void Read(BinaryReader rdr)
			{
				Name = rdr.ReadString();
				byte b = rdr.ReadByte(); // Null byte
                // Nox demo thing.bin follows with 0xFFFFFFFF and then %file%.pcx strings instead 
                SpriteIcon = rdr.ReadUInt32(); 
                SpriteIconActive = rdr.ReadUInt32();
                SpriteIconOff = rdr.ReadUInt32();

				NameString = rdr.ReadString();
				DescriptionString = new string(rdr.ReadChars(rdr.ReadUInt16()));
				SoundCast = rdr.ReadString();
				SoundOn = rdr.ReadString();
				SoundOff = rdr.ReadString();
			}
		}

        public class Animation
        {
            public enum AnimationType
            {
                Loop,
                OneShot,
                OneShotRemove,
                LoopAndFade,
                Slave,
                Random
            }

            public AnimationType Type;
            public byte MonsterAnimId;
            public List<uint> Frames;
            public List<Sequence> Sequences = new List<Sequence>();

            public class Sequence
            {
                public string Name;
                public uint[] Frames;

                internal void Read(BinaryReader rdr)
                {
                    var framez = new List<uint>();

                    while (true)
                    {
                        uint next = rdr.ReadUInt32();
                        rdr.BaseStream.Seek(-4, SeekOrigin.Current);
                        if (next != (uint)ThingToken.SEQU//HACK: this condition is used to detect end of Frame list within a Sequence(FIXME)
                            && next != (uint)ThingToken.STAT
                            && next != (uint)ThingToken.END)
                            framez.Add(rdr.ReadUInt32());
                        else
                            break;
                    }

                    Frames = framez.ToArray();
                }
            }

            public Animation()
            {
                Frames = new List<uint>();
            }

            public void Read(BinaryReader rdr, bool isSequence)
            {
                byte count = rdr.ReadByte();

                MonsterAnimId = rdr.ReadByte();
                Type = (AnimationType)Enum.Parse(typeof(AnimationType), rdr.ReadString());

                if (isSequence)
                {
                    ThingToken tok;
                    while ((tok = (ThingToken)rdr.ReadUInt32()) == ThingToken.SEQU)
                    {
                        var sequ = new Sequence();
                        sequ.Name = rdr.ReadString();
                        sequ.Read(rdr);
                        Sequences.Add(sequ);
                    }
                    rdr.BaseStream.Seek(-4, SeekOrigin.Current);
                }
                else
                {
                    while (count-- > 0)
                        Frames.Add(rdr.ReadUInt32());
                }
            }
        }

        public class State
        {
            public string Name;
            public Animation Animation;
            public byte ActionId;

            public State()
            {
            }

            public void Read(BinaryReader rdr, string drawType)
            {
                if (drawType == "AnimateStateDraw") // the States usually come in threes, the first has type 2, then 4, then 8
                {
                    uint type = rdr.ReadUInt32();
                    //Note that this format is similar to that of MonsterDraw, except that monster draw has 1 byte preceding this part, not 4
                    Name = rdr.ReadString();
                    short maybeCount = rdr.ReadInt16();
                    Animation = new Animation();
                    Animation.Read(rdr, false);
                }
                else
                {
                    if (drawType == "PlayerDraw")
                    {
                        Name = rdr.ReadString();
                        Animation = new Animation();
                        Animation.Read(rdr, true);
                    }
                    else //for MonsterDraw, MaidenDraw
                    {
                        ActionId = rdr.ReadByte();
                        Name = rdr.ReadString();
                        uint c = rdr.ReadUInt16();//always 0x0001?
                        //HACK keep tacking on till we see a STAT or END
                        Animation = new Animation();
                        Animation.Read(rdr, false);
                        while (true)
                        {
                            uint next = rdr.ReadUInt32();
                            rdr.BaseStream.Seek(-4, SeekOrigin.Current);
                            if (next == (uint)ThingToken.STAT
                                || next == (uint)ThingToken.END)
                                break; // We hit next entry
                            else
                                Animation.Frames.Add(rdr.ReadUInt32());
                        }
                    }
                }
            }
        }

		public class Sprite
		{
			public string Name;

			public int Type1Sprite = -1;
			public Animation Type2Animation = null;

			public Sprite()
			{
			}

            public void Read(BinaryReader rdr)
			{
				Name = rdr.ReadString();
				byte type = rdr.ReadByte();
                if (type == 1)
                {
                    Type1Sprite = rdr.ReadInt32();
                }
                else if (type == 2)
                {
                    Type2Animation = new Animation();
                    Type2Animation.Read(rdr, false);
                }
			}
		}

		public class Thing
		{
			// These field names must remain as is!! Read() uses reflection to initialize them
			public string Name;
			public uint Speed;
			public uint Health;
			public uint Worth;
			public int SizeX;
			public int SizeY;
			public string Size;
            public string Extent;
            public string ExtentType;
            public int ExtentX;
            public int ExtentY;
			public int Z;
			public string ZSize;
            public int ZSizeX;
            public int ZSizeY;
			public ObjectFlag Flags;
			public ObjectClass Class;
			public BitArray Subclass = new BitArray(subclassBitCount);
			public uint Weight;
			public MaterialFlag Material;
			public float Mass;
			public string Pickup;
			public string Drop;
			public string Collide;
			public string Xfer;
			public string Create;
			public string Damage;
			public string Die;
			public string Init;
			public string Update;
			public string PrettyName;
			public string Description;
			public string Use;
			public byte WandCharges;
			public string DrawType = "NoDraw";
			public int SpritePrettyImage;
			public int SpriteMenuIcon;
			public List<uint> SpriteAnimFrames = null;
			public List<State> SpriteStates = null;
			
			protected const int subclassBitCount = 97;
			public enum SubclassBitIndex : int // Index into the bitarray
			{
				//these values are arbitrary and not necessarily what Nox uses internally.
				//assumes that the first element is 0 then 1,2,3,etc. (verify this in c# specification)
				NULL,
				ABILITY_BOOK,
				APPLE,
				ARM_ARMOR,
				ARROW,
				AXE,
				BACK,
				BOLT,
				BOMBER,
				BOOTS,
				BOW,
				BREASTPLATE,
				CHAKRAM,
				CHEST_NE,
				CHEST_NW,
				CHEST_SE,
				CHEST_SW,
				CROSSBOW,
				CURE_POISON_POTION,
				DAGGER,
				FEMALE_NPC,
				FIELD_GUIDE,
				FIRE_PROTECT_POTION,
				GATE,
				GENERATOR_NE,
				GENERATOR_NW,
				GENERATOR_SE,
				GENERATOR_SW,
				GREAT_SWORD,
				HAMMER,
				HAS_SOUL,
				HASTE_POTION,
				HEALTH_POTION,
				HEAVY,
				HELMET,
				IMMUNE_ELECTRICITY,
				IMMUNE_FEAR,
				IMMUNE_FIRE,
				IMMUNE_POISON,
				INFRAVISION_POTION,
				INVISIBILITY_POTION,
				INVISIBLE_OBELISK,
				INVULNERABILITY_POTION,
				JUG,
				LARGE_MONSTER,
				LAVA,
				LEG_ARMOR,
				LONG_SWORD,
				LOOK_AROUND,
				LOTD,
				MACE,
				MAGIC,
				MANA_POTION,
				MEDIUM_MONSTER,
				MISSILE_COUNTERSPELL,
				MUSHROOM,
				NO_SPELL_TARGET,
				NO_TARGET,
				NPC,
				NPC_WIZARD,
				OGRE_AXE,
				PANTS,
				POISON_PROTECT_POTION,
				POTION,
				QUEST_EXIT,
				QUEST_WARP_EXIT,
				QUIVER,
				SHIELD,
				SHIELD_POTION,
				SHIRT,
				SHOCK_PROTECT_POTION,
				SHOPKEEPER,
				SHURIKEN,
				SIMPLE,
				SMALL_MONSTER,
				SPELL_BOOK,
				STAFF,
				STAFF_DEATH_RAY,
				STAFF_FIREBALL,
				STAFF_FORCE_OF_NATURE,
				STAFF_LIGHTNING,
				STAFF_OBLIVION_HALBERD,
				STAFF_OBLIVION_HEART,
				STAFF_OBLIVION_ORB,
				STAFF_OBLIVION_WIERDLING,
				STAFF_SULPHOROUS_FLARE,
				STAFF_SULPHOROUS_SHOWER,
				STAFF_TRIPLE_FIREBALL,
				STONE_DOOR,
				SWORD,
				TECH,
				UNDEAD,
				USEABLE,
				VAMPIRISM_POTION,
				VISIBLE_OBELISK,
				WARCRY_STUN,
				WOUNDED_NPC,
			}

			public Thing()
			{
			}
			
			public bool HasClassFlag(ObjectClass flag)
			{
				if ((Class & flag) == flag) return true;
				return false;
			}
			
			public bool HasObjectFlag(ObjectFlag flag)
			{
				if ((Flags & flag) == flag) return true;
				return false;
			}

			public void Read(BinaryReader rdr)
			{
				Name = rdr.ReadString();

				while (true)
				{
					byte nextByte = rdr.ReadByte();
					if (nextByte == 0)//thing entry is terminated by a null byte
						break;
					rdr.BaseStream.Seek(-1, SeekOrigin.Current);

					string line = rdr.ReadString();
					if (line.EndsWith("Draw"))
						DrawType = line;

					//skip, length, raw frames
					if (line == "StaticDraw"
						|| line == "ArmorDraw"
						|| line == "WeaponDraw"
						|| line == "SlaveDraw"
						|| line == "BaseDraw")//single frame
					{
						rdr.BaseStream.Seek((8 - rdr.BaseStream.Position % 8) % 8, SeekOrigin.Current);//SkipToNextBoundary
						long finishPos = rdr.ReadInt64() + rdr.BaseStream.Position;
						SpriteAnimFrames = new List<uint>();
						if (line == "SlaveDraw")
                        {
                            byte len = rdr.ReadByte(); 
                            while ((len--) != 0)
                            	SpriteAnimFrames.Add(rdr.ReadUInt32());
                        }
						else
							SpriteAnimFrames.Add(rdr.ReadUInt32());
                        rdr.BaseStream.Seek(finishPos, SeekOrigin.Begin);
					}
					//skip, length, 1 animation
					else if (line == "AnimateDraw"
						|| line == "SphericalShieldDraw"
						|| line == "WeaponAnimateDraw"
						|| line == "FlagDraw"
						|| line == "SummonEffectDraw"
						|| line == "ReleasedSoulDraw"
						|| line == "GlyphDraw")
					{
						rdr.BaseStream.Seek((8 - rdr.BaseStream.Position % 8) % 8, SeekOrigin.Current);//SkipToNextBoundary
						long finishPos = rdr.ReadInt64() + rdr.BaseStream.Position;
                        
						Animation ani = new Animation();
                        ani.Read(rdr, false);
						while (rdr.BaseStream.Position < finishPos)//HACK
						{
							ani.Frames.Add(rdr.ReadUInt32());
						}
						SpriteAnimFrames = ani.Frames;
						Debug.Assert(rdr.BaseStream.Position == finishPos);
					}
					//skip, length, State entries
					else if (line == "AnimateStateDraw"
						|| line == "PlayerDraw"
						|| line == "MonsterDraw"
						|| line == "MaidenDraw")
					{
						rdr.BaseStream.Seek((8 - rdr.BaseStream.Position % 8) % 8, SeekOrigin.Current);//SkipToNextBoundary
                        SpriteStates = new List<State>();
						ulong length = rdr.ReadUInt64();
                        ThingToken token;
                        while ((token = (ThingToken)rdr.ReadUInt32()) != ThingToken.END)
                        {
                            if (token == ThingToken.STAT)
                            {
                                var stat = new State();
                                stat.Read(rdr, line);
                                SpriteStates.Add(stat);
                            }
                            else
                            {
                                Debug.Fail(String.Format("Unexpected token was hit during parsing {0}: {1}", line, token));
                            }
                        }
					}
					//skip, length, byte(numFrames) prefixed raw frames
					else if (line == "BoulderDraw"
						|| line == "StaticRandomDraw"
						|| line == "DoorDraw"
						|| line == "ArrowDraw"
						|| line == "HarpoonDraw"
						|| line == "WeakArrowDraw")
					{
						rdr.BaseStream.Seek((8 - rdr.BaseStream.Position % 8) % 8, SeekOrigin.Current);//SkipToNextBoundary
						ulong length = rdr.ReadUInt64();
						byte numFrames = rdr.ReadByte();
						SpriteAnimFrames = new List<uint>(numFrames);
						while (numFrames-- > 0)
							SpriteAnimFrames.Add(rdr.ReadUInt32());
					}
					else if (line == "VectorAnimateDraw")
					{
						rdr.BaseStream.Seek((8 - rdr.BaseStream.Position % 8) % 8, SeekOrigin.Current);//SkipToNextBoundary
						long finishPos = rdr.ReadInt64() + rdr.BaseStream.Position;
						Animation ani = new Animation();
                        ani.Read(rdr, false);
						//FIXME: this may be a Loop of Loops and should probably be constructed as such
						//HACK: right now we just read until we reach the given length, tacking on the frames to the existing ones
						while (rdr.BaseStream.Position < finishPos)
						{
							ani.Frames.Add(rdr.ReadUInt32());
						}
						SpriteAnimFrames = ani.Frames;
					}
					//skip, length, number of animations, animations
					else if (line == "ConditionalAnimateDraw"
						|| line == "MonsterGeneratorDraw")
					{
						rdr.BaseStream.Seek((8 - rdr.BaseStream.Position % 8) % 8, SeekOrigin.Current);//SkipToNextBoundary
						long finishPos = rdr.ReadInt64() + rdr.BaseStream.Position;
						byte numAni = rdr.ReadByte();
                        while (numAni-- > 0)
                        {
                            new Animation().Read(rdr, false);
                        }
					}
					// 2 pretty useless ints, then skip -- this is the most common and simplest, so the default
					else if (line.EndsWith("Draw") && line.IndexOf(" ") == -1)
					{
                        rdr.BaseStream.Seek((8 - rdr.BaseStream.Position % 8) % 8, SeekOrigin.Current);//SkipToNextBoundary
                        ulong length = rdr.ReadUInt64(); // Should always be zero, since there are no frames to be stored
					}
					else if (line == "MENUICON")
						SpriteMenuIcon = rdr.ReadInt32();
					else if (line == "PRETTYIMAGE")
						SpritePrettyImage = rdr.ReadInt32();
					else
						Parse(line);
				}
			}

            private Regex regex1 = new Regex("(?<field>.*)( = )(?<value>.*)", RegexOptions.IgnoreCase);
            private Regex regex2 = new Regex("(?<flag>\\w+)");

            protected uint ParseFlagField(Regex regex, string value, Type enumType)
            {
                uint result = 0;

                foreach (Match match in regex.Matches(value))
                {
                    string flag = match.Groups["flag"].Value;
                    // get rid of those pesky exceptions
                    if (Enum.IsDefined(enumType, flag))
                        result |= (uint)Enum.Parse(enumType, flag);
                }

                return result;
            }

			protected void Parse(string line)
			{
				CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
                Regex regex = regex1;
                Match regexmatch = regex.Match(line);
                string fldString = regexmatch.Groups["field"].Value;
                string valString = regexmatch.Groups["value"].Value;
                
				FieldInfo field = GetType().GetField(fldString, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
				if (field != null)
				{
					object val = null; string[] strs = null;

					// Special handling for enumed types
					switch (field.Name)
					{
						case "Flags":
	                        regex = regex2;//group "flag" will have whatever's between plus signs
							
							field.SetValue(this, ParseFlagField(regex, valString, typeof(ObjectFlag)));
							break;
						case "Z":
                        	field.SetValue(this, Convert.ToInt32(valString));
                        	break;
                        case "ZSize":
                        	field.SetValue(this, valString);
	                        strs = valString.Split(' ');
	                        ZSizeX = Convert.ToInt32(strs[0]);
	                        ZSizeY = Convert.ToInt32(strs[1]);
	                        break;
	                    case "Size":
	                        field.SetValue(this, valString);
                       		strs = valString.Split(' ');
                        	SizeX = Convert.ToInt32(strs[0]);
                        	SizeY = Convert.ToInt32(strs[1]);
                        	break;
                        case "Extent":
                        	field.SetValue(this, valString);
	                        if (valString.Contains("CIRCLE"))
	                        {
	                            strs = valString.Split(' ');
	                            ExtentType = strs[0];
	                            ExtentX = Convert.ToInt32(strs[1]);
                                ExtentY = ExtentX;
	                        }
	                        else if (valString.Contains("BOX"))
	                        {
	                            strs = valString.Split(' ');
	                            ExtentType = strs[0];
	                            ExtentX = Convert.ToInt32(strs[1]);
	                            ExtentY = Convert.ToInt32(strs[2]);
	                        }
	                        else if (valString.Contains("CENTER"))
	                        {
	                            ExtentType = valString;
	                        }
	                        break;
						case "Class":
	                        regex = regex2;

                            field.SetValue(this, ParseFlagField(regex, valString, typeof(ObjectClass)));
	                        break;
	                    case "Subclass":
                        	regex = regex2;

							foreach (Match match in regex.Matches(valString))
							    Subclass[(int) (SubclassBitIndex) Enum.Parse(typeof(SubclassBitIndex),  match.Groups["flag"].Value)] = true;
							break;
						case "Material":
	                        regex = regex2;

                            field.SetValue(this, ParseFlagField(regex, valString, typeof(MaterialFlag)));
							break;
						case "Use":
							val = valString;
							// Handle WandUse, WandCastUse
							strs = valString.Split(' ');
							if (strs[0] == "WandUse" || strs[0] == "WandCastUse")
							{
								WandCharges = 0;
								Byte.TryParse(strs[1], out WandCharges);
							}
							break;
						default:
							// Parse any other fields by their type
							if (field.FieldType == typeof(String))
								val = valString;
							else if (field.FieldType == typeof(Int32))
								val = Convert.ToInt32(valString, culture);
							else if (field.FieldType == typeof(UInt32))
								val = Convert.ToUInt32(valString, culture);
							else if (field.FieldType == typeof(Byte))
								val = Convert.ToByte(valString, culture);
							else if (field.FieldType == typeof(Single))
								val = Convert.ToSingle(valString, culture);
							break;
					}

					if (val != null) field.SetValue(this, val);
				}
			}
		}

		public static List<Tile> FloorTiles = new List<Tile>();
        public static List<Tile> EdgeTiles = new List<Tile>();
        public static List<Wall> Walls = new List<Wall>();
        public static SortedDictionary<string, Thing> Things = new SortedDictionary<string, Thing>();
        public static SortedDictionary<string, AudioMapping> AudioMappings = new SortedDictionary<string, AudioMapping>();
        public static SortedDictionary<string, AudioEvent> AudioEvents = new SortedDictionary<string, AudioEvent>();
        public static List<Spell> Spells = new List<Spell>();
        public static List<Ability> Abilities = new List<Ability>();
        public static SortedDictionary<string, Sprite> ImageMappings = new SortedDictionary<string, Sprite>();
        private static LightLog _Log = null;
        public static bool IsLoaded = false;

        public static List<string> FloorTileNames
		{
			get
			{
				List<string> list = new List<string>();
				foreach (Tile tile in FloorTiles)
					list.Add(tile.Name);
				return list;
			}
		}

        public static List<string> EdgeTileNames
		{
			get
			{
				List<string> list = new List<string>();
				foreach (Tile tile in EdgeTiles)
					list.Add(tile.Name);
				return list;
			}
		}

        public static List<string> WallNames
		{
			get
			{
				List<string> list = new List<string>();
				foreach (Wall wall in Walls)
					list.Add(wall.Name);
				return list;
			}
		}

        public static List<string> SpellNames
        {
            get
            {
                List<string> list = new List<string>();
                foreach (Spell sp in Spells)
                    list.Add(sp.SysName);
                return list;
            }
        }

		private ThingDb()
		{
		}

        private static void Cleanup()
        {
            FloorTiles.Clear();
            EdgeTiles.Clear();
            Walls.Clear();
            Things.Clear();
            AudioMappings.Clear();
            AudioEvents.Clear();
            Spells.Clear();
            Abilities.Clear();
            ImageMappings.Clear();
            IsLoaded = false;
        }

        public static void ReadWithLog(string filepath, LightLog log)
        {
            _Log = log;
            ReadFromFile(filepath);
        }

        public static void ReadFromFile(string filepath)
        {
            var fileStream = File.OpenRead(filepath);
            NoxBinaryReader rdr = new NoxBinaryReader(fileStream, CryptApi.NoxCryptFormat.THING);

            Cleanup();
            ThingToken token; int entryCount;
            while (true)
            {
                token = (ThingToken)rdr.ReadUInt32();
                if (token == 0) break; // End of Thing.bin

                // AVNT and THNG tokens don't have entrycount following
                if (token == ThingToken.AVNT || token == ThingToken.THNG) 
                    entryCount = 1;
                else
                    entryCount = rdr.ReadInt32();

                ReadTypedEntries(rdr, token, entryCount);
            }

            if (rdr.BaseStream.Length - rdr.BaseStream.Position >= 8)
            {
                if (_Log != null)
                    _Log.Critical("[ThingDb] Unable to parse the file completely.");
            }
            else
            {
                // Mark as successfully loaded
                IsLoaded = true;
            }
            // Close stream and free file handle
            rdr.Close();
            fileStream.Dispose();
        }

        protected static int ReadTypedEntries(BinaryReader rdr, ThingToken type, int numEntries)
        {
            int i;
            for (i = 0; i < numEntries; i++)
            {
                switch (type)
                {
                    case ThingToken.AUD:
                        var aud = new AudioMapping();
                        aud.Read(rdr);
                        AudioMappings.Add(aud.Name, aud);
                        break;

                    case ThingToken.SPEL:
                        var spel = new Spell();
                        spel.Read(rdr);
                        Spells.Add(spel);
                        break;

                    case ThingToken.ABIL:
                        var abil = new Ability();
                        abil.Read(rdr);
                        Abilities.Add(abil);
                        break;

                    case ThingToken.IMAG:
                        var imag = new Sprite();
                        imag.Read(rdr);
                        if (!ImageMappings.ContainsKey(imag.Name))
                            ImageMappings.Add(imag.Name, imag);
                        else
                        {
                            if (_Log != null)
                                _Log.Warn("[ThingDb] duplicate IMAG {0}", imag.Name);
                        }

                        break;

                    case ThingToken.AVNT:
                        var avnt = new AudioEvent();
                        avnt.Read(rdr);
                        AudioEvents.Add(avnt.Name, avnt);
                        break;

                    case ThingToken.WALL:
                        var wall = new Wall();
                        wall.Read(rdr);
                        wall.Id = Walls.Count;
                        Walls.Add(wall);
                        break;

                    case ThingToken.FLOR:
                        var flor = new Tile();
                        flor.Read(rdr, Tile.TileType.Floor);
                        flor.Id = FloorTiles.Count;
                        FloorTiles.Add(flor);
                        break;

                    case ThingToken.EDGE:
                        var edge = new Tile();
                        edge.Read(rdr, Tile.TileType.Edge);
                        edge.Id = EdgeTiles.Count;
                        EdgeTiles.Add(edge);
                        break;

                    case ThingToken.THNG:
                        var thng = new Thing();
                        thng.Read(rdr);
                        if (!Things.ContainsKey(thng.Name))//there are a few duplicates, but they seem to be identical
                            Things.Add(thng.Name, thng);
                        else
                        {
                            if (_Log != null)
                                _Log.Warn("[ThingDb] duplicate THNG {0}", thng.Name);
                        }
                        break;

                    default:
                        Debug.Fail(String.Format("Unknown ThingDb entry type: {0:x}", type));
                        break;
                }
            }

            // These tokens normally have an END closure
            if (type == ThingToken.EDGE || type == ThingToken.FLOR || type == ThingToken.WALL)
            {
                uint end = rdr.ReadUInt32();
                if (end != (uint)ThingToken.END)
                {
                    if (_Log != null)
                        _Log.Critical("[ThingDb] Failed to parse {0} entry (c {1})", type, numEntries);
                    return -1;
                }
            }

            return i;
        }
	}
}
