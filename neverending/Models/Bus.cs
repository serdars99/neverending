using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using neverending;
using System.Data.Entity;
using System.Linq.Expressions;
using Resources;
using System.Resources;
using System.Collections;
using System.Globalization;

namespace neverending.Models
{
    public static class Bus
    {
        public static string testef()
        {
            using (Model1 ent2 = new Model1())
            {
                //ent2.ItemLike.Where(p => p.LikeID > 5).ToList().ForEach(delegate(ItemLike item) { item.MemberID = 22; item.IsActive = true; });
                //ent2.SaveChanges();

                //return "";
                //ent2.AddToCommentType(new CommentType { TypeName = "asd" });
                //ent2.SaveChanges();
                return ent2.CommentType.Take(1).SingleOrDefault().TypeName;
            }
        }
    }
}