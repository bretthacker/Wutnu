using System;
using System.Linq;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Web;

using System.Collections.Concurrent;
using Wutnu.Data;
using Wutnu.Common;
using Wutnu.Data.Models;

namespace Wutnu.Data
{
    public class WutCache: IDisposable
    {
        private IDatabase urlcache;
        private IDatabase usercache;
        private ConnectionMultiplexer conn;
        private WutNuContext wutContext;

        public WutCache(WutNuContext model)
        {
            var wb = new HttpContextWrapper(HttpContext.Current);
            wutContext = model;

            conn = Cache.Connection;
            urlcache = conn.GetDatabase(Cache.RedisUrlDBNum);
            usercache = conn.GetDatabase(Cache.RedisUserDBNum);

            if (!conn.IsConnected)
                Logging.WriteMessageToErrorLog("Redis is down", wutContext);
        }

        public UserPoco GetUserFromApiKey(string apiKey)
        {
            UserPoco res = null;

            //check local cache
            var user = Cache.UserColl.SingleOrDefault(a => a.Key == apiKey);

            if (user.Value == null)
            {
                //check Redis cache
                var userObj = TryGetRedisValue(usercache, apiKey);
                if (userObj.HasValue)
                {
                    res = JsonConvert.DeserializeObject<UserPoco>(userObj.ToString());
                    //if we're here, it wasn't in the local cache (server restarted)
                    SetLocalUser(res);
                }
                else
                {
                    //Go to SQL
                    var usr = wutContext.Users.SingleOrDefault(u => u.ApiKey == apiKey);
                    res = UserPoco.UserToUserPoco(usr);
                    //update local and remote cache
                    SetUser(res);
                }
            }
            else
            {
                res = JsonConvert.DeserializeObject<UserPoco>(user.Value);
            }
            return res;
        }

        public WutLink GetUrl(string shortKey)
        {
            WutLink url = null;
            //check local cache
            var gid = Cache.UrlColl.SingleOrDefault(a => a.Key == shortKey);
            if (gid.Value==null)
            {
                //check Redis cache
                var gidObj = TryGetRedisValue(urlcache, shortKey);
                if (gidObj.HasValue)
                {
                    url = JsonConvert.DeserializeObject<WutLink>(gidObj.ToString());
                    //if we're here, it wasn't in the local cache (server restarted)
                    SetLocalUrl(url);
                }
                else
                {
                    //Go to SQL
                    url = wutContext.WutLinks.SingleOrDefault(g => g.ShortUrl == shortKey);
                    //update local and remote cache
                    SetUrl(url);
                }
            }
            else
            {
                url = JsonConvert.DeserializeObject<WutLink>(gid.Value);
            }
            return url;
        }

        public void SetLocalUser(UserPoco user)
        {
            Cache.UserColl.AddOrUpdate(user.ApiKey, JsonConvert.SerializeObject(user));
        }

        public bool SetUser(UserPoco user)
        {
            try
            {
                if (conn.IsConnected)
                {
                    usercache.StringSet(user.ApiKey, JsonConvert.SerializeObject(user));
                }

                SetLocalUser(user);

                return true;
            }
            catch (Exception ex)
            {
                Logging.WriteDebugInfoToErrorLog("Error updating or adding user cache", ex);
                return false;
            }
        }

        public bool RemoveUser(UserPoco user)
        {
            try
            {
                if (conn.IsConnected)
                    usercache.KeyDelete(user.ApiKey);

                string sOut;
                Cache.UserColl.TryRemove(user.ApiKey, out sOut);

                return true;
            }
            catch (Exception ex)
            {
                Logging.WriteDebugInfoToErrorLog("Error deleting user from cache", ex);
                return false;
            }
        }


        public void SetLocalUrl(WutLink url)
        {
            Cache.UrlColl.AddOrUpdate(url.ShortUrl, JsonConvert.SerializeObject(url));
        }

        public bool SetUrl(WutLink url)
        {
            try
            {
                if (conn.IsConnected)
                {
                    urlcache.StringSet(url.ShortUrl, JsonConvert.SerializeObject(url));
                }

                SetLocalUrl(url);

                return true;
            }
            catch (Exception ex)
            {
                Logging.WriteDebugInfoToErrorLog("Error updating or adding url cache", ex);
                return false;
            }
        }

        public bool RemoveUrl(WutLink url)
        {
            try
            {
                if (conn.IsConnected)
                    urlcache.KeyDelete(url.ShortUrl);

                string sOut;
                Cache.UrlColl.TryRemove(url.ShortUrl, out sOut);

                return true;
            }
            catch (Exception ex)
            {
                Logging.WriteDebugInfoToErrorLog("Error deleting url from cache", ex);
                return false;
            }
        }

        private RedisValue TryGetRedisValue(IDatabase db, string key)
        {
            RedisValue val = new RedisValue();
            if (conn.IsConnected)
            {
                val = db.StringGet(key);
            }
            return val;
        }

        public void Dispose()
        {
            
        }
    }

    public static class Cache
    {
        public static string RedisConnectionString { get; set; }
        public static ConcurrentDictionary<string, string> UrlColl;
        public static ConcurrentDictionary<string, string> UserColl;

        public static int RedisUrlDBNum { get; set; }
        public static int RedisUserDBNum { get; set; }
        
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            UrlColl = new ConcurrentDictionary<string, string>();
            UserColl = new ConcurrentDictionary<string, string>();
            
            return ConnectionMultiplexer.Connect(RedisConnectionString);
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }
    static class ExtensionMethods
    {
        // Either Add or overwrite
        public static void AddOrUpdate<K, V>(this ConcurrentDictionary<K, V> dictionary, K key, V value)
        {
            dictionary.AddOrUpdate(key, value, (oldkey, oldvalue) => value);
        }
    }
}
