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
    
    public partial class tbl_contractor
    {
        public long pkid { get; set; }
        public string contratorName { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string email { get; set; }
        public Nullable<int> cid { get; set; }
        public Nullable<System.DateTime> cdate { get; set; }
    }
}
