using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using neverending.Models;
using neverending.Helpers;

namespace neverending
{
    /// <summary>
    /// Summary description for Jobs
    /// </summary>
    public class Jobs : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            //context.Response.Write("Hello World");
            string jobname = context.Request["j"];
            using (Model1 model = new Model1())
            {
                Job job = model.Job.Where(p => p.JobName == jobname).FirstOrDefault();
                job.LastRunTime = DateTime.Now;
                //model.Job. ApplyCurrentValues(job);
                model.JobWorkLog.Add(new JobWorkLog { JobID = job.JobID, CreateDate = DateTime.Now });
                model.SaveChanges();
                if (!job.IsActive.Value)
                    return;
            }
            switch (context.Request["j"])
            {
                case "nextstep":
                    Common.NextStepAllStories(context);
                    break;
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}