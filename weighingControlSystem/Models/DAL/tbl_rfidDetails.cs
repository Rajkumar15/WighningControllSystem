//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace weighingControlSystem.Models.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_rfidDetails
    {
        public long pkid { get; set; }
        public string RFIDNUMBER { get; set; }
        public string comments { get; set; }
        public Nullable<long> cid { get; set; }
        public Nullable<System.DateTime> cdate { get; set; }
    }
}
