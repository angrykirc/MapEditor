using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenNoxLibrary.Util;

namespace OpenNoxLibrary.Encryption
{
    // Such efforts on optimization are not needed for now -- the largest encrypted file, namely thing.bin, is never larger than 2 Mb's
    /*
    public class NoxBinaryReader 
    {
        Stream BaseStream;

        byte[] Buffer;
        uint BufferSize;
        uint BufferPos;

        uint BufferAvailable
        {
            get { return BufferSize - BufferPos; }
        }

        uint CurrentPosition;
        uint BlockPosition;
        uint Length;

        
        public uint AlignTo8B()
        {
            return (uint)(8 - BaseStream.Position % 8) % 8;
        }
    }
     */

    /// <summary>
    /// NoxBinaryReader functions as a regular System.IO.BinaryReader except that it decrypts the stream automatically
    /// </summary>
    public class NoxBinaryReader : BinaryReader
    {
        public NoxBinaryReader(Stream stream, CryptApi.NoxCryptFormat format)
            : base(CryptApi.DecryptStream(stream, format))
        {
        }

        public NoxBinaryReader(Stream stream)
            : this(stream, CryptApi.NoxCryptFormat.NONE)
        {
        }

        // Nox usually stores string lengths as bytes, not ints, so override this method
        public override string ReadString()
        {
            return ReadString(Type.GetType("System.Byte"));
        }

        public string ReadScriptEventString()
        {
            string result = "";
            if (ReadInt16() <= 1)
            {
                int len = ReadInt32();
                byte[] tmp = ReadBytes(len);
                if (tmp.Length > 0)
                    result = Encoding.ASCII.GetString(tmp);
            }
            ReadInt32();
            return result;
        }

        public string ReadString(Type lengthType)
        {
            string str;

            if (lengthType.Equals(Type.GetType("System.Byte")))
                str = ReadString(ReadByte());
            else if (lengthType.Equals(Type.GetType("System.Int16")))
                str = ReadString(ReadInt16());
            else if (lengthType.Equals(Type.GetType("System.Int32")))
                str = ReadString(ReadInt32());
            else
                str = null;//throw exception instead?

            return str;
        }

        //read the specified number of bytes as a string
        //and throw away anything after the first null encountered
        public string ReadString(int bytes)
        {
            string str = new string(ReadChars(bytes));

            if (str.IndexOf('\0') >= 0)
                str = str.Substring(0, str.IndexOf('\0'));

            return str;
        }

        public string ReadUnicodeString()
        {
            //read the first byte as the string's length
            return Encoding.Unicode.GetString(ReadBytes(ReadByte() * 2));
        }

        public Color24 ReadColor()
        {
            return new Color24(ReadByte(), ReadByte(), ReadByte());
        }

        /// <summary>
        /// Skips to the next qword (8 byte) boundary and returns the number of bytes skipped
        /// does not skip any bytes if already on a qword boundary.
        /// </summary>
        public int SkipToNextBoundary()
        {
            int skip = (int)(8 - BaseStream.Position % 8) % 8;
            BaseStream.Seek(skip, SeekOrigin.Current);
            return skip;
        }

    }

    public class NoxBinaryWriter : BinaryWriter
    {
        protected CryptApi.NoxCryptFormat format;

        public NoxBinaryWriter(Stream stream, CryptApi.NoxCryptFormat format)
            : base(stream)
        {
            this.format = format;
        }

        public override void Close()
        {
            // Encrypt entire stream, if needed
            if (format != CryptApi.NoxCryptFormat.NONE)
            {
                SkipToNextBoundary(); // Add padding bytes, so total length is divisible by 8
                int length = (int)BaseStream.Position;
                byte[] buffer = new byte[length];

                BaseStream.Seek(0, SeekOrigin.Begin);
                BaseStream.Read(buffer, 0, length);

                buffer = CryptApi.NoxEncrypt(buffer, format);

                BaseStream.Seek(0, SeekOrigin.Begin);
                Write(buffer);
            }
            base.Close();
        }

        public void WriteScriptEvent(string str)
        {
            Write((short)1);
            Write(str.Length);
            Write(Encoding.ASCII.GetBytes(str));
            Write((int)0);
        }

        public int SkipToNextBoundary()
        {
            int skip = (int)(8 - BaseStream.Position % 8) % 8;//0 iff BaseStream%8 == 0
            BaseStream.Seek(skip, SeekOrigin.Current);
            return skip;
        }

        public override void Write(string str)
        {
            Write((byte)str.Length);
            Write(Encoding.ASCII.GetBytes(str));
        }

        public void WriteColor(Color24 color)
        {
            Write(color.R);
            Write(color.G);
            Write(color.B);
        }
    }
}
