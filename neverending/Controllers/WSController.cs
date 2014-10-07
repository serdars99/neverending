using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace neverending.Controllers
{
    public class WSController : Controller
    {
        //
        // GET: /WS/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult asd1()
        {
            string version = "1.0"; //your dynamic version number
            HttpCachePolicyBase cache = HttpContext.Response.Cache;
            TimeSpan expireTs = TimeSpan.FromDays(1);
            cache.SetCacheability(HttpCacheability.Private);
            cache.SetETag(version);
            cache.SetExpires(DateTime.Now.Add(expireTs));
            cache.SetMaxAge(expireTs);

            //HttpContext.Response.ExpiresAbsolute = DateTime.Now.AddHours(30);
            List<string> strs = new List<string> { "uşşs", "yellow3" };
            return Json(strs, JsonRequestBehavior.AllowGet);
        }
        public ActionResult asd2()
        {
            List<string> strs = new List<string> { "uşşs", "yellow222" };
            return Json(strs, JsonRequestBehavior.AllowGet);
        }

    }
}
