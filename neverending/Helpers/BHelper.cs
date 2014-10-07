using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using neverending.Models;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Text;
using System.Web.Mvc;
using neverending;

namespace neverending.Helpers
{
    public static class BHelper
    {
        public static string GetActionClass(string action)
        {
            string bodyclass = "home";
            if (action == "event")
                bodyclass = "det";
            else if (action == "eventchat" || action == "mentions")
                bodyclass = "det chat";
            return bodyclass;
        }
        public static string ShortStr(string str, int length)
        {
            if (str.Length > length)
                return str.Substring(0, length) + "...";
            else
                return str;
        }
        public static string GetDescriptionVisible()
        {
            if (HttpContext.Current.Request.Cookies["HideDesc"] != null)
                return "none";
            else
                return "block";
        }
        public static HtmlString GetSystemsText(List<int> systems)
        {
            if (systems == null || systems.Count == 0)
                return new HtmlString("");
            string systemstext = "";
            foreach (int i in systems)
                systemstext += i.ToString() + ",";
            return new HtmlString("Sistem: <strong>" + systemstext.Substring(0, systemstext.Length - 1) + "</strong>");
        }
        public static HtmlString RenderCouponLike(Member member, int ownermemberid, int type, int uniqueID, int likecount, bool isdislike)
        {
            if (member == null)
                return new HtmlString("");
            string lks1 = @"<a class=""reply like"" href=""javascript:void(0);"" onclick=""ItemLike({0}, {1}, false)"" id=""liker{1}"">{2}</a>";
            string lks2 = @"<a class=""reply like dislike"" href=""javascript:void(0);"" onclick=""ItemLike({0}, {1}, true)"" id=""disliker{1}"">{2}</a>";

            if (!isdislike)
                return new HtmlString(string.Format(lks1, type, uniqueID, likecount));
            else
                return new HtmlString(string.Format(lks2, type, uniqueID, likecount));
        }
        public static HtmlString RenderFollow(Member loggedmember, int followedmemberid)
        {
            if (loggedmember == null)
                return new HtmlString("");
            string item = string.Format("{0};", followedmemberid);
            HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies["flw"];
            string flw1 = @"<span class=""pFollow flw1"" onclick=""Follow(#memberid, false);"" style=""display:#display"">Takip Et!</span>";
            string flw2 = @"<span class=""pFollow flw2"" onclick=""Follow(#memberid, true);"" style=""display:#display"">Takibi Bırak!</span>";
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value) && Decode(Decode(cookie.Value)).IndexOf(item) > -1)
            {
                flw1 = flw1.Replace("#memberid", followedmemberid.ToString()).Replace("#display", "none");
                flw2 = flw2.Replace("#memberid", followedmemberid.ToString()).Replace("#display", "inline-block");
            }
            else
            {
                flw1 = flw1.Replace("#memberid", followedmemberid.ToString()).Replace("#display", "inline-block");
                flw2 = flw2.Replace("#memberid", followedmemberid.ToString()).Replace("#display", "none");
            }
            return new HtmlString(flw1 + flw2);
        }
        public static string GetRequestString(string key, string defaultValue)
        {
            string keyvalue = defaultValue;
            if (!string.IsNullOrEmpty(HttpContext.Current.Request[key]))
                keyvalue = HttpContext.Current.Request[key];
            return keyvalue;
        }
        public static int? GetRequestInt(string key, int? defaultValue)
        {
            int? keyvalue = defaultValue;
            if (!string.IsNullOrEmpty(HttpContext.Current.Request[key]))
                keyvalue = int.Parse(HttpContext.Current.Request[key]);
            return keyvalue;
        }
        public static HtmlString CommentHTML(string comment)
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            string userlink1 = @"<a href=""#link"">#link</a>";
            string userlink2 = @"<a href=""#link"" rel=""nofollow"" target=""_blank"">#link</a>";
            string link1 = @"<a href=""{0}"">{1}</a>";
            comment = Regex.Replace(comment, @"https?://([^/]+)/?[^\s]*", delegate(Match match)
            {
                string v = match.ToString();
                string domain = match.Groups[1].Value;
                if (domain.ToLower().IndexOf("bahisor.com") > -1)
                    return userlink1.Replace("#link", v);
                else
                    return userlink2.Replace("#link", v);
            });

            return new HtmlString(comment);
        }
        public static string GetAvatar(string avatarcode)
        {
            string avatar = "/content/images/avatar.jpg";
            if (!string.IsNullOrEmpty(avatarcode))
                avatar = "/UserImage/" + avatarcode + ".jpg";
            return avatar;
        }
        public static Paging CalculatePaging(Paging pd, int pagesize)//pointless without cacheditems
        {
            pd.totalpage = (pd.totalitem + pagesize - 1) / pagesize;
            pd.pagestart = 1;
            if (pd.currentpage > 5)
                pd.pagestart = pd.currentpage - 5;
            pd.pageend = pd.pagestart + 10;
            if (pd.pageend > pd.totalpage)
                pd.pageend = pd.totalpage;
            return pd;
        }
        public static string Encode(string encodeMe)
        {
            byte[] encoded = System.Text.Encoding.UTF8.GetBytes(encodeMe);
            return Convert.ToBase64String(encoded);
        }

        public static string Decode(string decodeMe)
        {
            byte[] encoded = Convert.FromBase64String(decodeMe);
            return System.Text.Encoding.UTF8.GetString(encoded);
        }
        public static int GetConfigValueInt(string key)
        {
            return Convert.ToInt32(GetConfigValue(key));
        }
        public static string GetConfigValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
  
        public static bool IsMobile()
        {
            return HttpContext.Current.Request.Browser.IsMobileDevice;
        }
        public static bool isauthok(string AccessLevel)
        {
            List<int> intlevels = BHelper.GetAccessLevel(AccessLevel);
            Member member = Sessioner.GetSessionValue<Member>("member");
            if (member != null && member.AdminRoleID.HasValue && intlevels.Contains(member.AdminRoleID.Value))
                return true;
            return false;
        }
        public static List<int> GetAccessLevel(string AccessLevel)
        {
            List<int> intlevels = new List<int>();
            string[] levels = AccessLevel.Split(',');
            foreach (string level in levels)
                intlevels.Add(int.Parse(level));
            return intlevels;
        }
        public static string rwtext(string ts)
        {
            const string letters = "0123456789qwertyuiopasdfghjklzxcvbnm";
            string newts = string.Empty;
            ts = ts.ToLower().Replace("ı", "i").Replace("ğ", "g").Replace("ş", "s").Replace("ç", "c").Replace("ü", "u").Replace("ö", "o");
            for (int i = 0; i < ts.Length; i++)
                if (letters.IndexOf(ts.Substring(i, 1)) == -1)
                    newts += "-";
                else
                    newts += ts.Substring(i, 1);
            return newts;
        }
    }
}