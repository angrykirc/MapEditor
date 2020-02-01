using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNoxLibrary.Util
{
    /// <summary>
    /// Utility class for cpu-cheap writing and reading value types from byte arrays.
    /// </summary>
    public static class FastBytes
    {
        public static uint GetUInt32(byte[] src, int off)
        {
            return (uint)src[off] | (uint)(src[off + 1] << 8) | (uint)(src[off + 2] << 16) | (uint)(src[off + 3] << 24);
        }

        public static uint GetUInt32(byte[] src, ref int off)
        {
            uint res = GetUInt32(src, off);
            off += 4;
            return res;
        }

        public static void SetUInt32(byte[] src, int off, uint val)
        {
            src[off] = (byte)(val & 0xFF);
            src[off + 1] = (byte)(val >> 8 & 0xFF);
            src[off + 2] = (byte)(val >> 16 & 0xFF);
            src[off + 3] = (byte)(val >> 24 & 0xFF);
        }

        public static void SetUInt32(byte[] src, ref int off, uint val)
        {
            SetUInt32(src, off, val);
            off += 4;
        }

        public static int GetInt32(byte[] src, int off)
        {
            return (int)src[off] | (int)(src[off + 1] << 8) | (int)(src[off + 2] << 16) | (int)(src[off + 3] << 24);
        }

        public static int GetInt32(byte[] src, ref int off)
        {
            int res = GetInt32(src, off);
            off += 4;
            return res;
        }

        public static void SetInt32(byte[] src, int off, int val)
        {
            src[off] = (byte)(val & 0xFF);
            src[off + 1] = (byte)(val >> 8 & 0xFF);
            src[off + 2] = (byte)(val >> 16 & 0xFF);
            src[off + 3] = (byte)(val >> 24 & 0xFF);
        }

        public static void SetInt32(byte[] src, ref int off, int val)
        {
            SetInt32(src, off, val);
            off += 4;
        }

        public static ushort GetUInt16(byte[] src, ref int off)
        {
            ushort res = (ushort)(src[off] | src[off + 1] << 8);
            off += 2;
            return res;
        }

        public static string GetAsciiString_BytePref(byte[] src, ref int off)
        {
            byte len = src[off];
            string result = "";
            for (int i = 0; i < len; i++)
                result += (char)src[off + i];

            off += len + 1;
            return result;
        }
    }
}
