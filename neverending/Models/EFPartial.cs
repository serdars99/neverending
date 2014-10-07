using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace neverending
{
    [MetadataType(typeof(EntryMetadata))]
    public partial class Entry
    {
    }
    public class EntryMetadata
    {
        //[Required]
        //[DisplayName("boat name")]
        //public int ParentID { get; set; }
    }
}