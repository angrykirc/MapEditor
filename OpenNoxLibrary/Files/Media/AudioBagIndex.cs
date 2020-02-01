using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace OpenNoxLibrary.Files.Media
{
    public class AudioBagIndex : GenericBagIndex
    {
        public class FileRecord
        {
            private const int NAME_LENGTH = 0x10;
            public string Name;
            public uint SampleRate;
            public uint BagOffset;
            public uint Length;
            public uint Flags;
            public uint ChunkSize;

            public FileRecord()
            {
            }

            public void Read(BinaryReader rdr)
            {
                Name = new string(rdr.ReadChars(NAME_LENGTH));
                Name = Name.TrimEnd(new char[] { '\0' });
                BagOffset = rdr.ReadUInt32();
                Length = rdr.ReadUInt32();
                SampleRate = rdr.ReadUInt32();
                Flags = rdr.ReadUInt32();
                ChunkSize = rdr.ReadUInt32();
            }
        }

        protected List<FileRecord> _Records;

        public uint Version { get; set; }

        public AudioBagIndex(string idxFilePath, string bagFilePath)
            : base(idxFilePath, bagFilePath)
        {
            _Records = new List<FileRecord>();
        }

        /// <summary>
        /// Returns numerical entry index found by specified sound name, or -1 if not found.
        /// </summary>
        /// <param name="name"></param>
        public int GetIndexByName(string name)
        {
            int c = 0;
            foreach (FileRecord fi in _Records)
            {
                if (fi.Name == name)
                    return c;
                c++;
            }
            return -1;
        }

        /// <summary>
        /// Returns a FileIndex referencing the packed file, found by specified numerical index.
        /// </summary>
        public FileRecord GetRecordByIndex(int index)
        {
            if (!IndexReady) throw new InvalidOperationException("Index file is not parsed yet.");
            if (_Records.Count <= index) throw new IndexOutOfRangeException("Specified index is out of record range");

            return _Records[index];
        }

        protected override bool ReadIndexFile_Impl()
        {
            using (var br = new BinaryReader(File.OpenRead(_IdxFilePath)))
            {
                uint header = br.ReadUInt32();
                Version = br.ReadUInt32();

                int count = br.ReadInt32();
                _Records = new List<FileRecord>(count);

                while ((count--) > 0)
                {
                    var e = new FileRecord();
                    e.Read(br);
                    _Records.Add(e);
                }
            }
            return true;
        }
    }
}
