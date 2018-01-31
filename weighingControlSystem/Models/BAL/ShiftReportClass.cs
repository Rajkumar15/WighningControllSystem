using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace weighingControlSystem.Models.BAL
{
    public class ShiftReportClass
    {
        public int pkid { get; set; }
        public string operatorname { get; set; }
         public string contractorname { get; set; }
         public string truckno { get; set; }     
         public int nooftrps { get; set; }
         public decimal tgress { get; set; }
         public decimal ttare { get; set; }
         public decimal tnett { get; set; }
         public string avgtat { get; set; }
         public decimal avgfillfactor { get; set; }
         public string Shiftname { get; set; }
         public TimeSpan intime { get; set; }
         public TimeSpan outTime { get; set; }

         public DateTime timeforoprator { get; set; }
        
    }
}