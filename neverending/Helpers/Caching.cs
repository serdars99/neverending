using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using neverending.Models;
using System.Collections;

namespace neverending.Helpers
{
    public enum cnum
    {
        ActiveStory, ChosenEntries, StoryTags, LastWinningEntry, MemberDetail, EntriesOnVote, Story, TagSearch, MemberEntries, EntriesOnVoteBefore, StoryMembers, MemberStories
    }
    public static class Cacher
    {
        public static string getckey(cnum keycode)
        {
            Hashtable ht = new Hashtable();
            ht.Add(cnum.ActiveStory, "ActiveStory");
            ht.Add(cnum.ChosenEntries, "ChosenEntries{0}_{1}");
            ht.Add(cnum.StoryTags, "StoryTags{0}");
            ht.Add(cnum.LastWinningEntry, "LastWinningEntry{0}");
            ht.Add(cnum.MemberDetail, "MemberDetail{0}");
            ht.Add(cnum.EntriesOnVote, "EntriesOnVote{0}");
            ht.Add(cnum.Story, "Story_{0}");
            ht.Add(cnum.TagSearch, "TagSearch_{0}_{1}");
            ht.Add(cnum.MemberEntries, "MemberEntries{0}_{1}");
            ht.Add(cnum.EntriesOnVoteBefore, "EntriesOnVoteBefore{0}_{1}");
            ht.Add(cnum.StoryMembers, "StoryMembers{0}");
            ht.Add(cnum.MemberStories, "MemberStories{0}");
            
            
            return ht[keycode].ToString();
        }
        public static object Get(string key)
        {
            return HttpContext.Current.Cache.Get(key);
        }
        public static object Get(string key, string region)
        {
            return HttpContext.Current.Cache.Get(region + "#" + key);
        }
        public static T GetCachedData<T>(System.Func<object> func, string cacheKey, int timeInMinutes)
        {
            if (HttpContext.Current.Request.Url.ToString().IndexOf("fc=ok") != -1 && !Common.SkipCacheClear(cacheKey))
            {
                HttpContext.Current.Cache.Remove(cacheKey);
                Common.AddCacheClearKey(cacheKey);
            }
            object cacheResult = HttpContext.Current.Cache.Get(cacheKey);
            //#if DEBUG
            //            cacheResult = null;
            //#endif

            if (cacheResult == null)
            {
                cacheResult = (T)func();
                if (cacheResult != null)
                    HttpContext.Current.Cache.Add(cacheKey, cacheResult, null, DateTime.UtcNow.AddMinutes(timeInMinutes), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
            }
            return (T)cacheResult;
        }
        public static T Get<T>(string key)
        {
            if (HttpContext.Current.Request.Url.ToString().IndexOf("fc=ok") != -1)
                HttpContext.Current.Cache.Remove(key);
            return (T)HttpContext.Current.Cache.Get(key);
        }

        public static T Get<T>(string key, string regionName)
        {
            if (HttpContext.Current.Request.Url.ToString().IndexOf("fc=ok") != -1)
                HttpContext.Current.Cache.Remove(regionName + "#" + key);
            return (T)HttpContext.Current.Cache.Get(regionName + "#" + key);
        }

        //public void Add(string key, object value)
        //{
        //    throw new NotImplementedException();
        //}

        public static void Add(string key, object value, TimeSpan time)
        {
            //insert overwrites
            HttpContext.Current.Cache.Insert(key, value, null, DateTime.UtcNow.Add(time), TimeSpan.Zero);
        }

        //public void Add(string key, object value, string region)
        //{
        //    throw new NotImplementedException();
        //}

        public static void Add(string key, object value, string region, TimeSpan time)
        {
            HttpContext.Current.Cache.Add(region + "#" + key, value, null, DateTime.UtcNow.Add(time), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);
        }

        public static void Remove(string key)
        {
            HttpContext.Current.Cache.Remove(key);
        }

        public static void Remove(string key, string regionName)
        {
            HttpContext.Current.Cache.Remove(regionName + "#" + key);
        }
    }
}