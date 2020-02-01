using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace OpenNoxLibrary.Files.Media
{
    public abstract class GenericBagIndex
    {
        protected readonly string _IdxFilePath;
        protected readonly string _BagFilePath;

        /// <summary>
        /// Path to the file that contains indexes.
        /// </summary>
        public string IdxPath
        {
            get
            {
                return _IdxFilePath;
            }
        }

        /// <summary>
        /// Path to the file that contains compressed data.
        /// </summary>
        public string BagPath
        {
            get
            {
                return _BagFilePath;
            }
        }

        protected bool _IndexFileParsed;
        
        /// <summary>
        /// Used for reading contents of .bag file in the real-time.
        /// </summary>
        protected FileStream _BagFileStream;

        /// <summary>
        /// Signifies whenether the .idx file had been parsed.
        /// </summary>
        public bool IndexReady
        {
            get
            {
                return _IndexFileParsed;
            }
        }

        /// <summary>
        /// Signifies whenether the .bag data stream is available.
        /// </summary>
        public bool DataReady
        {
            get
            {
                return _BagFileStream != null;
            }
        }

        protected GenericBagIndex(string idxFilePath, string bagFilePath)
        {
            this._IdxFilePath = idxFilePath;
            this._BagFilePath = bagFilePath;

            _IndexFileParsed = false;
            _BagFileStream = null;
        }

        protected abstract bool ReadIndexFile_Impl();

        /// <summary>
        /// Reads all index entries, stores them in memory.
        /// </summary>
        public bool ReadIndexFile()
        {
            if (!File.Exists(_IdxFilePath)) return false;
            _IndexFileParsed = ReadIndexFile_Impl();

            return _IndexFileParsed;
        }

        public bool OpenDataStream()
        {
            if (_BagFileStream != null)
                throw new InvalidOperationException(".bag FileStream is already open");

            if (!File.Exists(_BagFilePath)) return false;
            _BagFileStream = File.Open(_BagFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            return true;
        }

        public void CloseDataStream()
        {
            if (_BagFileStream == null)
                throw new InvalidOperationException(".bag FileStream wasn't open");

            _BagFileStream.Close();
            _BagFileStream = null;
        }
    }
}
