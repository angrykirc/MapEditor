using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using OpenNoxLibrary.Util;

namespace MapEditor.MapInt
{
    public static class ExtensionColor
    {
        public static Color Interpolate(this Color source, Color target, double percent)
        {
            var r = (byte)(source.R + (target.R - source.R) * percent);
            var g = (byte)(source.G + (target.G - source.G) * percent);
            var b = (byte)(source.B + (target.B - source.B) * percent);

            return Color.FromArgb(255, r, g, b);
        }
    }

    public static class ExtensionPointF
    {
        public static Point ToPoint(this PointF source)
        {
            return new Point((int)source.X, (int)source.Y);
        }

        public static PointF32 ToLibPoint(this PointF source)
        {
            return new PointF32(source.X, source.Y);
        }
    }

    public static class ExtensionPoint
    {
        public static PointS32 ToLibPoint(this Point source)
        {
            return new PointS32(source.X, source.Y);
        }

        public static Point GetNearestTilePoint(this Point pt)
        {
            pt.Offset(0, -squareSize);
            return GetNearestWallPoint(pt);
        }

        public static Point GetCenterPoint(this Point pt, bool wallPt = false)
        {
            Point pti = GetNearestTilePoint(pt);
            int x = (pti.X * squareSize);
            int y = (pti.Y * squareSize) + squareSize / 2;

            if (!wallPt)
                return new Point(x + squareSize / 2, y + (3 / 2) * squareSize);
            else
                return new Point((x + squareSize / 2) / squareSize, (y + (3 / 2) * squareSize) / squareSize);
        }

        public static Point GetNearestWallPoint(this Point pt, bool cart = false)
        {
            int sqSize = squareSize;
            if (cart) sqSize = 1;

            Point tl = new Point((pt.X / squareSize) * squareSize, (pt.Y / squareSize) * squareSize);
            if (tl.X / squareSize % 2 == tl.Y / squareSize % 2)
                return new Point(tl.X / sqSize, tl.Y / sqSize);
            else
            {
                Point left = new Point(tl.X, tl.Y + squareSize / 2);
                Point right = new Point(tl.X + squareSize, tl.Y + squareSize / 2);
                Point top = new Point(tl.X + squareSize / 2, tl.Y);
                Point bottom = new Point(tl.X + squareSize / 2, tl.Y + squareSize);
                Point closest = left;
                foreach (Point point in new Point[] { left, right, top, bottom })
                    if (point.DistanceSq(pt) < closest.DistanceSq(pt))
                        closest = point;

                if (closest == left)
                    return new Point(tl.X / sqSize - 1, tl.Y / sqSize);
                else if (closest == right)
                    return new Point(tl.X / sqSize + 1, tl.Y / sqSize);
                else if (closest == top)
                    return new Point(tl.X / sqSize, tl.Y / sqSize - 1);
                else
                    return new Point(tl.X / sqSize, tl.Y / sqSize + 1);
            }
        }

        public static double DistanceSq(this Point ptA, Point ptB)
        {
            double distance = Math.Pow(ptA.X - ptB.X, 2) + Math.Pow(ptA.Y - ptB.Y, 2);
            return distance;
        }

        public static double Distance(this Point ptA, Point ptB)
        {
            return Math.Sqrt(ptA.DistanceSq(ptB));
        }

        public static Point GetWallMapCoords(this Point wallCoords)
        {
            // Works with both walls and tiles; i.e. 10, 15 => 230, 345
            var x = wallCoords.X * squareSize;
            var y = wallCoords.Y * squareSize;
            return new Point(x, y);
        }

        const int squareSize = 23;
    }

    public static class ExtensionPointS32
    {
        public static PointS32 Rotate(this PointS32 point, PointS32 pivot, double angleDegree)
        {
            double angle = angleDegree * Math.PI / 180;
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            float dx = point.X - pivot.X;
            float dy = point.Y - pivot.Y;
            double x = cos * dx - sin * dy + pivot.X;
            double y = sin * dx + cos * dy + pivot.Y;

            var rotated = new PointS32((int)Math.Round(x), (int)Math.Round(y));
            return rotated;
        }

        public static PointS32 GetNearestTilePoint(this PointS32 pt)
        {
            pt.Y = pt.Y - squareSize;
            return GetNearestWallPoint(pt);
        }

        public static PointS32 GetCenterPoint(this PointS32 pt, bool wallPt = false)
        {
            PointS32 pti = pt.GetNearestTilePoint();
            int x = (pti.X * squareSize);
            int y = (pti.Y * squareSize) + squareSize / 2;

            if (!wallPt)
                return new PointS32(x + squareSize / 2, y + (3 / 2) * squareSize);
            else
                return new PointS32((x + squareSize / 2) / squareSize, (y + (3 / 2) * squareSize) / squareSize);
        }

        public static PointS32 GetNearestWallPoint(this PointS32 pt, bool cart = false)
        {
            int sqSize = squareSize;
            if (cart) sqSize = 1;

            PointS32 tl = new PointS32((pt.X / squareSize) * squareSize, (pt.Y / squareSize) * squareSize);
            if (tl.X / squareSize % 2 == tl.Y / squareSize % 2)
                return new PointS32(tl.X / sqSize, tl.Y / sqSize);
            else
            {
                PointS32 left = new PointS32(tl.X, tl.Y + squareSize / 2);
                PointS32 right = new PointS32(tl.X + squareSize, tl.Y + squareSize / 2);
                PointS32 top = new PointS32(tl.X + squareSize / 2, tl.Y);
                PointS32 bottom = new PointS32(tl.X + squareSize / 2, tl.Y + squareSize);
                PointS32 closest = left;
                foreach (PointS32 point in new PointS32[] { left, right, top, bottom })
                    if (point.DistanceSq(pt) < closest.DistanceSq(pt))
                        closest = point;

                if (closest.Equals(left))
                    return new PointS32(tl.X / sqSize - 1, tl.Y / sqSize);
                else if (closest.Equals(right))
                    return new PointS32(tl.X / sqSize + 1, tl.Y / sqSize);
                else if (closest.Equals(top))
                    return new PointS32(tl.X / sqSize, tl.Y / sqSize - 1);
                else
                    return new PointS32(tl.X / sqSize, tl.Y / sqSize + 1);
            }
        }

        public static double DistanceSq(this PointS32 ptA, PointS32 ptB)
        {
            double distance = Math.Pow(ptA.X - ptB.X, 2) + Math.Pow(ptA.Y - ptB.Y, 2);
            return distance;
        }

        public static double Distance(this PointS32 ptA, PointS32 ptB)
        {
            return Math.Sqrt(ptA.DistanceSq(ptB));
        }

        public static PointS32 GetWallMapCoords(this PointS32 wallCoords)
        {
            // Works with both walls and tiles; i.e. 10, 15 => 230, 345
            var x = wallCoords.X * squareSize;
            var y = wallCoords.Y * squareSize;
            return new PointS32(x, y);
        }

        const int squareSize = 23;
    }

    public static class ExtensionPointF32
    {
        public static PointF32 Rotate(this PointF32 point, PointF32 pivot, double angleDegree)
        {
            double angle = angleDegree * Math.PI / 180;
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            float dx = point.X - pivot.X;
            float dy = point.Y - pivot.Y;
            double x = cos * dx - sin * dy + pivot.X;
            double y = sin * dx + cos * dy + pivot.Y;

            var rotated = new PointF32((float)Math.Round(x), (float)Math.Round(y));
            return rotated;
        }
    }
}
