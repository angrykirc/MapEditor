using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

using OpenNoxLibrary.Files;
using OpenNoxLibrary.Files.Media;
using OpenNoxLibrary.Util;

namespace MapEditor.videobag
{
	/// <summary>
	/// Layer2 cache for Bitmap objects
	/// </summary>
	public class VideoBagCachedProvider
	{
		protected readonly VideoBagCached _VideoBag;

        protected MRUMemoryCache<CachedBitmap> _NormalBitmapCache;
        protected MRUMemoryCache<Bitmap> _TileBitmapCache;

		public uint[] Type46Colors = { 0x00FF0000, 0x00FF0000, 0x00FF0000, 0x00FF0000, 0x00FF0000, 0x00FF0000 };

        protected static int CalculateBitmapHash(int index, int color1, int color2, int color3, int color4)
        {
            int result = 37;

            result *= 397;
            result += index;

            result *= 397;
            result += color1;

            result *= 397;
            result += color2;

            result *= 397;
            result += color3;

            result *= 397;
            result += color4;

            return result;
        }

		protected class CachedBitmap : IEquatable<CachedBitmap>
		{
			public readonly int Index;
			public Bitmap BitCaps;
			public uint[] Colors;
            public int OffX, OffY;

            public CachedBitmap(int index, Bitmap result, int offX, int offY)
            {
                this.Index = index;
                this.BitCaps = result;
                this.Colors = null;
                this.OffX = offX;
                this.OffY = offY;
            }
			
			public bool Equals(CachedBitmap other)
			{
				if (other.Index != Index) return false;
                if (other.OffX != OffX || other.OffY != OffY) return false;

                if (other.Colors != null && Colors != null)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (other.Colors[i] != Colors[i])
                            return false;
                    }
                }
				
				return true;
			}

            public override int GetHashCode()
            {
                int col1 = 0, col2 = 0, col3 = 0, col4 = 0;
                if (Colors != null)
                {
                    col1 = (int)Colors[0];
                    col2 = (int)Colors[1];
                    col3 = (int)Colors[2];
                    col4 = (int)Colors[3];
                }
                return CalculateBitmapHash(Index, col1, col2, col3, col4);
            }
		}
		
		public VideoBagCachedProvider(string path)
		{
            int TileCacheSize = 300;
            int ObjectCacheSize = 300;

			_VideoBag = new VideoBagCached(path + ".idx", path + ".bag");
            _NormalBitmapCache = new MRUMemoryCache<CachedBitmap>(ObjectCacheSize);
            _TileBitmapCache = new MRUMemoryCache<Bitmap>(TileCacheSize);
		}
		
		protected unsafe CachedBitmap ReadBitmap(int index, int edgeTile = 0)
		{
            var info = _VideoBag.PullFileIndex(index);

            _VideoBag.Type46Colors = Type46Colors;

            var data = _VideoBag.PullImageData(index, edgeTile);

            int stride = data.Width * 4 + ((data.Width * 4) % 4);
            Bitmap bitmap;
            fixed (void* ptr = data.ColorData)
            {
                IntPtr iptr = new IntPtr(ptr);
                bitmap = new Bitmap(data.Width, data.Height, stride, PixelFormat.Format32bppArgb, iptr);
            }

            var cbit = new CachedBitmap(index, bitmap, data.OffsX, data.OffsY);
            // DYNAMIC
            if (info.DataType == 4 || info.DataType == 6) 
                cbit.Colors = Type46Colors;
            // EDGE
            if (info.DataType == 1)
                cbit.OffX = edgeTile;
			
			return cbit;
		}
		
		/// <summary>
		/// Retrieves a dynamic-color Bitmap from videobag by its index, using cached approach.
		/// </summary>
		public Bitmap GetBitmapDynamic(int index, out int offX, out int offY, uint[] cols = null)
		{
            int col1 = 0, col2 = 0, col3 = 0, col4 = 0;
            if (cols != null)
            {
                col1 = (int)cols[0];
                col2 = (int)cols[1];
                col3 = (int)cols[2];
                col4 = (int)cols[3];
            }
            int hash = CalculateBitmapHash(index, col1, col2, col3, col4);
            var cached = _NormalBitmapCache.Fetch(hash);

            // Found one
            if (cached != null)
            {
                offX = cached.OffX;
                offY = cached.OffY;
                return cached.BitCaps;
            }

			// Else pull and cache a new entry
            Type46Colors = new uint[] { cols[0], cols[1], cols[2], cols[3] };
            cached = ReadBitmap(index, 0);
            _NormalBitmapCache.Add(hash, cached);

            offX = cached.OffX;
            offY = cached.OffY;
            return cached.BitCaps;
		}

        protected static int CalculateTileHash(MapInt.EditableNoxMap.Tile tile)
        {
            int result = 37;

            result *= 397;
            result += tile.TypeId;

            result *= 397;
            result += tile.Variation;

            foreach (var edge in tile.EdgeTiles)
            {
                result *= 397;
                result += edge.EdgeTileMat;

                result *= 397;
                result += edge.EdgeTileVar;

                result *= 397;
                result += edge.TypeId;

                result *= 397;
                result += (byte)edge.Dir;
            }
            return result;
        }

        public unsafe Bitmap CacheTile(MapInt.EditableNoxMap.Tile tile)
        {
            // The lowest part of the tile
            Bitmap surface = ReadBitmap((int)tile.TileDef.Variations[tile.Variation]).BitCaps;

            // Now for edges...
            var lockedBD = surface.LockBits(new Rectangle(Point.Empty, surface.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            uint* ptr = (uint*) lockedBD.Scan0;
            foreach (var edge in tile.EdgeTiles)
            {
                var edgeSprite = ThingDb.EdgeTiles[edge.TypeId].Variations[(byte)edge.Dir];
                var coverSprite = ThingDb.FloorTiles[edge.EdgeTileMat].Variations[edge.EdgeTileVar];
                uint[] data = _VideoBag.PullImageData(edgeSprite, coverSprite).ColorData;
                for (int i = 0; i < data.Length; i++)
                {
                    if ((data[i] & 0xFF000000) > 0) // Non-transparent
                        ptr[i] = data[i];
                }
            }
            surface.UnlockBits(lockedBD);

            _TileBitmapCache.Add(CalculateTileHash(tile), surface);

            return surface;
        }
	}
}
