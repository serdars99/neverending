using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using neverending.Models;

namespace neverending.Helpers
{
    public static class Sessioner
    {
        public static void SetSessionMember(object obj)
        {
            SetSession("member", obj);
        }
        public static Member GetSessionMember()
        {
            return GetSessionValue<Member>("member");
        }
        public static void SetSession(string sessionKey, object obj)
        {
            HttpContext.Current.Session[sessionKey] = obj;
        }
        public static T GetSessionValue<T>(string sessionKey)
        {
            if (HttpContext.Current.Session[sessionKey] != null)
                return (T)HttpContext.Current.Session[sessionKey];
            else
                return default(T);
        }
    }
}