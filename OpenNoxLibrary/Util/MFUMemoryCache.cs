using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNoxLibrary.Util
{
    /// <summary>
    /// Provides a very efficient memory cache following the "keep most frequently used objects" policy. Discards the most infrequently used objects.
    /// </summary>
    public class MFUMemoryCache<T>
    {
        CEntry[] cachedEntries;
        int cacheSizeLimit;
        int refCounterLimit;

        private class CEntry
        {
            public int Index = -1;
            public int AccessCounter = -1;
            public T Data = default(T);
        }

        /// <summary>
        /// Constructs a new cache instance, with specified entry limit.
        /// </summary>
        public MFUMemoryCache(int sizeLimit, int counterLimit = 1000)
        {
            cachedEntries = new CEntry[sizeLimit];
            refCounterLimit = counterLimit;
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
        /// Returns cached object by specified index, or null if not found.
        /// </summary>
        public T Fetch(int index)
        {
            T result = default(T);

            for (int i = 0; i < cacheSizeLimit; i++)
            {
                var entry = cachedEntries[i];
                if (entry.Index == index)
                {
                    if (entry.AccessCounter < refCounterLimit)
                        entry.AccessCounter += 2; // Two cache misses will eliminate newly added entry
                    result = entry.Data;
                }
                else
                {
                    if (entry.AccessCounter > 0) // Don't go into sub-zero area
                        entry.AccessCounter--;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Adds another object to the cache.
        /// </summary>
        public void Add(int index, T data)
        {
            int now = int.MaxValue;
            int oldest = 0;

            // Find the oldest [or unused] cache element
            for (int i = 0; i < cacheSizeLimit; i++)
            {
                if (now > cachedEntries[i].AccessCounter)
                {
                    now = cachedEntries[i].AccessCounter;
                    oldest = i;
                }
            }

            cachedEntries[oldest].AccessCounter = 0;
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
                    cachedEntries[i].AccessCounter = -1;
                    cachedEntries[i].Index = -1;
                    cachedEntries[i].Data = default(T);
                }
            }
        }
    }
}
