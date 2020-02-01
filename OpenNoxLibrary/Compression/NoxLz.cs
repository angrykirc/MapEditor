using System;
using System.Linq;
using System.Collections.Generic;

namespace OpenNoxLibrary.Compression
{
	/// <summary>
	/// Implementation of a common compression algorithm used by Nox: LZ77 + Symbol table encoding [pseudo-Huffman]
	/// </summary>
	public class NoxLz
	{
		// ***** Constants *****
		/// <summary>
		/// Max number of possible encoded symbols
		/// </summary>
		const int SYMBOLS = 274; // 0x111 + 1
		/// <summary>
		/// LZ77 history window length (64KB)
		/// </summary>
		const int WINDOW_SIZE = 0x10000;
		/// <summary>
		/// Compressor will rebuild symbol alphabet after this num of symbols
		/// </summary>
		int RebuildCounter = WINDOW_SIZE / 4;
		/// <summary>
		/// LZ77 index table for length
		/// </summary>
		static int[] LENGTH_TABLE = 
		{
			1, 0x008,
			2, 0x00A,
			3, 0x00E,
			4, 0x016,
			5, 0x026,
			6, 0x046,
			7, 0x086,
			8, 0x106
		};
		/// <summary>
		/// LZ77 index table for offset distances
		/// </summary>
		static int[] DISTANCE_TABLE = 
		{
			0, 0x00,
			0, 0x01,
			1, 0x02,
			2, 0x04,
			3, 0x08,
			4, 0x10,
			5, 0x20,
			6, 0x40
		};
		
		// ***** Variables *****
		/// <summary>
		/// Huffman alphabet containing symbols sorted by their occurence rate
		/// </summary>
		int[] SymbolAlphabet = new int[SYMBOLS];
		/// <summary>
		/// Table used to decode and encode symbols, contains pair of values (bitsize; prefix).
		/// </summary>
		int[] SymbIndexTable;
		/// <summary>
		/// Bit-by-bit reader implementation
		/// note that bits of each byte are read in reverse order
		/// </summary>
		BitReader bitrdr;
		/// <summary>
		/// Bit-by-bit writer implementation
		/// </summary>
		BitWriter bitwtr;
		/// <summary>
		/// Table addressing how many times each symbol occurs
		/// </summary>
		int[] SymbolCounter;
		/// <summary>
		/// Offset in source byte array for compression (destination for decompression)
		/// </summary>
		int wrkPos = 0;
		
		private NoxLz() {}

		private void BuildInitialAlphabet()
		{
			int pos = 0;
			for (; pos <= 15; pos++) SymbolAlphabet[pos] = pos + 0x100;

			SymbolAlphabet[pos] = 0; pos++;
			SymbolAlphabet[pos] = 0x20; pos++;
			SymbolAlphabet[pos] = 0x30; pos++;
			SymbolAlphabet[pos] = 0xFF; pos++;
			for (int i = 1; i <= 0x111; i++)
			{
				if (Array.IndexOf(SymbolAlphabet, i) < 0)
				{
					SymbolAlphabet[pos] = i; pos++;
				}
			}
			
			SymbIndexTable = new int[] {
				0x00000002, 0x00000000, 
				0x00000003, 0x00000004, 
				0x00000003, 0x0000000C, 
				0x00000004, 0x00000014, 
				0x00000004, 0x00000024, 
				0x00000004, 0x00000034, 
				0x00000004, 0x00000044, 
				0x00000004, 0x00000054, 
				0x00000004, 0x00000064, 
				0x00000004, 0x00000074, 
				0x00000004, 0x00000084, 
				0x00000004, 0x00000094, 
				0x00000004, 0x000000A4, 
				0x00000005, 0x000000B4, 
				0x00000005, 0x000000D4, 
				0x00000005, 0x000000F4
			};
		}
		
		private void RebuildAlphabet()
		{
			int[] huffman = new int[SYMBOLS];
			int i = SYMBOLS;
			while (i > 0) { i--; huffman[i] = i; }
			
			List<int> l = huffman.ToList();
			l.Sort((x1, x2) => ((SymbolCounter[x2] << 16) + x2) - ((SymbolCounter[x1] << 16) + x1));
			SymbolAlphabet = l.ToArray();
        }

        private bool DecompressImpl(byte[] dst, int dstLen)
		{
			int code, bits, offset, idx, symbol, length, distance;
            // Warning: if you wish to reimplement this code, please use unsigned integer types.
            int[] LZWindow = new int[WINDOW_SIZE];
            int LZPosWindow = 0;

			while (wrkPos < dstLen)
			{
				code = bitrdr.Read(4);
				// SHL one bit is analogical to multiplying by 2
				bits = SymbIndexTable[code << 1];
				offset = SymbIndexTable[(code << 1) + 1]; // aka prefix
				// position of the symbol in alphabet
				idx = bitrdr.Read(bits) + offset;
				symbol = SymbolAlphabet[idx];
				SymbolCounter[symbol]++;
				
				if (symbol < 0x100)
				{
					// Output literal (plain byte)
					dst[wrkPos] = (byte) symbol; wrkPos++;
                    // Also store in in the history
					LZWindow[LZPosWindow % WINDOW_SIZE] = symbol;
					LZPosWindow++;
				}
				else if (symbol == 0x110)
				{
					// Rebuild symbol alphabet
					RebuildAlphabet();
					// Divide all counts by two
					int i = SYMBOLS;
					while (i > 0)
					{
						i--;
						SymbolCounter[i] = SymbolCounter[i] >> 1;
					}
					// Parse new Huffman table
					bits = 0;
					offset = 0;
					length = 0;
					i = 16;
					while (i > 0)
					{
						i--;
						while (bitrdr.ReadBit() == 0) bits++;
						SymbIndexTable[length] = bits;
						SymbIndexTable[length + 1] = offset;
						length += 2;
						offset += 1 << bits;
					}
				}
				else if (symbol >= 0x111)
				{
					return false; // symbol out of range
				}
				else
				{
					// LZ77
					// find sequence length
					length = 4;
					if (symbol < 0x108)
					{
						length += symbol - 0x100;
					}
					else
					{
						idx = symbol - 0x108;
						bits = LENGTH_TABLE[idx << 1];
						offset = LENGTH_TABLE[(idx << 1) + 1];
						length += offset + bitrdr.Read(bits);
					}
					
					// read window offset the sequence is located at
					code = bitrdr.Read(3);
					bits = DISTANCE_TABLE[code << 1];
					offset = DISTANCE_TABLE[(code << 1) + 1];
					distance = (offset << 9) + bitrdr.Read(bits + 9);
					
					idx = LZPosWindow - distance;
					// Copy sequence from history window
					for (int i = 0; i < length; i++)
					{
						symbol = LZWindow[(idx + i) % WINDOW_SIZE];
						
						dst[wrkPos] = (byte) symbol; wrkPos++;
						
						LZWindow[LZPosWindow % WINDOW_SIZE] = symbol;
						LZPosWindow++;
					}
				}
			}
			
			return true;
		}
		
		public unsafe byte[] CompressImpl(byte[] src, int srcLen)
		{
			int tabPos, refLen, refOff = 0, symbol = 0;

            int rebuildCounter = RebuildCounter;
            int longestMatchLen = 0;
            int longestMatchPos = 0;

			fixed (byte* srcp = src)
			{
				while (wrkPos < srcLen)
				{
					if ((rebuildCounter--) < 0) // Decrement+comparison is the fastest option (benchmark checked)
					{
						// Rebuild symbol alphabet
						// so that more frequent symbols will be on top and will take less bits to encode
						symbol = 0x110;
                        rebuildCounter = RebuildCounter;
					}
                    else 
					{
                        // Make sure you reset this variable or you could get stuck with same LZ literals written over and over
                        // Stores the position of the longest match found in the window
                        longestMatchLen = 0;

                        // We don't need to attempt to search for repeating sequence for every single byte added to the window
                        // This is just a waste of CPU time; you can set the divisor to 4, but it won't really help to achieve better compression
                        if (wrkPos % 8 == 0)
                        {
                            // Attempt to find a sequence of bytes that repeats itself in the history window
                            int LZWindowStart = wrkPos - WINDOW_SIZE; // offset back to some position (which is the left side of the window)
                            if (LZWindowStart < 0) LZWindowStart = 0; // index cannot be negative

                            // We are going in reverse order, from right to left
                            int LZSearchPos = wrkPos;
                            while (LZSearchPos >= LZWindowStart)
                            {
                                // Search for repeats of this sequence, which is the 4 bytes after the window end
                                uint seq = *(uint*)(srcp + wrkPos);
                                int matchLen = 0;

                                // Determine the length of the matching sequence by comparing 4byte chunks
                                while (*(uint*)(srcp + LZSearchPos + matchLen) == seq)
                                {
                                    // Check if we are breaking the window boundaries (don't confuse wrkPos with srcLen)
                                    if ((LZSearchPos + matchLen) >= wrkPos || matchLen >= 516 || (wrkPos + matchLen + 4) >= srcLen) break;
                                    matchLen += 4;
                                    seq = *(uint*)(srcp + wrkPos + matchLen); // Next part of the sequence
                                }

                                // The sooner we find a repeating sequence, the smaller offset we get, and thus less bits to encode
                                // This is true for short repeating sequences (1-2 blocks or so), but we prefer to save bytes (sequence repeats) instead of bits (offset)
                                if (matchLen > longestMatchLen)
                                {
                                    longestMatchLen = matchLen;
                                    longestMatchPos = LZSearchPos;

                                    if (matchLen >= 516)
                                        break; // We can't encode larger values, going further is a waste of time
                                }

                                LZSearchPos -= 4;
                            }
                        }
                        
                        // If we found a repeating sequence
                        if (longestMatchLen >= 4) 
						{
							// Proceed on LZ encoding
							// Encode length into symbol
                            if (longestMatchLen <= 11) // 7+4 max
							{
								// decoded as: length = symbol - 0x100 + 4
                                symbol = 0x100 + (longestMatchLen - 4);
							}
							else
							{
								// else use length table
								for (tabPos = 0; tabPos < 8; tabPos++)
								{
									// maximum offset value that can be represented by using this table entry
									refLen = LENGTH_TABLE[(tabPos << 1) + 1] + (1 << (LENGTH_TABLE[tabPos << 1])) - 1;
                                    if ((longestMatchLen - 4) <= refLen) break;
								}
								symbol = 0x108 + tabPos;
							}
							// proceed - write symbol and then encode
						}
						else // If we failed to find repeating values
						{
							// Literal symbol (plain byte)
							symbol = src[wrkPos]; wrkPos++;
						}
					}
					
					// increment symbol counter
					SymbolCounter[symbol]++;
					// Search for that symbol in the Huffman alphabet
					int huffmanPos = 0;
					while (huffmanPos <= SYMBOLS)
					{
						if (SymbolAlphabet[huffmanPos] == symbol) break; // Found
						huffmanPos++;
					}
					// Encode symbol reference
					// we want to have this encoded in less #of bits
					for (tabPos = 0; tabPos < 16; tabPos++)
					{
						// maximum value that can be represented directly by #bits
						refLen = (1 << SymbIndexTable[tabPos << 1]) - 1;
						// this val is added to the offset
						refOff = SymbIndexTable[(tabPos << 1) + 1];
						// check if offset is less than pos itself
						if (huffmanPos >= refOff)
						{
							// enough bits to represent remaining part of the offset
							if (refLen >= (huffmanPos - refOff)) break;
							// if not then lookup next entry
						}
						else
						{
                            // Either too large symbol or messed up index table
							throw new ApplicationException("Unable to encode symbol: " + symbol);
						}
					}
					// position of symbol in alphabet minus table offset
					int remain = huffmanPos - refOff;
					// write encoded symbol
					bitwtr.Write(tabPos, 4);
					bitwtr.Write(remain, SymbIndexTable[tabPos << 1]);
					
					if (symbol == 0x110)
					{
						// Rebuild symbol alphabet
						RebuildAlphabet();
						// Divide all counts by two
						int i = SYMBOLS;
						while (i > 0)
						{
							i--;
							SymbolCounter[i] = SymbolCounter[i] >> 1;
						}
						// TODO actually *REBUILD* symbol indexer according to symbol usage frequency
						// For now just output default table
						for (i = 0; i < 32; i += 2)
						{
							int back = SymbIndexTable[i];
							if (i >= 2) back -= SymbIndexTable[i - 2];
							while (back > 0)
							{
								back--;
								bitwtr.Write(0, 1);
							}
							bitwtr.Write(1, 1);
						}
					}
					else if (symbol >= 0x100)
					{
						// if there was an application of LZ77
						int wndoff = wrkPos - longestMatchPos;
                        wrkPos += longestMatchLen;
						
						// if length table was used, write entry index and offset
						if (symbol >= 0x108)
                            bitwtr.Write(longestMatchLen - LENGTH_TABLE[((symbol - 0x108) << 1) + 1] - 4, LENGTH_TABLE[(symbol - 0x108) << 1]);
						
						// encode DISTANCE_TABLE
						for (tabPos = 0; tabPos < 8; tabPos++)
						{
							// maximum offset value that can be represented by using this table entry
							refLen = ((DISTANCE_TABLE[(tabPos << 1) + 1]) << 9) + (1 << (9 + DISTANCE_TABLE[tabPos << 1])) - 1;
							if (wndoff <= refLen) break;
						}
						
						// write code
						bitwtr.Write(tabPos, 3);
						bitwtr.Write(wndoff - (DISTANCE_TABLE[(tabPos << 1) + 1] << 9), DISTANCE_TABLE[tabPos << 1] + 9);
					}
				}
			}
			
			// close stream, return contents
			return bitwtr.Close();
		}
		
		public static bool Decompress(byte[] src, byte[] dst)
		{
			NoxLz nxz = new NoxLz();
			nxz.BuildInitialAlphabet();
			// initialize required variables
			nxz.bitrdr = new BitReader(src);
			nxz.SymbolCounter = new int[SYMBOLS];
			
			return nxz.DecompressImpl(dst, dst.Length);
		}
		
		public static byte[] Compress(byte[] src)
		{
			NoxLz nxz = new NoxLz();
			nxz.BuildInitialAlphabet();
			// init variables
			nxz.bitwtr = new BitWriter(WINDOW_SIZE);
			nxz.SymbolCounter = new int[SYMBOLS];
			
			return nxz.CompressImpl(src, src.Length);
		}
	}
}
