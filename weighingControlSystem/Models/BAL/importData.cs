using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace weighingControlSystem.Models.BAL
{
    public class importData
    {

        public long pkid { get; set; }
        public long pkidOut { get; set; }
        public string ScaleId { get; set; }
        public string truckNo { get; set; }
        public string tareWeight { get; set; }
        public string grossWeight { get; set; }
        public decimal grossWeightOut { get; set; }     
        public string rfid { get; set; }
        public string netweight { get; set; }
        public DateTime dateTimeMachine { get; set; }
        public DateTime outTime { get; set; }
        public string outLocation { get; set; }
        public string status { get; set; }
        public string shift { get; set; }
  

    }
    public class actualimportdata
    {
        public long pkid { get; set; }
        public string truckNo { get; set; }
        public string ScaleId { get; set; }
        public string tareWeight { get; set; }
        public string netweight { get; set; }
        public string truckIn { get; set; }
        public string truckOut { get; set; }
        public string fairWeight { get; set; }
        public string grossWeight { get; set; }      
        public DateTime dateTimeMachine { get; set; }
        public string status { get; set; }
    }
}
