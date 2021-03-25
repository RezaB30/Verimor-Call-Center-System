using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace WebHook
{
    public class WebHookEventCache
    {
        public static MemoryCache cache = MemoryCache.Default;
        public void AddEvent(string internalID, object Event)
        {
            cache.Remove(internalID);
            cache.Add(internalID, Event, DateTimeOffset.Now.AddSeconds(600));
        }
        public CacheItem GetEvent(string internalID)
        {
            CacheItem cacheItem = cache.GetCacheItem(internalID);
            return cacheItem;
        }
    }
}
