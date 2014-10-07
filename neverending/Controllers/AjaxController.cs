using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using neverending.Models;
using neverending.Helpers;
using Facebook;
using Resources;
using Newtonsoft.Json;
using System.Drawing;
using TweetSharp;
using System.Configuration;
//using TweetSharp.Twitter.Service;

namespace neverending.Controllers
{
    public class AjaxController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult linkedincallback(string id, string name, string job)
        {
            using (Model1 model = new Model1())
            {
                Member member = Sessioner.GetSessionMember();
                member.LinkedinJob = string.Format("{0}~{1}~{2}", id, name, job);
                model.Member.Where(p => p.MemberID == member.MemberID).First().LinkedinJob = member.LinkedinJob;
                model.SaveChanges();
                Sessioner.SetSessionMember(member);
            }
            return Json("ok", JsonRequestBehavior.AllowGet);
        }
        public ActionResult twitcallback(string oauth_token, string oauth_verifier)
        {
            var requestToken = new OAuthRequestToken { Token = oauth_token };

            // Step 3 - Exchange the Request Token for an Access Token
            TwitterService service = new TwitterService(ConfigurationManager.AppSettings["twitkey"], ConfigurationManager.AppSettings["twitsecret"]);
            OAuthAccessToken accessToken = service.GetAccessToken(requestToken, oauth_verifier);

            // Step 4 - User authenticates using the Access Token
            service.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);
            TwitterUser user = service.VerifyCredentials(new VerifyCredentialsOptions { SkipStatus = true });
            string tokensmix = string.Format("{0}~{1}~{2}~{3}", oauth_token, oauth_verifier, user.Name, user.ScreenName);
            using (Model1 model = new Model1())
            {
                Member member = Sessioner.GetSessionMember();
                if (member == null)
                {
                    member = model.Member.Where(p => p.TwitterID == user.Id.ToString()).FirstOrDefault();
                    if (member == null)
                    {//new entry
                        member = new Member();
                        member.FirstName = user.Name.Split(' ')[0];
                        member.LastName = user.Name.Split(' ')[1];
                        member.TwitterID = user.Id.ToString();
                        member.TwitterTokens = tokensmix;
                        member.NickName = user.ScreenName;
                        member.Email = "";
                        member.FaceID = "";
                        member.FirstVisitDate = DateTime.Now;
                        member.FirstVisitIP = Common.GetUserIP();
                        member.LastVisitIP = Common.GetUserIP();
                        member.LastVisitDate = DateTime.Now;
                        member.VoteMultiplier = 1;
                        member.StatusID = 1;
                        model.Member.Add(member);
                    }
                    else
                    {//already connected update time
                        model.Member.Attach(member);
                        member.LastVisitIP = Common.GetUserIP();
                        member.LastVisitDate = DateTime.Now;
                    }
                }
                else
                {
                    model.Member.Attach(member);
                    member.TwitterTokens = tokensmix;
                    member.TwitterID = user.Id.ToString();
                    member.LastVisitIP = Common.GetUserIP();
                    member.LastVisitDate = DateTime.Now;
                }
                //model.Member.Where(p => p.MemberID == member.MemberID).First().TwitterTokens = member.TwitterTokens;
                model.SaveChanges();
                Sessioner.SetSessionMember(member);
            }
            //return Json(user.ScreenName, JsonRequestBehavior.AllowGet);
            return PartialView("/views/shared/closer.cshtml");
        }
        public ActionResult twitauth()
        {
            TwitterService service = new TwitterService(ConfigurationManager.AppSettings["twitkey"], ConfigurationManager.AppSettings["twitsecret"]);
            OAuthRequestToken requestToken = service.GetRequestToken("http://" + System.Web.HttpContext.Current.Request.Url.Host + "/ajax/twitcallback");
            //Uri uri = service.GetAuthorizationUri(requestToken);
            Uri uri = service.GetAuthenticationUrl(requestToken);
            //return new RedirectResult(uri.ToString(), false /*permanent*/);
            return Json(uri.ToString(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult FBLogin(string id, string email, string first_name, string last_name)
        {
            string result = "0";
            Member member = Sessioner.GetSessionMember();
            if (member != null)
                goto finish;
            using (Model1 model = new Model1())
            {
                member = model.Member.Where(p => p.FaceID == id).SingleOrDefault();
                if (member != null)
                {
                    member.LastVisitIP = Common.GetUserIP();
                    member.LastVisitDate = DateTime.Now;
                    //model.Member.ApplyCurrentValues(member);
                    result = "2";
                }
                else
                {
                    member = new Member();
                    member.FirstName = first_name;
                    member.LastName = last_name;
                    member.NickName = first_name + last_name;
                    member.Email = email;
                    member.FaceID = id;
                    member.FirstVisitDate = DateTime.Now;
                    member.FirstVisitIP = Common.GetUserIP();
                    member.LastVisitIP = Common.GetUserIP();
                    member.LastVisitDate = DateTime.Now;
                    member.VoteMultiplier = 1;
                    member.StatusID = 1;
                    model.Member.Add(member);
                    result = "1";
                }
                model.SaveChanges();
                Sessioner.SetSessionMember(member);
            }
        finish:
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult uploadfiles()
        {
            HttpPostedFileBase file1 = Request.Files[0];
            Image img1 = Image.FromStream(file1.InputStream);
            img1.Save("d:\\test1.jpg");

            return Json("jok", JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveTags(string tagsdata)
        {
            string result = "0";
            Member member = Sessioner.GetSessionMember();
            if (member == null)
                return Json(result, JsonRequestBehavior.AllowGet);
            dynamic jsoncoupon = JsonConvert.DeserializeObject(tagsdata);
            int entryid = int.Parse(jsoncoupon.entryid.Value);
            using (Model1 model = new Model1())
            {
                List<EntryTag> entrytags = model.EntryTag.Where(p => p.EntryID == entryid).ToList();
                entrytags.ForEach(delegate(EntryTag item) { model.EntryTag.Remove(item); });
                model.SaveChanges();


                Entry entry = model.Entry.Where(p => p.EntryID == entryid).First();
                if (entry.MemberID != member.MemberID && member.AdminRoleID != 1)
                    return Json(result, JsonRequestBehavior.AllowGet);
                int addedtagcount = 0;
                foreach (dynamic tag in jsoncoupon.tags)
                {
                    string tagtext = tag.tag;
                    int tagtype = int.Parse(tag.type.Value);
                    Tag dbtag = model.Tag.Where(p => p.TagText == tagtext).FirstOrDefault();
                    if (dbtag == null)
                    {
                        dbtag = new Tag { MemberID = member.MemberID, IsActive = true, TagText = tag.tag };
                        model.Tag.Add(dbtag);
                        model.SaveChanges();
                    }
                    if (!model.EntryTag.Any(p => p.EntryID == entryid && p.TagID == dbtag.TagID && p.TagTypeID == tagtype))
                    {
                        model.EntryTag.Add(new EntryTag { EntryID = entryid, TagID = dbtag.TagID, TagTypeID = tagtype });
                        model.SaveChanges();
                        addedtagcount++;
                    }
                }
                //if (addedtagcount > 0)
                //{
                entry.IsTagged = true;
                model.SaveChanges();
                //}

            }

            Cacher.Remove(string.Format(Cacher.getckey(cnum.StoryTags), jsoncoupon.storyid.Value));
            Cacher.Remove(string.Format(Cacher.getckey(cnum.LastWinningEntry), jsoncoupon.storyid.Value));
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Tags(int storyid)
        {
            string cachekey = string.Format(Cacher.getckey(cnum.StoryTags), storyid);
            List<taglist> cached = Cacher.Get<List<taglist>>(cachekey);
            if (cached == null)
            {
                using (Model1 model = new Model1())
                {
                    //Story story = model.Story.Where(p => p.StoryID == storyid).First();
                    cached = model.EntryTag.Where(p => p.Entry.StoryID == storyid).Select(p => new taglist { entrytagid = p.EntryTagID, entryid = 0, tag = p.Tag.TagText, tagtype = p.TagTypeID }).Distinct().ToList();
                    Cacher.Add(cachekey, cached, new TimeSpan(1, 0, 0));
                    ViewBag.tags = cached;
                }
            }
            else
                ViewBag.tags = cached;
            return PartialView();
        }
        public ActionResult checklastentry(int storyid)
        {
            string result = "0";
            Member member = Sessioner.GetSessionMember();
            if (member == null)
                return Json(result, JsonRequestBehavior.AllowGet);
            using (Model1 model = new Model1())
            {
                Story story = model.Story.Where(p => p.StoryID == storyid).First();
                Entry query = model.Entry.Where(p => p.StoryID == storyid && p.MemberID == member.MemberID && p.StatusID == 1 && p.CreateDate > story.LastEntryChosenDate).FirstOrDefault();
                if (query != null)
                    return Json(query.EntryText, JsonRequestBehavior.AllowGet);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getfollows()
        {
            string result = "0";
            Member member = Sessioner.GetSessionMember();
            if (member == null)
                return Json(result, JsonRequestBehavior.AllowGet);
            using (Model1 model = new Model1())
            {
                List<int> follows = model.Follow.Where(p => p.FollowerID == member.MemberID).Select(p => p.FollowedID).ToList();
                return Json(follows, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult switchfollow(int targetmemberid)
        {
            string result = "0";
            Member member = Sessioner.GetSessionMember();
            if (member == null)
                return Json(result, JsonRequestBehavior.AllowGet);
            using (Model1 model = new Model1())
            {
                Follow fw = model.Follow.Where(p => p.FollowerID == member.MemberID && p.FollowedID == targetmemberid).FirstOrDefault();
                if (fw == null)
                {
                    fw = new Follow { CreateDate = DateTime.Now, FollowerID = member.MemberID, FollowedID = targetmemberid };
                    model.Follow.Add(fw);
                    model.Member.Where(p => p.MemberID == targetmemberid).First().FollowerCount++;
                    model.SaveChanges();
                    result = "1";
                }
                else if (fw != null)
                {
                    model.Follow.Remove(fw);
                    model.Member.Where(p => p.MemberID == targetmemberid).First().FollowerCount--;
                    model.SaveChanges();
                    result = "-1";
                }

                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult admingettags(int entryid)
        {
            string result = "0";
            Member member = Sessioner.GetSessionMember();
            if (member == null)
                return Json(result, JsonRequestBehavior.AllowGet);
            using (Model1 model = new Model1())
            {
                var tags = model.EntryTag.Where(p => p.EntryID == entryid).Select(p => new { tagtext = p.Tag.TagText, tagtype = p.TagTypeID }).ToArray();
                return Json(tags, JsonRequestBehavior.AllowGet);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult voteentry(int storyid, int entryid, bool islike)
        {
            Member member = Sessioner.GetSessionMember();
            if (member == null)
                return Json(false, JsonRequestBehavior.AllowGet);

            var res = Common.voteentry(member, storyid, entryid, islike);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public ActionResult sendfeedback(int itemid, string iteminfo)
        {
            Member member = Sessioner.GetSessionMember();
            if (member == null)
                return Json(false, JsonRequestBehavior.AllowGet);

            var res = Common.sendfeedback(member, itemid, iteminfo);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getmembervotes()
        {
            Member member = Sessioner.GetSessionMember();
            if (member == null)
                return Json(false, JsonRequestBehavior.AllowGet);
            using (Model1 db = new Model1())
            {
                //                var query = db.ItemLike         // source
                //.Join(db.Entry,         // target
                //  il => il.ItemID,          // FK
                //  ent => ent.EntryID,   // PK
                //  (ent, il) => new { Entry = ent, ItemLike = il }) // project result
                //.Select(x => x.Entry);
                var query = db.ItemLike.Where(p => p.MemberID == member.MemberID && p.TypeID == 1).Select(p => new { p.ItemID, p.IsDislike }).ToList();
                return Json(query, JsonRequestBehavior.AllowGet);

            }

        }
        public ActionResult switchadmin(int memberid, bool makeadmin)
        {
            Member member = Sessioner.GetSessionMember();
            if (member == null || member.AdminRoleID != 1)
                return Json(false, JsonRequestBehavior.AllowGet);
            using (Model1 db = new Model1())
            {
                if (makeadmin)
                    db.Member.Where(p => p.MemberID == memberid).First().AdminRoleID = 2;
                else
                    db.Member.Where(p => p.MemberID == memberid).First().AdminRoleID = null;
                db.SaveChanges();
                Cacher.Remove(string.Format(Cacher.getckey(cnum.MemberDetail), memberid));
                return Json("ok", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult admineditentrytext(int entryid, string entrytext)
        {
            Member member = Sessioner.GetSessionMember();
            if (member.AdminRoleID != 1)
                return Json(false, JsonRequestBehavior.AllowGet);
            using (Model1 db = new Model1())
            {
                Entry entry = db.Entry.Where(p => p.EntryID == entryid).FirstOrDefault();
                entry.EntryText = entrytext;
                string cachekey = string.Format(Cacher.getckey(cnum.ChosenEntries), entry.StoryID, entry.PageNo);
                Cacher.Remove(cachekey);
                db.SaveChanges();
                return Json("ok", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult admindeleteentrytag(int entrytagid)
        {
            Member member = Sessioner.GetSessionMember();
            if (member.AdminRoleID != 1)
                return Json(false, JsonRequestBehavior.AllowGet);
            using (Model1 db = new Model1())
            {
                EntryTag entrytag = db.EntryTag.Where(p => p.EntryTagID == entrytagid).FirstOrDefault();
                db.EntryTag.Remove(entrytag);
                Entry entry = db.Entry.Where(p => p.EntryID == entrytag.EntryID).FirstOrDefault();
                string cachekey = string.Format(Cacher.getckey(cnum.StoryTags), entry.StoryID);
                Cacher.Remove(cachekey);
                db.SaveChanges();
                return Json("ok", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult admindeleteentry(int entryid)
        {
            Member member = Sessioner.GetSessionMember();
            if (member.AdminRoleID != 1)
                return Json(false, JsonRequestBehavior.AllowGet);
            using (Model1 db = new Model1())
            {
                Entry entry = db.Entry.Where(p => p.EntryID == entryid).FirstOrDefault();
                entry.StatusID = 2;
                string cachekey = string.Format(Cacher.getckey(cnum.ChosenEntries), entry.StoryID, entry.PageNo);
                Cacher.Remove(cachekey);
                string cachekey2 = string.Format(Cacher.getckey(cnum.EntriesOnVote), entry.StoryID);
                Cacher.Remove(cachekey2);
                db.SaveChanges();
                return Json("ok", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult switchprg(int entryid, bool setprg)
        {
            Member member = Sessioner.GetSessionMember();
            if (member.AdminRoleID != 1)
                return Json(false, JsonRequestBehavior.AllowGet);
            using (Model1 db = new Model1())
            {
                Entry entry = db.Entry.Where(p => p.EntryID == entryid).FirstOrDefault();
                entry.IsParagraphStart = setprg;
                string cachekey = string.Format(Cacher.getckey(cnum.ChosenEntries), entry.StoryID, entry.PageNo);
                Cacher.Remove(cachekey);
                db.SaveChanges();
                return Json("ok", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getpageentries(int storyid, int pageno)
        {
            List<Entry> entries = Common.GetEntriesForPage(storyid, pageno);
            var json = entries.Select(p => new
            {
                text = p.EntryText,
                isprgstart = p.IsParagraphStart,
                eid = p.EntryID,
                pid = p.PageNo,
                mid = p.MemberID,
                mname = p.Member.MemberName,
                isanon = p.IsAnonymous,
                sname = BHelper.rwtext(p.Story.StoryName)
            }).ToList();
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        //public ActionResult getentriesonvote(int lastid)
        //{
        //    List<Entry> entries = Common.GetEntriesForPage(lastid);
        //    var json = entries.Select(p => p.EntryText).ToList();
        //    return Json(json, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult ScrollEntries(int storyid, int lastid)
        {
            List<Entry> entries = Common.GetEntriesOnVote(storyid, lastid);
            ViewBag.entriesonvote = entries;
            return PartialView();
        }
        public ActionResult EntryInfo()
        {
            return PartialView();
        }
        public ActionResult postentry(int storyid, string text, bool isanon)
        {
            string result = ""; //
            Member member = Sessioner.GetSessionMember();
            if (member == null)
                goto finish;
            Story currentstory = Common.GetStoryDetail(storyid);
            if (currentstory.StateID == 2 || currentstory.NextTickDate < DateTime.Now)
            {
                result = res.cantpostnow;
                goto finish;
            }
            using (Model1 model = new Model1())
            {
                Entry entry = model.Entry.Where(p => p.StoryID == currentstory.StoryID && p.MemberID == member.MemberID && p.StatusID == 1 && p.CreateDate > currentstory.LastEntryChosenDate).FirstOrDefault();
                if (entry != null)
                {
                    result = res.youpostedbefore;
                    goto finish;
                }
                else
                    entry = new Entry();

                if (currentstory.IsPrivateWriting && currentstory.AdminID != member.MemberID)
                {
                    if (!model.StoryMember.Any(p => p.MemberID == member.MemberID && p.StoryID == currentstory.StoryID))
                        goto finish;
                }
                string lastchar = text.Substring(text.Length - 1, 1);
                if (!currentstory.AllowMultipleSentences)
                {
                    if (lastchar != "." && lastchar != "?" && lastchar != "!")
                    {
                        text += ".";
                        lastchar = ".";//means set default
                    }
                    text = text.Substring(0, text.IndexOf(lastchar)) + lastchar;
                }

                entry.CreateDate = DateTime.Now;
                entry.EntryText = text.Length > currentstory.CharLimit ? text.Substring(0, currentstory.CharLimit) : text;//nokta isareti???????????????????+1 mi
                entry.StatusID = 1;
                entry.IsAnonymous = isanon;
                entry.MemberID = member.MemberID;
                entry.StoryID = currentstory.StoryID;
                model.Entry.Add(entry);
                model.SaveChanges();
                result = res.postedsuccesfully;
                Cacher.Remove(string.Format(Cacher.getckey(cnum.EntriesOnVote), currentstory.StoryID));
                HttpCookie cookie = new HttpCookie("loglastentry");
                cookie.Expires = DateTime.Now.AddYears(-1);//clear this
                Response.Cookies.Add(cookie);
            }
        finish:
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult saveuserstory()
        {
            string result = ""; //
            Member member = Sessioner.GetSessionMember();
            if (member == null)
                goto finish;
            using (Model1 model = new Model1())
            {
                int counter1 = int.Parse(Request["newcounter1"]);
                int counter2 = int.Parse(Request["newcounter2"]);
                int charlimit = int.Parse(Request["newcharlimit"]);
                bool allowmultiple = Request["newallowmultiplesentences"] == "on" || Request["newallowmultiplesentences"] == "true";
                int storyid = int.Parse(Request["hdnuserstoryid"]);
                Story story;
                if (storyid != 0)
                {
                    story = model.Story.Where(p => p.StoryID == storyid).First();
                    story.FirstCounter = counter1;
                    story.SecondCounter = counter2;
                    story.CharLimit = charlimit;
                    story.AllowMultipleSentences = allowmultiple;
                    story.StoryName = Request["newstoryname"];
                    story.StoryText = Request["newstorytext"];
                }
                else
                {
                    story = new Story
                             {
                                 AdminID = member.MemberID,
                                 FirstCounter = counter1,
                                 SecondCounter = counter2,
                                 CharLimit = charlimit,
                                 AllowMultipleSentences = allowmultiple,
                                 StateID = 1,
                                 StatusID = 1,
                                 PageCount = 1,
                                 StoryName = Request["newstoryname"],
                                 StoryText = Request["newstorytext"],
                                 NextTickDate = DateTime.Now,
                                 CreateDate = DateTime.Now,
                                 LastEntryChosenDate = DateTime.Now
                             };
                    model.Story.Add(story);
                }
                model.SaveChanges();
                string cachekey = string.Format(Cacher.getckey(cnum.Story), storyid);
                Cacher.Remove(cachekey);
                Cacher.Remove(string.Format(Cacher.getckey(cnum.MemberStories), story.AdminID));

                result = story.StoryID.ToString();
            }
        finish:
            return Redirect(Request.UrlReferrer.ToString());//sssssssss
        }
        public ActionResult StoryStarter(int? storyid)
        {
            Member member = Sessioner.GetSessionMember();
            if (member != null && storyid.HasValue)
            {
                using (Model1 model = new Model1())
                {
                    Story story = model.Story.Where(p => p.StoryID == storyid).First();
                    if (story.AdminID == member.MemberID || story.StoryMember.Any(p => p.MemberID == member.MemberID && p.TypeID == 3))
                        ViewBag.story = story;
                    if (story.AdminID == member.MemberID)
                    {
                        List<StoryMember> storymembers = model.StoryMember.Where(p => p.StoryID == storyid).ToList();
                        storymembers.ForEach(delegate(StoryMember item) { string s = item.Member.MemberName; });

                        ViewBag.storymembers = storymembers;
                        List<Follow> followers = model.Follow.Where(p => p.FollowedID == member.MemberID).ToList();
                        followers.ForEach(delegate(Follow item) { string s = item.Member.MemberName; });
                        ViewBag.followers = followers;
                    }
                }
            }
            return PartialView();
        }
        public ActionResult AddRemoveStoryMember(int storyid, int memberid, int type, bool isremove)
        {
            string result = ""; //
            Member member = Sessioner.GetSessionMember();
            if (member == null)
                goto finish;
            using (Model1 model = new Model1())
            {
                Story story = model.Story.Where(p => p.AdminID == member.MemberID && p.StoryID == storyid).FirstOrDefault();
                if (story == null)
                    goto finish;
                StoryMember smember = model.StoryMember.Where(p => p.MemberID == memberid && p.StoryID == storyid && p.TypeID == type).FirstOrDefault();
                if (!isremove && smember != null)
                    goto finish;
                if (isremove && smember == null)
                    goto finish;

                if (!isremove)
                {
                    smember = new StoryMember { TypeID = type, StoryID = storyid, MemberID = memberid };
                    model.StoryMember.Add(smember);
                }
                else
                {
                    model.StoryMember.Remove(smember);
                }

                model.SaveChanges();
                string cachekey = string.Format(Cacher.getckey(cnum.StoryMembers), storyid);
                Cacher.Remove(cachekey);
                result = "ok";
            }

        finish:
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeStoryView(int storyid, int type)
        {
            string result = ""; //
            Member member = Sessioner.GetSessionMember();
            if (member == null)
                goto finish;
            using (Model1 model = new Model1())
            {
                Story story = model.Story.Where(p => p.AdminID == member.MemberID && p.StoryID == storyid).FirstOrDefault();
                if (story == null)
                    goto finish;
                story.IsPrivateView = type == 3;
                story.IsPrivateWriting = type != 1;

                model.SaveChanges();
                string cachekey = string.Format(Cacher.getckey(cnum.Story), storyid);
                Cacher.Remove(cachekey);
                result = "ok";
            }

        finish:
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetResources()
        {
            return null;
        }
        public ActionResult Logout()
        {
            Sessioner.SetSessionMember(null);
            return Json("OK", JsonRequestBehavior.AllowGet);
        }
    }
}
