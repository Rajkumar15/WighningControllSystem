using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace weighingControlSystem.Models.BAL
{
    public class ShiftReportCalculation
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
        public string avgfillfactor { get; set; }
        public string Shiftname { get; set; }

        public DateTime shiftOutTime { get; set; }
        public int operatorCount { get; set; }

    }
}