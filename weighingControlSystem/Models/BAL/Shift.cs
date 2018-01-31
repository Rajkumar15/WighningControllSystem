using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace weighingControlSystem.Models.BAL
{
    public class Shift
    {
        public long pkid { get; set; }
        public string shiftname { get; set; }
        public string shiftstsrtTime { get; set; }
        public string shiftEndTime { get; set; }
    }
}