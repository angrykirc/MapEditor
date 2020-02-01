using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNoxLibrary.Util
{
    /// <summary>a
    /// Provides a very efficient fixed-size memory cache, that keeps the most used entries and discards the least used ones.
    /// </summary>
    public class MRUMemoryCache<T>
    {
        CEntry[] cachedEntries;
        int cacheSizeLimit;

        private class CEntry
        {
            public int Index = -1;
            public int LastAccessTime = -1; // We are using Environment.TickCount to track the time, this resets each 25 days but it's ok for most use cases
            public T Data = default(T);
        }

        /// <summary>
        /// Constructs a new cache instance, with specified entry limit.
        /// </summary>
        public MRUMemoryCache(int sizeLimit)
        {
            cachedEntries = new CEntry[sizeLimit];
            cacheSizeLimit = sizeLimit;
        }

        /// <summary>
        /// Checks if specified index was cached, returns true if so.
        /// </summary>
        public bool Lookup(int index)
        {
            for (int i = 0; i < cacheSizeLimit; i++)
            {
                if (cachedEntries[i].Index == index)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns byte array from cache by specified index, or null if not found.
        /// </summary>
        public T Fetch(int index)
        {
            for (int i = 0; i < cacheSizeLimit; i++)
            {
                var entry = cachedEntries[i];
                if (entry.Index == index)
                {
                    entry.LastAccessTime = Environment.TickCount;
                    return entry.Data;
                }
            }
            
            return default(T);
        }

        /// <summary>
        /// Adds another byte array to the cache.
        /// </summary>
        public void Add(int index, T data)
        {
            int now = Environment.TickCount;
            int oldest = 0;

            // Find the oldest [or unused] cache element
            for (int i = 0; i < cacheSizeLimit; i++)
            {
                if (now > cachedEntries[i].LastAccessTime)
                {
                    now = cachedEntries[i].LastAccessTime;
                    oldest = i;
                }
            }

            cachedEntries[oldest].LastAccessTime = Environment.TickCount;
            cachedEntries[oldest].Index = index;
            cachedEntries[oldest].Data = data;
        }

        /// <summary>
        /// Frees specified cache entry by its index.
        /// </summary>
        public void Free(int index)
        {
            for (int i = 0; i < cacheSizeLimit; i++)
            {
                if (cachedEntries[i].Index == index)
                {
                    cachedEntries[i].LastAccessTime = -1;
                    cachedEntries[i].Index = -1;
                    cachedEntries[i].Data = default(T);
                }
            }
        }
    }
}
