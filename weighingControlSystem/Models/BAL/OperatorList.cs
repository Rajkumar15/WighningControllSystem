using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace weighingControlSystem.Models.BAL
{
    public class OperatorList
    {
        public long pkid { get; set; }
        public string date { get; set; }
        public string truckNo { get; set; }
        public string shiftName { get; set; }
        public string operatorName { get; set; }
    }
}