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
    
    public partial class tbl_operatorMapping
    {
        public long pkid { get; set; }
        public Nullable<long> contractorFKID { get; set; }
        public Nullable<long> shift_fkid { get; set; }
        public Nullable<long> operator_fkId { get; set; }
        public Nullable<long> rfiDfkId { get; set; }
        public string operatorFKID { get; set; }
        public Nullable<System.DateTime> workingdate { get; set; }
        public Nullable<System.DateTime> cid { get; set; }
        public Nullable<System.DateTime> cdate { get; set; }
    }
}
