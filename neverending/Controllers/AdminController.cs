using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using neverending.Models;
using neverending.Helpers;
using Facebook;
using Resources;

namespace neverending.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult resources()
        {
            return View();
        }
        [CusAuth(AccessLevel = "1,2")]
        public ActionResult feedbacks()
        {
            Member admin = Sessioner.GetSessionMember();
            using (Model1 model = new Model1())
            {
                int feedid = -1;
                FeedBack feed = new FeedBack();
                if (!string.IsNullOrEmpty(Request["feedid"]))
                {
                    feedid = int.Parse(Request["feedid"]);
                    feed = model.FeedBack.Where(p => p.FeedBackID == feedid).First();

                    if (!string.IsNullOrEmpty(Request["setread"]))
                    {
                        feed.StatusID = 2;
                        model.SaveChanges();

                    }
                }

                List<FeedBack> feeds = model.FeedBack.Where(p => p.StatusID == 1).ToList();

                feeds.ForEach(delegate(FeedBack item) { string s = item.Member.NickName; });
                feeds.ForEach(delegate(FeedBack item) { string s = item.Entry.Story.StoryName; });
                return View(feeds);
            }

        }
        [CusAuth(AccessLevel = "1,2")]
        public ActionResult Stories()
        {
            Member admin = Sessioner.GetSessionMember();
            using (Model1 model = new Model1())
            {
                Story story = new Story();
                int storyID = -1;
                if (!string.IsNullOrEmpty(Request["storyid"]))
                {
                    storyID = int.Parse(Request["storyid"]);
                    story = model.Story.Where(p => p.StoryID == storyID).First();

                    if (!string.IsNullOrEmpty(Request["activate"]))
                    {
                        story.StatusID = 2;
                        model.Story.Where(p => p.StoryID != storyID).ToList().ForEach(delegate(Story item) { item.StatusID = 1; });
                        model.SaveChanges();
                        if (story.StatusID == 2)//active story
                            Cacher.Remove("ActiveStory");
                    }
                    else if (!string.IsNullOrEmpty(Request["deactivate"]))
                    {
                        story.StatusID = 1;
                        model.SaveChanges();
                        if (story.StatusID == 2)//active story
                            Cacher.Remove("ActiveStory");
                    }
                    else if (!string.IsNullOrEmpty(Request["makepassive"]))
                    {
                        story.StatusID = 3;
                        model.SaveChanges();
                        if (story.StatusID == 2)//active story
                            Cacher.Remove("ActiveStory");
                    }
                    else if (!string.IsNullOrEmpty(Request["makeactive"]))
                    {
                        story.StatusID = 1;
                        model.SaveChanges();
                        if (story.StatusID == 2)//active story
                            Cacher.Remove("ActiveStory");
                    }
                    else if (!string.IsNullOrEmpty(Request["delete"]))
                    {
                        story.StatusID = 4;
                        model.SaveChanges();
                        if (story.StatusID == 2)//active story
                            Cacher.Remove("ActiveStory");
                        Cacher.Remove(string.Format(Cacher.getckey(cnum.Story), story.StoryID));
                    }
                    else
                        ViewBag.EditStory = story;
                }

                if (!string.IsNullOrEmpty(Request["editstoryid"]))
                {
                    int estoryid = int.Parse(Request["editstoryid"]);
                    Story editstory = model.Story.Where(p => p.StoryID == estoryid).First();
                    editstory.StoryName = Request["storyname"];
                    editstory.StoryText = Request["storytext"];
                    editstory.FirstCounter = int.Parse(Request["counter1"]);
                    editstory.SecondCounter = int.Parse(Request["counter2"]);
                    editstory.CharLimit = int.Parse(Request["charlimit"]);
                    editstory.AllowMultipleSentences = Request["allowmultiplesentences"] == "on" || Request["allowmultiplesentences"] == "true";
                    //model.Story.ApplyCurrentValues(editstory);
                    model.SaveChanges();
                    if (editstory.StatusID == 2)//active story
                        Cacher.Remove("ActiveStory");
                    Cacher.Remove(string.Format(Cacher.getckey(cnum.Story), story.StoryID));
                }
                else if (!string.IsNullOrEmpty(Request["newstoryname"]))
                {
                    int counter1 = int.Parse(Request["newcounter1"]);
                    int counter2 = int.Parse(Request["newcounter2"]);
                    int charlimit = int.Parse(Request["newcharlimit"]);
                    bool allowmultiple = Request["newallowmultiplesentences"] == "on" || Request["newallowmultiplesentences"] == "true";
                    model.Story.Add(new Story
                    {
                        AdminID = admin.MemberID,
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
                    });
                    model.SaveChanges(); ;
                }

                List<Story> stories = model.Story.Where(p => p.StatusID != 4).OrderByDescending(p => p.CreateDate).ToList();
                stories.ForEach(delegate(Story item) { string s = item.Member.NickName; string st = item.StoryState.StateName; });
                return View(stories);
            }
        }
    }
}
