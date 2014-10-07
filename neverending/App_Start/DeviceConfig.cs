using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace neverending
{
    public class DeviceConfig
    {
        public static void EvaluateDisplayMode()
        {
            DisplayModeProvider.Instance.Modes.Insert(0,
                new DefaultDisplayMode("Phone")
                {  //...modify file (view that is served)
                    //Query condition
                    ContextCondition = (ctx => (
                        //look at user agent
                        (ctx.GetOverriddenUserAgent() != null) &&
                        (//...either iPhone or iPad                           
                            (ctx.GetOverriddenUserAgent().IndexOf("iPhone", StringComparison.OrdinalIgnoreCase) >= 0) ||
                            (ctx.GetOverriddenUserAgent().IndexOf("iPod", StringComparison.OrdinalIgnoreCase) >= 0)
                        )
                ))
                });
            DisplayModeProvider.Instance.Modes.Insert(0,
                new DefaultDisplayMode("Tablet")
                {
                    ContextCondition = (ctx => (
                        (ctx.GetOverriddenUserAgent() != null) &&
                        (
                            (ctx.GetOverriddenUserAgent().IndexOf("iPad", StringComparison.OrdinalIgnoreCase) >= 0) ||
                            (ctx.GetOverriddenUserAgent().IndexOf("Playbook", StringComparison.OrdinalIgnoreCase) >= 0)
                        )
                ))
                });
        }
    }
}