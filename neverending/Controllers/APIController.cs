using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace neverending.Controllers
{
    public class CustomHttpAuthorize : System.Web.Http.AuthorizeAttribute
    {
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            return;
            //base.IsAuthorized(actionContext);
            //if (!IsAuthorized(actionContext))
            //    HandleUnauthorizedRequest(actionContext);
            try
            {
                //string password = actionContext.Request.Headers.GetValues("Password").First();
                string password = actionContext.Request.GetQueryNameValuePairs().Where(p => p.Key == "pass").First().Value;
                // instead of hard coding the password you can store it in a config file, database, etc.
                if (password != "abc123")
                {
                    // password is not correct, return 401 (Unauthorized)
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    return;
                }
            }
            catch (Exception e)
            {
                // if any errors occur, or the Password Header is not present we cannot trust the user
                // log the error and return 401 again
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                return;
            }

        }

        protected override void HandleUnauthorizedRequest(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Redirect);
            //response.Headers.Add("Location", "http://www.google.com");
            response.StatusCode = HttpStatusCode.Unauthorized;
            actionContext.Response = response;
        }
        protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            return false;
        }

    }
    public class APIController : ApiController
    {
        private void SetCachingPolicy()
        {
            HttpCachePolicy cache = HttpContext.Current.Response.Cache;
            cache.SetCacheability(HttpCacheability.Private);
            cache.SetExpires(DateTime.Now.AddHours((double)30));


            FieldInfo maxAgeField = cache.GetType().GetField(
                "_maxAge", BindingFlags.Instance | BindingFlags.NonPublic);
            maxAgeField.SetValue(cache, new TimeSpan(0, 0, 30));
        }
        //[CustomHttpAuthorize]
        //public IEnumerable<string> getallproducts3()
        //{
        //    //httpcontext.current.response.cache.
        //    //httpcontext.current.response.cache.setexpires(datetime.now.addminutes(1));
        //    List<string> strs = new List<string> { "uşşs", "yellow3" };
        //    return strs;
        //}
        [HttpGet]
        public HttpResponseMessage check4()
        {
            var response = Request.CreateResponse(HttpStatusCode.OK, "asd");
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                MaxAge = TimeSpan.FromSeconds(60),
                MustRevalidate = true,
                Private = true
            };
            //CacheControlHeaderValue header1 = new CacheControlHeaderValue();
            //header1.Public = true;

            //header1.MaxAge = TimeSpan.FromSeconds(10);
            //header1.NoCache = false;
            //response.Headers.CacheControl = header1;
            return response;
        }
        [HttpGet]
        public string checkuser()
        {
            HttpResponse response = HttpContext.Current.Response;
            return "check2";
        }
        [ActionName("check3")]
        [HttpGet]
        public string checkuser3()
        {
            return "check3";
        }

        //[CustomHttpAuthorize]
        //public IEnumerable<string> getallproducts2()
        //{
        //    //httpcontext.current.response.cache.
        //    //httpcontext.current.response.cache.setexpires(datetime.now.addminutes(1));
        //    List<string> strs = new List<string> { "uşşs", "yellow2" };
        //    return strs;
        //}
        //public HttpResponseMessage GetAllProducts()
        //{
        //    //var doc = XDocument.Load(path);
        //    //var result = doc
        //    //    .Element("Persons")
        //    //    .Elements("Person")
        //    //    .Single(x => (int)x.Element("PersonID") == personId);

        //    //var xml = new XElement("TheRootNode", result).ToString();
        //    //return new HttpResponseMessage
        //    //{
        //    //    Content = new StringContent(xml, Encoding.UTF8, "application/xml")
        //    //};
        //    List<string> strs = new List<string> { "uşşs", "yellow" };
        //    HttpResponseMessage result = Request.CreateResponse<List<string>>(HttpStatusCode.OK, strs);
        //    result.Headers.CacheControl = new CacheControlHeaderValue();
        //    result.Headers.CacheControl.MaxAge = TimeSpan.FromSeconds(30);
        //    result.Headers.CacheControl.Public = true;
        //    //result.Headers.CacheControl.MustRevalidate = true;
        //    return result;
        //}
    }
}
