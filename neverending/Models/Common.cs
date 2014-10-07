using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using neverending;
using System.Data.Entity;
using System.Linq.Expressions;
using Resources;
using System.Resources;
using System.Collections;
using System.Globalization;
using Newtonsoft.Json;
using neverending.Helpers;

namespace neverending.Models
{
    public static class Common
    {
        public static bool StoryStatusNeedsChange(Story story)
        {
            if (story.NextTickDate > DateTime.Now)
                return false;

            NextStepForStory(story);
            return true;
        }
        public static bool CheckStoryPrivacy(Story story)
        {
            if (!story.IsPrivateView)
                return true;

            Member member = Sessioner.GetSessionMember();
            if (member == null)
                return false;

            if (story.AdminID == member.MemberID)
                return true;

            List<int> smembers = GetStoryMembers(story.StoryID);
            if (!smembers.Contains(member.MemberID))
                return false;

            return true;
        }
        public static List<int> GetStoryMembers(int storyid)
        {
            string cachekey = string.Format(Cacher.getckey(cnum.StoryMembers), storyid);
            List<int> cached = Cacher.Get<List<int>>(cachekey);
            if (cached == null)
            {
                using (Model1 model = new Model1())
                {
                    cached = model.StoryMember.Where(p => p.StoryID == storyid && p.TypeID == 4).Select(p => p.MemberID).ToList();
                    if (cached == null)
                        cached = new List<int>();
                    Cacher.Add(cachekey, cached, new TimeSpan(1, 0, 0));
                    return cached;
                }
            }
            else
                return cached;
        }
        public static void NextStepForStory(Story parstory)
        {
            using (Model1 model = new Model1())
            {
                Story story = model.Story.Where(p => p.StoryID == parstory.StoryID).First();
                if (story.StateID == 1)
                {
                    var entries = model.Entry.Where(p => p.StoryID == story.StoryID && p.StatusID == 1 && p.CreateDate > story.LastEntryChosenDate);
                    if (entries.Count() > 0)
                    {//new entries found
                        DateTime fixedcheckdate;
                        if (story.NextTickDate.AddMinutes(story.SecondCounter) < DateTime.Now)
                            fixedcheckdate = DateTime.Now.AddMinutes(story.SecondCounter);
                        else
                            fixedcheckdate = story.NextTickDate.AddMinutes(story.SecondCounter);
                        story.NextTickDate = fixedcheckdate;
                        story.StateID = 2;
                    }
                    else
                    {//no new entries                            
                        DateTime fixedcheckdate;
                        if (story.NextTickDate.AddMinutes(story.FirstCounter) < DateTime.Now)
                            fixedcheckdate = DateTime.Now.AddMinutes(story.FirstCounter);
                        else
                            fixedcheckdate = story.NextTickDate.AddMinutes(story.FirstCounter);
                        story.NextTickDate = fixedcheckdate;
                    }
                }
                else if (story.StateID == 2)
                {
                    var entries = model.Entry.Where(p => p.StoryID == story.StoryID && p.StatusID == 1 && p.CreateDate > story.LastEntryChosenDate && p.VotesAvg > 0);
                    if (entries.Count() > 0)
                    {
                        //voting succesful
                        DateTime fixedcheckdate;
                        if (story.NextTickDate.AddMinutes(story.FirstCounter) < DateTime.Now)
                            fixedcheckdate = DateTime.Now.AddMinutes(story.FirstCounter);
                        else
                            fixedcheckdate = story.NextTickDate.AddMinutes(story.FirstCounter);
                        story.NextTickDate = fixedcheckdate;
                        story.StateID = 1;
                        entries.First().IsChosen = true;
                        int chosenid = entries.First().EntryID;
                        story.LastEntryChosenDate = DateTime.Now;
                        model.Entry.Where(p => p.StoryID == story.StoryID && p.EntryID != chosenid && !p.IsChosen.HasValue).ToList().ForEach(delegate(Entry item) { item.IsChosen = false; });
                        model.SaveChanges();
                        model.FixStoryPages(story.StoryID);
                        Cacher.Remove(string.Format(Cacher.getckey(cnum.ChosenEntries), story.StoryID, story.PageCount));
                    }
                    else
                    {//turn to counter 1
                        DateTime fixedcheckdate;
                        if (story.NextTickDate.AddMinutes(story.SecondCounter) < DateTime.Now)
                            fixedcheckdate = DateTime.Now.AddMinutes(story.SecondCounter);
                        else
                            fixedcheckdate = story.NextTickDate.AddMinutes(story.SecondCounter);
                        story.NextTickDate = fixedcheckdate;
                        story.StateID = 1;
                    }
                }
                //model.Story.ApplyCurrentValues(story);
                model.SaveChanges();
                //model.CalculateVotes();
                if (story.StatusID == 2)//active story
                    Cacher.Remove(Cacher.getckey(cnum.ActiveStory));
                Cacher.Remove(string.Format(Cacher.getckey(cnum.Story), story.StoryID));
                Cacher.Remove(string.Format(Cacher.getckey(cnum.EntriesOnVote), story.StoryID));
                Cacher.Remove(string.Format(Cacher.getckey(cnum.LastWinningEntry), story.StoryID));
            }
        }
        public static void NextStepAllStories(HttpContext context)
        {
            List<Story> stories;
            //using (Model1 model = new Model1())
            //{
            //    model.CalculateVotes();
            //    //stories = model.Story.Where(p => p.StatusID == 2 && DateTime.Now > p.NextTickDate).ToList();
            //}
            //foreach (Story story in stories)
            //{
            //    NextStepForStory(story);
            //}
            context.Response.Write("OK");
        }
        public static Entry GetLastWinningEntry(int storyid)
        {
            string cachekey = string.Format(Cacher.getckey(cnum.LastWinningEntry), storyid);
            Entry cached = Cacher.Get<Entry>(cachekey);
            if (cached == null)
            {
                using (Model1 model = new Model1())
                {
                    cached = model.Entry.Where(p => p.StatusID == 1 && p.IsChosen == true).OrderByDescending(p => p.CreateDate).FirstOrDefault();
                    string str = cached.Story.StoryName;
                    if (cached == null)
                        cached = new Entry();
                    Cacher.Add(cachekey, cached, new TimeSpan(1, 0, 0));
                    return cached;
                }
            }
            else
                return cached;
        }
        public static List<Entry> TagSearch(int storyid, string tag)
        {
            string cachekey = string.Format(Cacher.getckey(cnum.TagSearch), storyid, tag);
            List<Entry> cached = Cacher.Get<List<Entry>>(cachekey);
            if (cached == null)
            {
                using (Model1 model = new Model1())
                {
                    int tagid = model.Tag.Where(p => p.TagText == tag).First().TagID;
                    cached = model.Entry.Where(p => p.StoryID == storyid && p.EntryTag.Any(c => c.TagID == tagid)).ToList();
                    cached.ForEach(delegate(Entry item) { string s = item.Story.StoryName; string s2 = item.Member.MemberName; if (item.IsAnonymous) { item.Member.MemberName = ""; item.MemberID = 0; } });
                    Cacher.Add(cachekey, cached, new TimeSpan(1, 0, 0));
                    return cached;
                }
            }
            else
                return cached;
        }
        public static Story GetActiveStory()
        {
            string cachekey = Cacher.getckey(cnum.ActiveStory);
            Story cached = Cacher.Get<Story>(cachekey);
            if (cached == null)
            {
                using (Model1 model = new Model1())
                {
                    cached = model.Story.Where(p => p.StatusID == 2).FirstOrDefault();
                    if (cached != null)
                        Cacher.Add(cachekey, cached, new TimeSpan(1, 0, 0));
                    return cached;
                }
            }
            else
                return cached;
        }
        public static Story GetStoryDetail(int storyid)
        {
            string cachekey = string.Format(Cacher.getckey(cnum.Story), storyid);
            Story cached = Cacher.Get<Story>(cachekey);
            if (cached == null)
            {
                using (Model1 model = new Model1())
                {
                    cached = model.Story.Where(p => p.StatusID < 3 && p.StoryID == storyid).FirstOrDefault();
                    Cacher.Add(cachekey, cached, new TimeSpan(1, 0, 0));
                    return cached;
                }
            }
            else
                return cached;
        }
        public static List<Entry> GetEntries(int? storyid)
        {
            Story story = null;
            if (storyid.HasValue)
                story = GetStoryDetail(storyid.Value);
            else
                story = GetActiveStory();
            int? page = BHelper.GetRequestInt("page", null);
            if (!page.HasValue)
                page = story.PageCount;
            return GetEntriesForPage(story.StoryID, page.Value);
        }

        public static bool voteentry(Member member, int storyid, int entryid, bool islike)
        {
            using (Model1 model = new Model1())
            {
                if (model.ItemLike.Any(p => p.MemberID == member.MemberID && p.ItemID == entryid && p.TypeID == 1))
                    return false;
                ItemLike item = new ItemLike { MemberID = member.MemberID, ItemID = entryid, TypeID = 1, IsDislike = !islike, Weight = member.VoteMultiplier, CreateDate = DateTime.Now };
                Entry entry = model.Entry.Where(p => p.EntryID == entryid).First();
                if (islike)
                    entry.LikeCount++;
                else
                    entry.DislikeCount++;
                entry.VotesAvg = entry.LikeCount - entry.DislikeCount;
                entry.TotalVotes++;
                model.ItemLike.Add(item);
                model.SaveChanges();

                string cachekey = string.Format(Cacher.getckey(cnum.EntriesOnVote), storyid);
                Cacher.Remove(cachekey);
                return true;
            }
        }
        public static bool sendfeedback(Member member, int itemid, string iteminfo)
        {
            using (Model1 model = new Model1())
            {
                if (model.FeedBack.Any(p => p.MemberID == member.MemberID && p.ItemID == itemid && p.TypeID == 1 && p.TextInfo == iteminfo))
                    return false;
                FeedBack item = new FeedBack { MemberID = member.MemberID, ItemID = itemid, TypeID = 1, TextInfo = iteminfo, CreateDate = DateTime.Now, StatusID = 1 };
                model.FeedBack.Add(item);
                model.SaveChanges();
                return true;
            }
        }
        public static List<Entry> GetMemberEntriesForPage(int memberid, int page)
        {
            string cachekey = string.Format(Cacher.getckey(cnum.MemberEntries), memberid, page);
            int pagesize = 20;
            List<Entry> cached = Cacher.Get<List<Entry>>(cachekey);
            if (cached == null)
            {
                using (Model1 model = new Model1())
                {
                    var query = model.Entry.Where(p => p.MemberID == memberid && p.StatusID == 1 && p.Story.StatusID == 2 && !p.IsAnonymous).OrderByDescending(p => p.CreateDate);
                    cached = query.Skip((page - 1) * pagesize).Take(pagesize).ToList();
                    cached.Select(p => p.Story.StoryName).ToList();
                    //cached.Select(p => p.Member.MemberName).ToList();
                    Cacher.Add(cachekey, cached, new TimeSpan(1, 0, 0));
                    return cached;
                }
            }
            else
                return cached;
        }
        public static Member GetMemberDetailCached(int memberid)
        {
            string cachekey = string.Format(Cacher.getckey(cnum.MemberDetail), memberid);
            Member cached = Cacher.Get<Member>(cachekey);
            if (cached == null)
            {
                using (Model1 model = new Model1())
                {
                    cached = model.Member.Where(p => p.MemberID == memberid).FirstOrDefault();
                    //cached.Select(p => p.Member.MemberName).ToList();
                    Cacher.Add(cachekey, cached, new TimeSpan(1, 0, 0));
                    return cached;
                }
            }
            else
                return cached;
        }
        public static List<Story> GetMemberStoriesCached(int memberid)
        {
            string cachekey = string.Format(Cacher.getckey(cnum.MemberStories), memberid);
            List<Story> cached = Cacher.Get<List<Story>>(cachekey);
            if (cached == null)
            {
                using (Model1 model = new Model1())
                {
                    cached = model.Story.Where(p => p.StatusID < 3 && (p.AdminID == memberid || p.StoryMember.Any(s => s.MemberID == memberid && s.TypeID == 3))).Distinct().ToList();
                    //cached.Select(p => p.Member.MemberName).ToList();
                    Cacher.Add(cachekey, cached, new TimeSpan(1, 0, 0));
                    return cached;
                }
            }
            else
                return cached;
        }
        public static List<Entry> GetEntriesForPage(int storyid, int page)
        {
            string cachekey = string.Format(Cacher.getckey(cnum.ChosenEntries), storyid, page);
            List<Entry> cached = Cacher.Get<List<Entry>>(cachekey);
            if (cached == null)
            {
                using (Model1 model = new Model1())
                {
                    cached = model.Entry.Where(p => p.StoryID == storyid && p.PageNo == page && p.StatusID == 1).ToList();
                    cached.Select(p => p.Member.MemberName).ToList();
                    cached.Select(p => p.Story.StoryName).ToList();
                    cached.ForEach(delegate(Entry item) { if (item.IsAnonymous) { item.Member.MemberName = ""; item.MemberID = 0; } });
                    Cacher.Add(cachekey, cached, new TimeSpan(1, 0, 0));
                    return cached;
                }
            }
            else
                return cached;
        }
        public static List<Entry> GetEntriesOnVoteForStory(int? storyid)
        {
            int pagesize = 3; ;
            Story story = null;
            if (storyid.HasValue)
                story = GetStoryDetail(storyid.Value);
            else
                story = GetActiveStory();
            string cachekey = string.Format(Cacher.getckey(cnum.EntriesOnVote), story.StoryID);
            List<Entry> cached = Cacher.Get<List<Entry>>(cachekey);
            if (cached == null)
            {
                using (Model1 model = new Model1())
                {
                    var query = model.Entry.Where(p => p.StoryID == story.StoryID && p.StatusID == 1 && !p.IsChosen.HasValue).OrderByDescending(p => p.EntryID);
                    if (query.Count() > pagesize)
                        cached = query.Take(pagesize).ToList();
                    else
                        cached = query.ToList();
                    cached.ForEach(delegate(Entry item) { string s = item.Member.NickName; });
                    Cacher.Add(cachekey, cached, new TimeSpan(1, 0, 0));
                    return cached;
                }
            }
            else
                return cached;
        }
        public static List<Entry> GetEntriesOnVote(int storyid, int getbeforeid)
        {
            int pagesize = 3;
            string cachekey = string.Format(Cacher.getckey(cnum.EntriesOnVoteBefore), storyid, getbeforeid);
            List<Entry> cached = Cacher.Get<List<Entry>>(cachekey);
            if (cached == null)
            {
                using (Model1 model = new Model1())
                {
                    var query = model.Entry.Where(p => p.StoryID == storyid && p.StatusID == 1 && p.EntryID < getbeforeid && !p.IsChosen.HasValue);
                    if (query.Count() > pagesize)
                        cached = query.OrderByDescending(p => p.EntryID).ToList();
                    else
                        cached = query.ToList();

                    cached.ForEach(delegate(Entry item) { string s = item.Member.NickName; });
                    Cacher.Add(cachekey, cached, new TimeSpan(1, 0, 0));
                    return cached;
                }
            }
            else
                return cached;
        }
        public static string GetResFileName()
        {
            string guid = Guid.NewGuid().ToString().Substring(0, 10);
            string fname = string.Format("res-{0}.js", guid);

            Dictionary<string, string> dictres = new Dictionary<string, string>();
            using (Model1 model = new Model1())
            {
                Parameter parprog = model.Parameter.Where(p => p.Code == "Resources").SingleOrDefault();
                if (parprog.Value != string.Empty)
                    System.IO.File.Delete(HttpContext.Current.Server.MapPath("~\\gen\\") + parprog.Value);
                parprog.Value = fname;
                //model.Parameter.ApplyCurrentValues(parprog);
                model.SaveChanges();

                foreach (DictionaryEntry entry in res.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true))
                    dictres.Add(entry.Key.ToString(), entry.Value.ToString());

                //var list2 = dictres.Select(p => new { p., p.SportID, p.CompetitionID, p.Competition.CompName });
                //string json = JsonConvert.SerializeObject(new[] { JsonConvert.SerializeObject(dictres) });
                string json = JsonConvert.SerializeObject(dictres);
                System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath("~\\gen\\") + fname, json);
                Cacher.Add("res", fname, new TimeSpan(1, 0, 0));
                return fname;
            }
        }
        public static string GetUserIP()
        {
            string ip = "127.0.0.1";
            if (HttpContext.Current.Request.ServerVariables["HTTP_CLIENT_IP"] != null)
            {
                ip = HttpContext.Current.Request.ServerVariables["HTTP_CLIENT_IP"];
            }
            else if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            }
            else
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            if (ip.Length > 15)
                ip = ip.Substring(0, 15);
            return ip;
        }
        public static bool SkipCacheClear(string keyname)
        {
            if (HttpContext.Current.Cache["#check#" + keyname] != null)
                return true;
            else
                return false;
        }
        public static void AddCacheClearKey(string keyname)
        {
            HttpContext.Current.Cache.Insert("#check#" + keyname, 1, null, DateTime.Now.AddSeconds(10), TimeSpan.Zero);
        }
        public static bool IsKeyLocked(string keyname)
        {
            HttpApplicationState state = HttpContext.Current.Application;
            List<string> keys = (List<string>)state["lockedkeys"];
            if (keys.Contains(keyname))
                return true;
            else
                return false;
        }
        public static void LockKey(string keyname)
        {
            HttpApplicationState state = HttpContext.Current.Application;
            List<string> keys = (List<string>)state["lockedkeys"];
            state.Lock();
            if (keys == null)
                keys = new List<string>();

            if (!keys.Contains(keyname))
                keys.Add(keyname);

            state["lockedkeys"] = keys;
            state.UnLock();
        }
        public static string getefconn()
        {
            return HttpContext.Current.Application["efstring"] as string;
        }
        public static string getfacekey()
        {
            string facekey = "";
#if DEBUG
            facekey = "AppFaceIDTest";
#else
        facekey = "AppFaceID"; 
#endif
            return System.Configuration.ConfigurationManager.AppSettings[facekey];
        }
    }

}