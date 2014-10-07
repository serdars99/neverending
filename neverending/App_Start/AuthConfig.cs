using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using neverending.Models;
using System.Web.Mvc;
using System.Web;
using System.Web.Routing;
using neverending.Helpers;

namespace neverending
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            //OAuthWebSecurity.RegisterFacebookClient(
            //    appId: "",
            //    appSecret: "");

            //OAuthWebSecurity.RegisterGoogleClient();
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CusAuth : AuthorizeAttribute
    {
        // Custom property
        public string AccessLevel { get; set; }
        public enum AuthAction
        {
            Read,
            Create,
            Update,
            Delete
        }
        public AuthAction AAction { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            List<int> intlevels = BHelper.GetAccessLevel(this.AccessLevel);
            Member member = Sessioner.GetSessionMember();
            if (member != null && member.AdminRoleID.HasValue && intlevels.Contains(member.AdminRoleID.Value))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                controller = "Home",
                                action = "Index",
                                msg = "unauthorized"
                            })
                        );
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CusAuthMembersOnly : AuthorizeAttribute
    {
        // Custom property
        public string AccessLevel { get; set; }
        public enum AuthAction
        {
            Read,
            Create,
            Update,
            Delete
        }
        public AuthAction AAction { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            Member member = Sessioner.GetSessionMember();
            if (member != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("/?msg=membersonly");
                        //new RouteValueDictionary(
                        //    new
                        //    {
                        //        controller = "Home",
                        //        action = "Index",
                        //        msg = "membersonly"
                        //    })
                        //);
        }
    }
}
