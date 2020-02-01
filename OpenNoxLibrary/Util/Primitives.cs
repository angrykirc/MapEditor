using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNoxLibrary.Util
{
    /// <summary>
    /// Represents a 24-bit color value with 8-bit-wide R/G/B channels
    /// </summary>
    public struct Color24
    {
        public byte R;
        public byte G;
        public byte B;

        public uint UInt32
        {
            get
            {
                return (uint)(R << 16 | G << 8 | B);
            }
        }

        public static Color24 WHITE { get { return new Color24(255, 255, 255); } }
        public static Color24 BLACK { get { return new Color24(0, 0, 0); } }

        public Color24(uint color)
        {
            R = (byte)((color >> 16) & 0xFF);
            G = (byte)((color >> 8) & 0xFF);
            B = (byte)(color & 0xFF);
        }

        public Color24(byte R, byte G, byte B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
        }

        public override bool Equals(object obj)
        {
            if (this.GetType().Equals(obj.GetType()))
            {
                var col = (Color24) obj;
                return col.R == R && col.G == G && col.B == B;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return B | G << 8 | R << 16;
        }
    }

    public struct PointF32 : IComparable<PointF32>
    {
        public float X;
        public float Y;

        public static PointF32 Empty { get { return new PointF32(0, 0); } }

        public bool IsEmpty { get { return X == 0 && Y == 0; } }

        public PointF32(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public override bool Equals(object obj)
        {
            if (this.GetType().Equals(obj.GetType()))
            {
                var c = (PointF32)obj;
                return c.X == X && c.Y == Y;
            }
            else
                return false;
        }

        public int CompareTo(PointF32 other)
        {
            if (other.X != X)
                return other.X.CompareTo(X);
            else
                return other.Y.CompareTo(Y);
        }
    }

    public struct PointS32 : IComparable<PointS32>
    {
        public int X;
        public int Y;

        public static PointS32 Empty { get { return new PointS32(0, 0); } }

        public bool IsEmpty { get { return X == 0 && Y == 0; } }

        public PointS32(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override bool Equals(object obj)
        {
            if (this.GetType().Equals(obj.GetType()))
            {
                var c = (PointS32)obj;
                return c.X == X && c.Y == Y;
            }
            else
                return false;
        }

        public int CompareTo(PointS32 other)
        {
            if (other.X != X)
                return other.X.CompareTo(X);
            else
                return other.Y.CompareTo(Y);
        }
    }

    public struct PointU8 : IComparable<PointU8>
    {
        public byte X;
        public byte Y;

        public static PointU8 Empty { get { return new PointU8(0, 0); } }

        public PointU8(byte x, byte y)
        {
            this.X = x;
            this.Y = y;
        }

        public override bool Equals(object obj)
        {
            if (this.GetType().Equals(obj.GetType()))
            {
                var c = (PointU8)obj;
                return c.X == X && c.Y == Y;
            }
            else
                return false;
        }

        public int CompareTo(PointU8 other)
        {
            if (other.X != X)
                return other.X.CompareTo(X);
            else
                return other.Y.CompareTo(Y);
        }
    }
}
