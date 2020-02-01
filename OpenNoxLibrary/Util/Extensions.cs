using System;
using System.IO;
using System.Text;

namespace OpenNoxLibrary.Util
{
    public static class Extensions
    {
        public static string ReadUnprefixedString(this BinaryReader rdr, int bytes)
        {
            string str = new string(rdr.ReadChars(bytes));

            if (str.IndexOf('\0') >= 0)
                str = str.Substring(0, str.IndexOf('\0'));

            return str;
        }
    }
}
