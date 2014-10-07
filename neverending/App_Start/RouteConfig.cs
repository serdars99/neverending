using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace neverending
{
    public class RouteConfig
    {
        public static void RegisterRewrites()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            //if (System.Globalization.CultureInfo.CurrentCulture.ToString() == "tr-TR")
            //{
            //    dict.Add("story", "hikaye");
            //}
            HttpContext.Current.Application["rewrites"] = dict;
        }
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            bool istr = System.Globalization.CultureInfo.CurrentCulture.ToString() == "tr-TR";
            //foreach (KeyValuePair<string, string> pair in HttpContext.Current.Application["rewrites"] as Dictionary<string, string>)
            //    routes.MapRoute(pair.Value, "site/" + pair.Value, new { controller = "Page", action = pair.Key, id = UrlParameter.Optional });

            routes.MapRoute(
                "Profile", // Route name
                "{nick}", // URL with parameters
                new { controller = "Home", action = "Profile", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "WS", // Route name
                "ws/{action}", // URL with parameters
                new { controller = "ws", id = UrlParameter.Optional } // Parameter defaults
            );
            //routes.MapRoute(
            //    "DefaultPage", // Route name
            //    "Iddaa/{action}", // URL with parameters
            //    new { controller = "Page", id = UrlParameter.Optional } // Parameter defaults
            //);
            routes.MapRoute(
                "Ajax", // Route name
                "Ajax/{action}", // URL with parameters
                new { controller = "Ajax", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                name: "Admin",
                url: "Admin/{action}",
                defaults: new { controller = "Admin", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "story",
                url: (istr ? "hikaye" : "story") + "/{title}",
                defaults: new { controller = "Home", action = "Story", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "DefaultOther",
                url: "site/{action}",
                defaults: new { controller = "Home", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}