using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Text.RegularExpressions;
using System.IO.Compression;
using neverending.Models;
using System.Collections;

namespace neverending
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class LowercasingRoute : RouteBase
    {
        public LowercasingRoute(RouteBase routeToWrap)
        {
            _inner = routeToWrap;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            return _inner.GetRouteData(httpContext);
        }
        public override VirtualPathData GetVirtualPath(RequestContext context, RouteValueDictionary values)
        {
            Dictionary<string, string> dict = HttpContext.Current.Application["rewrites"] as Dictionary<string, string>;
            var result = _inner.GetVirtualPath(context, values);
            if (result != null && result.VirtualPath != null)
            {
                string pagename = Regex.Replace(result.VirtualPath, @"\w+/", "");
                pagename = Regex.Replace(pagename, @"\?.*", "");
                if (dict.Any(p => p.Key == pagename))
                {
                    KeyValuePair<string, string> pair = dict.Where(p => p.Key == pagename).SingleOrDefault();
                    result.VirtualPath = Regex.Replace(result.VirtualPath, "/" + pagename, "/" + pair.Value);
                }
                result.VirtualPath = result.VirtualPath.ToLowerInvariant();
            }
            return result;
        }
        RouteBase _inner;
    }
    public class MvcApplication : System.Web.HttpApplication
    {
        private void DecorateRoutes(RouteCollection routeCollection)
        {
            for (int i = 0; i < routeCollection.Count; i++)
            {
                routeCollection[i] = new LowercasingRoute(routeCollection[i]);
            }
        }
        protected void Application_BeginRequest()
        {
            if (!Request.Url.Host.StartsWith("www") && !Request.Url.IsLoopback)
            {
                UriBuilder builder = new UriBuilder(Request.Url);
                builder.Host = "www." + Request.Url.Host;
                Response.StatusCode = 301;
                Response.AddHeader("Location", builder.ToString());
                Response.End();
            }

            HttpContext context = HttpContext.Current;
            string reqpath = context.Request.CurrentExecutionFilePath;
            string url = context.Request.Url.ToString().ToLower();
            //if (url.EndsWith(".css") || url.IndexOf("script") > -1)
            //    return;
#if (DEBUG)
            return;
#else
            return;
#endif
            context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
            HttpContext.Current.Response.AppendHeader("Content-encoding", "gzip");
            HttpContext.Current.Response.Cache.VaryByHeaders["Accept-encoding"] = true;
        }
        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            //if (HttpContext.Current != null && HttpContext.Current.Server.MachineName != "SETH-COMPUTER")
            {
                //HttpContext.Current.Response.Headers.Remove("X-AspNet-Version");
                //HttpContext.Current.Response.Headers.Remove("X-AspNetMvc-Version");
                //HttpContext.Current.Response.Headers.Remove("Server");
            }
        }
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRewrites();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            DecorateRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
            //DeviceConfig.EvaluateDisplayMode();
            //Common.ResetResources();
        }
    }
    public class CustomSqlErrorLog : Elmah.SqlErrorLog
    {
        protected string connectionStringName;
        public CustomSqlErrorLog(IDictionary config)
            : base(config)
        {
            connectionStringName = (string)config["Elmah"];
        }

    }
}