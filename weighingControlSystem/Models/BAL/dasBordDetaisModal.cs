using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace weighingControlSystem.Models.BAL
{
    public class dasBordDetaisModal
    {
        public string TruckNo { get; set; }
        public int noTripShifA { get; set; }
        public int noTripShifB { get; set; }
        public int noTripShifC { get; set; }
        public int noTripShifFD { get; set; }

        public decimal TotalTonShiftA { get; set; }
        public decimal TotalTonShiftB { get; set; }
        public decimal TotalTonShiftC { get; set; }
        public decimal TotalTonShiftFD { get; set; }

        public decimal ToltalFillFactorA { get; set; }
        public decimal ToltalFillFactorB { get; set; }
        public decimal ToltalFillFactorC { get; set; }
        public decimal ToltalFillFactorFD { get; set; }


        public decimal ToltalFillFactorAvgShiftA { get; set; }
        public decimal ToltalFillFactorAvgShiftB { get; set; }
        public decimal ToltalFillFactorAvgShiftC { get; set; }
        public decimal ToltalFillFactorAvgFD { get; set; }

        

    }
}