using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

using OpenNoxLibrary.Compression;

namespace OpenNoxLibrary.Files.Media
{
    /// <summary>
    /// Provides retrieval of the contents of game's packed graphics data files.
    /// </summary>
    public class VideoBagIndex : GenericBagIndex
    {
        // List of all sections that contain file indexes
        protected List<Section> sections;
        // List of ALL file indexes in ALL sections
        protected List<FileIndex> indexes;

        public int SectionCount
        {
            get
            {
                return sections.Count;
            }
        }

        public int IndexCount
        {
            get
            {
                return indexes.Count;
            }
        }

        public uint Unknown1 { get; set; } // From all what I have seen, it's always 0x8000
        public uint Flags2 { get; set; } // 0x8000 for 8-bit files, any other value otherwise
        public uint InitialIndexes { get; set; } // Seems to be very close to index count

        public bool Is8Bit
        {
            get { return Flags2 == 0x8000; }
        }

        /// <summary>
        /// Constructs a new interface for pair of files. If specified file paths do not exist, no error is generated.
        /// </summary>
        public VideoBagIndex(string idxFilePath, string bagFilePath) : base(idxFilePath, bagFilePath)
        {
            sections = new List<Section>();
            indexes = new List<FileIndex>();
        }

        /// <summary>
        /// Returns all image indexes in all sections
        /// </summary>
        public FileIndex[] GetAllFileIndexes()
        {
            return indexes.ToArray();
        }

        /// <summary>
        /// Represents a packedimage index record
        /// </summary>
        public struct FileIndex
        {
            // Tracks which section this file belongs to
            public int SectionId;
            // Offset in uncompressed section data
            public uint SectionOffset;
            // General purpose
        	
            //public string Filename;
        	public byte DataType;
        	public uint DataLength;
            // It's half of colorspace (0x422/0x844) size for tiles (logic: of entire 46x46 square, 23x23 pixels are left blank), idk what this is for others 
            // Most probably, this is 'clean' source file size before packing, or some kind of buffer length
        	public uint Unknown;
        	
            /// <summary>
            /// Constructs empty index record
            /// </summary>
            public FileIndex(string Filename, byte DataType, uint Size, uint Unknown) 
            {
                SectionId = 0;
                SectionOffset = 0;

                //this.Filename = Filename;
                this.DataType = DataType;
                this.DataLength = Size;
                this.Unknown = Unknown;
            }

            /// <summary>
            /// Constructs index from an incoming data buffer
            /// </summary>
            public FileIndex(BinaryReader br)
        	{
                SectionId = 0;
                SectionOffset = 0;

                br.ReadBytes(br.ReadByte());

                DataType = br.ReadByte();
                DataLength = br.ReadUInt32();
                Unknown = br.ReadUInt32();
        	}
        }

        /// <summary>
        /// Represents a section that encapsulates some number of media files. AKA chunk in data/archive terminology
        /// </summary>
        public struct Section
        {
            /// <summary>
            /// In-file offset for this section's compressed data
            /// </summary>
            public uint VideoBagOffset; 

            /// <summary>
            /// Size/offset in compressed data
            /// </summary>
            public uint LengthCompressed;

            /// <summary>
            /// Image data size when uncompressed
            /// </summary>
            public uint LengthUncompressed;

            /// <summary>
            /// Constructs a section from an incoming data buffer
            /// </summary>
            public Section(BinaryReader br)
            {
                br.ReadUInt32(); // IDX section entry length, not needed for now
                VideoBagOffset = 0;
                LengthUncompressed = br.ReadUInt32();
                LengthCompressed = br.ReadUInt32();
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write((uint)0xFFFFFFFF); // idx section length, will be rewritten later
                bw.Write(LengthUncompressed);
                bw.Write(LengthCompressed);
            }
        }

        protected override bool ReadIndexFile_Impl()
        {
            var videoIdxStream = new BinaryReader(File.Open(_IdxFilePath, FileMode.Open), Encoding.ASCII);

            uint magic = videoIdxStream.ReadUInt32();

            if (magic == 0xFAEDBCEB)
            {
                uint Length = videoIdxStream.ReadUInt32();
                uint SectionCount = videoIdxStream.ReadUInt32();

                Unknown1 = videoIdxStream.ReadUInt32();
                Flags2 = videoIdxStream.ReadUInt32();
                InitialIndexes = videoIdxStream.ReadUInt32();

                // Now sections/chunks follow
                sections = new List<Section>();
                // And inside each section, there are index entries
                indexes = new List<FileIndex>();

                // Accumulates offset for every section in video.bag
                uint sectionBagOffsetAcc = 0;

                // Read sections
                for (int sid = 0; sid < SectionCount; sid++)
                {
                    // All reading operations are done in constructor
                    Section section = new Section(videoIdxStream);

                    // Calculate offset in video.bag
                    section.VideoBagOffset = sectionBagOffsetAcc;
                    sectionBagOffsetAcc += section.LengthCompressed;

                    // Accumulates offset for each file data in uncompressed data
                    uint sectionOffsetAcc = 0;

                    // Now read file indexes
                    uint indexCount = videoIdxStream.ReadUInt32();
                    if (indexCount == 0xFFFFFFFF)
                        indexCount = 1; // Maybe defines modified section? But that's wrong

                    for (int i = 0; i < indexCount; i++)
                    {
                        // All reading operations are done in constructor
                        var fe = new FileIndex(videoIdxStream);
                        fe.SectionId = sid;

                        // Calculate file offset in section data
                        fe.SectionOffset = sectionOffsetAcc;
                        sectionOffsetAcc += fe.DataLength;

                        indexes.Add(fe);
                    }

                    sections.Add(section);
                }
                videoIdxStream.Close();
            }
            else
            {
                videoIdxStream.Close();
                throw new ApplicationException(String.Format("Wrong VideoBag header: expected 0xFAEDBCEB, got 0x{0:X}", magic));
            }
            return true;
        }

        /// <summary>
        /// Pulls packedfile index record by specified numerical index
        /// </summary>
        public FileIndex PullFileIndex(int globalIndex)
        {
            if (!IndexReady) throw new InvalidOperationException("Index file is not parsed yet.");
            if (globalIndex >= indexes.Count) throw new ArgumentOutOfRangeException("Given index wasn't found in the list.");

            return indexes[globalIndex];
        }

        public byte[] PullSectionData(int sectionId)
        {
            if (!DataReady)
                throw new InvalidOperationException("VideoBag stream is not yet open, call OpenDataStream() first");

            if (sectionId >= sections.Count) throw new ArgumentOutOfRangeException("Section ID not found in this file / too large value");

            Section sect = sections[sectionId];

            _BagFileStream.Seek(sections[sectionId].VideoBagOffset, SeekOrigin.Begin);
            byte[] compressed = new byte[sect.LengthCompressed];
            byte[] uncompressed = new byte[sect.LengthUncompressed];

            _BagFileStream.Read(compressed, 0, (int)sect.LengthCompressed);

            NoxLz.Decompress(compressed, uncompressed);
            return uncompressed;
        }
    }
}
