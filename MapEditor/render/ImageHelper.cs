using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace MapEditor.render
{
    public static class ImageHelper
    {
        public static Bitmap SmartCrop(Bitmap image, int size, int padding)
        {
            try
            {
                var img = ResizeImage(image, new Size(size, size), true);
                var backupimg = ResizeImage(image, new Size(size, size), true);
                image.Dispose();
                var r = SquareEdges(FindEdges(img.Width, img.Height, ImageToPixels(img)));
                img.Dispose();
                var final = CropAtRect(backupimg, r, padding);
                backupimg.Dispose();
                return final;
            }
            catch { return null; }
        }

        public static Image CreateNonIndexedImage(string path)
        {
            // Pulled from stackoverflow.com to resolve deleting image after .FromFile()
            using (var sourceImage = Image.FromFile(path))
            {
                var targetImage = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);
                using (var canvas = Graphics.FromImage(targetImage))
                {
                    //canvas.DrawImageUnscaled(sourceImage, 0, 0);
                    canvas.DrawImage(sourceImage, 0, 0);
                }
                return targetImage;
            }
        }
        public static Bitmap ResizeImage(Bitmap image, Size size, bool preserveAspectRatio)
        {
            if (image.Size == size) { return new Bitmap(image); }
            if ((image.Size.Width < 1) || (image.Size.Height < 1)) { return null; }
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                int originalWidth = image.Width;
                int originalHeight = image.Height;
                float percentWidth = (float)size.Width / originalWidth;
                float percentHeight = (float)size.Height / originalHeight;
                float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                newWidth = (int)(originalWidth * percent);
                newHeight = (int)(originalHeight * percent);
            }
            else
            {
                newWidth = size.Width;
                newHeight = size.Height;
            }
            Bitmap newImage = new Bitmap(newWidth, newHeight, image.PixelFormat);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.SmoothingMode = SmoothingMode.HighQuality;
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphicsHandle.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight));
            }
            return newImage;
        }

        /// <summary>
        /// Breaks an image into pixels
        /// </summary>
        /// <param name="img"></param>
        /// <returns>Returns a 3D array of pixel colors</returns>
        public static int[,] ImageToPixels(Bitmap img)
        {
            var data = img.LockBits(new Rectangle(Point.Empty, img.Size), ImageLockMode.ReadWrite, img.PixelFormat);
            var pixelSize = data.PixelFormat == PixelFormat.Format32bppArgb ? 4 : 3; // only works with 32 or 24 pixel-size bitmap!
            var padding = data.Stride - (data.Width * pixelSize);
            var bytes = new byte[data.Height * data.Stride];

            // copy the bytes from bitmap to array
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            var index = 0;
            int[,] pixels = new int[data.Width, data.Height];

            for (var y = 0; y < data.Height; y++)
            {
                for (var x = 0; x < data.Width; x++)
                {
                    Color pixelColor = Color.FromArgb(
                        pixelSize == 3 ? 255 : bytes[index + 3], // A component if present
                        bytes[index + 2], // R component
                        bytes[index + 1], // G component
                        bytes[index]      // B component
                        );

                    pixels[x, y] = pixelColor.R + pixelColor.G + pixelColor.B;  // Combine since only detecting for black (0, 0, 0)
                    index += pixelSize;
                }
                index += padding;
            }
            // copy back the bytes from array to the bitmap
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);

            return pixels;
        }

        /// <summary>
        /// Takes the pixels of an image and finds all 4 edges
        /// </summary>
        /// <param name="width">Width of image</param>
        /// <param name="height">Height of image</param>
        /// <param name="pixels">3D array of pixel colors</param>
        /// <returns>Returns a 4 count array: Point[Left, Top, Right, Bottom]; 0 count if failed</returns>
        public static Point[] FindEdges(int width, int height, int[,] pixels)
        {
            Point[] pts = new Point[4];

            //Left: // Scan top [->] bottom; left -> right
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pixels[x, y] > 10)
                    {
                        pts[0] = new Point(x, y);
                        goto Top;   // Not ideal but simple and effective way to break out of nested loop
                    }
                }
            }
            Top: // Scan left [->] right; top -> bottom
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (pixels[x, y] > 10)
                    {
                        pts[1] = new Point(x, y);
                        goto Right;
                    }
                }
            }
            Right: // Scan top [->] bottom; right -> left
            for (int x = width - 1; x >= 0; x--)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pixels[x, y] > 10)
                    {
                        pts[2] = new Point(x, y);
                        goto Bottom;
                    }
                }
            }
            Bottom: // Scan left [->] right; bottom -> top
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    if (pixels[x, y] > 10)
                    {
                        pts[3] = new Point(x, y);
                        return pts;
                    }
                }
            }
            return new Point[0];
        }

        /// <summary>
        /// Form a rectangle at the intersections of the edges
        /// </summary>
        /// <param name="edges">Takes 4 points</param>
        /// <returns>A fat trimmed rectangle</returns>
        public static Rectangle SquareEdges(Point[] edges)
        {
            Point a = new Point(edges[0].X, edges[1].Y); // Left-X, Top-Y
            Point b = new Point(edges[2].X, edges[3].Y); // Right-X, Bottom-Y

            return new Rectangle(a.X, a.Y, b.X - a.X, b.Y - a.Y);
        }

        /// <summary>
        /// Crops an image based on a rectangle
        /// </summary>
        /// <param name="b">Original image</param>
        /// <param name="r">Crop region</param>
        /// <param name="buffer">Border buffer, 0 touches the edges</param>
        /// <returns>Cropped image</returns>
        public static Bitmap CropAtRect(Image b, Rectangle r, int buffer)
        {
            Bitmap nb = new Bitmap(r.Width + buffer * 2, r.Height + buffer * 2);
            nb.SetResolution(b.HorizontalResolution, b.VerticalResolution);
            Graphics g = Graphics.FromImage(nb);
            g.DrawImage(b, -r.X + buffer, -r.Y + buffer);
            return nb;
        }
    }
}
