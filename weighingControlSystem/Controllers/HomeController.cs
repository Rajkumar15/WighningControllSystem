using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using weighingControlSystem.Models.DAL;
using itechDll;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.Sql;
using weighingControlSystem.Models.BAL;
using System.Threading.Tasks;
using System.Data.Objects;
namespace weighingControlSystem.Controllers
{
    public class HomeController : AsyncController
    {
        //
        // GET: /Home/
        [Authorize]
        public  ActionResult Index()
        {
            poolData _objectPooldata = new poolData();
            _objectPooldata.start();
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult CreateDemoRecord()
        {
            return View();
        }


        [HttpPost]
        [Authorize]
        public ActionResult CreateDemoRecord(tbl_weighingcontroller_demo model)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            model.cdate = DateTime.Now.Date;
            _db.tbl_weighingcontroller_demo.Add(model);
            _db.SaveChanges();
            ViewBag.alert = "Data Save successfully....";
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult truecDetailList()
        {
            return View();
        }

          [Authorize]
        public ActionResult rtruckList()
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            var search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            string sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            string sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            try
            {
                //dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
                var v = (from a in _db.tbl_weighingcontroller_demo.Select(x => new { x.pkid,x.serialNo,x.scaleId,x.RFID1}) select a);
                //SORT
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    v = (from b in _db.tbl_weighingcontroller_demo.Where(x => x.serialNo.Contains(search)).Select(x => new { x.pkid,x.serialNo,x.scaleId,x.RFID1}) select b);
                }
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.OrderBy(sortColumn, sortColumnDir);
                    //v = v.OrderBy(x=>x.orderfkid);
                }
                recordsTotal = v.Count();
                var data = v.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            { Commonfunction.LogError(e, HttpContext.Server.MapPath("~/ErrorLog.txt")); return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = "" }, JsonRequestBehavior.AllowGet); }
        
        
        }
        [Authorize]
          public ActionResult DbConfiguration(int _id=0)
          {
              if (_id > 0)
              {
                  weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
                  return View(_db.tbl_RemoteConnection.Where(x=>x.pkid==_id).FirstOrDefault());

              }
              else { 
                  
                  
                  return View(); }
                
              
          }


        [Authorize]
        [HttpPost]
        public ActionResult DbConfiguration(tbl_RemoteConnection model)
        {
            string connectionstring = "Data Source=" + model.serverName + ";Initial Catalog=" + model.dataBaseName + ";User ID=" + model.userName + ";Password=" + model.password + ";";
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            if (model.pkid > 0)
            {
              
                try
                {

                    using (SqlConnection con = new SqlConnection(connectionstring))
                    {
                        con.Open();
                        _db.Entry(model).State = System.Data.EntityState.Modified;
                        _db.SaveChanges();
                        con.Close();
                    }
                    ViewBag.alert = "Connection Modified ....";
                    ModelState.Clear();
                    return View();
                }
                catch (Exception ex)
                {
                    Commonfunction.LogError(ex, HttpContext.Server.MapPath("~/ErrorLog.txt"));
                    ViewBag.alert = ex.Message;
                    return View();
                }

            }
            else {

                try
                {

                    using (SqlConnection con = new SqlConnection(connectionstring))
                    {
                        con.Open();
                        con.Close();
                        _db.tbl_RemoteConnection.Add(model);
                        _db.SaveChanges();
                        ViewBag.alert = "Connection Saved ....";
                        ModelState.Clear();
                        return View();
                    }
                   
                }
                catch (Exception ex)
                {
                    Commonfunction.LogError(ex, HttpContext.Server.MapPath("~/ErrorLog.txt"));
                    ViewBag.alert = ex.Message;
                    return View();
                }
             
            
            }

            return View();
        }

        // Test Database connection
        public ActionResult testDbConnection(string serverName, string dataBaseName, string userName, string password)
        {

            string connectionstring = "Data Source=" + serverName + ";Initial Catalog=" + dataBaseName + ";User ID=" + userName + ";Password=" + password + ";";
            try
            {

                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    con.Open();
                    con.Close();
                    return Json("Connection successful...", JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                Commonfunction.LogError(ex, HttpContext.Server.MapPath("~/ErrorLog.txt"));
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }


           return Json("Connection successfull...", JsonRequestBehavior.AllowGet);
        
        }


        // Remote database List

        public ActionResult remoteServerlist()
        {

            return PartialView();
        }


        public ActionResult databaselist()
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            var search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            string sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            string sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            try
            {
                //dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
                var v = (from a in _db.tbl_RemoteConnection.Select(x => new { x.pkid, x.dataBaseName, x.serverName, x.userName, x.comments }) select a);
                //SORT
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    v = (from b in _db.tbl_RemoteConnection.Where(x => x.serverName.Contains(search)).Select(x => new { x.pkid, x.dataBaseName, x.serverName, x.userName,x.comments }) select b);
                }
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.OrderBy(sortColumn, sortColumnDir);
                    //v = v.OrderBy(x=>x.orderfkid);
                }
                recordsTotal = v.Count();
                var data = v.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            { Commonfunction.LogError(e, HttpContext.Server.MapPath("~/ErrorLog.txt")); return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = "" }, JsonRequestBehavior.AllowGet); }
        
        
        
        }




        public ActionResult getTruckWorkingList()
        {
            
                weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
                var search = Request.Form.GetValues("search[value]")[0];
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                //Find Order Column
                string sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                string sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                //DateTime dtcurr = Convert.ToDateTime("2017-04-27");
                try
                {
                DateTime dtcurr = DateTime.Now.Date;

                List<importData> importData = new List<importData>();

                var data = (from report in _db.tbl_storemachineData
                            where EntityFunctions.TruncateTime(report.dateTimeMachine) <= dtcurr && report.truckRFIDOUT != ""
                            select new
                            {
                                report.pkid,
                                report.ScaleId,
                                report.truckRFIDIN,
                                report.truckRFIDOUT,
                                report.fairWeight,
                                report.operatorRFID,
                                report.grossWeight,
                                report.netWeight,
                                report.status,
                                report.dateTimeMachine,
                                report.outPkid,
                                report.outTime,
                                report.outGrossWeight,
                                report.outLocation,
                                report.shiftTime
                            }).ToList();
                foreach (var item in data.Skip(skip).Take(pageSize))
                {
                    importData _obj = new importData();
                    _obj.pkid = item.pkid;
                    _obj.ScaleId = item.ScaleId;
                    if (item.truckRFIDIN != null && item.truckRFIDIN != "")
                    {

                        _obj.truckNo = (from rfId1 in _db.tbl_rfidDetails
                                        where rfId1.RFIDNUMBER == item.truckRFIDIN
                                        join truckMap in _db.tbl_truckMapping on rfId1.pkid equals truckMap.rfif_fkId
                                        join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
                                        select new { truckdetails.truckNo }).FirstOrDefault().truckNo;

                    }
                    else
                    {

                        _obj.truckNo = (from rfId1 in _db.tbl_rfidDetails
                                        where rfId1.RFIDNUMBER == item.truckRFIDOUT
                                        join truckMap in _db.tbl_truckMapping on rfId1.pkid equals truckMap.rfif_fkId
                                        join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
                                        select new { truckdetails.truckNo }).FirstOrDefault().truckNo;

                    }
                    _obj.rfid = item.operatorRFID;
                    _obj.netweight = item.netWeight.ToString();
                    _obj.tareWeight = item.fairWeight.ToString();
                    _obj.grossWeight = item.grossWeight.ToString();
                    _obj.status = item.status;
                    _obj.outLocation = item.outLocation;
                    _obj.shift = item.shiftTime.ToString();

                    _obj.pkidOut = (long?)item.outPkid ?? 0;
                    try
                    {
                        _obj.grossWeightOut = (decimal)item.outGrossWeight;
                        _obj.outTime = (DateTime)item.outTime;
                    }
                    catch
                    {


                    }
                    _obj.dateTimeMachine = (DateTime)item.dateTimeMachine;
                    importData.Add(_obj);


                }

                List<ExcelExport> lsitExcel = new List<ExcelExport>();
                int i = 1;
                foreach (var item in importData)
                {
                    ExcelExport _obj = new ExcelExport();
                    _obj.SRNO = i;

                    _obj.TRUCKNO = item.truckNo;
                    _obj.inLocation = item.ScaleId;
                    try
                    {
                        _obj.CAPACITY = _db.tbl_TruckDetails.Where(x => x.truckNo == item.truckNo).FirstOrDefault().Contact;
                    }
                    catch { _obj.CAPACITY = ""; }
                    _obj.TAREWEIGHT = item.tareWeight;
                    _obj.GROSSWEIGHT = item.grossWeight;

                    if (item.rfid != null && item.rfid != "")
                    {
                        try
                        {
                            long rfidpkid = _db.tbl_rfidDetails.Where(x => x.RFIDNUMBER == item.rfid).FirstOrDefault().pkid;
                            long operatorPkid = (long)_db.tbl_operatorMapping.Where(x => x.rfiDfkId == rfidpkid).FirstOrDefault().operator_fkId;
                            long ContactorId = (long)_db.tbl_operatorMapiingWithContactor.Where(x => x.operatorid == operatorPkid).FirstOrDefault().contactorId;

                            _obj.CONTRACTOR = _db.tbl_contractor.Where(x => x.pkid == ContactorId).FirstOrDefault().contratorName;
                            _obj.OPERATOR = _db.tbl_operatorDetails.Where(x => x.pkid == operatorPkid).FirstOrDefault().OperatorName;
                        }
                        catch
                        {

                        }
                    }

                    _obj.NETWEIGHT = item.netweight;
                    try
                    {
                        _obj.FILL_FACTOR = (Convert.ToDecimal(((Convert.ToDecimal(item.netweight)) / Convert.ToDecimal(_db.tbl_TruckDetails.Where(x => x.truckNo == item.truckNo).FirstOrDefault().Contact))) * 100).ToString("0.00");
                    }
                    catch
                    {
                        _obj.FILL_FACTOR = "0";
                    }
                    _obj.TIME = item.dateTimeMachine;
                    _obj.STATUS = item.ScaleId;
                    _obj.outGrossWeight = item.grossWeightOut.ToString();
                    _obj.outpkid = item.pkidOut;
                    _obj.OutTime = item.outTime;
                    _obj.outLocation = item.outLocation;

                    long shiftId = 0;
                    if (item.outTime != default(DateTime))
                    {
                        TimeSpan curr = item.outTime.TimeOfDay;
                        var shifdata = _db.tbl_operatorshift.ToList();
                        foreach (var sht in shifdata)
                        {
                            if ((curr >= sht.shiftstsrtTime) && (curr <= sht.shiftEndTime))
                            {
                                _obj.shift = sht.shiftname;
                                shiftId = sht.pkid;
                            }
                        }
                    }
                    else
                    {
                        TimeSpan curr = item.dateTimeMachine.TimeOfDay;
                        var shifdata = _db.tbl_operatorshift.ToList();
                        foreach (var sht in shifdata)
                        {
                            if ((curr >= sht.shiftstsrtTime) && (curr <= sht.shiftEndTime))
                            {
                                _obj.shift = sht.shiftname;
                                shiftId = sht.pkid;
                            }
                        }
                    }

                    if (_obj.shift != "")
                    {
                        long truckPkid = 36;
                        try
                        {
                            truckPkid = _db.tbl_TruckDetails.Where(x => x.truckNo == _obj.TRUCKNO).FirstOrDefault().pkid;
                        }
                        catch { truckPkid = 36; }
                        DateTime workingdate = _obj.TIME.Date;

                        if (_db.tbl_operatorMapping.Where(x => x.shift_fkid == shiftId && x.rfiDfkId == truckPkid && x.workingdate == workingdate).Count() > 0)
                        {
                            long operatorPkid = (long)_db.tbl_operatorMapping.Where(x => x.shift_fkid == shiftId && x.rfiDfkId == truckPkid && x.workingdate == workingdate).FirstOrDefault().operator_fkId;
                            long ContactorId = (long)_db.tbl_operatorMapiingWithContactor.Where(x => x.operatorid == operatorPkid).FirstOrDefault().contactorId;

                            _obj.CONTRACTOR = _db.tbl_contractor.Where(x => x.pkid == ContactorId).FirstOrDefault().contratorName;
                            _obj.OPERATOR = _db.tbl_operatorDetails.Where(x => x.pkid == operatorPkid).FirstOrDefault().OperatorName;
                        }
                        else
                        {



                            if (_db.tbl_operatorMapping.Where(x => x.rfiDfkId == truckPkid && x.shift_fkid == shiftId).Count() > 0)
                            {
                                try
                                {
                                    long maxId = _db.tbl_operatorMapping.Where(x => x.rfiDfkId == truckPkid && x.shift_fkid == shiftId).Max(x => x.pkid);

                                    long operatorPkid = (long)_db.tbl_operatorMapping.Where(x => x.pkid == maxId).FirstOrDefault().operator_fkId;
                                    long ContactorId = (long)_db.tbl_operatorMapiingWithContactor.Where(x => x.operatorid == operatorPkid).FirstOrDefault().contactorId;

                                    _obj.CONTRACTOR = _db.tbl_contractor.Where(x => x.pkid == ContactorId).FirstOrDefault().contratorName;
                                    _obj.OPERATOR = _db.tbl_operatorDetails.Where(x => x.pkid == operatorPkid).FirstOrDefault().OperatorName;
                                }
                                catch
                                {
                                    _obj.CONTRACTOR = "";
                                    _obj.OPERATOR = "";
                                }
                            }
                            else
                            {
                                _obj.CONTRACTOR = "";
                                _obj.OPERATOR = "";

                            }
                        }

                    }


                    lsitExcel.Add(_obj);
                    i++;
                }

                List<dasBordDetaisModal> dasboardModel = new List<dasBordDetaisModal>();

                foreach (var dm in lsitExcel.GroupBy(x => new { x.TRUCKNO }).ToList())
                {
                    dasBordDetaisModal dmObj = new dasBordDetaisModal();
                    var firstRow = dm.FirstOrDefault();
                    dmObj.TruckNo = firstRow.TRUCKNO;

                    if (lsitExcel.Where(x => x.TRUCKNO == firstRow.TRUCKNO && x.shift == "Shift A").Count() > 0)
                    {

                        dmObj.TotalTonShiftA = (decimal)lsitExcel.Where(x => x.TRUCKNO == firstRow.TRUCKNO && x.shift == "Shift A").Sum(x => decimal.Parse(x.NETWEIGHT));
                        dmObj.noTripShifA = lsitExcel.Where(x => x.TRUCKNO == firstRow.TRUCKNO && x.shift == "Shift A").Count();
                        dmObj.ToltalFillFactorA = lsitExcel.Where(x => x.TRUCKNO == firstRow.TRUCKNO && x.shift == "Shift A").Sum(x => decimal.Parse(x.FILL_FACTOR)) / dmObj.noTripShifA;

                    }

                    if (lsitExcel.Where(x => x.TRUCKNO == firstRow.TRUCKNO && x.shift == "Shift B").Count() > 0)
                    {


                        dmObj.noTripShifB = lsitExcel.Where(x => x.TRUCKNO == firstRow.TRUCKNO && x.shift == "Shift B").Count();
                        dmObj.TotalTonShiftB = (decimal)lsitExcel.Where(x => x.TRUCKNO == firstRow.TRUCKNO && x.shift == "Shift B").Sum(x => decimal.Parse(x.NETWEIGHT));
                        dmObj.ToltalFillFactorB = lsitExcel.Where(x => x.TRUCKNO == firstRow.TRUCKNO && x.shift == "Shift A").Sum(x => decimal.Parse(x.FILL_FACTOR)) / dmObj.noTripShifB;
                    }

                    if (lsitExcel.Where(x => x.TRUCKNO == firstRow.TRUCKNO && x.shift == "Shift C").Count() > 0)
                    {
                        dmObj.TotalTonShiftC = (decimal)lsitExcel.Where(x => x.TRUCKNO == firstRow.TRUCKNO && x.shift == "Shift C").Sum(x => decimal.Parse(x.NETWEIGHT));
                        dmObj.noTripShifC = lsitExcel.Where(x => x.TRUCKNO == firstRow.TRUCKNO && x.shift == "Shift C").Count();


                        dmObj.ToltalFillFactorC = lsitExcel.Where(x => x.TRUCKNO == firstRow.TRUCKNO && x.shift == "Shift A").Sum(x => decimal.Parse(x.FILL_FACTOR)) / dmObj.noTripShifC;

                        //dmObj.ToltalFillFactorC = (decimal)lsitExcel.Where(x => x.TRUCKNO == firstRow.TRUCKNO && x.shift == "Shift C").Sum(x => decimal.Parse(x.FILL_FACTOR));

                    }


                    dmObj.noTripShifFD = dmObj.noTripShifC + dmObj.noTripShifB + dmObj.noTripShifA;
                    dmObj.TotalTonShiftFD = dmObj.TotalTonShiftC + dmObj.TotalTonShiftB + dmObj.TotalTonShiftA;
                    dmObj.ToltalFillFactorFD = dmObj.ToltalFillFactorC + dmObj.ToltalFillFactorB + dmObj.ToltalFillFactorA;

                    dasboardModel.Add(dmObj);

                }

                try
                {
                    //dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
                    var v = (from a in dasboardModel select a);
                    //SORT
                    if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                    {
                        v = (from b in dasboardModel.Where(x => x.TruckNo.Contains(search)) select b);
                    }
                    if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                    {
                        v = v.OrderBy(sortColumn, sortColumnDir);
                        //v = v.OrderBy(x=>x.orderfkid);
                    }
                    recordsTotal = data.Count();
                    var dataList = v.ToList();
                    return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = dataList }, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                { Commonfunction.LogError(e, HttpContext.Server.MapPath("~/ErrorLog.txt")); return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = "" }, JsonRequestBehavior.AllowGet); }

            }
            catch (Exception e){
                ViewBag.Msg = "Your RFID and Operator Mapping Not Done...";
                Commonfunction.LogError(e, HttpContext.Server.MapPath("~/ErrorLog.txt")); return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = "" }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}
