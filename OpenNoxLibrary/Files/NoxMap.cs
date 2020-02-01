using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using OpenNoxLibrary.Util;
using OpenNoxLibrary.Log;
using OpenNoxLibrary.Xfer;
using OpenNoxLibrary.Compression;
using OpenNoxLibrary.Encryption;

namespace OpenNoxLibrary.Files
{
	public class NoxMap
	{
        // ----PUBLIC MEMBERS----

        // WallMap
        public SortedDictionary<PointS32, Wall> Walls = null;
        // FloorMap
        public SortedDictionary<PointS32, Tile> Tiles = null;
        // ObjectTOC + ObjectData
        public List<NoxMap.Object> Objects = null;
        // PolygonData
        public List<Polygon> Polygons = null;
        // WayPoints
        public SortedDictionary<int, Waypoint> Waypoints = null;
        public List<WayPath> WayPaths = null;
        // ScriptObject
        public List<string> ScriptStrings = null;
        public List<Function> ScriptFunctions = null;
        // GroupData
        public SortedDictionary<string, Group> Groups = null;
        // DebugData
        public List<DebugEntry> DebugStrings = null;
        // AmbientData
        public Color24 GlobalAmbientColor = Color24.WHITE;
        // ScriptData
        public List<ScriptDataEntry> ScriptDataList = null;
		// MapIntro
        public string MapIntroText = null;
        // MapInfo
        public Section_MapInfo Info = null;
        
        public string FileName { get { return _FileName; } }

		// ----PROTECTED MEMBERS----
		protected MapHeader _Header = new MapHeader();

        /// <summary>
        /// Temporary buffer for writing/reading ObjectData
        /// </summary>
        protected SortedDictionary<ushort, string> _ObjectTOC = null;

        /// <summary>
        /// Temporary buffer used for writing/reading map file
        /// </summary>
        protected byte[] mapData;

        /// <summary>
        /// File name of this map, used for saving/reading
        /// </summary>
		protected string _FileName = "";
		
        /// <summary>
        /// Active Logger instance, can be null
        /// </summary>
        protected static LightLog _Log = null;

		// ----CONSTRUCTORS----
		public NoxMap() { }

        public NoxMap(LightLog log) { _Log = log; }

        public void ReadFile(string file)
        {
            _FileName = file;
            bool encrypted = true;

            using (NoxBinaryReader rdr = new NoxBinaryReader(File.OpenRead(file), CryptApi.NoxCryptFormat.NONE))
            {
                // All map files are encrypted by default, but let's keep that clear
                if (rdr.ReadUInt32() == 0xFADEFACE)
                {
                    encrypted = false;

                    rdr.BaseStream.Seek(0, SeekOrigin.Begin); // Reset to start
                    ReadStream(rdr);
                }
            }

            if (encrypted) // Doesn't start with FADEFACE
            {
                using (NoxBinaryReader rdr = new NoxBinaryReader(File.OpenRead(file), CryptApi.NoxCryptFormat.MAP))
                {
                    ReadStream(rdr);
                }
            }
        }

		#region Inner Classes and Enumerations

        public abstract class Section
		{
            /// <summary>
            /// Returns derived class name, if not overidden by the child class
            /// </summary>
			protected virtual string SectionName { get { return GetType().Name; } }
			
            /// <summary>
            /// Sections' data length, including the _Version value
            /// </summary>
            protected long _EntryDataLen;

            /// <summary>
            /// Sections' data structure can change depending on this value
            /// </summary>
            protected ushort _Version;

            /// <summary>
            /// Reference to the Map that contains/will contain this section's data
            /// </summary>
            protected NoxMap _Map;

            /// <summary>
            /// The version that will be used when writing the section
            /// </summary>
            protected virtual ushort GetLatestVersion() { return 1; }

            /// <summary>
            /// Sections' data structure can change depending on this value
            /// </summary>
            public int CodeVersion { get { return _Version; } }

            public Section(NoxMap map) { _Map = map; }

            /// <summary>
            /// Reads the section contents from the stream.
            /// </summary>
            internal void Read(BinaryReader rdr)
			{
				_EntryDataLen = rdr.ReadInt64();
                long endPos = _EntryDataLen + rdr.BaseStream.Position;

                _Version = rdr.ReadUInt16();
				ReadContents(rdr);

				long compensate = (endPos - rdr.BaseStream.Position);
				rdr.BaseStream.Seek(compensate, SeekOrigin.Current);
				if (compensate > 0 && _Log != null)
                    _Log.Warn("[Map.Section.Read] Section {0} is probably corrupted, compensating for {1} bytes.", SectionName, compensate);
			}

            /// <summary>
            /// This method needs to be implemented in the child class.
            /// </summary>
            protected abstract void ReadContents(BinaryReader rdr);

            /// <summary>
            /// Writes the section contents to the stream.
            /// </summary>
            internal void Write(BinaryWriter wtr)
			{
				wtr.Write(SectionName + "\0");
				wtr.BaseStream.Seek((8 - wtr.BaseStream.Position % 8) % 8, SeekOrigin.Current);//SkipToNextBoundary
				wtr.Write((long)0); // Dummy length value
				long startPos = wtr.BaseStream.Position;

                _Version = GetLatestVersion();
                wtr.Write(_Version);
				WriteContents(wtr);

                // Calculate and rewrite the section length
				_EntryDataLen = wtr.BaseStream.Position - startPos;
				wtr.Seek((int)startPos - 8, SeekOrigin.Begin);
                wtr.Write((long)_EntryDataLen);
                wtr.Seek((int)_EntryDataLen, SeekOrigin.Current);
			}

            protected abstract void WriteContents(BinaryWriter wtr);
		}

        protected class Section_WallMap : Section
		{
			public int Var1;
			public int Var2;
			public int Var3;
			public int Var4;

            public Section_WallMap(NoxMap map) : base(map) { }

            protected override void ReadContents(BinaryReader rdr)
			{
                _Map.Walls = new SortedDictionary<PointS32, Wall>();

				Var1 = rdr.ReadInt32();
				Var2 = rdr.ReadInt32();
				Var3 = rdr.ReadInt32();
				Var4 = rdr.ReadInt32();

				byte x, y; 
				while ((x = rdr.ReadByte()) != 0xFF && (y = rdr.ReadByte()) != 0xFF)//we'll get an 0xFF for x to signal end of section
				{
					rdr.BaseStream.Seek(-2, SeekOrigin.Current);
					Wall wall = new Wall(rdr);

					if (!_Map.Walls.ContainsKey(wall.Location))
					    _Map.Walls.Add(wall.Location, wall);
				}
			}

			protected override void WriteContents(BinaryWriter wtr)
			{
				wtr.Write((int)Var1);
				wtr.Write((int)Var2);
				wtr.Write((int)Var3);
				wtr.Write((int)Var4);

				foreach (Wall wall in _Map.Walls.Values)
					wall.Write(wtr);

				wtr.Write((byte)0xFF);//wallmap terminates with this byte
			}
		}

        protected class Section_DestructableWalls : Section
        {
            public Section_DestructableWalls(NoxMap map) : base(map) { }

            protected override void ReadContents(BinaryReader rdr)
            {
                ushort num = rdr.ReadUInt16();

                while ((num--) > 0)
                {
                    int x = rdr.ReadInt32();
                    int y = rdr.ReadInt32();
                    Wall wall = _Map.Walls[new PointS32(x, y)];
                    wall.Destructable = true;
                }
            }

            protected override void WriteContents(BinaryWriter wtr)
            {
                int start = (int)wtr.BaseStream.Position;

                ushort count = 0;
                wtr.Write(count);
                foreach (Wall wall in _Map.Walls.Values)
                {
                    if (!wall.Destructable) continue;

                    wtr.Write((uint)wall.Location.X);
                    wtr.Write((uint)wall.Location.Y);
                    count++;
                }

                // Write count
                int end = (int)wtr.BaseStream.Position;
                wtr.Seek(start, SeekOrigin.Begin);
                wtr.Write(count);
                wtr.Seek(end, SeekOrigin.Begin);
            }
        }

        protected class Section_WindowWalls : Section
        {
            public Section_WindowWalls(NoxMap map) : base(map) { }

            protected override void ReadContents(BinaryReader rdr)
            {
                ushort num = rdr.ReadUInt16();

                while ((num--) > 0)
                {
                    int x = rdr.ReadInt32();
                    int y = rdr.ReadInt32();
                    Wall wall = _Map.Walls[new PointS32(x, y)];
                    wall.Window = true;
                }
            }

            protected override void WriteContents(BinaryWriter wtr)
            {
                int start = (int)wtr.BaseStream.Position;

                ushort count = 0;
                wtr.Write(count);
                foreach (Wall wall in _Map.Walls.Values)
                {
                    if (!wall.Window) continue;

                    wtr.Write((uint)wall.Location.X);
                    wtr.Write((uint)wall.Location.Y);
                    count++;
                }

                // Write count
                int end = (int)wtr.BaseStream.Position;
                wtr.Seek(start, SeekOrigin.Begin);
                wtr.Write(count);
                wtr.Seek(end, SeekOrigin.Begin);
            }
        }

        protected class Section_SecretWalls : Section
        {
            public Section_SecretWalls(NoxMap map) : base(map) { }

            protected override void ReadContents(BinaryReader rdr)
            {
                ushort num = rdr.ReadUInt16();

                while ((num--) > 0)
                {
                    int x = rdr.ReadInt32();
                    int y = rdr.ReadInt32();
                    Wall wall = _Map.Walls[new PointS32(x, y)];
                    // PE 0x4297C0
                    wall.Secret_OpenWaitSeconds = rdr.ReadInt32();
                    wall.Secret_ScanFlags = rdr.ReadByte();
                    wall.Secret_WallState = rdr.ReadByte();
                    wall.Secret_OpenDelayFrames = rdr.ReadByte();
                    wall.Secret_LastOpenTime = rdr.ReadInt32();
                    wall.Secret_r2 = rdr.ReadUInt32();
                    // BugFix: Wall will not be treated as secret if scanflags == 0
                    // This is wrong since it is in secretwalls section anyway
                    if (wall.Secret_ScanFlags == 0) { wall.Secret_ScanFlags = 1; }
                }
            }

            protected override void WriteContents(BinaryWriter wtr)
            {
                int start = (int)wtr.BaseStream.Position;

                ushort count = 0;
                wtr.Write(count);
                foreach (Wall wall in _Map.Walls.Values)
                {
                    if (!wall.Secret) continue;
                    
                    wtr.Write((uint)wall.Location.X);
                    wtr.Write((uint)wall.Location.Y);
                    wtr.Write((uint)wall.Secret_OpenWaitSeconds);
                    wtr.Write((byte)wall.Secret_ScanFlags);
                    wtr.Write((byte)wall.Secret_WallState);
                    wtr.Write((byte)wall.Secret_OpenDelayFrames);
                    wtr.Write((uint)wall.Secret_LastOpenTime);
                    wtr.Write((uint)wall.Secret_r2);
                    count++;
                }

                // Write count
                int end = (int)wtr.BaseStream.Position;
                wtr.Seek(start, SeekOrigin.Begin);
                wtr.Write(count);
                wtr.Seek(end, SeekOrigin.Begin);
            }
        }

        protected class Section_AmbientData : Section
		{
            public Section_AmbientData(NoxMap map) : base(map) { }

			protected override void ReadContents(BinaryReader rdr)
			{
                Color24 AmbColor = Color24.WHITE;
				AmbColor.R = (byte)rdr.ReadInt32();
                AmbColor.G = (byte)rdr.ReadInt32();
                AmbColor.B = (byte)rdr.ReadInt32();
                _Map.GlobalAmbientColor = AmbColor;
			}

			protected override void WriteContents(BinaryWriter wtr)
			{
                Color24 AmbColor = _Map.GlobalAmbientColor;
				wtr.Write((int)AmbColor.R);
                wtr.Write((int)AmbColor.G);
                wtr.Write((int)AmbColor.B);
			}
		}

        /// <summary>
        /// This section is only used in saved-game maps. It stores data related to the script timers.
        /// </summary>
        protected class Section_ScriptData : Section
		{
            public Section_ScriptData(NoxMap map) : base(map) { }

            protected override void ReadContents(BinaryReader rdr)
			{
                // TODO
				//Count = rdr.ReadByte();

				// Count must be zero normally, else this is a saved map
				rdr.ReadBytes((int)(_EntryDataLen - 2));
			}

			protected override void WriteContents(BinaryWriter wtr)
			{
				//wtr.Write((byte)Count);
				//wtr.Write((byte)entries.Count);
				/*foreach (ScriptDataEntry sde in entries)
					sde.Write(wtr);*/
				//wtr.Write(data);
			}
		}

        public class ScriptDataEntry
        {
            Int64 Unknown1;
            Int64 Unknown2;
            Int16 Unknown3; //Might be a count
            Int64 Unknown4;

            public ScriptDataEntry()
            {
            }

            public ScriptDataEntry(BinaryReader rdr)
            {
                Read(rdr);
            }

            public void Read(BinaryReader rdr)
            {
                Unknown1 = rdr.ReadInt64();
                Unknown2 = rdr.ReadInt64();
                Unknown3 = rdr.ReadInt16();
                Unknown4 = rdr.ReadInt64();
            }

            public void Write(BinaryWriter wtr)
            {
                wtr.Write(Unknown1);
                wtr.Write(Unknown2);
                wtr.Write(Unknown3);
                wtr.Write(Unknown4);
            }
        }

        protected class Section_MapIntro : Section
		{
            public Section_MapIntro(NoxMap map) : base(map) { }

			protected override void ReadContents(BinaryReader rdr)
			{
                string text = rdr.ReadUnprefixedString(rdr.ReadInt32());
                if (text.Length > 0 && _Log != null)
                    _Log.Info("[Map.MapIntro.ReadContents] MapIntro section text: {0}", text);

                _Map.MapIntroText = text;
            }

			protected override void WriteContents(BinaryWriter wtr)
			{
                wtr.Write((int)_Map.MapIntroText.Length);
                wtr.Write(Encoding.UTF8.GetBytes(_Map.MapIntroText)); // FIXME: not really sure about an encoding
            }
		}

        public class Function
        {
            public string Name;
            public bool Returns;
            public int ArgsCount;
            public byte[] ByteCode;
            public int[] Variables; // aka SYMBols

            public Function()
            {
                Name = "[EMPTY]";
                Returns = false;
                ArgsCount = 0;
                ByteCode = null;
                Variables = null;
            }
        }

        protected class Section_ScriptObject : Section
        {
            public Section_ScriptObject(NoxMap map) : base(map) { }

            private bool VerifyToken(BinaryReader rdr, string token)
            {
                return rdr.ReadUnprefixedString(token.Length) == token;
            }

			protected override void ReadContents(BinaryReader rdr)
			{
                // Nox will load this section in strict order: STRG [], CODE, [FUNC [SYMB, DATA]], DONE
                // tokens are pretty much unneeded here, then
                if (!VerifyToken(rdr, "SCRIPT03STRG"))
                    return;

                int stringCount = rdr.ReadInt32();
                var Strings = new List<string>(stringCount);
                for (int i = 0; i < stringCount; i++)
                    Strings.Add(Encoding.UTF8.GetString(rdr.ReadBytes(rdr.ReadInt32())));

                if (!VerifyToken(rdr, "CODE"))
                    return;

                int funcCount = rdr.ReadInt32();
                var Functions = new List<Function>(funcCount);
                for (int i = 0; i < funcCount; i++)
                {
                    var func = new Function();

                    func.Name = Encoding.UTF8.GetString(rdr.ReadBytes(rdr.ReadInt32()));
                    func.Returns = rdr.ReadInt32() > 0;
                    func.ArgsCount = rdr.ReadInt32();

                    rdr.ReadInt32(); // SYMB
                    int varsc = rdr.ReadInt32();
                    rdr.ReadInt32(); // Skipped by the Nox loader

                    func.Variables = new int[varsc];
                    for (int var = 0; var < varsc; i++)
                        func.Variables[var] = rdr.ReadInt32();

                    rdr.ReadInt32(); // DATA
                    func.ByteCode = rdr.ReadBytes(rdr.ReadInt32());
                }

                if (!VerifyToken(rdr, "DONE"))
                    return;

                _Map.ScriptStrings = Strings;
                _Map.ScriptFunctions = Functions;
			}

            protected override void WriteContents(BinaryWriter wtr)
			{
                wtr.Write("SCRIPT03STRG".ToCharArray()); 
                wtr.Write(_Map.ScriptStrings.Count); // write number of strings
                foreach (string s in _Map.ScriptStrings) // write each string
                {
                    byte[] tmp = Encoding.UTF8.GetBytes(s);
                    wtr.Write(tmp.Length);
                    wtr.Write(tmp);
                }
                wtr.Write("CODE".ToCharArray());
                wtr.Write(_Map.ScriptFunctions.Count);
                foreach (Function sf in _Map.ScriptFunctions)
                {
                    wtr.Write("FUNC".ToCharArray());
                    byte[] tmp = Encoding.UTF8.GetBytes(sf.Name);
                    wtr.Write(tmp.Length);
                    wtr.Write(tmp);
                    wtr.Write(sf.Returns ? 1 : 0);
                    wtr.Write(sf.ArgsCount);
                    wtr.Write("SYMB".ToCharArray());
                    if (sf.Variables == null) // Fool protection
                        wtr.Write((long)0);
                    else
                    {
                        wtr.Write(sf.Variables.Length);
                        wtr.Write((int)0); // Unused (var/count actually is an uint32)
                        foreach (int var in sf.Variables)
                            wtr.Write(var);
                    }
                    wtr.Write("DATA".ToCharArray());
                    if (sf.ByteCode == null)
                        wtr.Write((int)0);
                    else
                    {
                        wtr.Write(sf.ByteCode.Length);
                        wtr.Write(sf.ByteCode);
                    }
                }
                wtr.Write("DONE".ToCharArray());
			}
        }

        public struct DebugEntry
        {
            // These strings are marked in game code to have an UTF-16LE encoding
            public string A;
            public string B;

            public DebugEntry(string A, string B)
            {
                this.A = A;
                this.B = B;
            }

            public static DebugEntry Read(BinaryReader rdr)
            {
                string a = Encoding.Unicode.GetString(rdr.ReadBytes(rdr.ReadInt32()));
                string b = Encoding.Unicode.GetString(rdr.ReadBytes(rdr.ReadInt32()));
                return new DebugEntry(a, b);
            }

            public void Write(BinaryWriter wtr)
            {
                byte[] tmp = Encoding.Unicode.GetBytes(A);
                wtr.Write(tmp.Length);
                wtr.Write(tmp);
                tmp = Encoding.Unicode.GetBytes(B);
                wtr.Write(tmp.Length);
                wtr.Write(tmp);
            }
        }

        protected class Section_DebugData : Section
		{
            public Section_DebugData(NoxMap map) : base(map) { }

			protected override void ReadContents(BinaryReader rdr)
			{
				int count = rdr.ReadInt32();

                for (int i = 0; i < count; i++)
                    _Map.DebugStrings.Add(DebugEntry.Read(rdr));
			}

            protected override void WriteContents(BinaryWriter wtr)
			{
                wtr.Write(_Map.DebugStrings.Count);

                foreach (DebugEntry e in _Map.DebugStrings)
                    e.Write(wtr);
			}
		}

        public class Group : ArrayList
        {
            public GroupTypes GType;
            public string Name;
            public int Id;

            public Group(string name, GroupTypes t, int id) : base()
            {
                GType = t;
                Name = name;
                Id = id;
            }

            public enum GroupTypes : byte
            {
                objects = 0,
                waypoint = 1, // This could be used for other stuff; only saw in Con01A
                walls = 2
            };

			public override string ToString()
			{
				return String.Format("{0}: {1}", Name, Enum.GetName(typeof(GroupTypes), GType));
			}
        }

        protected class Section_GroupData : Section
		{
            public Section_GroupData(NoxMap map) : base(map) { }

            protected override ushort GetLatestVersion()
            {
                return 3;
            }

            protected override void ReadContents(BinaryReader rdr)
			{
                _Map.Groups = new SortedDictionary<string, Group>();
				int count = rdr.ReadInt32();
				
                int size = 0;
                for (int i = 0; i < count; i++)
                {
                    Group grp = new Group(rdr.ReadString(), (Group.GroupTypes)rdr.ReadByte(), rdr.ReadInt32());
                    size = rdr.ReadInt32();
                    for (int k = 0; k < size; k++)
                    {
                        switch (grp.GType)
                        {
                            case Group.GroupTypes.walls:
                                grp.Add(new PointS32(rdr.ReadInt32(), rdr.ReadInt32()));
                                break;
                            case Group.GroupTypes.waypoint:
                            case Group.GroupTypes.objects:
                                grp.Add(rdr.ReadInt32());
                                break;
                            default:
                                if (_Log != null)
                                    _Log.Warn("[Map.GroupData.ReadContents] Unknown group type 0x" + grp.GType.ToString("x"));
                                break;
                        }
                    }
                    if (!_Map.Groups.ContainsKey(grp.Name)) _Map.Groups.Add(grp.Name, grp);
                }
			}

            protected override void WriteContents(BinaryWriter wtr)
            {
				int index = _Map.Groups.Count;
				wtr.Write(index);
				foreach (Group grp in _Map.Groups.Values)
                {
                    wtr.Write(grp.Name);
                    wtr.Write((byte)grp.GType);
                    wtr.Write(index--);
                    wtr.Write(grp.Count);
                    
                    foreach (System.Object obj in grp)
                    {
                        if (obj.GetType() == typeof(Int32))
                            wtr.Write((Int32)obj);
                        else if (obj.GetType() == typeof(PointS32))
                        {
                            wtr.Write(((PointS32)obj).X);
                            wtr.Write(((PointS32)obj).Y);
                        }
                        else if (_Log != null)
                            _Log.Warn("[Map.GroupData.WriteContents] Unknown type 0x" + grp.GType.ToString("x"));
                    }
                }
			}
		}

		public class Section_PolygonList : Section
		{
            internal PointF32[] AllPoints = null; // HACK temporary buffer for loading the polygons

            public Section_PolygonList(NoxMap map) : base(map) { }

            protected override ushort GetLatestVersion()
            {
                return 4; // enforce using latest version if we want to save quest maps
            }

			protected override void ReadContents(BinaryReader rdr)
			{
                _Map.Polygons = new List<Polygon>();
				int numPoints = rdr.ReadInt32();
                // In order to save some space on the disk and remove duplicates, Nox saves polygon points separately and refers to them by indexes
                AllPoints = new PointF32[numPoints];
				while (numPoints-- > 0)
				{
                    AllPoints[rdr.ReadInt32() - 1] = new PointF32(rdr.ReadSingle(), rdr.ReadSingle());
				}

				int numPolygons = rdr.ReadInt32();
                while (numPolygons-- > 0)
                {
                    var poly = new Polygon();
                    poly.Read(rdr, this);
                    _Map.Polygons.Add(poly);
                }
                // Free memory
                AllPoints = null; 
			}

			protected override void WriteContents(BinaryWriter wtr)
			{
                List<PointF32> parr = new List<PointF32>();
                // Form the points array, while removing duplicates
				foreach (Polygon poly in _Map.Polygons)
					foreach (PointF32 pt in poly.Points)
						if (!parr.Contains(pt))
							parr.Add(pt);

                // Write points array
				wtr.Write((int) parr.Count);
				foreach (PointF32 pt in parr)
				{
					wtr.Write((int) (parr.IndexOf(pt)+1));
					wtr.Write((float) pt.X);
					wtr.Write((float) pt.Y);
				}

                // Fill the point buffer and write polygons
                AllPoints = parr.ToArray();
                wtr.Write((int)_Map.Polygons.Count);
                foreach (Polygon poly in _Map.Polygons)
					poly.Write(wtr, this);

                // Free memory
                AllPoints = null; 
			}
		}

		public class Polygon
		{
			public string Name;
            public string EnterFuncPlayer; // script
            public string EnterFuncMonster; // script
            public uint SecretFlags; // 1 signifies secret area in Quest maps
            public Color24 AmbientLightColor;//the area's ambient light color
			public byte MinimapGroup;//the visible wall group when in this area
			public List<PointF32> Points = new List<PointF32>();//the unindexed points that define the polygon

            public Polygon() { }

            public Polygon(string name, Color24 ambient, byte mmGroup, List<PointF32> points, string enterfunc, string unknownfunc, uint secflags)
			{
				Name = name;
				AmbientLightColor = ambient;
				MinimapGroup = mmGroup;
				Points = points;
                EnterFuncPlayer = enterfunc;
                EnterFuncMonster = unknownfunc;
                SecretFlags = secflags;
			}
			
			public bool IsPointInside(PointF32 p)
		    {
		        PointF32 p1, p2;
		
		        bool inside = false;
		
		        if (Points.Count < 3)
		        {
		            return inside;
		        }
		
		        var oldPoint = new PointF32(
		            Points[Points.Count - 1].X, Points[Points.Count - 1].Y);
		
		        for (int i = 0; i < Points.Count; i++)
		        {
		            var newPoint = new PointF32(Points[i].X, Points[i].Y);
		
		            if (newPoint.X > oldPoint.X)
		            {
		                p1 = oldPoint;
		                p2 = newPoint;
		            }
		            else
		            {
		                p1 = newPoint;
		                p2 = oldPoint;
		            }
		
		            if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
		                && (p.Y - (long) p1.Y)*(p2.X - p1.X)
		                < (p2.Y - (long) p1.Y)*(p.X - p1.X))
		            {
		                inside = !inside;
		            }
		
		            oldPoint = newPoint;
		        }
		
		        return inside;
		    }

			internal void Read(BinaryReader rdr, Section_PolygonList list)
			{
				Name = rdr.ReadString();
				AmbientLightColor = new Color24(rdr.ReadByte(), rdr.ReadByte(), rdr.ReadByte());
				MinimapGroup = rdr.ReadByte();

				// Points forming a polygon
				short ptCount = rdr.ReadInt16();
				while (ptCount-- > 0)
                    Points.Add(list.AllPoints[rdr.ReadInt32() - 1]);

				if (list.CodeVersion >= 2)
				{
					// Typical script handler entries
					short s1 = rdr.ReadInt16();
					if (s1 <= 1)
					{
						EnterFuncPlayer = Encoding.ASCII.GetString(rdr.ReadBytes(rdr.ReadInt32()));
						rdr.ReadInt32();
					}
					short s2 = rdr.ReadInt16();
					if (s2 <= 1)
					{
						EnterFuncMonster = Encoding.ASCII.GetString(rdr.ReadBytes(rdr.ReadInt32()));
						rdr.ReadInt32();
					}
				}
				if (list.CodeVersion >= 4)
				{
					SecretFlags = rdr.ReadUInt32();
				}
			}

            internal void Write(BinaryWriter wtr, Section_PolygonList list)
			{
				wtr.Write(Name);
				wtr.Write((byte) AmbientLightColor.R);
				wtr.Write((byte) AmbientLightColor.G);
				wtr.Write((byte) AmbientLightColor.B);
				wtr.Write((byte) MinimapGroup);
				wtr.Write((short) Points.Count);

                foreach (PointF32 pt in Points)
                {
                    int index = Array.IndexOf<PointF32>(list.AllPoints, pt) + 1;
                    wtr.Write(index);
                }

				if (list.CodeVersion >= 2)
				{
					wtr.Write((short) 1); // VERIFY: maybe it's kind of an ENABLED marker?
					wtr.Write(EnterFuncPlayer.Length);
					wtr.Write(Encoding.ASCII.GetBytes(EnterFuncPlayer));
					wtr.Write((int) 0);
					wtr.Write((short) 1);
					wtr.Write(EnterFuncMonster.Length);
					wtr.Write(Encoding.ASCII.GetBytes(EnterFuncMonster));
					wtr.Write((int) 0);
				}

				if (list.CodeVersion >= 4)
				{
					//if (IsQuestSecret) flags |= 1;
					wtr.Write(SecretFlags);
				}
			}
			
			public override string ToString()
			{
				return String.Format("{0} {1} {2}, {3}", Name, MinimapGroup, Points[0].X, Points[1].Y);
			}
		}

        public class Section_WayPoints : Section
        {
            public Section_WayPoints(NoxMap map) : base(map) { }

            protected override ushort GetLatestVersion()
            {
                return 3;
            }

            protected override void ReadContents(BinaryReader rdr)
            {
                _Map.Waypoints = new SortedDictionary<int, Waypoint>();
                _Map.WayPaths = new List<WayPath>();

                int numWaypoints = rdr.ReadInt32();
                while (numWaypoints-- > 0)
                {
                    Waypoint wp = new Waypoint(rdr, _Map);
                    if (!_Map.Waypoints.ContainsKey(wp.Id))
                        _Map.Waypoints.Add(wp.Id, wp);
                    else if (_Log != null)
                        _Log.Info("[Map.WaypointList.Read] Skipped a duplicate waypoint: {0}", wp.Id);
                }

                _Map.VerifyPaths();
            }

            protected override void WriteContents(BinaryWriter wtr)
            {
                wtr.Write((int)_Map.Waypoints.Count);

                foreach (Waypoint wp in _Map.Waypoints.Values)
                    wp.Write(wtr, _Map);
            }
        }

        /// <summary>
        /// Verifies all waypoint connections to be referencing valid waypoints
        /// </summary>
        public int VerifyPaths()
        {
            var pathsClone = new List<WayPath>(WayPaths);
            int count = 0;
            foreach (Waypoint wp in Waypoints.Values)
            {
                foreach (WayPath p in pathsClone)
                {
                    if (!Waypoints.ContainsKey(p.WayPointA) || !Waypoints.ContainsKey(p.WayPointB))
                    {
                        WayPaths.Remove(p);
                        count++;
                        if (_Log != null)
                            _Log.Info("[Map.WaypointList.VerifyPaths] Removing a wrong WayPath: {0}", p);
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Deletes all paths that specified waypoint is a part of.
        /// </summary>
        /// <returns>Number of paths deleted.</returns>
        public int RemoveAllPathsFor(int wpId)
        {
            var pathsClone = new List<WayPath>(WayPaths);
            int count = 0;
            foreach (WayPath p in pathsClone)
            {
                if (p.WayPointA == wpId || p.WayPointB == wpId)
                {
                    WayPaths.Remove(p); // Remove from the original
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Returns all paths that specified waypoint is a part of.
        /// </summary>
        public WayPath[] GetAllPathsFor(int wpId)
        {
            List<WayPath> result = new List<WayPath>();
            foreach (WayPath p in WayPaths)
            {
                if (p.WayPointA == wpId || p.WayPointB == wpId)
                    result.Add(p);

            }
            return result.ToArray();
        }

        public bool AddPath(int wpA, int wpB, byte flag = WayPath.DefaultFlag)
        {
            if (CheckPathExists(wpA, wpB)) return false;

            WayPath wp = new WayPath(wpA, wpB, flag);
            WayPaths.Add(wp);
            return true;
        }

        /// <summary>
        /// Returns a path from specified waypoint A to waypoint B.
        /// </summary>
        public WayPath GetPath(int wpA, int wpB)
        {
            foreach (WayPath p in WayPaths)
            {
                if (p.WayPointA == wpA && p.WayPointB == wpB)
                    return p;

            }
            return null;
        }

        /// <summary>
        /// Checks whenether the specified path from Waypoint A to Waypoint B (A->B) is present (and not vice-versa).
        /// </summary>
        public bool CheckPathExists(int wpA, int wpB)
        {
            return GetPath(wpA, wpB) != null;
        }

        public class WayPath
        {
            public int WayPointA;
            public int WayPointB;
            public byte Flags;
            public const byte DefaultFlag = 0x80;

            public WayPath(int a, int b, byte flg = DefaultFlag)
            {
                WayPointA = a;
                WayPointB = b;
                Flags = flg;
            }

            public Waypoint GetPointA(NoxMap map)
            {
                if (!map.Waypoints.ContainsKey(WayPointA))
                    return null;

                return map.Waypoints[WayPointA];
            }

            public Waypoint GetPointB(NoxMap map)
            {
                if (!map.Waypoints.ContainsKey(WayPointB))
                    return null;

                return map.Waypoints[WayPointB];
            }

            public override string ToString()
            {
                return String.Format("{0} -> {1} (f:{2})", WayPointA, WayPointB, Flags);
            }
        }

        public class Waypoint
        {
            public string Name;
            public PointF32 Point;
            public uint Flags; // 1=enabled
            public int Id;

            public Waypoint(string HumanName, PointF32 point, int Id)
            {
                this.Name = HumanName;
                this.Point = point;
                this.Id = Id;
            }

            internal Waypoint(BinaryReader rdr, NoxMap map)
            {
                Read(rdr, map);
            }

            private string TrimName(string fullName)
            {
                string result = fullName;
                int ptIndex = result.LastIndexOf(':');
                // Trim the map name
                if (ptIndex >= 0) result = result.Substring(ptIndex + 1);

                return result;
            }

            internal void Read(BinaryReader rdr, NoxMap map)
            {
                Id = rdr.ReadInt32(); 
                Point = new PointF32(rdr.ReadSingle(), rdr.ReadSingle());
                Name = TrimName(rdr.ReadString());
                Flags = rdr.ReadUInt32();

                int connections = rdr.ReadByte();
                for (int i = rdr.ReadByte(); i > 0; i--)
                {
                    map.WayPaths.Add(new WayPath(Id, rdr.ReadInt32(), rdr.ReadByte()));
                }
            }

            internal void Write(BinaryWriter wtr, NoxMap map)
            {
                wtr.Write(Id);
                wtr.Write(Point.X);
                wtr.Write(Point.Y);
                wtr.Write(Name);
                wtr.Write(Flags);

                // Write connections/paths
                var conns = map.GetAllPathsFor(Id);
                wtr.Write((byte)(conns.Length));
                foreach (WayPath p in conns)
                {
                    wtr.Write(p.WayPointB);
                    wtr.Write(p.Flags);
                }
            }

            public override string ToString()
            {
                return String.Format("{2}: {3} {0}, {1}", Point.X, Point.Y, Id, Name);
            }
        }

        public class Tile
		{
			public PointS32 Location;
			public byte TypeId;
			public UInt16 Variation;
            public List<EdgeTile> EdgeTiles = new List<EdgeTile>();

			public ThingDb.Tile TileDef
			{
				get
				{
					return ((ThingDb.Tile) ThingDb.FloorTiles[TypeId]);
				}
			}

            public Color24 SubstColor // RENAMED from col
            {
                get
                {
                    return new Color24(TileDef.SubstColor);
                }
            }

            public Tile(PointS32 loc, byte tilemat, UInt16 variation, List<EdgeTile> edgetiles)
			{
                Location = loc;
				TypeId = tilemat; Variation = variation;
				EdgeTiles = edgetiles;
			}

            public Tile(PointS32 loc, byte tilemat, UInt16 variation) : this(loc, tilemat, variation, new List<EdgeTile>()) { }

            internal Tile(BinaryReader rdr)
            {
                Read(rdr);
            }

			internal void Read(BinaryReader rdr)
			{
				TypeId = rdr.ReadByte();
				Variation = rdr.ReadUInt16();
				rdr.ReadBytes(2);//these are always null for first tilePair of a blending group (?)
				for (int numEdgeTiles = rdr.ReadByte(); numEdgeTiles > 0; numEdgeTiles--)
					EdgeTiles.Add(new EdgeTile(rdr));
			}

			internal void Write(BinaryWriter wtr)
			{
				wtr.Write((byte )TypeId);
				wtr.Write((UInt16) Variation);
				wtr.Write(new byte[2]);//3 nulls
				wtr.Write((byte) EdgeTiles.Count);
				foreach (EdgeTile edge in EdgeTiles)
					edge.Write(wtr);
			}

			public class EdgeTile//maybe derive from tile?
			{
				public byte EdgeTileMat;
				public UInt16 EdgeTileVar;
				public Direction Dir;
				public byte TypeId;

				public enum Direction : byte
				{
					SW_Tip,//0x00
					West,
					West_02,
					West_03,
					NW_Tip,//0x04
					South,
					North,
					South_07,
					North_08,//0x08
					South_09,
					North_0A,
					SE_Tip,
					East,//0x0C
					East_D,
					East_E,
					NE_Tip,
					SW_Sides,//0x10
					NW_Sides,
					NE_Sides,
					SE_Sides//0x13
					// 16 + 4 corners? There are also duplicates
                    // Some edge types don't have images for the corresponding direction
				}

				public EdgeTile(byte tilemat, ushort variation, Direction dir, byte edge)
				{
					EdgeTileMat = tilemat; EdgeTileVar = variation; Dir = dir; TypeId = edge;
				}

                internal EdgeTile(BinaryReader rdr)
				{
					Read(rdr);
				}

				internal void Read(BinaryReader rdr)
				{
					EdgeTileMat = rdr.ReadByte();
					EdgeTileVar = rdr.ReadUInt16();
					TypeId = rdr.ReadByte();
					Dir = (Direction) rdr.ReadByte();

                    if (_Log != null && !Enum.IsDefined(typeof(Direction), (byte)Dir))
                        _Log.Warn("[Map.Tile.EdgeTile.Read] Edgetile direction {0} is undefined.", (byte)Dir);
				}

				internal void Write(BinaryWriter wtr)
				{
					wtr.Write((byte) EdgeTileMat);
					wtr.Write((UInt16) EdgeTileVar);
					wtr.Write((byte) TypeId);
					wtr.Write((byte) Dir);
				}
			}

			public override string ToString()
			{
				return String.Format("{0}, {1} {2}", Location.X, Location.Y, TileDef.Name);
			}
		}

		public class Section_FloorMap : Section
		{
			public int Var1 = 24;
			public int Var2 = 7;
			public int Var3 = 103;
			public int Var4 = 110;

            public Section_FloorMap(NoxMap map) : base(map) { }

            protected override ushort GetLatestVersion()
            {
                return 4;
            }

			protected override void ReadContents(BinaryReader rdr)
			{
                _Map.Tiles = new SortedDictionary<PointS32, Tile>();
				List<TilePair> tilePairs = new List<TilePair>();

				Var1 = rdr.ReadInt32();
				Var2 = rdr.ReadInt32();
				Var3 = rdr.ReadInt32();
				Var4 = rdr.ReadInt32();
				
				if (_Version <= 3)
				{
					throw new NotImplementedException("Unsupported subtile entry detected");
				}
				
				while (true)//we'll get an 0xFF for both x and y to signal end of section
				{
					byte y = rdr.ReadByte();
					byte x = rdr.ReadByte();

					if (y == 0xFF && x == 0xFF)
						break;

					rdr.BaseStream.Seek(-2, SeekOrigin.Current);//rewind back to beginning of current entry
					TilePair tilePair = new TilePair(rdr);
					if (!tilePairs.Contains(tilePair))//do not add duplicates (there should not be duplicate entries in a file anyway)
						tilePairs.Add(tilePair);
				}

				foreach (TilePair tp in tilePairs)
				{
					if (tp.Left != null) _Map.Tiles.Add(tp.Left.Location, tp.Left);
                    if (tp.Right != null) _Map.Tiles.Add(tp.Right.Location, tp.Right);
				}
			}

			protected override void WriteContents(BinaryWriter wtr)
			{
				wtr.Write((int)Var1);
				wtr.Write((int)Var2);
				wtr.Write((int)Var3);
				wtr.Write((int)Var4);

				ArrayList tilePairs = new ArrayList();

				SortedDictionary<PointS32, Tile> tiles = new SortedDictionary<PointS32, Tile>(new LocationComparer());
				foreach (PointS32 key in _Map.Tiles.Keys)
                    tiles.Add(key, _Map.Tiles[key]);

				List<Tile> tileList = new List<Tile>(tiles.Values);
				List<Tile>.Enumerator tEnum = tileList.GetEnumerator();
				while (tEnum.MoveNext())
				{
					Tile left = null, right = null;
					Tile tile1 = tEnum.Current;
					if (tile1.Location.X % 2 == 1)//we got a right tile. the right tile will always come before it's left tile
					{
						right = tile1;
						PointS32 t2p = new PointS32(tile1.Location.X - 1, tile1.Location.Y + 1);
						if (tiles.ContainsKey(t2p))
							left = tiles[t2p];
						tilePairs.Add(new TilePair((byte) ((right.Location.X-1)/2), (byte) ((right.Location.Y+1)/2), left, right));
					}
					else //assume that this tile is a single since the ordering would have forced the right tile to be handled first
					{
						left = tile1;
						tilePairs.Add(new TilePair((byte) (left.Location.X/2), (byte) (left.Location.Y/2), left, right));
					}
					if (left != null) tiles.Remove(left.Location);
					if (right != null) tiles.Remove(right.Location);
				}

				//... and write them
				foreach (TilePair tilePair in tilePairs)
					tilePair.Write(wtr);

				wtr.Write((ushort) 0xFFFF); // Terminating marker
			}
		}

		protected class MapHeader
		{
			private const int LENGTH = 0x18;
			private const uint FILE_ID = 0xFADEFACE;
            /// <summary>
            /// Checksum for map file. Used by the game to determine whenether downloading a new map is necessary.
            /// </summary>
			public uint CheckSum;
            // Don't really sure how they are used in-game
			public int MapOffsetX;
			public int MapOffsetY;

			public MapHeader() { }

            public void Read(BinaryReader rdr)
            {
				uint id = rdr.ReadUInt32(); // 0xFADEFACE or 0xFADEBEEF
                if (id != FILE_ID && _Log != null)
                    _Log.Warn("[Map.MapHeader.Read] Unknown header: {0:x}", id);

				int check;
				check = rdr.ReadInt32();
				if (check != 0 && _Log != null) _Log.Debug("[Map.MapHeader.Read] int in header was not null: 0x" + check.ToString("x"));
				CheckSum = rdr.ReadUInt32();
				check = rdr.ReadInt32();
                if (check != 0 && _Log != null) _Log.Debug("[Map.MapHeader.Read] int in header was not null: 0x" + check.ToString("x"));
				MapOffsetX = rdr.ReadInt32();
				MapOffsetY = rdr.ReadInt32();
			}

			public void Write(BinaryWriter wtr)
			{
				wtr.Write(FILE_ID);
				wtr.Write((int) 0);
				wtr.Write((int) CheckSum);
				wtr.Write((int) 0);
				wtr.Write((int) MapOffsetX);
				wtr.Write((int) MapOffsetY);
			}

            public void GenerateChecksum(byte[] data)
            {
                CheckSum = CRC32.Calculate(data);
                if (_Log != null)
                    _Log.Debug("[Map.MapHeader] CRC32: 0x{0:X}", CheckSum);
            }
		}

		public class Section_MapInfo : Section
		{
			public string Summary;//the map's brief name
			public string Description;//the map's long description
			public string Author;
			public string Email;
			public string Author2;
			public string Email2;
			public string Version;//the map's current version
			public string Copyright;
			public string Date;
			public MapTypeFlags Type = MapTypeFlags.DM_ELIGIBLE;
			public byte RecommendedMin;
			public byte RecommendedMax;
			public String QIntroTitle = "";
			public String QIntroGraphic = "";

			protected override string SectionName { get { return "MapInfo"; } }

            public Section_MapInfo(NoxMap map) : base(map) { }

			[Flags]
			public enum MapTypeFlags : uint
			{
                SOLO_EXCLUSIVE     = 0x01,
                QUEST_EXCLUSIVE    = 0x02,
                // If QUEST or SOLO flags are set, the map will be treated as eligible ONLY for the described game modes.
                DM_ELIGIBLE        = 0x04,
                UNUSED_8           = 0x08,
                KOTR_ELIGIBLE      = 0x10,
                CTF_ELIGIBLE       = 0x20,
                FLAGBALL_ELIGIBLE  = 0x40,
                UNUSED_80          = 0x80,
                UNUSED_100         = 0x100,
                UNUSED_200         = 0x200,
                ELIM_ELIGIBLE      = 0x400,
                SOCIAL_ELIGIBLE    = 0x80000000
			}

			const int PREFIX = 0x02;
			const int TITLE = 0x40;
			const int DESCRIPTION = 0x200;
			const int VERSION = 0x10;
			const int AUTHOR = 0x40;
			const int EMAIL = 0xC0;
			const int EMPTY = 0x80;
			const int COPYRIGHT = 0x80;//only on very few maps
			const int DATE = 0x20;
			const int TYPE = 0x04;
			const int MINMAX = 0x02;
			const int TOTAL = PREFIX + TITLE + DESCRIPTION + VERSION + 2*(AUTHOR + EMAIL) + EMPTY + COPYRIGHT + DATE + TYPE + MINMAX;

            protected override ushort GetLatestVersion()
            {
                if ((Type & MapTypeFlags.QUEST_EXCLUSIVE) > 0) 
                    return 3;
                else 
                    return 2;
            }

			protected override void ReadContents(BinaryReader rdr)
			{
                Summary = rdr.ReadUnprefixedString(TITLE);
                Description = rdr.ReadUnprefixedString(DESCRIPTION);
                Version = rdr.ReadUnprefixedString(VERSION);
                Author = rdr.ReadUnprefixedString(AUTHOR);
                Email = rdr.ReadUnprefixedString(EMAIL);
                Author2 = rdr.ReadUnprefixedString(AUTHOR);
                Email2 = rdr.ReadUnprefixedString(EMAIL);
				rdr.ReadBytes((int) EMPTY);
                Copyright = rdr.ReadUnprefixedString(COPYRIGHT);
                Date = rdr.ReadUnprefixedString(DATE);
				Type = (MapTypeFlags)rdr.ReadUInt32();

				if ((Type & MapTypeFlags.QUEST_EXCLUSIVE) > 0) // Quest maps have an extra variable length section
				{
					QIntroTitle = Encoding.ASCII.GetString(rdr.ReadBytes(rdr.ReadByte()));
					QIntroGraphic = Encoding.ASCII.GetString(rdr.ReadBytes(rdr.ReadByte()));
				}
				else
				{
					RecommendedMin = rdr.ReadByte();
					RecommendedMax = rdr.ReadByte();
				}
			}

			protected override void WriteContents(BinaryWriter wtr)
			{
				wtr.Write(Summary.ToCharArray());
				wtr.BaseStream.Seek((int) TITLE - Summary.Length, SeekOrigin.Current);

				wtr.Write(Description.ToCharArray());
				wtr.BaseStream.Seek((int) DESCRIPTION - Description.Length, SeekOrigin.Current);

				wtr.Write(Version.ToCharArray());
				wtr.BaseStream.Seek((int) VERSION - Version.Length, SeekOrigin.Current);

				wtr.Write(Author.ToCharArray());
				wtr.BaseStream.Seek((int) AUTHOR - Author.Length, SeekOrigin.Current);

				wtr.Write(Email.ToCharArray());
				wtr.BaseStream.Seek((int) EMAIL - Email.Length, SeekOrigin.Current);

				wtr.Write(Author2.ToCharArray());
				wtr.BaseStream.Seek((int) AUTHOR - Author2.Length, SeekOrigin.Current);

				wtr.Write(Email2.ToCharArray());
				wtr.BaseStream.Seek((int) EMAIL - Email2.Length, SeekOrigin.Current);

				wtr.BaseStream.Seek((int) EMPTY, SeekOrigin.Current);

				wtr.Write(Copyright.ToCharArray());
				wtr.BaseStream.Seek((int) COPYRIGHT - Copyright.Length, SeekOrigin.Current);

				wtr.Write(Date.ToCharArray());
				wtr.BaseStream.Seek((int) DATE - Date.Length, SeekOrigin.Current);

				wtr.Write((int) Type);

				if ((Type & MapTypeFlags.QUEST_EXCLUSIVE) > 0)
				{
					wtr.Write((byte) QIntroTitle.Length);
					wtr.Write(Encoding.ASCII.GetBytes(QIntroTitle));
					wtr.Write((byte) QIntroGraphic.Length);
					wtr.Write(Encoding.ASCII.GetBytes(QIntroGraphic));
				}
				else
				{
					wtr.Write((byte) RecommendedMin);
					wtr.Write((byte) RecommendedMax);
				}
			}
		}

        protected class LocationComparer : IComparer<PointS32>
		{
            public int Compare(PointS32 lhs, PointS32 rhs)
			{
				if (lhs.Y != rhs.Y)
					return lhs.Y - rhs.Y;
				else
					return lhs.X - rhs.X;
			}

            public bool Equals(PointS32 lhs, PointS32 rhs)
			{
				return lhs.Equals(rhs);
			}

            public int GetHashCode(PointS32 p)
			{
				return p.GetHashCode();
			}
		}

        protected class Int32Comparer : IComparer<int>
		{
            public int Compare(int lhs, int rhs)
			{
                return lhs - rhs;
			}

            public bool Equals(int lhs, int rhs)
			{
				return lhs.Equals(rhs);
			}

            public int GetHashCode(int p)
			{
				return p.GetHashCode();
			}
		}

		public struct TilePair : IComparable
		{
            public PointS32 Location;
			public bool OneTileOnly
			{
				get
				{
					return Left == null || Right == null;
				}
			}

			// Set one of these to null if you want a single-tile entry
			public Tile Left;
			public Tile Right;

			public TilePair(byte x, byte y, Tile left, Tile right)
			{
                Location = new PointS32(x, y);
				Left = left;
				Right = right;
			}

            public TilePair(BinaryReader rdr)
			{
				Left = null;
				Right = null;
                Location = PointS32.Empty;
				Read(rdr);
			}

            public void Read(BinaryReader rdr)
			{
				byte y = rdr.ReadByte(), x = rdr.ReadByte();
                Location = new PointS32((x & 0x7F), (y & 0x7F)); // Ignore sign bits for coordinates

				if ((x & y & 0x80) == 0) // First bit signifies whether only the left, right, or both tilePairs are listed in this entry
				{
					if ((y & 0x80) != 0) // If y-coordinate has sign bit set, then the left tilePair is specified
						Left = new Tile(rdr);
					else if ((x & 0x80) != 0) // Same with the x-coordinate
						Right = new Tile(rdr);
					else if (NoxMap._Log != null)
                        NoxMap._Log.Warn("[Map.TilePair.Read] Invalid x, y for tilepair entry");
				}
				else // Otherwise, read right then left
				{
					Right = new Tile(rdr);
					Left = new Tile(rdr);
				}

                if (Left != null) Left.Location = new PointS32(Location.X * 2, Location.Y * 2);
                if (Right != null) Right.Location = new PointS32(Location.X * 2 + 1, Location.Y * 2 - 1);
			}

			public void Write(BinaryWriter wtr)
			{
				byte x = (byte) (Location.X | 0x80), y = (byte) (Location.Y | 0x80);

				if (OneTileOnly)
				{
					if (Left == null)
						y &= 0x7F;
					else
						x &= 0x7F;
				}

				wtr.Write((byte) y);
				wtr.Write((byte) x);

				//write the right one first
				if (Right != null)
					Right.Write(wtr);
				if (Left != null)
					Left.Write(wtr);
			}

			public int CompareTo(object obj)
			{
				TilePair rhs = (TilePair) obj;
				if (Location.Y != rhs.Location.Y)
					return Location.Y - rhs.Location.Y;
				else
					return Location.X - rhs.Location.X;
			}

			public override bool Equals(object obj)
			{
				return obj is TilePair && CompareTo(obj) == 0;
			}

			public override int GetHashCode()
			{
				return Location.GetHashCode();
			}
		}

		public class Wall : IComparable
		{
			public enum WallFacing : byte
			{
				NORTH,//same as SOUTH
				WEST,//same as EAST
				CROSS,
				
				SOUTH_T,
				EAST_T,//4
				NORTH_T,
				WEST_T,

				SW_CORNER,
				NW_CORNER,//8
				NE_CORNER,
				SE_CORNER//10
			}

			public PointS32 Location;
			public WallFacing Facing;
			public byte matId;
			public byte Variation;
			public byte Minimap = 0x64;
			protected byte _ModifiedMB;
			public bool Destructable;
			public bool Secret
			{
				get
				{
					return Secret_ScanFlags != 0;
				}
			}
			public bool Window;

			/*** SECRET WALLS ONLY ***/
			public int Secret_OpenWaitSeconds = 3; // How long (in seconds) the wall will take to open/close
			public byte Secret_ScanFlags = 0; // 4 - Auto Open, 8 - Auto Close
			public byte Secret_WallState = 0; // 4 - Closed, 3 - Ready to close, 2 - Ready to open, 1 - Open
			public byte Secret_OpenDelayFrames = 0; // Used by the game in process of switching aforementioned states
			public int Secret_LastOpenTime = 0;
			public uint Secret_r2 = 0;
			
			[Flags]
			public enum SecretScanFlags : byte
			{
				Scripted = 1,
				AutoOpen = 2,
				AutoClose = 4,
				Unknown8 = 8 // Set by the game when player is nearby
			}

            public ThingDb.Wall WallDef
            {
                get
                {
                    return ThingDb.Walls[matId];
                }
            }

            internal Wall(BinaryReader rdr)
			{
				Read(rdr);
			}

			public Wall(PointS32 loc, WallFacing facing, byte mat)
			{
				Location = loc;	Facing = facing; matId = mat;
			}

			public Wall(PointS32 loc, WallFacing facing, byte mat, byte mmGroup, byte var)
			{
				Location = loc; Facing = facing; matId = mat; Minimap = mmGroup; Variation = var;
			}

			internal void Read(BinaryReader rdr)
			{
				Location = new PointS32(rdr.ReadByte(), rdr.ReadByte());
				Facing = (WallFacing) (rdr.ReadByte() & 0x7F);//I'm almost certain the sign bit is just garbage and does not signify anything about the wall
				matId = rdr.ReadByte();
				Variation = rdr.ReadByte();
				Minimap = rdr.ReadByte();
				_ModifiedMB = rdr.ReadByte(); // may be 1 in saved maps
			}

			internal void Write(BinaryWriter wtr)
			{
				wtr.Write((byte) Location.X);
				wtr.Write((byte) Location.Y);
				wtr.Write((byte) Facing);
				wtr.Write((byte) matId);
				wtr.Write((byte) Variation);
				wtr.Write((byte) Minimap);
				wtr.Write((byte) _ModifiedMB);
			}

			public int CompareTo(object obj)
			{
				Wall rhs = (Wall) obj;
				if (Location.Y != rhs.Location.Y)
					return Location.Y - rhs.Location.Y;
				else
					return Location.X - rhs.Location.X;
			}

			public override string ToString()
			{
				return String.Format("{0}, {1}", Location.X, Location.Y);
			}
		}

        protected class Section_ObjectTOC : Section
        {
            public Section_ObjectTOC(NoxMap map) : base(map) { }

            protected override void ReadContents(BinaryReader rdr)
            {
                ushort numEntries = rdr.ReadUInt16();
                _Map._ObjectTOC = new SortedDictionary<ushort, string>();

                while ((numEntries--) > 0)
                {
                    ushort id = rdr.ReadUInt16();
                    _Map._ObjectTOC.Add(id, rdr.ReadString());
                }
            }

            protected override void WriteContents(BinaryWriter wtr)
            {
                RebuildTOC();
                var toc = _Map._ObjectTOC;
                wtr.Write((ushort)toc.Count);

                foreach (ushort key in toc.Keys)
                {
                    wtr.Write(key);
                    wtr.Write(toc[key]);
                }
            }

            ushort tocTracker;

            protected void AddEmbeddedObjects(NoxMap.Object o)
            {
                var toc = _Map._ObjectTOC;
                if (!toc.ContainsValue(o.Name))
                    toc.Add(tocTracker++, o.Name);

                foreach (NoxMap.Object obj in o.InventoryList)
                    AddEmbeddedObjects(obj); // Recursive loop through inventories
            }

            protected void RebuildTOC()
            {
                _Map._ObjectTOC = new SortedDictionary<ushort, string>();
                tocTracker = 0;

                foreach (NoxMap.Object obj in _Map.Objects)
                    AddEmbeddedObjects(obj);
            }
        }

        protected class Section_ObjectData : Section
        {
            public Section_ObjectData(NoxMap map) : base(map) { }

            protected override void ReadContents(BinaryReader rdr)
            {
                _Map.Objects = new List<Object>();

                Object currentObj;
                while (true)
                {
                    // The list is terminated by a null 
                    if (rdr.ReadUInt16() == 0)
                        break;
                    else //roll back
                        rdr.BaseStream.Seek(-2, SeekOrigin.Current); 

                    currentObj = new NoxMap.Object(rdr, _Map._ObjectTOC);
                    if (currentObj.Extent >= 0) // Skip embedded (inventory) objects (they are read along with their parent)
                        _Map.Objects.Add(currentObj);
                }
            }

            protected override void WriteContents(BinaryWriter wtr)
            {
                foreach (Object obj in _Map.Objects)
                    obj.Write(wtr, _Map._ObjectTOC);
            }
        }

		[Serializable]
		public class Object : IComparable, ICloneable
		{
			public string Name;
			public short VersionXfer; // AngryKirC - XFer parsing rule
			public short VersionEntry; // AngryKirC - Object entry parsing rule
			// ReadRule1 and ReadRule2 in newest Westwood maps nearly always appear to be 0x40
			public int Extent;
			public PointF32 Location;
			public int IngameID; // Global ID used by Nox itself
			public byte Terminator;
			public byte Team; // Team ID (0 = unassigned, 1 = Red, 2 = Blue etc)
			public string Scr_Name = ""; //Name used in Script Section
            public string ScrNameShort
            {
                get
                {
                    string result = Scr_Name;
                    int ptIndex = result.LastIndexOf(':');
                    // Trim the map name and : 
                    if (ptIndex >= 0) result = result.Substring(ptIndex + 1);

                    return result;
                }
            }
            public List<NoxMap.Object> InventoryList = new List<NoxMap.Object>(); //Objects in its inventory
			public string pickup_func = ""; //Function to execute when picked-up
			public List<UInt32> SlaveGIDList = new List<UInt32>(); // list of objects ref'd by GlobalID
			public uint CreateFlags = 0; // Will be added to normal object flags. research by AngryKirC
            public uint AnimFlags = 0x00; // max 0xA1
			public uint DestroyFrame = 0xFFFFFFFB;
			protected DefaultXfer ExtraData = new DefaultXfer();

            public ThingDb.Thing ThingDef
            {
                get
                {
                    return ThingDb.Things[Name];
                }
            }
			
			/// <summary>
			/// Initializes new ExtraData container with default values
			/// </summary>
			public void NewDefaultExtraData()
			{
				ExtraData = ObjectXferProvider.Get(ThingDb.Things[Name].Xfer);
			}
			
			public T GetExtraData<T>() where T : DefaultXfer
			{
				return (T) ExtraData;
			}
			
			public Object()
			{
				//default values
				Name = "ExtentShortCylinderSmall";
				Extent = -1;
				VersionXfer = 0x3C; // DefaultXfer
				VersionEntry = 0x40;
				CreateFlags = 0x1000000; // ENABLED
                Location = PointF32.Empty;
			}

			public Object(string name, PointF32 loc) : this()
			{
				Name = name;
				Location = loc;
				NewDefaultExtraData();
			}

            internal Object(BinaryReader rdr, IDictionary toc)
			{
				Extent = -1;
				Read(rdr, toc);
			}

            /*
			public bool HasFlag(ThingDb.Thing.FlagsFlags flag)
			{
				if ((ThingDb.Things[Name].Flags & flag) == flag) return true;
				if ((CreateFlags & (uint) flag) == (uint) flag) return true;
				return false;
			}
            */
		
			/// <summary>
			/// Logs warning message including this object's name and extent
			/// </summary>
			/// <param name="msg"></param>
			internal void LogObjectWarning(string msg)
			{
                if (_Log != null)
				    _Log.Warn("[Map.Object.?] ({0}) {1}", this, msg);
			}
			
			/// <summary>
			/// Read an object from the stream, using the provided toc to identify the object
			/// </summary>
			internal void Read(BinaryReader rdr, IDictionary toc)
			{
				Name = (string) toc[rdr.ReadUInt16()];
				rdr.BaseStream.Seek((8 - rdr.BaseStream.Position % 8) % 8, SeekOrigin.Current);//SkipToNextBoundary
				long endOfData = rdr.ReadInt64() + rdr.BaseStream.Position;
				
				NewDefaultExtraData();
				VersionXfer = rdr.ReadInt16();
				VersionEntry = rdr.ReadInt16(); // entry structure/version sign
				
				if (VersionEntry < 0x3D || VersionEntry > 0x40)
				{
					LogObjectWarning(String.Format("Unsupported entry structure sign {0:X}.\n Skipping this object...", VersionEntry));
					rdr.BaseStream.Seek(endOfData, SeekOrigin.Begin);
					return;
				}
				
				Extent = rdr.ReadInt32();
				IngameID = rdr.ReadInt32();
				Location = new PointF32(rdr.ReadSingle(), rdr.ReadSingle()); // X, then Y

				if(Location.X > 5880 || Location.Y > 5880)
					Location = new PointF32(5870, 5870);
				
				byte inven = 0;
				Terminator = rdr.ReadByte();
				if (Terminator != 0)
				{
					CreateFlags = rdr.ReadUInt32();
					Scr_Name = rdr.ReadString();
					Team = rdr.ReadByte();
					inven = rdr.ReadByte();
					for (int i = rdr.ReadInt16(); i > 0; i--)
						SlaveGIDList.Add(rdr.ReadUInt32());
					
					AnimFlags = rdr.ReadUInt32();
					if (VersionEntry >= 0x3F)
					{
						if (rdr.ReadInt16() <= 1)
						{
							int len = rdr.ReadInt32();
							byte[] temp = rdr.ReadBytes(len);
							rdr.ReadUInt32();
							pickup_func = Encoding.ASCII.GetString(temp);
						}
						
						if (VersionEntry >= 0x40) DestroyFrame = rdr.ReadUInt32();
					}
				}
				
				ThingDb.Thing tt = ThingDb.Things[Name];
				if (VersionXfer > ExtraData.MaxVersion)
				{
					// temporarily allowed for WeaponXfer and ElevatorXfer (see implementation)
					// for other types this signals data corruption
					LogObjectWarning(String.Format("{0} version {1} is greater than max supported {2}", tt.Xfer, VersionXfer, ExtraData.MaxVersion));
				}
				
				long pos = rdr.BaseStream.Position;
				if (pos <= endOfData)
				{				
					bool result = ExtraData.FromStream(rdr.BaseStream, VersionXfer, tt);
					if (!result && tt.Xfer != null && tt.Xfer != "DefaultXfer")
					{
						// Unable to fully parse Xfer data
						LogObjectWarning(String.Format("Failed to fully parse {0} data", tt.Xfer));
					}
				}
				else
				{
					// Corrupted header
					LogObjectWarning("Corrupted header structure!");
				}
				
				pos = rdr.BaseStream.Position;
				if (pos != endOfData)
				{
					// Corrupted header OR Xfer data. It would be better to ignore this object
					LogObjectWarning(String.Format("Object entry out of bounds (diff {0})", pos - endOfData));
				}
				rdr.BaseStream.Seek(endOfData, SeekOrigin.Begin); // Ensure correct position
				
				// Read subitems (inventory)
				if (inven > 0)
				{
					InventoryList.Clear();
					for(int i = inven; i > 0; i--)
						InventoryList.Add(new Object(rdr, toc));
				}
			}
			/// <summary>
			/// Writes the object to the stream
			/// </summary>
			/// <param name="stream">The stream to write to</param>
			/// <param name="toc">A Mapping of string to short IDs</param>
			internal void Write(BinaryWriter wtr, IDictionary toc)
			{
				if (pickup_func != null && pickup_func.Length > 0 && VersionEntry < 0x3F) { VersionEntry = 0x40; };

				wtr.Write((short) toc[Name]);
				wtr.BaseStream.Seek((8 - wtr.BaseStream.Position % 8) % 8, SeekOrigin.Current);//SkipToNextBoundary
				long lengthRecordPos = wtr.BaseStream.Position;
				
				wtr.Write((long) 0);
				VersionXfer = ExtraData.MaxVersion;
				wtr.Write((short) VersionXfer);
				wtr.Write((short) VersionEntry);
				wtr.Write((int) Extent);
				wtr.Write((int) IngameID);
				wtr.Write((float) Location.X);
				wtr.Write((float) Location.Y);
				wtr.Write((byte) Terminator);
				
				if (Terminator != 0)
				{
					wtr.Write(CreateFlags);
					wtr.Write(Scr_Name);
					wtr.Write(Team);
					wtr.Write((byte) InventoryList.Count);
					wtr.Write((short) SlaveGIDList.Count);
                    foreach (UInt32 u in SlaveGIDList)  // single player only
						wtr.Write(u);

                    wtr.Write(AnimFlags);
					if (VersionEntry >= 0x3F)
					{
						wtr.Write((short) 1);
						wtr.Write(pickup_func.Length);
						wtr.Write(pickup_func.ToCharArray());
						wtr.Write((uint) 0);
						
						if (VersionEntry >= 0x40) wtr.Write(DestroyFrame);
					}
                }
				
				ExtraData.WriteToStream(wtr.BaseStream, VersionXfer, ThingDb.Things[Name]);
				
				long currentPos = wtr.BaseStream.Position;
				long entryLength = currentPos - (lengthRecordPos + 8);
				wtr.BaseStream.Seek(lengthRecordPos, SeekOrigin.Begin);
				wtr.Write(entryLength);
				wtr.BaseStream.Seek(currentPos, SeekOrigin.Begin);
				
				if (Terminator != 0)
				{
					foreach (Object o in InventoryList) o.Write(wtr, toc);
				}
			}

			public int CompareTo(object obj)
			{
				return (Name.CompareTo(((Object)obj).Name) == 0) ? Extent.CompareTo(((Object)obj).Extent) : Name.CompareTo(((Object)obj).Name);
			}

			public override string ToString()
			{
				return Name + " " + Extent.ToString();
			}

			public object Clone()
			{
				Object copy = (Object) MemberwiseClone();
				// Clone inventory, not copy the reference
				copy.InventoryList = new List<NoxMap.Object>();
				foreach (NoxMap.Object o in InventoryList) copy.InventoryList.Add((NoxMap.Object) o.Clone());
				copy.SlaveGIDList = new List<UInt32>();
				foreach (uint i in SlaveGIDList) copy.SlaveGIDList.Add(i);
				// Clone transferdata, not copy the reference
				copy.ExtraData = (DefaultXfer) ExtraData.Clone();
				// fields are already copied by MemberwiseClone
				return copy;
			}
		}
		#endregion

		#region Reading and Writing Methods
        protected void ReadStream(NoxBinaryReader rdr)
        {
            if (_Log != null)
                _Log.Info("Reading {0}.", FileName);

            _Header = new MapHeader();
            _Header.Read(rdr);

            while (rdr.BaseStream.Position < rdr.BaseStream.Length)
            {
                string section = rdr.ReadString();
                rdr.SkipToNextBoundary();

                if (_Log != null & section.Length <= 0)
                    _Log.Debug("[Map.ReadFile] Empty section " + section);

                if (_Log != null && !SectionClasses.ContainsKey(section))
                    _Log.Warn("[Map.ReadFile] Unhandled section: " + section + ".");
                else
                {
                    var secClass = (Section)Activator.CreateInstance(SectionClasses[section], this);
                    secClass.Read(rdr);
                    _Log.Info("[Map.ReadFile] Section {0} read OK, version {1} ", section, secClass.CodeVersion);
                }
            }

            if (_Log != null)
                _Log.Info("Map has been read successfully.");

            // Free TOC memory
            _ObjectTOC = null;
        }

		protected void WriteMapData()
		{
			if (_Log != null)
                _Log.Info("Writing mapdata for {0}.", FileName);

            using (var memStream = new MemoryStream())
            {
                var wtr = new BinaryWriter(memStream);

                _Header.Write(wtr);
                Info.Write(wtr);
                new Section_WallMap(this).Write(wtr);
                new Section_FloorMap(this).Write(wtr);
                new Section_SecretWalls(this).Write(wtr);
                new Section_DestructableWalls(this).Write(wtr);
                new Section_WayPoints(this).Write(wtr);
                new Section_DebugData(this).Write(wtr);
                new Section_WindowWalls(this).Write(wtr);
                new Section_GroupData(this).Write(wtr);
                new Section_ScriptObject(this).Write(wtr);
                new Section_AmbientData(this).Write(wtr);
                new Section_PolygonList(this).Write(wtr);
                new Section_MapIntro(this).Write(wtr);
                new Section_ScriptData(this).Write(wtr);
                new Section_ObjectTOC(this).Write(wtr);
                new Section_ObjectData(this).Write(wtr);

                // Free TOC memory
                _ObjectTOC = null;

                //write null bytes to next boundary -- this is needed only because
                // no more data is going to be written,
                // so the null bytes are not written implicitly by 'Seek()'ing
                wtr.Write(new byte[(8 - memStream.Position % 8) % 8]);

                //go back and write header again, with a proper checksum
                _Header.GenerateChecksum(memStream.ToArray());
                wtr.BaseStream.Seek(0, SeekOrigin.Begin);
                _Header.Write(wtr);
                wtr.BaseStream.Seek(0, SeekOrigin.End);
                wtr.Close();

                mapData = memStream.ToArray();
            }

			if (_Log != null)
			    _Log.Info("Map has been written successfully.");
		}

        /// <summary>
        /// Saves the map into the file specified by FileName (overwrites the file silently)
        /// </summary>
		public void WriteMap(bool writeNxz)
		{
			WriteMapData();
            mapData = CryptApi.NoxEncrypt(mapData, CryptApi.NoxCryptFormat.MAP);

            // Write .map
			if (File.Exists(FileName)) File.Delete(FileName);
            using (BinaryWriter fileWtr = new BinaryWriter(File.Create(FileName)))
            {
                fileWtr.Write(mapData);
            }

            if (!writeNxz) return;
            // Write .nxz
            string nxzName = Path.GetDirectoryName(FileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(FileName) + ".nxz";
            if (File.Exists(nxzName)) File.Delete(nxzName);

            using (BinaryWriter fileWtr = new BinaryWriter(File.Create(nxzName)))
            {
                fileWtr.Write((uint)mapData.Length);
                fileWtr.Write(NoxLz.Compress(mapData));
            }
            // Free buffer
            mapData = null;
		}
		#endregion

        protected static Dictionary<string, Type> SectionClasses = null;
        static NoxMap()
        {
            SectionClasses = new Dictionary<string, Type>();
            SectionClasses.Add("MapInfo", typeof(Section_MapInfo));
            SectionClasses.Add("WallMap", typeof(Section_WallMap));
            SectionClasses.Add("FloorMap", typeof(Section_FloorMap));
            SectionClasses.Add("SecretWalls", typeof(Section_SecretWalls));
            SectionClasses.Add("DestructableWalls", typeof(Section_DestructableWalls));
            SectionClasses.Add("WayPoints", typeof(Section_WayPoints));
            SectionClasses.Add("DebugData", typeof(Section_DebugData));
            SectionClasses.Add("WindowWalls", typeof(Section_WindowWalls));
            SectionClasses.Add("GroupData", typeof(Section_GroupData));
            SectionClasses.Add("ScriptObject", typeof(Section_ScriptObject));
            SectionClasses.Add("AmbientData", typeof(Section_AmbientData));
            SectionClasses.Add("Polygons", typeof(Section_PolygonList));
            SectionClasses.Add("MapIntro", typeof(Section_MapIntro));
            SectionClasses.Add("ScriptData", typeof(Section_ScriptData));
            SectionClasses.Add("ObjectTOC", typeof(Section_ObjectTOC));
            SectionClasses.Add("ObjectData", typeof(Section_ObjectData));
        }
	}
}
