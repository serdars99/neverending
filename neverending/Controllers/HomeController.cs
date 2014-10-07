using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using neverending.Models;
using Facebook;

namespace neverending.Controllers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class MyAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            //clsSession sess = httpContext.Session["Current"] as clsSession;
            //if (sess.IsLogin)
            //return true;
            if (1 == 2)
                return true;
            return false;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            string s = "";
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "Index", error = "asd" }));
        }
    }

    public class HomeController : Controller
    {
        public ActionResult Index(int? page)
        {
            ViewBag.story = Common.GetActiveStory();
            if (ViewBag.story != null)
            {
                if (Common.StoryStatusNeedsChange(ViewBag.story))
                    ViewBag.story = Common.GetActiveStory();
                ViewBag.entries = Common.GetEntries(null);
                ViewBag.entriesonvote = Common.GetEntriesOnVoteForStory(null);
            }
            return View();
        }
        public ActionResult Story(int id, int? page)
        {
            ViewBag.story = Common.GetStoryDetail(id);
            if (ViewBag.story != null)
            {
                if (Common.StoryStatusNeedsChange(ViewBag.story))
                    ViewBag.story = Common.GetStoryDetail(id);

                if (Common.CheckStoryPrivacy(ViewBag.story))
                {
                    ViewBag.entries = Common.GetEntries(id);
                    ViewBag.entriesonvote = Common.GetEntriesOnVoteForStory(id);
                }
                else
                {
                    return new RedirectResult("/?msg=limitedview");
                }
            }
            return View("~/views/home/index.cshtml");
        }
        public ActionResult Tagged(int storyid, string tag)
        {
            var entries = Common.TagSearch(storyid, tag);
            ViewBag.entriesonvote = Common.GetEntriesOnVoteForStory(storyid);
            ViewBag.tagged = entries;
            ViewBag.story = Common.GetStoryDetail(storyid);
            return View();
        }
        public ActionResult Profile(string nick, int id)
        {
            ViewBag.Nick = nick;
            ViewBag.MemberID = id;
            var entrymember = Common.GetMemberDetailCached(id);
            ViewBag.member = entrymember;
            var entries = Common.GetMemberEntriesForPage(id, 1);
            var memberstories = Common.GetMemberStoriesCached(id);
            ViewBag.memberstories = memberstories;
            ViewBag.entries = entries;
            return View();
        }
        [CusAuthMembersOnly]
        public ActionResult Settings()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult PLUpload()
        {
            return View();
        }
        [CusAuth(AccessLevel = "1")]
        public ActionResult Loginer(int id)
        {
            using (Model1 model = new Model1())
            {
                Member member = model.Member.Where(p => p.MemberID == id).First();
                neverending.Helpers.Sessioner.SetSessionMember(member);
                return Redirect("/");
            }
        }
        public ActionResult FBTest()
        {
            // GET https://graph.facebook.com/oauth/access_token?          // client_id=YOUR_APP_ID           //&client_secret=YOUR_APP_SECRET           //&grant_type=client_credentials
            //GET https://graph.facebook.com/oauth/access_token?client_id=238182746342145&client_secret=6642743dbfd3eac2c0801ef9e6db139c&grant_type=client_credentials
            //goto finish;
            string longtoken = "https://graph.facebook.com/oauth/access_token?grant_type=fb_exchange_token&client_id=238182746342145&client_secret=6642743dbfd3eac2c0801ef9e6db139c&fb_exchange_token=";
            //var appid = "238182746342145";
            //var appsecretid = "6642743dbfd3eac2c0801ef9e6db139c";
            var pagetoken = "CAADYoD38wwEBADAXySHyhZCfiw0N4A8geZBjaNzlwLC8Qw7MNHYcEaLGvdDUZBO00CTPrSqkrIZC7GGZBwOJFPxaTKxYkdG0O1mmtOJlHdL3fTsDgZBNfEBzvYSxEnUZAWENVQBiOJ5b4LZC0dLEUPUEuMhUQkS8uaAZBXXcissvpHigKifdigZC35gw6W582om2oZD";
            var pagetokennew = "CAADYoD38wwEBAJAH7Pf55rN93gIY0tCG7gAimrSHZBGVxGabEgxEpYJTQZBE0aC7o6UmsjrBc0rnHEqB8jLZB8fTscCHY2qYE0HpLLsObBu1O32oPtlPldaxZBsdhKhur2ae3LaiOlA7JpiZCapn2kpKmweXy4BmK1hHHX2xfI8oEzaiCqfLH";
            var appaccessToken = "238182746342145|8vkHi4n6XlYETE_siqyaiSAOxM4";
            //var accessToken = "CAADYoD38wwEBAJJpzf7gweMepMcQfnPl9XzpJExmDznpqM80ckRvbwyVi3ZCIujr6ix64zM8KjvBZBAGSzB30GoMFC30bnB9LSIOD1YJdhgBjV0wI2B6OJdV0vzB87KPtunb3MUNGlZBscK2R78RSHifmHNKAtZAXOJAG7pJ1W7izQ8oNdHkVLM7pZAxZAq4QZD";
            ////var client = new FacebookClient(appaccessToken);
            var client = new FacebookClient(pagetokennew);
            dynamic result = client.Post("me/feed", new
            {
                message = @"My first wall post using Facebook SDK for .NET
                asd adsadasd 
                xxxxxx line"
            });
        //dynamic result = client.Post("100001676009005/notifications", new
        //{
        //    template = "My first wall post using Facebook SDK for .NET",
        //    href = "http://www.google.com"
        //});
        //var newPostId = result.id;

        finish:
            return View();
        }
        public ActionResult strap1()
        {
            return View();
        }
        [MyAuthorizeAttribute]
        public ActionResult AuthTest()
        {
            return View();
        }
        public ActionResult TestKnockout()
        {
            return View();
        }
    }
}
