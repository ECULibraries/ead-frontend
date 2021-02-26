using System;
using System.Web;

namespace ead_frontend.Services
{
    public static class CacheService
    {
        //store on server for fast and better access
        public static void Add<T>(T o, string key, DateTime expiration)
        {
            HttpContext.Current.Cache.Insert(key, o, null, expiration, System.Web.Caching.Cache.NoSlidingExpiration);
        }

        //remove it from the server
        public static void Clear(string key)
        {
            HttpContext.Current.Cache.Remove(key);
        }

        //verify data is on the server
        public static bool Exists(string key)
        {
            return HttpContext.Current.Cache[key] != null;
        }

        //using data the way that you would have used database
        public static bool Get<T>(string key, out T value)
        {
            try
            {
                if (!Exists(key))
                {
                    value = default(T);
                    return false;
                }

                value = (T)HttpContext.Current.Cache[key];
            }
            catch
            {
                value = default(T);
                return false;
            }

            return true;
        }
    }
}