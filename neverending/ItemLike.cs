//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace neverending
{
    using System;
    using System.Collections.Generic;
    
    public partial class ItemLike
    {
        public int LikeID { get; set; }
        public int ItemID { get; set; }
        public int TypeID { get; set; }
        public int MemberID { get; set; }
        public bool IsActive { get; set; }
        public bool IsDislike { get; set; }
        public int Weight { get; set; }
        public System.DateTime CreateDate { get; set; }
    }
}