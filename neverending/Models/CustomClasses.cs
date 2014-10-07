using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Runtime.Serialization;

namespace neverending.Models
{
    [Serializable]
    public class JSONLookup
    {
        [DataMember]
        public int key;

        [DataMember]
        public string value;
    }

    public class PageData
    {
        public dynamic items { get; set; }
        public Paging paging { get; set; }
    }
    public class Paging
    {
        public int currentpage { get; set; }
        public int totalpage { get; set; }
        public int totalitem { get; set; }
        public int pagesize { get; set; }
        public int pagestart { get; set; }
        public int pageend { get; set; }
        public string pagelink { get; set; }
    }
    public class OnlineData
    {
        public DateTime LastUpdate { get; set; }
        public List<OnlineMember> onliners { get; set; }
    }
    public class OnlineMember
    {
        public int MemberID { get; set; }
        public string Nick { get; set; }
        public DateTime LastDate { get; set; }
        public DateTime FirstDate { get; set; }
        public int Status { get; set; }//1 online;2 around
        public bool IsAdmin { get; set; }//1 online;2 around
    }
    public class taglist
    {
        public int entryid { get; set; }
        public int entrytagid { get; set; }
        public int tagtype { get; set; }
        public string tag { get; set; }
    }
}
