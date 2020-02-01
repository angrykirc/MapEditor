using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenNoxLibrary.Files.Media
{
    public class FontFile
    {
        protected int _Unknown1;
        protected int _Unknown2;
        protected int _SizeType;
        protected int _SymbolHeight;
        // SymbolWidth = _SingleSymbolLength / _SymbolHeight
        protected int _ListsCount;
        protected int _SingleSymbolLength;

        // There are two possible methods on organizing the storage:
        // 1. Store the lists entirely as byte buffers, as well as FirstChar/LastChar values, return the symbols on-demand [i * _SingleSymbolLength; (i + 1) * _SingleSymbolLength]
        // 2. Store each symbol as a ready-to-go byte array
        // Since we are not limited on the RAM usage, I preferred to go with the 2nd variant.
        protected byte[][] _SymbolsData;

        public FontFile()
        {
            _SymbolsData = new byte[256][];
        }

        private void ReadSymbols(BinaryReader br)
        {
            br.BaseStream.Seek(0x1C, SeekOrigin.Begin);
            for (int i = 0; i < _ListsCount; i++)
            {
                int Indent = br.ReadInt32();
                int FirstChar = br.ReadInt16();
                int LastChar = br.ReadInt16();

                for (int c = FirstChar; c <= LastChar; c++)
                {
                    _SymbolsData[c] = br.ReadBytes(_SingleSymbolLength);
                }
                if (Indent > 0) System.Diagnostics.Debug.Fail("Indent is more than zero");
                br.BaseStream.Seek(Indent, SeekOrigin.Current);
            }
        }

        public void ReadFrom(string FilePath)
        {
            using (var br = new BinaryReader(File.OpenRead(FilePath)))
            {
                // FIXME: number.fnt does not have a header
                if (br.ReadUInt32() != 0x466F4E74) // tNoF
                {
                    throw new InvalidDataException("The file specified is not a valid Nox Font file");
                }

                _Unknown1 = br.ReadInt32();
                _Unknown2 = br.ReadInt32();
                _SizeType = br.ReadInt32();
                _SymbolHeight = br.ReadInt32();
                _ListsCount = br.ReadInt32();
                _SingleSymbolLength = br.ReadInt32();
                // Symbol 'lists'/'chunks' then follow
                ReadSymbols(br);
            }
        }
    }
}
