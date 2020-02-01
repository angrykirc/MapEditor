using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenNoxLibrary.Files.Media
{
    public class AudioBagReader : AudioBagIndex
    {
        public AudioBagReader(string bagPath, string idxPath) : base(bagPath, idxPath)
		{
		}

        public byte[] ReadSoundData(FileRecord rec)
        {
            if (!DataReady) throw new InvalidOperationException("VideoBag stream is not yet open, call OpenDataStream() first");
            if (rec == null) return null; 

            _BagFileStream.Seek(rec.BagOffset, SeekOrigin.Begin);
            byte[] result = new byte[rec.Length];
            _BagFileStream.Read(result, 0, (int)rec.Length);

            return result;
        }
    }
}
