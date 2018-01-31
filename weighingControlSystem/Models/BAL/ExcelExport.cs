using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace weighingControlSystem.Models.BAL
{
    public class ExcelExport
    {
        public int SRNO { get; set; }
        public string STATUS { get; set; }
        public string TRUCKNO { get; set; }
        public string CONTRACTOR { get; set; }
        public string OPERATOR { get; set; }
        public string CAPACITY { get; set; }
        public string TAREWEIGHT { get; set; }
        public string GROSSWEIGHT { get; set; }       
        public string NETWEIGHT { get; set; }
        public string FILL_FACTOR { get; set; }
        public DateTime TIME { get; set; }
        public DateTime OutTime { get; set; }
        public string outGrossWeight { get;set; }
        public long outpkid { get; set; }
        public string outLocation { get; set; }
        public string inLocation { get; set; }
        public string shift { get; set; }
        public long pkid { get; set; }
        public string status { get; set; }
    }
}