using System;

namespace OpenNoxLibrary.Util
{
    public static class CRC32
    {
        static uint[] table = new uint[256];

        static CRC32()
        {
            uint dwPolynomial = 0xEDB88320;//official PKZIP polynomial
            uint dwCrc;
            for (uint i = 0; i < 256; i++)
            {
                dwCrc = i;
                for (int j = 8; j > 0; j--)
                {
                    if ((dwCrc & 1) != 0)
                        dwCrc = (dwCrc >> 1) ^ dwPolynomial;
                    else
                        dwCrc >>= 1;
                }
                table[i] = dwCrc;
            }
        }

        public static uint Calculate(byte[] data, int offset, int length)
        {
            uint crc32 = 0xFFFFFFFF;
            for (int i = offset; i < length; i++)
                crc32 = (crc32 >> 8) ^ table[data[i] ^ (crc32 & 0xFF)];
            return (uint)~crc32;
        }

        public static uint Calculate(byte[] data)
        {
            return Calculate(data, 0, data.Length);
        }
    }
}
