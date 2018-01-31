using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using weighingControlSystem.Models.DAL;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using System.Linq.Dynamic;
using itechDll;
using System.Data.Objects;
using System.Configuration;
using System.IO;
using System.Data.OleDb;
using weighingControlSystem.Models.BAL;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace weighingControlSystem.Controllers
{
    public class poolMachineDataController : AsyncController
    {
        weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
        //
        // GET: /poolMachineData/
        [HttpGet]
        [Authorize]
        public ActionResult poolMacineData()
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            ViewBag.server = new SelectList(_db.tbl_RemoteConnection.ToList(), "pkid", "serverName");
            ViewBag.refresh = DateTime.Now;

            poolMachineDataController pooldata = new poolMachineDataController();
            poolData _objectPooldata = new poolData();
            _objectPooldata.start();
            return View();
        }
        [HttpPost]
        [Authorize]
        public ActionResult poolMacineData(string date = "")
        {
            ViewBag.refresh = DateTime.Now;
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            ViewBag.server = new SelectList(_db.tbl_RemoteConnection.ToList(), "pkid", "serverName");
            int serverId = Convert.ToInt32(Request.Form.GetValues("serverName")[0]);
            string date1 = Request.Form.GetValues("fromDate")[0];
            var model = _db.tbl_RemoteConnection.Where(x => x.pkid == serverId).FirstOrDefault();

            string connectionstring = "Data Source=" + model.serverName + ";Initial Catalog=" + model.dataBaseName + ";User ID=" + model.userName + ";Password=" + model.password + ";";
            //string connectionString=_db.tbl_accessConnection.FirstOrDefault().accessconnection;

            SqlConnection con = new SqlConnection(connectionstring);
            //OleDbConnection con= new OleDbConnection(connectionstring);
            SqlCommand cmd;
            var scaleId = from c in _db.tbl_storemachineData
                          group c by c.ScaleId into uniqueIds
                          select uniqueIds.FirstOrDefault();
            if (date1 != "")
            {

                long idint = 0;
                try
                {
                    idint = (long)_db.tbl_storemachineData.Max(x => x.machinePkid);
                }
                catch
                {
                    idint = 0;
                }
                if (date1 == "")
                {
                    cmd = new SqlCommand("select * from tbl_WeighingMachineFill", con);
                }
                else
                {

                    DateTime dt = Convert.ToDateTime(date1).Date;
                    string datestring = dt.ToString("MM-dd-yyyy");
                    cmd = new SqlCommand("select * from tbl_WeighingMachineFill where  CAST(cdateTime as date)='" + datestring + "' ", con);

                }
                try
                {
                    con.Open();
                    SqlDataAdapter sda = new SqlDataAdapter();
                    sda.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    con.Close();
                    using (var ctx = new weighingcontrolSystemEntities())
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {

                            var data = ds.Tables[0].Rows[i][1];

                            tbl_storemachineData smd = new tbl_storemachineData();
                            smd.ScaleId = ds.Tables[0].Rows[i][1].ToString().Trim();
                            smd.truckRFIDIN = ds.Tables[0].Rows[i][2].ToString().Trim();
                            smd.truckRFIDOUT = ds.Tables[0].Rows[i][3].ToString().Trim();
                            smd.operatorRFID = ds.Tables[0].Rows[i][4].ToString().Trim();
                            TimeSpan time1 = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString());
                            TimeSpan time2 = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString());
                            if (_db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= time1 && x.shiftEndTime >= time2).Count() > 0)
                            {
                                smd.shiftTime = _db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= time1 && x.shiftEndTime >= time2).FirstOrDefault().pkid;
                            }
                            else
                            {

                                smd.shiftTime = 0;

                            }
                            smd.material = ds.Tables[0].Rows[i][5].ToString().Trim();
                            smd.grossWeight = Convert.ToDecimal(ds.Tables[0].Rows[i][6].ToString().Trim());
                            smd.dateTimeMachine = Convert.ToDateTime(ds.Tables[0].Rows[i][7].ToString().Trim());
                            smd.machinePkid = Convert.ToInt32(ds.Tables[0].Rows[i][0].ToString().Trim());

                            // rfid MappedOrNot
                            if (_db.tbl_truckMapping.Where(x => x.truckRFID == smd.truckRFIDIN || x.truckRFID == smd.truckRFIDOUT).Count() == 1)
                            {
                                // Truck In and unque Record save into database othe wise skip
                                if (smd.truckRFIDIN != "" && _db.tbl_storemachineData.Where(x => x.truckRFIDIN == smd.truckRFIDIN && x.ScaleId == smd.ScaleId && x.dateTimeMachine == smd.dateTimeMachine).Count() == 0)
                                {

                                    smd.status = "IN";
                                    smd.netWeight = 0;
                                    smd.fairWeight = smd.grossWeight;
                                    _db.tbl_storemachineData.Add(smd);
                                    _db.SaveChanges();

                                }
                                else
                                {
                                    if (smd.truckRFIDOUT != "" && _db.tbl_storemachineData.Where(x => x.truckRFIDOUT == smd.truckRFIDOUT && x.ScaleId == smd.ScaleId && x.dateTimeMachine == smd.dateTimeMachine).Count() == 0)
                                    {

                                        if (_db.tbl_storemachineData.Where(x => x.truckRFIDIN == smd.truckRFIDOUT).Count() > 0)
                                        {
                                            long maxId = _db.tbl_storemachineData.Where(x => x.truckRFIDIN == smd.truckRFIDOUT).Max(x => x.pkid);

                                            decimal maxVal = (decimal)_db.tbl_storemachineData.Where(x => x.pkid == maxId).FirstOrDefault().grossWeight;
                                            if (maxVal != 0)
                                            {

                                                if (smd.truckRFIDOUT != "" && smd.truckRFIDOUT != null && smd.status == "OUT")
                                                {

                                                    var updateData = _db.tbl_storemachineData.Where(x => x.pkid == maxId).FirstOrDefault();
                                                    updateData.outPkid = Convert.ToInt32(ds.Tables[0].Rows[i][0].ToString().Trim());
                                                    updateData.material = ds.Tables[0].Rows[i][5].ToString().Trim();
                                                    updateData.outGrossWeight = Convert.ToDecimal(ds.Tables[0].Rows[i][6].ToString().Trim());
                                                    updateData.outTime = Convert.ToDateTime(ds.Tables[0].Rows[i][7].ToString().Trim());
                                                    TimeSpan Outtime1 = TimeSpan.Parse(ds.Tables[0].Rows[i][7].ToString().Trim());
                                                    TimeSpan Outtime2 = TimeSpan.Parse(ds.Tables[0].Rows[i][7].ToString().Trim());

                                                    if (_db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= time1 && x.shiftEndTime >= time2).Count() > 0)
                                                    {
                                                        updateData.outShift = _db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= Outtime1 && x.shiftEndTime >= Outtime2).FirstOrDefault().pkid;
                                                    }
                                                    else
                                                    {

                                                        updateData.shiftTime = 0;
                                                    }
                                                    _db.Entry(updateData).State = EntityState.Modified;
                                                    _db.SaveChanges();
                                                }

                                                smd.status = "OUT";
                                                smd.netWeight = smd.grossWeight - maxVal;
                                                smd.fairWeight = maxVal;
                                            }

                                        }
                                        else
                                        {
                                            smd.status = "OUT";
                                            smd.netWeight = smd.grossWeight;
                                            smd.fairWeight = smd.grossWeight;
                                            smd.outGrossWeight = smd.grossWeight;
                                            smd.outTairWeight = smd.grossWeight;
                                            smd.outTime = smd.dateTimeMachine;
                                            smd.outNetWeight = 0;
                                        }

                                        if (smd.truckRFIDOUT != "" && smd.truckRFIDOUT != null && smd.status == "OUT")
                                        {
                                            try
                                            {
                                                long maxId = _db.tbl_storemachineData.Where(x => x.truckRFIDIN == smd.truckRFIDOUT).Max(x => x.pkid);
                                                if (maxId > 0)
                                                {
                                                    var updateData = _db.tbl_storemachineData.Where(x => x.pkid == maxId).FirstOrDefault();
                                                    updateData.outPkid = Convert.ToInt32(ds.Tables[0].Rows[i][0].ToString().Trim());
                                                    updateData.material = ds.Tables[0].Rows[i][5].ToString().Trim();
                                                    updateData.outGrossWeight = Convert.ToDecimal(ds.Tables[0].Rows[i][6].ToString().Trim());
                                                    updateData.outTime = Convert.ToDateTime(ds.Tables[0].Rows[i][7].ToString().Trim());
                                                    updateData.outLocation = smd.ScaleId;
                                                    TimeSpan Outtime1 = TimeSpan.Parse(ds.Tables[0].Rows[i][7].ToString().Trim());


                                                    if (_db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= Outtime1 && x.shiftEndTime >= Outtime1).Count() > 0)
                                                    {
                                                        updateData.outShift = _db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= Outtime1 && x.shiftEndTime >= Outtime1).FirstOrDefault().pkid;
                                                    }
                                                    else
                                                    {

                                                        updateData.shiftTime = 0;
                                                    }

                                                    _db.Entry(updateData).State = EntityState.Modified;
                                                    _db.SaveChanges();
                                                }
                                            }
                                            catch
                                            {


                                            }
                                        }


                                        _db.tbl_storemachineData.Add(smd);
                                        _db.SaveChanges();
                                    }
                                }


                            }





                        }



                    }
                }

                catch (Exception ex)
                {
                    //itechDll.Commonfunction.LogError(ex, @"~/Errorlog.txt");

                }

            }
            else
            {

                long idint = 0;
                try
                {
                    idint = (long)_db.tbl_storemachineData.Max(x => x.machinePkid);
                }
                catch
                {
                    idint = 0;

                }

                cmd = new SqlCommand("select * from tbl_WeighingMachineFill where  pkid>=" + idint + " ", con);
                try
                {
                    con.Open();
                    SqlDataAdapter sda = new SqlDataAdapter();
                    sda.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    con.Close();

                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {

                        var data = ds.Tables[0].Rows[i][1];
                        tbl_storemachineData smd = new tbl_storemachineData();
                        smd.ScaleId = ds.Tables[0].Rows[i][1].ToString().Trim();
                        smd.truckRFIDIN = ds.Tables[0].Rows[i][2].ToString().Trim();
                        smd.truckRFIDOUT = ds.Tables[0].Rows[i][3].ToString().Trim();
                        smd.operatorRFID = ds.Tables[0].Rows[i][4].ToString().Trim();
                        //shift Time
                        TimeSpan time1 = TimeSpan.Parse(ds.Tables[0].Rows[i][7].ToString().Trim());
                        TimeSpan time2 = TimeSpan.Parse(ds.Tables[0].Rows[i][7].ToString().Trim());
                        if (_db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= time1 && x.shiftEndTime >= time2).Count() > 0)
                        {
                            smd.shiftTime = _db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= time1 && x.shiftEndTime >= time2).FirstOrDefault().pkid;
                        }
                        else
                        {

                            smd.shiftTime = 0;
                        }
                        smd.material = ds.Tables[0].Rows[i][5].ToString().Trim();
                        smd.grossWeight = Convert.ToDecimal(ds.Tables[0].Rows[i][6].ToString().Trim());
                        smd.dateTimeMachine = Convert.ToDateTime(ds.Tables[0].Rows[i][7].ToString().Trim());
                        smd.machinePkid = Convert.ToInt32(ds.Tables[0].Rows[i][0].ToString().Trim());

                        // rfid MappedOrNot
                        if (_db.tbl_truckMapping.Where(x => x.truckRFID == smd.truckRFIDIN || x.truckRFID == smd.truckRFIDOUT).Count() == 1)
                        {
                            // Truck In and unque Record save into database othe wise skip
                            if (smd.truckRFIDIN != "" && _db.tbl_storemachineData.Where(x => x.truckRFIDIN == smd.truckRFIDIN && x.ScaleId == smd.ScaleId && x.dateTimeMachine == smd.dateTimeMachine).Count() == 0)
                            {

                                smd.status = "IN";
                                smd.netWeight = 0;
                                smd.fairWeight = smd.grossWeight;
                                _db.tbl_storemachineData.Add(smd);
                                _db.SaveChanges();

                            }
                            else
                            {
                                if (smd.truckRFIDOUT != "" && _db.tbl_storemachineData.Where(x => x.truckRFIDOUT == smd.truckRFIDOUT && x.ScaleId == smd.ScaleId && x.dateTimeMachine == smd.dateTimeMachine).Count() == 0)
                                {

                                    if (_db.tbl_storemachineData.Where(x => x.truckRFIDIN == smd.truckRFIDOUT).Count() > 0)
                                    {
                                        long maxId = _db.tbl_storemachineData.Where(x => x.truckRFIDIN == smd.truckRFIDOUT).Max(x => x.pkid);

                                        decimal maxVal = (decimal)_db.tbl_storemachineData.Where(x => x.pkid == maxId).FirstOrDefault().grossWeight;
                                        if (maxVal != 0)
                                        {


                                            smd.status = "OUT";
                                            smd.netWeight = smd.grossWeight - maxVal;
                                            smd.fairWeight = maxVal;
                                        }

                                    }
                                    else
                                    {
                                        smd.status = "OUT";
                                        smd.netWeight = smd.grossWeight;
                                        smd.fairWeight = smd.grossWeight;
                                        smd.outGrossWeight = smd.grossWeight;
                                        smd.outTairWeight = smd.grossWeight;
                                        smd.outTime = smd.dateTimeMachine;
                                        smd.outNetWeight = 0;



                                    }

                                    if (smd.truckRFIDOUT != "" && smd.truckRFIDOUT != null && smd.status == "OUT")
                                    {
                                        try
                                        {
                                            long maxId = _db.tbl_storemachineData.Where(x => x.truckRFIDIN == smd.truckRFIDOUT).Max(x => x.pkid);
                                            if (maxId > 0)
                                            {
                                                var updateData = _db.tbl_storemachineData.Where(x => x.pkid == maxId).FirstOrDefault();
                                                updateData.outPkid = Convert.ToInt32(ds.Tables[0].Rows[i][0].ToString().Trim());
                                                updateData.material = ds.Tables[0].Rows[i][5].ToString().Trim();
                                                updateData.outGrossWeight = Convert.ToDecimal(ds.Tables[0].Rows[i][6].ToString().Trim());
                                                updateData.outTime = Convert.ToDateTime(ds.Tables[0].Rows[i][7].ToString().Trim());
                                                updateData.outLocation = smd.ScaleId;

                                                TimeSpan Outtime1 = TimeSpan.Parse(ds.Tables[0].Rows[i][7].ToString().Trim());


                                                if (_db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= Outtime1 && x.shiftEndTime >= Outtime1).Count() > 0)
                                                {
                                                    updateData.outShift = _db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= Outtime1 && x.shiftEndTime >= Outtime1).FirstOrDefault().pkid;
                                                }
                                                else
                                                {

                                                    updateData.outShift = 0;
                                                }

                                                _db.Entry(updateData).State = EntityState.Modified;
                                                _db.SaveChanges();
                                            }
                                        }
                                        catch
                                        {


                                        }
                                    }


                                    _db.tbl_storemachineData.Add(smd);
                                    _db.SaveChanges();
                                }
                            }


                        }

                    }



                }
                catch (Exception ex)
                {

                    ViewBag.alert = ex.Message;
                    return View();
                }
            }
            ViewBag.alert = "Record imported successfully...";
            return View();
        }
        public ActionResult getmachineDataList()
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

            //string ft = "2017-09-26".ToString();
            //DateTime d = Convert.ToDateTime(ft);
            //var bota = _db.tbl_storemachineData.Where(x => x.dateTimeMachine.Value.Date == EntityFunctions.TruncateTime(d)).ToList();

            try
            {
                //dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
                var v = (from b in _db.tbl_storemachineData
                         orderby b.dateTimeMachine
                         select new
                         {
                             pkid = b.pkid,
                             ScaleId = b.ScaleId,
                             truckIn = b.truckRFIDIN ?? "",
                             truckOut = b.truckRFIDOUT ?? "",
                             fairWeight = b.fairWeight,
                             grossWeight = b.grossWeight,
                             netweight = b.netWeight,
                             dateTimeMachine = b.dateTimeMachine,
                             status = b.status,
                             outLocation = b.outLocation
                         }).Select(x => new
                         {
                             pkid = x.pkid,
                             ScaleId = x.ScaleId,
                             truckIn = x.truckIn,
                             truckOut = x.truckOut,
                             fairWeight = x.fairWeight,
                             grossWeight = x.grossWeight,
                             netweight = x.netweight,
                             dateTimeMachine = x.dateTimeMachine,
                             status = x.status,
                             outLocation = x.outLocation
                         });
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    v = (from b in _db.tbl_storemachineData
                         where b.truckRFIDIN.Contains(search) || b.truckRFIDOUT.Contains(search)
                         orderby b.dateTimeMachine
                         select new
                         {
                             pkid = b.pkid,
                             ScaleId = b.ScaleId,
                             truckIn = b.truckRFIDIN ?? "",
                             truckOut = b.truckRFIDOUT ?? "",
                             fairWeight = b.fairWeight,
                             grossWeight = b.grossWeight,
                             netweight = b.netWeight,
                             dateTimeMachine = b.dateTimeMachine,
                             status = b.status,
                             outLocation = b.outLocation
                         }).Select(x => new
                         {
                             pkid = x.pkid,
                             ScaleId = x.ScaleId,
                             truckIn = x.truckIn,
                             truckOut = x.truckOut,
                             fairWeight = x.fairWeight,
                             grossWeight = x.grossWeight,
                             netweight = x.netweight,
                             dateTimeMachine = x.dateTimeMachine,
                             status = x.status,
                             outLocation = x.outLocation

                         });

                }
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.OrderBy(sortColumn, sortColumnDir);

                }
                List<importData> importList = new List<importData>();

                foreach (var item in v.OrderByDescending(x => x.dateTimeMachine).Skip(skip).Take(pageSize))
                {
                    importData _obj = new importData();
                    _obj.pkid = item.pkid;
                    _obj.ScaleId = item.ScaleId;

                    if (item.truckIn != null && item.truckIn != "")
                    {

                        _obj.truckNo = (from rfId in _db.tbl_rfidDetails
                                        where rfId.RFIDNUMBER == item.truckIn
                                        join truckMap in _db.tbl_truckMapping on rfId.pkid equals truckMap.rfif_fkId
                                        join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
                                        select new { truckdetails.truckNo }).FirstOrDefault().truckNo;

                    }
                    else
                    {
                        _obj.truckNo = (from rfId in _db.tbl_rfidDetails
                                        where rfId.RFIDNUMBER == item.truckOut
                                        join truckMap in _db.tbl_truckMapping on rfId.pkid equals truckMap.rfif_fkId
                                        join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
                                        select new { truckdetails.truckNo }).FirstOrDefault().truckNo;

                    }

                    _obj.netweight = item.netweight.ToString();
                    _obj.tareWeight = item.fairWeight.ToString();
                    _obj.grossWeight = item.grossWeight.ToString();
                    _obj.status = item.status;
                    _obj.dateTimeMachine = (DateTime)item.dateTimeMachine;
                    _obj.outLocation = item.outLocation;
                    importList.Add(_obj);
                }
                recordsTotal = v.Count();
                var data = importList.ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            { Commonfunction.LogError(e, HttpContext.Server.MapPath("~/ErrorLog.txt")); return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = "" }, JsonRequestBehavior.AllowGet); }



        }
        [HttpGet]
        public ActionResult truckReport()
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            ViewBag.truckList = new SelectList(_db.tbl_TruckDetails.ToList(), "pkid", "truckNo");
            return View();
        }

        [HttpPost]
        public ActionResult truckReport(int truckNumber = 0, string fromdate = "", string todate = "")
        {
            try
            {
            ViewBag.truckList = new SelectList(_db.tbl_TruckDetails.ToList(), "pkid", "truckNo");
            DataGrid gv = new DataGrid();
            List<importData> importData = new List<importData>();
            DateTime dt1 = Convert.ToDateTime(fromdate);
            DateTime dt2 = Convert.ToDateTime(todate);
            if (truckNumber != 0)
            {
                long rfId = (long)_db.tbl_truckMapping.Where(x => x.truckFKId == truckNumber).FirstOrDefault().rfif_fkId;

                string RFIDS = _db.tbl_rfidDetails.Where(x => x.pkid == rfId).FirstOrDefault().RFIDNUMBER;

                var data = (from report in _db.tbl_storemachineData.Where(x => x.truckRFIDIN == RFIDS || x.truckRFIDOUT == RFIDS)
                            where EntityFunctions.TruncateTime(report.dateTimeMachine) >= dt1 && EntityFunctions.TruncateTime(report.dateTimeMachine) <= dt2
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
                            });
                foreach (var item in data)
                {
                    importData _obj = new importData();
                    _obj.pkid = item.pkid;
                    _obj.ScaleId = item.ScaleId;
                    if (item.truckRFIDIN != null && item.truckRFIDIN != "")
                    {
                        _obj.truckNo = _db.tbl_truckMapping.Where(x => x.truckRFID == item.truckRFIDIN).FirstOrDefault().truckNo ?? null;
                    }
                    else
                    {
                        _obj.truckNo = _db.tbl_truckMapping.Where(x => x.truckRFID == item.truckRFIDOUT).FirstOrDefault().truckNo ?? null;
                    }
                    _obj.rfid = item.operatorRFID;
                    _obj.netweight = item.netWeight.ToString();
                    _obj.tareWeight = item.fairWeight.ToString();
                    _obj.grossWeight = item.grossWeight.ToString();
                    _obj.status = item.status;
                    _obj.outLocation = item.outLocation;
                    _obj.shift = item.shiftTime.ToString();
                    _obj.pkidOut = (long?)item.outPkid ?? 0;
                    if (_obj.pkidOut != 0)
                    {
                        _obj.grossWeightOut = (decimal)item.outGrossWeight;
                        try
                        {
                            _obj.outTime = (DateTime)item.outTime;
                        }
                        catch { _obj.outTime = (DateTime)item.dateTimeMachine; }
                    }
                    else
                    {
                        _obj.grossWeightOut = Convert.ToDecimal(_obj.grossWeight);
                    }
                    _obj.dateTimeMachine = (DateTime)item.dateTimeMachine;
                    importData.Add(_obj);
                }
            }
            else
            {
                //.Where(x=>x.truckRFIDIN == "0013" || x.truckRFIDOUT == "0013")

                var data = (from report in _db.tbl_storemachineData
                            where EntityFunctions.TruncateTime(report.dateTimeMachine) >= dt1 && EntityFunctions.TruncateTime(report.dateTimeMachine) <= dt2
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
                            });
                foreach (var item in data)
                {
                    importData _obj = new importData();
                    _obj.pkid = item.pkid;
                    _obj.ScaleId = item.ScaleId;
                    if (item.truckRFIDIN != null && item.truckRFIDIN != "")
                    {
                        _obj.truckNo = _db.tbl_truckMapping.Where(x => x.truckRFID == item.truckRFIDIN).FirstOrDefault().truckNo ?? null;
                    }
                    else
                    {
                        _obj.truckNo = _db.tbl_truckMapping.Where(x => x.truckRFID == item.truckRFIDOUT).FirstOrDefault().truckNo ?? null;
                    }
                    _obj.rfid = item.operatorRFID;
                    _obj.netweight = item.netWeight.ToString();
                    _obj.tareWeight = item.fairWeight.ToString();
                    _obj.grossWeight = item.grossWeight.ToString();
                    _obj.status = item.status;
                    _obj.outLocation = item.outLocation;
                    _obj.shift = item.shiftTime.ToString();
                    _obj.pkidOut = (long?)item.outPkid ?? 0;
                    if (_obj.pkidOut != 0)
                    {
                        _obj.grossWeightOut = (decimal)item.outGrossWeight;
                        try
                        {
                            _obj.outTime = (DateTime)item.outTime;
                        }
                        catch { _obj.outTime = (DateTime)item.dateTimeMachine; }
                    }
                    else
                    {
                        _obj.grossWeightOut = Convert.ToDecimal(_obj.grossWeight);
                    }
                    _obj.dateTimeMachine = (DateTime)item.dateTimeMachine;
                    importData.Add(_obj);
                }
            }       
            List<ExcelExport> lsitExcel = new List<ExcelExport>();
            int i = 1;
            foreach (var item in importData)
            {
                #region
                ExcelExport _obj = new ExcelExport();
                _obj.pkid = item.pkid;
                _obj.SRNO = i;
                _obj.TRUCKNO = item.truckNo;
                _obj.inLocation = item.ScaleId;
                _obj.CAPACITY = _db.tbl_TruckDetails.Where(x => x.truckNo == item.truckNo).FirstOrDefault().Contact;
                _obj.TAREWEIGHT = item.tareWeight;
                _obj.GROSSWEIGHT = item.grossWeight;
                _obj.status = item.status;
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
               
                    _obj.NETWEIGHT = (Convert.ToDecimal(item.grossWeightOut) - Convert.ToDecimal(_obj.TAREWEIGHT)).ToString();
               
                try
                {
                    _obj.FILL_FACTOR = (Convert.ToDecimal(((Convert.ToDecimal(_obj.NETWEIGHT)) / Convert.ToDecimal(_db.tbl_TruckDetails.Where(x => x.truckNo == item.truckNo).FirstOrDefault().Contact))) * 100).ToString("0.00") + "%";
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
                    long truckPkid = _db.tbl_TruckDetails.Where(x => x.truckNo == _obj.TRUCKNO).FirstOrDefault().pkid;
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
                #endregion
            }
            DataTable _table = new DataTable();
            _table.TableName = "GeneralReport";
            _table.Columns.Add("S.No.", typeof(string));
            _table.Columns.Add("Shift.", typeof(string));
            _table.Columns.Add("Operator Name", typeof(string));
            _table.Columns.Add("Contractor", typeof(string));
            _table.Columns.Add("Truck ID", typeof(string));
            _table.Columns.Add("Capacity", typeof(string));
            _table.Columns.Add("Date IN", typeof(string));
            _table.Columns.Add("Clock IN", typeof(string));
            _table.Columns.Add("North_IN(Wt)", typeof(string));
            _table.Columns.Add("South_IN (Wt)", typeof(string));
            _table.Columns.Add("Date Out", typeof(string));
            _table.Columns.Add("Clock Out", typeof(string));
            _table.Columns.Add("North_OUT(Wt)", typeof(string));
            _table.Columns.Add("South_OUT(Wt)", typeof(string));
            _table.Columns.Add("Gross Wt", typeof(string));
            _table.Columns.Add("Tare Wt", typeof(string));
            _table.Columns.Add("Nett Wt", typeof(string));
            _table.Columns.Add("TAT", typeof(string));
            _table.Columns.Add("Fill Factor", typeof(string));

            long srno = 1;
            foreach (var data in lsitExcel)
            {

                if (data.TRUCKNO != "Blanck1")
                {
                    #region
                    if (data.outpkid != 0)
                    {

                        TimeSpan timeTAT = (data.OutTime).Subtract(data.TIME);
                        if (data.inLocation == "North ramp")
                        {

                            if (data.outLocation == "North ramp")
                            {
                                _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"), data.TIME.ToString("HH:mm") ?? "", data.TAREWEIGHT, "", data.OutTime.ToString("MM/dd/yyy"), data.OutTime.ToString("HH:mm") ?? "", data.outGrossWeight, "", data.outGrossWeight, data.TAREWEIGHT, data.NETWEIGHT, timeTAT, data.FILL_FACTOR);
                            }
                            else
                            {
                                _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"), data.TIME.ToString("HH:mm") ?? "", data.TAREWEIGHT, "", data.OutTime.ToString("MM/dd/yyy"), data.OutTime.ToString("HH:mm") ?? "", "", data.outGrossWeight, data.outGrossWeight, data.TAREWEIGHT, data.NETWEIGHT, timeTAT, data.FILL_FACTOR);
                            }
                        }
                        else
                        {
                            if (data.outLocation == "North ramp")
                            {
                                _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"), data.TIME.ToString("HH:mm") ?? "", "", data.TAREWEIGHT, data.OutTime.ToString("MM/dd/yyy"), data.OutTime.ToString("HH:mm") ?? "", data.outGrossWeight, "", data.outGrossWeight, data.TAREWEIGHT, data.NETWEIGHT, timeTAT, data.FILL_FACTOR);
                            }
                            else
                            {
                                _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"), data.TIME.ToString("HH:mm") ?? "", "", data.TAREWEIGHT, data.OutTime.ToString("MM/dd/yyy"), data.OutTime.ToString("HH:mm") ?? "", "", data.outGrossWeight, data.outGrossWeight, data.TAREWEIGHT, data.NETWEIGHT, timeTAT, data.FILL_FACTOR);
                            }

                        }
                    }
                    else
                    {
                        TimeSpan timeTATAAA = (data.OutTime).Subtract(data.TIME);
                        if (data.OutTime == DateTime.MinValue)
                        {
                            if (data.status == "OUT")
                            {
                                var rfid = _db.tbl_truckMapping.Where(x => x.truckNo == data.TRUCKNO).FirstOrDefault().truckRFID;
                                var beta = _db.tbl_storemachineData.Where(x => x.truckRFIDIN == rfid && x.outPkid == null).OrderByDescending(x => x.pkid).FirstOrDefault();
                                DateTime DDDDD = (DateTime)beta.dateTimeMachine;
                                TimeSpan timeTA = (DDDDD).Subtract(data.TIME);
                                if (data.inLocation == "North ramp")
                                {
                                    _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, DDDDD.ToString("MM/dd/yyyy"), DDDDD.ToString("HH:mm") ?? "", data.TAREWEIGHT, "", data.TIME.ToString("MM/dd/yyy") ?? data.TIME.ToString("HH:mm") ?? "", data.GROSSWEIGHT ?? "", "", data.GROSSWEIGHT ?? "", data.TAREWEIGHT ?? "", data.NETWEIGHT, timeTA, data.FILL_FACTOR ?? "");
                                }
                                else
                                {
                                    _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"), data.TIME.ToString("HH:mm") ?? "", "", data.TAREWEIGHT, data.OutTime.ToString("MM/dd/yyy") ?? data.OutTime.ToString("HH:mm") ?? "", "", data.GROSSWEIGHT ?? "", data.GROSSWEIGHT ?? "", data.TAREWEIGHT ?? "", data.NETWEIGHT, timeTA, data.FILL_FACTOR ?? "");
                                }
                            }
                        }
                        else
                        {
                            if (data.inLocation == "North ramp")
                            {
                                _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"), data.TIME.ToString("HH:mm") ?? "", data.TAREWEIGHT, "", data.OutTime.ToString("MM/dd/yyy") ?? data.OutTime.ToString("HH:mm") ?? "", data.GROSSWEIGHT ?? "", "", data.GROSSWEIGHT ?? "", data.TAREWEIGHT ?? "", data.NETWEIGHT, timeTATAAA, data.FILL_FACTOR ?? "");
                            }
                            else
                            {
                                _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"), data.TIME.ToString("HH:mm") ?? "", "", data.TAREWEIGHT, data.OutTime.ToString("MM/dd/yyy") ?? data.OutTime.ToString("HH:mm") ?? "", "", data.GROSSWEIGHT ?? "", data.GROSSWEIGHT ?? "", data.TAREWEIGHT ?? "", data.NETWEIGHT, timeTATAAA, data.FILL_FACTOR ?? "");
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    if (Convert.ToDecimal(data.outGrossWeight) < 50)
                    {
                        if (data.inLocation == "North ramp")
                        {
                            _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"), data.TIME.ToString("HH:mm") ?? "", data.TAREWEIGHT, "", "", "", "", "", "", "", "", "", "");

                        }
                        else
                        {
                            _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"), data.TIME.ToString("HH:mm") ?? "", "", data.TAREWEIGHT, "", "", "", "", "", "", "", "", "");

                        }
                    }
                    else
                    {
                        if (data.inLocation == "North ramp")
                        {
                            _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, "","", "","", data.OutTime.ToString("MM/dd/yyy"), data.OutTime.ToString("HH:mm") ?? "", data.outGrossWeight, "", data.outGrossWeight, data.TAREWEIGHT, data.NETWEIGHT, "", "");

                        }
                        else
                        {
                            _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, "", "", "", "",data.OutTime.ToString("MM/dd/yyy"), data.OutTime.ToString("HH:mm") ?? "", "", data.outGrossWeight, data.outGrossWeight, data.TAREWEIGHT, data.NETWEIGHT, "", "");

                        }
                    }
                }

            }
            gv.DataSource = _table;
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=truckReport.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            gv.RenderControl(htw);
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
            return View();
            }
            catch
            {
                ViewBag.msg = "RFID OR Operator Not Mapped...";
                return View();
            }
        }
        #region
        //    [HttpPost]
        //    public ActionResult truckReport(int truckNumber = 0, string fromdate = "", string todate = "")
        //    {
        //        try
        //        {
        //            ViewBag.truckList = new SelectList(_db.tbl_TruckDetails.ToList(), "pkid", "truckNo");
        //            DataGrid gv = new DataGrid();
        //            List<importData> importData = new List<importData>();
        //            DateTime dt1 = Convert.ToDateTime(fromdate);
        //            DateTime dt2 = Convert.ToDateTime(todate);
        //            if (truckNumber != 0)
        //            {
        //                long rfId = (long)_db.tbl_truckMapping.Where(x => x.truckFKId == truckNumber).FirstOrDefault().rfif_fkId;

        //                string RFIDS = _db.tbl_rfidDetails.Where(x => x.pkid == rfId).FirstOrDefault().RFIDNUMBER;

        //                var rfidlist = _db.tbl_truckMapping.Where(x => x.truckFKId == truckNumber).ToList();

        //                List<tbl_storemachineData> la = _db.tbl_storemachineData.Where(x => EntityFunctions.TruncateTime(x.dateTimeMachine) >= dt1 && EntityFunctions.TruncateTime(x.dateTimeMachine) <= dt2).ToList();

        //                List<tbl_storemachineData> abc = new List<tbl_storemachineData>();

        //                foreach (var item in la)
        //                {
        //                    foreach (var ceta in rfidlist)
        //                    {
        //                        if (item.truckRFIDIN == ceta.truckRFID || item.truckRFIDOUT == ceta.truckRFID)
        //                        {
        //                            abc.Add(item);
        //                        }
        //                    }
        //                }

        //                foreach (var item in abc)
        //                {
        //                    importData _obj = new importData();
        //                    _obj.pkid = item.pkid;
        //                    _obj.ScaleId = item.ScaleId;
        //                    if (item.truckRFIDIN != null && item.truckRFIDIN != "")
        //                    {

        //                        _obj.truckNo = (from rfId1 in _db.tbl_rfidDetails
        //                                        where rfId1.RFIDNUMBER == item.truckRFIDIN
        //                                        join truckMap in _db.tbl_truckMapping on rfId1.pkid equals truckMap.rfif_fkId
        //                                        join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
        //                                        select new { truckdetails.truckNo }).FirstOrDefault().truckNo;

        //                    }
        //                    else
        //                    {

        //                        _obj.truckNo = (from rfId1 in _db.tbl_rfidDetails
        //                                        where rfId1.RFIDNUMBER == item.truckRFIDOUT
        //                                        join truckMap in _db.tbl_truckMapping on rfId1.pkid equals truckMap.rfif_fkId
        //                                        join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
        //                                        select new { truckdetails.truckNo }).FirstOrDefault().truckNo;

        //                    }
        //                    _obj.rfid = item.operatorRFID;
        //                    _obj.netweight = item.netWeight.ToString();
        //                    _obj.tareWeight = item.fairWeight.ToString();
        //                    _obj.grossWeight = item.grossWeight.ToString();
        //                    _obj.status = item.status;
        //                    _obj.outLocation = item.outLocation;
        //                    _obj.shift = item.shiftTime.ToString();

        //                    _obj.pkidOut = (long?)item.outPkid ?? 0;
        //                    try
        //                    {
        //                        _obj.grossWeightOut = (decimal)item.outGrossWeight;
        //                        _obj.outTime = (DateTime)item.outTime;
        //                    }
        //                    catch
        //                    {


        //                    }
        //                    _obj.dateTimeMachine = (DateTime)item.dateTimeMachine;
        //                    importData.Add(_obj);


        //                }


        //            }
        //            else
        //            {

        //                var data = (from report in _db.tbl_storemachineData

        //                            where EntityFunctions.TruncateTime(report.dateTimeMachine) >= dt1 && EntityFunctions.TruncateTime(report.dateTimeMachine) <= dt2
        //                            select new
        //                            {
        //                                report.pkid,
        //                                report.ScaleId,
        //                                report.truckRFIDIN,
        //                                report.truckRFIDOUT,
        //                                report.fairWeight,
        //                                report.operatorRFID,
        //                                report.grossWeight,
        //                                report.netWeight,
        //                                report.status,
        //                                report.dateTimeMachine,
        //                                report.outPkid,
        //                                report.outTime,
        //                                report.outGrossWeight,
        //                                report.outLocation,
        //                                report.shiftTime
        //                            });
        //                foreach (var item in data)
        //                {
        //                    importData _obj = new importData();
        //                    _obj.pkid = item.pkid;
        //                    _obj.ScaleId = item.ScaleId;
        //                    if (item.truckRFIDIN != null && item.truckRFIDIN != "")
        //                    {

        //                        _obj.truckNo = (from rfId1 in _db.tbl_rfidDetails
        //                                        where rfId1.RFIDNUMBER == item.truckRFIDIN
        //                                        join truckMap in _db.tbl_truckMapping on rfId1.pkid equals truckMap.rfif_fkId
        //                                        join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
        //                                        select new { truckdetails.truckNo }).FirstOrDefault().truckNo;

        //                    }
        //                    else
        //                    {

        //                        _obj.truckNo = (from rfId1 in _db.tbl_rfidDetails
        //                                        where rfId1.RFIDNUMBER == item.truckRFIDOUT
        //                                        join truckMap in _db.tbl_truckMapping on rfId1.pkid equals truckMap.rfif_fkId
        //                                        join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
        //                                        select new { truckdetails.truckNo }).FirstOrDefault().truckNo;

        //                    }
        //                    _obj.rfid = item.operatorRFID;
        //                    _obj.netweight = item.netWeight.ToString();
        //                    _obj.tareWeight = item.fairWeight.ToString();
        //                    _obj.grossWeight = item.grossWeight.ToString();
        //                    _obj.status = item.status;
        //                    _obj.outLocation = item.outLocation;
        //                    _obj.shift = item.shiftTime.ToString();

        //                    _obj.pkidOut = (long?)item.outPkid ?? 0;
        //                    try
        //                    {
        //                        _obj.grossWeightOut = (decimal)item.outGrossWeight;
        //                        _obj.outTime = (DateTime)item.outTime;
        //                    }
        //                    catch
        //                    {


        //                    }
        //                    _obj.dateTimeMachine = (DateTime)item.dateTimeMachine;
        //                    importData.Add(_obj);


        //                }

        //            }// End if


        //            List<ExcelExport> lsitExcel = new List<ExcelExport>();
        //            int i = 1;
        //            foreach (var item in importData)
        //            {
        //                ExcelExport _obj = new ExcelExport();
        //                _obj.pkid = item.pkid;
        //                _obj.SRNO = i;
        //                try
        //                {

        //                }
        //                catch (Exception e)
        //                { }
        //                try { }
        //                catch (Exception e)
        //                { }
        //                _obj.TRUCKNO = item.truckNo;
        //                _obj.inLocation = item.ScaleId;
        //                _obj.CAPACITY = _db.tbl_TruckDetails.Where(x => x.truckNo == item.truckNo).FirstOrDefault().Contact;
        //                _obj.TAREWEIGHT = item.tareWeight;
        //                _obj.GROSSWEIGHT = item.grossWeight;

        //                if (item.rfid != null && item.rfid != "")
        //                {
        //                    try
        //                    {
        //                        long rfidpkid = _db.tbl_rfidDetails.Where(x => x.RFIDNUMBER == item.rfid).FirstOrDefault().pkid;
        //                        long operatorPkid = (long)_db.tbl_operatorMapping.Where(x => x.rfiDfkId == rfidpkid).FirstOrDefault().operator_fkId;
        //                        long ContactorId = (long)_db.tbl_operatorMapiingWithContactor.Where(x => x.operatorid == operatorPkid).FirstOrDefault().contactorId;

        //                        _obj.CONTRACTOR = _db.tbl_contractor.Where(x => x.pkid == ContactorId).FirstOrDefault().contratorName;
        //                        _obj.OPERATOR = _db.tbl_operatorDetails.Where(x => x.pkid == operatorPkid).FirstOrDefault().OperatorName;
        //                    }
        //                    catch
        //                    {


        //                    }
        //                }

        //                _obj.NETWEIGHT = item.netweight;
        //                try
        //                {
        //                    _obj.FILL_FACTOR = (Convert.ToDecimal(((Convert.ToDecimal(item.grossWeightOut)) / Convert.ToDecimal(_db.tbl_TruckDetails.Where(x => x.truckNo == item.truckNo).FirstOrDefault().Contact))) * 100).ToString("0.00") + "%";
        //                }
        //                catch
        //                {
        //                    _obj.FILL_FACTOR = "0";
        //                }
        //                _obj.TIME = item.dateTimeMachine;
        //                _obj.STATUS = item.ScaleId;
        //                _obj.outGrossWeight = item.grossWeightOut.ToString();
        //                _obj.outpkid = item.pkidOut.ToString();
        //                _obj.OutTime = item.outTime;
        //                _obj.outLocation = item.outLocation;

        //                long shiftId = 0;
        //                if (item.outTime != default(DateTime))
        //                {
        //                    TimeSpan curr = item.outTime.TimeOfDay;
        //                    var shifdata = _db.tbl_operatorshift.ToList();
        //                    foreach (var sht in shifdata)
        //                    {
        //                        if ((curr >= sht.shiftstsrtTime) && (curr <= sht.shiftEndTime))
        //                        {
        //                            _obj.shift = sht.shiftname;
        //                            shiftId = sht.pkid;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    TimeSpan curr = item.dateTimeMachine.TimeOfDay;
        //                    var shifdata = _db.tbl_operatorshift.ToList();
        //                    foreach (var sht in shifdata)
        //                    {
        //                        if ((curr >= sht.shiftstsrtTime) && (curr <= sht.shiftEndTime))
        //                        {
        //                            _obj.shift = sht.shiftname;
        //                            shiftId = sht.pkid;
        //                        }
        //                    }
        //                }

        //                if (_obj.shift != "")
        //                {
        //                    long truckPkid = _db.tbl_TruckDetails.Where(x => x.truckNo == _obj.TRUCKNO).FirstOrDefault().pkid;
        //                    DateTime workingdate = _obj.TIME.Date;

        //                    if (_db.tbl_operatorMapping.Where(x => x.shift_fkid == shiftId && x.rfiDfkId == truckPkid && x.workingdate == workingdate).Count() > 0)
        //                    {
        //                        long operatorPkid = (long)_db.tbl_operatorMapping.Where(x => x.shift_fkid == shiftId && x.rfiDfkId == truckPkid && x.workingdate == workingdate).FirstOrDefault().operator_fkId;
        //                        long ContactorId = (long)_db.tbl_operatorMapiingWithContactor.Where(x => x.operatorid == operatorPkid).FirstOrDefault().contactorId;

        //                        _obj.CONTRACTOR = _db.tbl_contractor.Where(x => x.pkid == ContactorId).FirstOrDefault().contratorName;
        //                        _obj.OPERATOR = _db.tbl_operatorDetails.Where(x => x.pkid == operatorPkid).FirstOrDefault().OperatorName;
        //                    }
        //                    else
        //                    {
        //                        if (_db.tbl_operatorMapping.Where(x => x.rfiDfkId == truckPkid && x.shift_fkid == shiftId).Count() > 0)
        //                        {
        //                            try
        //                            {
        //                                long maxId = _db.tbl_operatorMapping.Where(x => x.rfiDfkId == truckPkid && x.shift_fkid == shiftId).Max(x => x.pkid);

        //                                long operatorPkid = (long)_db.tbl_operatorMapping.Where(x => x.pkid == maxId).FirstOrDefault().operator_fkId;
        //                                long ContactorId = (long)_db.tbl_operatorMapiingWithContactor.Where(x => x.operatorid == operatorPkid).FirstOrDefault().contactorId;

        //                                _obj.CONTRACTOR = _db.tbl_contractor.Where(x => x.pkid == ContactorId).FirstOrDefault().contratorName;
        //                                _obj.OPERATOR = _db.tbl_operatorDetails.Where(x => x.pkid == operatorPkid).FirstOrDefault().OperatorName;
        //                            }
        //                            catch
        //                            {
        //                                _obj.CONTRACTOR = "";
        //                                _obj.OPERATOR = "";
        //                            }
        //                        }
        //                        else
        //                        {

        //                            _obj.CONTRACTOR = "";
        //                            _obj.OPERATOR = "";

        //                        }
        //                    }


        //                }


        //                lsitExcel.Add(_obj);
        //                i++;
        //            }


        //            DataTable _table = new DataTable();
        //            _table.TableName = "GeneralReport";
        //            _table.Columns.Add("S.No.", typeof(string));
        //            _table.Columns.Add("Shift.", typeof(string));
        //            _table.Columns.Add("Operator Name", typeof(string));
        //            _table.Columns.Add("Contractor", typeof(string));
        //            _table.Columns.Add("Truck ID", typeof(string));
        //            _table.Columns.Add("Capacity", typeof(string));
        //            _table.Columns.Add("Date IN", typeof(string));
        //            _table.Columns.Add("Clock IN", typeof(string));
        //            _table.Columns.Add("North_IN(Wt)", typeof(string));
        //            _table.Columns.Add("South_IN (Wt)", typeof(string));
        //            _table.Columns.Add("Date Out", typeof(string));
        //            _table.Columns.Add("Clock Out", typeof(string));
        //            _table.Columns.Add("North_OUT(Wt)", typeof(string));
        //            _table.Columns.Add("South_OUT(Wt)", typeof(string));
        //            _table.Columns.Add("Gross Wt", typeof(string));
        //            _table.Columns.Add("Tare Wt", typeof(string));
        //            _table.Columns.Add("Nett Wt", typeof(string));
        //            _table.Columns.Add("TAT", typeof(string));
        //            _table.Columns.Add("Fill Factor", typeof(string));



        #endregion
        //            long srno = 1;
        //            foreach (var data in lsitExcel)
        //            {
        //                string inTIme = data.TIME.ToString("HH:mm");

        //                decimal netWeight = (Convert.ToDecimal(data.GROSSWEIGHT) - Convert.ToDecimal(data.TAREWEIGHT));

        //                if (data.outpkid != null && data.OutTime != null && netWeight > 0)
        //                {

        //                    string grossWeight = data.GROSSWEIGHT;

        //                    string tairWeight = data.TAREWEIGHT;

        //                    string outTIme = data.OutTime.ToString("HH:mm");
        //                    if (outTIme == "00:00")
        //                    {
        //                        string RFID = _db.tbl_storemachineData.Where(x => x.pkid == data.pkid).FirstOrDefault().truckRFIDOUT ?? _db.tbl_storemachineData.Where(x => x.pkid == data.pkid).FirstOrDefault().truckRFIDIN;
        //                        DateTime timeasd = (DateTime)_db.tbl_storemachineData.Where(x => x.truckRFIDIN == RFID).OrderByDescending(x => x.pkid).FirstOrDefault().dateTimeMachine;
        //                        outTIme = timeasd.ToString("HH:mm");
        //                    }
        //                    data.OutTime = Convert.ToDateTime(outTIme);
        //                    TimeSpan timeTAT = (data.OutTime).Subtract(data.TIME);

        //                    try
        //                    {
        //                        data.FILL_FACTOR = (Convert.ToDecimal(((Convert.ToDecimal(grossWeight)) / Convert.ToDecimal(_db.tbl_TruckDetails.Where(x => x.truckNo == data.TRUCKNO).FirstOrDefault().Contact))) * 100).ToString("0.00") + "%";
        //                    }
        //                    catch
        //                    {
        //                        data.FILL_FACTOR = "0";
        //                    }
        //                    if (data.FILL_FACTOR == "0.00%")
        //                    {

        //                        _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"),

        //                          inTIme, "", data.TAREWEIGHT, "--", "--", "", data.outGrossWeight, grossWeight, tairWeight, tairWeight, "--", "--");
        //                    }
        //                    else
        //                    {
        //                        if (data.inLocation == "North ramp")
        //                        {

        //                            if (data.outLocation == "North ramp")
        //                            {
        //                                _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"),

        //                                inTIme, data.TAREWEIGHT, "", data.OutTime.ToString("MM/dd/yyy"), outTIme, data.outGrossWeight, "", grossWeight, tairWeight, netWeight, timeTAT, data.FILL_FACTOR);
        //                            }
        //                            else
        //                            {

        //                                _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"),

        //                            inTIme, "", data.TAREWEIGHT, data.OutTime.ToString("MM/dd/yyy"), outTIme, "", data.outGrossWeight, grossWeight, tairWeight, netWeight, timeTAT, data.FILL_FACTOR);
        //                            }


        //                        }
        //                        else
        //                        {

        //                            if (data.outLocation == "North ramp")
        //                            {
        //                                _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"),

        //                                inTIme, data.TAREWEIGHT, "", data.OutTime.ToString("MM/dd/yyy"), outTIme, data.outGrossWeight, "", grossWeight, tairWeight, netWeight, timeTAT, data.FILL_FACTOR);
        //                            }
        //                            else
        //                            {

        //                                _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"),

        //                            inTIme, "", data.TAREWEIGHT, data.TIME.ToString("MM/dd/yyy"), inTIme, "", data.outGrossWeight, grossWeight, tairWeight, netWeight, timeTAT, data.FILL_FACTOR);
        //                            }

        //                        }
        //                    }

        //                }
        //                else
        //                {
        //                    decimal netWeightout = (Convert.ToDecimal(data.outGrossWeight) - Convert.ToDecimal(data.TAREWEIGHT));
        //                    string outTImeAA = data.OutTime.ToString("HH:mm");
        //                    TimeSpan timeTATAA = (data.OutTime).Subtract(data.TIME);
        //                    if (data.FILL_FACTOR == "0.00%")
        //                    {
        //                        _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"),

        //inTIme, "", data.TAREWEIGHT, "--", "--", "", data.outGrossWeight ?? "", data.outGrossWeight ?? "", data.TAREWEIGHT, data.TAREWEIGHT, "--", "--");

        //                    }
        //                    else
        //                    {
        //                        if (data.inLocation == "North ramp")
        //                        {

        //                            _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"),

        //                                     inTIme, "", data.TAREWEIGHT, data.OutTime.ToString("MM/dd/yyy") ?? "", outTImeAA, "", data.outGrossWeight ?? "", data.outGrossWeight ?? "", data.TAREWEIGHT, netWeightout, timeTATAA, data.FILL_FACTOR ?? "");
        //                        }
        //                        else
        //                        {

        //                            _table.Rows.Add(data.SRNO, data.shift, data.OPERATOR, data.CONTRACTOR, data.TRUCKNO, data.CAPACITY, data.TIME.ToString("MM/dd/yyyy"),

        //                               inTIme, "", data.TAREWEIGHT, data.OutTime.ToString("MM/dd/yyy") ?? "", outTImeAA, "", data.outGrossWeight ?? "", data.outGrossWeight ?? "", data.TAREWEIGHT, netWeightout, timeTATAA, data.FILL_FACTOR ?? "");
        //                        }
        //                    }
        //                }

        //                srno++;
        //            }







        //            // create excel 
        //            gv.DataSource = _table;
        //            gv.DataBind();
        //            Response.ClearContent();
        //            Response.Buffer = true;
        //            Response.AddHeader("content-disposition", "attachment; filename=truckReport.xls");
        //            Response.ContentType = "application/ms-excel";
        //            Response.Charset = "";
        //            StringWriter sw = new StringWriter();
        //            HtmlTextWriter htw = new HtmlTextWriter(sw);
        //            gv.RenderControl(htw);
        //            Response.Output.Write(sw.ToString());
        //            Response.Flush();
        //            Response.End();
        //            return View();
        //        }
        //        catch
        //        {
        //            ViewBag.msg = "RFID OR Operator Not Mapped...";
        //            return View();
        //        }
        //    }

        // Shift Report
        [HttpGet]
        public ActionResult ShiftReport()
        {
            ViewBag.shiftlist = new SelectList(_db.tbl_operatorshift.ToList(), "pkid", "shiftname");
            return View();
        }
        [HttpPost]
        public ActionResult ShiftReport(int ShiftNumber = 0, string fromdate = "", string todate = "")
        {
            ViewBag.shiftlist = new SelectList(_db.tbl_operatorshift.ToList(), "pkid", "shiftname");
            DataGrid gv = new DataGrid();
            List<importData> importData = new List<importData>();
            DateTime dt1 = Convert.ToDateTime(fromdate);
            DateTime dt2 = Convert.ToDateTime(todate);

            var datafdgs = (from report in _db.tbl_storemachineData
                            where EntityFunctions.TruncateTime(report.dateTimeMachine) >= dt1 && EntityFunctions.TruncateTime(report.dateTimeMachine) <= dt2
                            && report.truckRFIDIN != "" && report.outPkid != null
                            select new
                            {
                                report.pkid,
                                report.truckRFIDIN,
                                report.dateTimeMachine,
                                report.shiftTime,
                                report.outTime,
                                report.operatorRFID,
                                report.ScaleId,
                                report.truckRFIDOUT,
                                report.fairWeight,
                                report.grossWeight,
                                report.netWeight,
                                report.status,
                                report.outGrossWeight,
                                report.outPkid,
                                report.outLocation,
                            }).ToList();

            List<importData> exceldata = new List<Models.BAL.importData>();
            List<ShiftReportClass> ShiftDataReportList = new List<Models.BAL.ShiftReportClass>();
            List<ShiftReportCalculation> shiftReportCalculation = new List<ShiftReportCalculation>();


            DataTable _table = new DataTable();
            _table.TableName = "ShiftReport";
            _table.Columns.Add("S.No.", typeof(string));
            _table.Columns.Add("shift", typeof(string));
            _table.Columns.Add("Operator", typeof(string));
            _table.Columns.Add("Contractor", typeof(string));
            _table.Columns.Add("Truck ID", typeof(string));
            _table.Columns.Add("No. Of. Trips", typeof(string));
            _table.Columns.Add("T Gross", typeof(string));
            _table.Columns.Add("T Tare", typeof(string));
            _table.Columns.Add("T Nett", typeof(string));
            _table.Columns.Add("Avg TAT", typeof(string));
            _table.Columns.Add("Avg Fill Factor", typeof(string));

            int srno = 1;

            foreach (var item in datafdgs)
            {
                //get operator and contractor
                #region
                ShiftReportClass singleshift = new ShiftReportClass();
                if (item.operatorRFID != null && item.operatorRFID != "")
                {
                    try
                    {
                        long rfidpkid = _db.tbl_rfidDetails.Where(x => x.RFIDNUMBER == item.operatorRFID).FirstOrDefault().pkid;
                        long operatorPkid = (long)_db.tbl_operatorMapping.Where(x => x.rfiDfkId == rfidpkid).FirstOrDefault().operator_fkId;
                        long ContactorId = (long)_db.tbl_operatorMapiingWithContactor.Where(x => x.operatorid == operatorPkid).FirstOrDefault().contactorId;

                        singleshift.contractorname = _db.tbl_contractor.Where(x => x.pkid == ContactorId).FirstOrDefault().contratorName;
                        singleshift.operatorname = _db.tbl_operatorDetails.Where(x => x.pkid == operatorPkid).FirstOrDefault().OperatorName;
                    }
                    catch
                    {


                    }
                }
                #endregion
                //get trunckno
                #region
                if (item.truckRFIDIN != null && item.truckRFIDIN != "")
                {
                    singleshift.truckno = (from rfId1 in _db.tbl_rfidDetails
                                           where rfId1.RFIDNUMBER == item.truckRFIDIN
                                           join truckMap in _db.tbl_truckMapping on rfId1.pkid equals truckMap.rfif_fkId
                                           join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
                                           select new { truckdetails.truckNo }).FirstOrDefault().truckNo;
                }
                else
                {
                    singleshift.truckno = (from rfId1 in _db.tbl_rfidDetails
                                           where rfId1.RFIDNUMBER == item.truckRFIDOUT
                                           join truckMap in _db.tbl_truckMapping on rfId1.pkid equals truckMap.rfif_fkId
                                           join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
                                           select new { truckdetails.truckNo }).FirstOrDefault().truckNo;

                }
                #endregion
                // Get tgross
                #region
                singleshift.tgress = (decimal)item.outGrossWeight;
                #endregion
                //Get Ttare
                #region
                singleshift.ttare = (decimal)item.fairWeight;
                #endregion
                singleshift.tnett = (singleshift.tgress) - (singleshift.ttare);
                //  TimeSpan timeTAT = (item.outTime).Subtract(item.dateTimeMachine);

                // get shift
                #region

                TimeSpan curr = new TimeSpan();
                curr = item.dateTimeMachine.Value.TimeOfDay;

                var shifdata = _db.tbl_operatorshift.ToList();
                foreach (var sht in shifdata)
                {
                    if ((curr >= sht.shiftstsrtTime) && (curr <= sht.shiftEndTime))
                    {
                        singleshift.Shiftname = sht.shiftname;
                    }
                }

                #endregion
                TimeSpan timeTAT = new TimeSpan(00, 00, 00);
                try
                {
                    singleshift.avgfillfactor = (Convert.ToDecimal(((Convert.ToDecimal(item.outGrossWeight) - Convert.ToDecimal(item.grossWeight)) / Convert.ToDecimal(_db.tbl_TruckDetails.Where(x => x.truckNo == singleshift.truckno).FirstOrDefault().Contact))) * 100);
                }
                catch
                {
                    singleshift.avgfillfactor = 0;
                }
                //singleshift.avgtat = "";
                //ShiftDataReportList.Add(singleshift);
                //_table.Rows.Add(srno, singleshift.operatorname, singleshift.contractorname, singleshift.truckno, "", singleshift.tgress, singleshift.ttare, singleshift.tnett,                                                 singleshift.avgtat, singleshift.avgfillfactor,singleshift.Shiftname);
                //srno++;
                singleshift.intime = (TimeSpan)item.dateTimeMachine.Value.TimeOfDay;
                singleshift.outTime = (TimeSpan)item.outTime.Value.TimeOfDay;
                singleshift.timeforoprator = (DateTime)item.outTime;

                ShiftDataReportList.Add(singleshift);
            }

            foreach (var item in ShiftDataReportList)
            {
                ShiftReportCalculation reportCal = new ShiftReportCalculation();

                TimeSpan tsIn = new TimeSpan();
                TimeSpan tsOut = new TimeSpan();

                TimeSpan tTotal = new TimeSpan();

                foreach (var timetat in ShiftDataReportList.Where(x => x.Shiftname == item.Shiftname && x.truckno == item.truckno).ToList())
                {

                    tsIn = (TimeSpan)timetat.intime;
                    tsOut = (TimeSpan)timetat.outTime;
                    tTotal += tsOut - tsIn;

                }

                reportCal.pkid = item.pkid;
                reportCal.contractorname = item.contractorname;
                reportCal.operatorname = item.operatorname;
                reportCal.truckno = item.truckno;
                reportCal.Shiftname = item.Shiftname;
                reportCal.tgress = ShiftDataReportList.Where(x => x.truckno == item.truckno && x.Shiftname == item.Shiftname).Sum(x => x.tgress);
                reportCal.ttare = ShiftDataReportList.Where(x => x.truckno == item.truckno && x.Shiftname == item.Shiftname).Sum(x => x.ttare);
                reportCal.tnett = (decimal)reportCal.tgress - (decimal)reportCal.ttare;
                reportCal.nooftrps = ShiftDataReportList.Where(x => x.truckno == item.truckno && x.Shiftname == item.Shiftname).Count();

                reportCal.shiftOutTime = item.timeforoprator;

                reportCal.avgtat = (new TimeSpan((ShiftDataReportList.Where(x => x.truckno == item.truckno && x.Shiftname == item.Shiftname).Sum(x => x.outTime.Ticks) - ShiftDataReportList.Where(x => x.truckno == item.truckno && x.Shiftname == item.Shiftname).Sum(x => x.intime.Ticks)) / reportCal.nooftrps)).ToString();

                try
                {
                    reportCal.avgfillfactor = (((reportCal.tnett) / Convert.ToDecimal(_db.tbl_TruckDetails.Where(x => x.truckNo == reportCal.truckno).FirstOrDefault().Contact) * 100) / reportCal.nooftrps).ToString("0.00") + "%";
                }
                catch
                {
                    reportCal.avgfillfactor = "0";
                }



                if (reportCal.Shiftname != "")
                {


                    long truckPkid = _db.tbl_TruckDetails.Where(x => x.truckNo == reportCal.truckno).FirstOrDefault().pkid;

                    long shiftId = _db.tbl_operatorshift.Where(x => x.shiftname == reportCal.Shiftname).FirstOrDefault().pkid;

                    DateTime workingdate = reportCal.shiftOutTime.Date;

                    if (_db.tbl_operatorMapping.Where(x => x.shift_fkid == shiftId && x.rfiDfkId == truckPkid && x.workingdate == workingdate).Count() > 0)
                    {
                        long operatorPkid = (long)_db.tbl_operatorMapping.Where(x => x.shift_fkid == shiftId && x.rfiDfkId == truckPkid && x.workingdate == workingdate).FirstOrDefault().operator_fkId;
                        long ContactorId = (long)_db.tbl_operatorMapiingWithContactor.Where(x => x.operatorid == operatorPkid).FirstOrDefault().contactorId;

                        reportCal.contractorname = _db.tbl_contractor.Where(x => x.pkid == ContactorId).FirstOrDefault().contratorName;
                        reportCal.operatorname = _db.tbl_operatorDetails.Where(x => x.pkid == operatorPkid).FirstOrDefault().OperatorName;
                    }
                    else
                    {



                        if (_db.tbl_operatorMapping.Where(x => x.rfiDfkId == truckPkid && x.shift_fkid == shiftId).Count() > 0)
                        {
                            long maxId = _db.tbl_operatorMapping.Where(x => x.rfiDfkId == truckPkid && x.shift_fkid == shiftId).Max(x => x.pkid);

                            long operatorPkid = (long)_db.tbl_operatorMapping.Where(x => x.pkid == maxId).FirstOrDefault().operator_fkId;
                            long ContactorId = (long)_db.tbl_operatorMapiingWithContactor.Where(x => x.operatorid == operatorPkid).FirstOrDefault().contactorId;

                            reportCal.contractorname = _db.tbl_contractor.Where(x => x.pkid == ContactorId).FirstOrDefault().contratorName;
                            reportCal.operatorname = _db.tbl_operatorDetails.Where(x => x.pkid == operatorPkid).FirstOrDefault().OperatorName;
                        }
                        else
                        {

                            reportCal.contractorname = "";
                            reportCal.operatorname = "";

                        }
                    }


                }


                shiftReportCalculation.Add(reportCal);

            }

            long totaltrips = 0; decimal totaltgress = 0; decimal totalttare = 0; decimal totaltnet = 0; decimal totalfillfactor = 0;
            foreach (var bindescel in shiftReportCalculation.GroupBy(x => new { x.truckno, x.Shiftname, x.operatorname }))
            {
                var dataexcel = bindescel.FirstOrDefault();
                if (dataexcel != null)
                {


                    if (ShiftNumber != 0)
                    {
                        string shiftname = _db.tbl_operatorshift.Where(x => x.pkid == ShiftNumber).FirstOrDefault().shiftname;
                        if (dataexcel.Shiftname == shiftname)
                        {
                            _table.Rows.Add(srno, dataexcel.Shiftname, dataexcel.operatorname, dataexcel.contractorname, dataexcel.truckno, dataexcel.nooftrps, dataexcel.tgress, dataexcel.ttare, dataexcel.tnett, dataexcel.avgtat, dataexcel.avgfillfactor);
                            totaltrips = totaltrips + dataexcel.nooftrps;
                            totaltgress = totaltgress + dataexcel.tgress;
                            totalttare = totalttare + dataexcel.ttare;
                            totaltnet = totaltnet + dataexcel.tnett;
                            totalfillfactor = (decimal)((totalfillfactor) + Convert.ToDecimal(dataexcel.avgfillfactor.TrimEnd('%')));
                            srno++;
                        }
                    }
                    else
                    {
                        _table.Rows.Add(srno, dataexcel.Shiftname, dataexcel.operatorname, dataexcel.contractorname, dataexcel.truckno, dataexcel.nooftrps, dataexcel.tgress, dataexcel.ttare, dataexcel.tnett, dataexcel.avgtat, dataexcel.avgfillfactor);
                        totaltrips = totaltrips + dataexcel.nooftrps;
                        totaltgress = totaltgress + dataexcel.tgress;
                        totalttare = totalttare + dataexcel.ttare;
                        totaltnet = totaltnet + dataexcel.tnett;
                        totalfillfactor = (decimal)((totalfillfactor) + Convert.ToDecimal(dataexcel.avgfillfactor.TrimEnd('%')));
                        srno++;
                    }

                }
            }
            _table.Rows.Add("Total", "", "", "", "", totaltrips, totaltgress, totalttare, totaltnet, "", totalfillfactor + "%");
            // GridView gv = new GridView();
            gv.DataSource = _table;
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=ShiftReport.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            gv.RenderControl(htw);
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
            return View();
        }

        [HttpGet]
        public ActionResult ContractorReport()
        {
            ViewBag.Contractorlist = new SelectList(_db.tbl_contractor.ToList(), "pkid", "contratorName");
            return View();
        }
        [HttpPost]
        public ActionResult ContractorReport(int ContractorNumber = 0, string fromdate = "", string todate = "")
        {
            ViewBag.Contractorlist = new SelectList(_db.tbl_contractor.ToList(), "pkid", "contratorName");
            DataGrid gv = new DataGrid();
            List<importData> importData = new List<importData>();
            DateTime dt1 = Convert.ToDateTime(fromdate);
            DateTime dt2 = Convert.ToDateTime(todate);
            var datafdgs = (from report in _db.tbl_storemachineData
                            where EntityFunctions.TruncateTime(report.dateTimeMachine) >= dt1 && EntityFunctions.TruncateTime(report.dateTimeMachine) <= dt2
                            && report.truckRFIDIN != "" && report.outPkid != null
                            select new
                            {
                                report.pkid,
                                report.truckRFIDIN,
                                report.dateTimeMachine,
                                report.shiftTime,
                                report.outTime,
                                report.operatorRFID,
                                report.ScaleId,
                                report.truckRFIDOUT,
                                report.fairWeight,
                                report.grossWeight,
                                report.netWeight,
                                report.status,
                                report.outGrossWeight,
                                report.outPkid,
                                report.outLocation,
                            }).ToList();

            List<importData> exceldata = new List<Models.BAL.importData>();
            List<ShiftReportClass> ShiftDataReportList = new List<Models.BAL.ShiftReportClass>();
            List<ShiftReportCalculation> shiftReportCalculation = new List<ShiftReportCalculation>();


            DataTable _table = new DataTable();
            _table.TableName = "ShiftReport";
            _table.Columns.Add("S.No.", typeof(string));
            _table.Columns.Add("Contractor", typeof(string));
            _table.Columns.Add("No Of Operator", typeof(string));
            _table.Columns.Add("No. Of. Trips", typeof(string));
            _table.Columns.Add("T Nett", typeof(string));
            _table.Columns.Add("Avg Fill Factor", typeof(string));

            int srno = 1;

            foreach (var item in datafdgs)
            {
                //get operator and contractor
                #region
                ShiftReportClass singleshift = new ShiftReportClass();
                if (item.operatorRFID != null && item.operatorRFID != "")
                {
                    try
                    {
                        long rfidpkid = _db.tbl_rfidDetails.Where(x => x.RFIDNUMBER == item.operatorRFID).FirstOrDefault().pkid;
                        long operatorPkid = (long)_db.tbl_operatorMapping.Where(x => x.rfiDfkId == rfidpkid).FirstOrDefault().operator_fkId;
                        long ContactorId = (long)_db.tbl_operatorMapiingWithContactor.Where(x => x.operatorid == operatorPkid).FirstOrDefault().contactorId;

                        singleshift.contractorname = _db.tbl_contractor.Where(x => x.pkid == ContactorId).FirstOrDefault().contratorName;
                        singleshift.operatorname = _db.tbl_operatorDetails.Where(x => x.pkid == operatorPkid).FirstOrDefault().OperatorName;
                    }
                    catch
                    {


                    }
                }
                #endregion
                //get trunckno
                #region
                if (item.truckRFIDIN != null && item.truckRFIDIN != "")
                {
                    singleshift.truckno = (from rfId1 in _db.tbl_rfidDetails
                                           where rfId1.RFIDNUMBER == item.truckRFIDIN
                                           join truckMap in _db.tbl_truckMapping on rfId1.pkid equals truckMap.rfif_fkId
                                           join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
                                           select new { truckdetails.truckNo }).FirstOrDefault().truckNo;
                }
                else
                {
                    singleshift.truckno = (from rfId1 in _db.tbl_rfidDetails
                                           where rfId1.RFIDNUMBER == item.truckRFIDOUT
                                           join truckMap in _db.tbl_truckMapping on rfId1.pkid equals truckMap.rfif_fkId
                                           join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
                                           select new { truckdetails.truckNo }).FirstOrDefault().truckNo;

                }
                #endregion
                // Get tgross
                #region
                singleshift.tgress = (decimal)item.outGrossWeight;
                #endregion
                //Get Ttare
                #region
                singleshift.ttare = (decimal)item.fairWeight;
                #endregion
                singleshift.tnett = (singleshift.tgress) - (singleshift.ttare);
                //  TimeSpan timeTAT = (item.outTime).Subtract(item.dateTimeMachine);

                // get shift
                #region

                TimeSpan curr = new TimeSpan();
                curr = item.dateTimeMachine.Value.TimeOfDay;

                var shifdata = _db.tbl_operatorshift.ToList();
                foreach (var sht in shifdata)
                {
                    if ((curr >= sht.shiftstsrtTime) && (curr <= sht.shiftEndTime))
                    {
                        singleshift.Shiftname = sht.shiftname;
                    }
                }

                #endregion
                TimeSpan timeTAT = new TimeSpan(00, 00, 00);
                try
                {
                    singleshift.avgfillfactor = (Convert.ToDecimal(((Convert.ToDecimal(item.outGrossWeight) - Convert.ToDecimal(item.grossWeight)) / Convert.ToDecimal(_db.tbl_TruckDetails.Where(x => x.truckNo == singleshift.truckno).FirstOrDefault().Contact))) * 100);
                }
                catch
                {
                    singleshift.avgfillfactor = 0;
                }
                //singleshift.avgtat = "";
                //ShiftDataReportList.Add(singleshift);
                //_table.Rows.Add(srno, singleshift.operatorname, singleshift.contractorname, singleshift.truckno, "", singleshift.tgress, singleshift.ttare, singleshift.tnett,                                                 singleshift.avgtat, singleshift.avgfillfactor,singleshift.Shiftname);
                //srno++;
                singleshift.intime = (TimeSpan)item.dateTimeMachine.Value.TimeOfDay;
                singleshift.outTime = (TimeSpan)item.outTime.Value.TimeOfDay;
                singleshift.timeforoprator = (DateTime)item.outTime;
                ShiftDataReportList.Add(singleshift);
            }

            foreach (var item in ShiftDataReportList)
            {
                ShiftReportCalculation reportCal = new ShiftReportCalculation();

                reportCal.pkid = item.pkid;
                reportCal.contractorname = item.contractorname;
                if (item.contractorname != null)
                {
                    int contractorid = Convert.ToInt32(_db.tbl_contractor.Where(x => x.contratorName == item.contractorname).FirstOrDefault().pkid);
                    reportCal.operatorname = _db.tbl_operatorMapiingWithContactor.Where(x => x.contactorId == contractorid).Count().ToString();
                }
                else { reportCal.operatorname = "0"; }
                reportCal.truckno = item.truckno;
                reportCal.Shiftname = item.Shiftname;
                reportCal.shiftOutTime = item.timeforoprator;
                reportCal.tgress = ShiftDataReportList.Sum(x => x.tgress);
                reportCal.ttare = ShiftDataReportList.Sum(x => x.ttare);
                reportCal.tnett = (decimal)reportCal.tgress - (decimal)reportCal.ttare;
                reportCal.nooftrps = ShiftDataReportList.Count();

                try
                {
                    reportCal.avgfillfactor = (((reportCal.tnett) / Convert.ToDecimal(_db.tbl_TruckDetails.Where(x => x.truckNo == reportCal.truckno).FirstOrDefault().Contact) * 100) / reportCal.nooftrps).ToString("0.00") + "%";
                }
                catch
                {
                    reportCal.avgfillfactor = "0";
                }



                if (reportCal.Shiftname != "")
                {


                    long truckPkid = _db.tbl_TruckDetails.Where(x => x.truckNo == reportCal.truckno).FirstOrDefault().pkid;

                    long shiftId = _db.tbl_operatorshift.Where(x => x.shiftname == reportCal.Shiftname).FirstOrDefault().pkid;

                    DateTime workingdate = reportCal.shiftOutTime.Date;

                    if (_db.tbl_operatorMapping.Where(x => x.shift_fkid == shiftId && x.rfiDfkId == truckPkid && x.workingdate == workingdate).Count() > 0)
                    {
                        long operatorPkid = (long)_db.tbl_operatorMapping.Where(x => x.shift_fkid == shiftId && x.rfiDfkId == truckPkid && x.workingdate == workingdate).FirstOrDefault().operator_fkId;
                        long ContactorId = (long)_db.tbl_operatorMapiingWithContactor.Where(x => x.operatorid == operatorPkid).FirstOrDefault().contactorId;

                        reportCal.operatorCount = _db.tbl_operatorMapiingWithContactor.Where(x => x.contactorId == ContactorId).Count();
                        reportCal.contractorname = _db.tbl_contractor.Where(x => x.pkid == ContactorId).FirstOrDefault().contratorName;
                    }
                    else
                    {



                        if (_db.tbl_operatorMapping.Where(x => x.rfiDfkId == truckPkid && x.shift_fkid == shiftId).Count() > 0)
                        {
                            long maxId = _db.tbl_operatorMapping.Where(x => x.rfiDfkId == truckPkid && x.shift_fkid == shiftId).Max(x => x.pkid);

                            long operatorPkid = (long)_db.tbl_operatorMapping.Where(x => x.pkid == maxId).FirstOrDefault().operator_fkId;
                            long ContactorId = (long)_db.tbl_operatorMapiingWithContactor.Where(x => x.operatorid == operatorPkid).FirstOrDefault().contactorId;

                            reportCal.operatorCount = _db.tbl_operatorMapiingWithContactor.Where(x => x.contactorId == ContactorId).Count();
                            reportCal.contractorname = _db.tbl_contractor.Where(x => x.pkid == ContactorId).FirstOrDefault().contratorName;
                        }
                        else
                        {

                            reportCal.contractorname = "";
                            reportCal.operatorname = "";

                        }
                    }



                    shiftReportCalculation.Add(reportCal);

                }
            }

            long totaltrips = 0; decimal totalnooperator = 0; decimal totaltnet = 0; decimal totalfillfactor = 0;
            foreach (var bindescel in shiftReportCalculation.GroupBy(x => new { x.contractorname }))
            {

                var dataexcel = bindescel.FirstOrDefault();
                if (dataexcel != null)
                {

                    if (ContractorNumber != 0)
                    {

                        string contractorname = _db.tbl_contractor.Where(x => x.pkid == ContractorNumber).FirstOrDefault().contratorName;
                        if (dataexcel.contractorname == contractorname)
                        {
                            _table.Rows.Add(srno, dataexcel.contractorname, dataexcel.operatorCount, dataexcel.nooftrps, dataexcel.tnett, dataexcel.avgfillfactor);

                            totaltrips = totaltrips + dataexcel.nooftrps;
                            totaltnet = totaltnet + dataexcel.tnett;
                            totalnooperator = (totalnooperator) + Convert.ToInt32(dataexcel.operatorCount);
                            totalfillfactor = (decimal)((totalfillfactor) + Convert.ToDecimal(dataexcel.avgfillfactor.TrimEnd('%')));
                            srno++;
                        }
                    }
                    else
                    {



                        _table.Rows.Add(srno, dataexcel.contractorname, dataexcel.operatorCount, dataexcel.nooftrps, dataexcel.tnett, dataexcel.avgfillfactor);
                        totaltrips = totaltrips + dataexcel.nooftrps;
                        totaltnet = totaltnet + dataexcel.tnett;
                        totalnooperator = (totalnooperator) + Convert.ToInt32(dataexcel.operatorCount);
                        totalfillfactor = (decimal)((totalfillfactor) + Convert.ToDecimal(dataexcel.avgfillfactor.TrimEnd('%')));
                        srno++;
                    }
                }
            }
            _table.Rows.Add("Total", "", totalnooperator, totaltrips, totaltnet, totalfillfactor + "%");
            // GridView gv = new GridView();
            gv.DataSource = _table;
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=ShiftReport.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            gv.RenderControl(htw);
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
            return View();
        }


        [HttpGet]
        public ActionResult ActualTruckReport()
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            ViewBag.truckList = new SelectList(_db.tbl_TruckDetails.ToList(), "pkid", "truckNo");
            return View();
        }
        [HttpPost]
        public ActionResult ActualTruckReport(int truckNumber = 0, string fromdate = "", string todate = "")
        {
            try
            {
                ViewBag.truckList = new SelectList(_db.tbl_TruckDetails.ToList(), "pkid", "truckNo");
                DataGrid gv = new DataGrid();
                List<actualimportdata> importData = new List<actualimportdata>();
                DateTime dt1 = Convert.ToDateTime(fromdate);
                DateTime dt2 = Convert.ToDateTime(todate);

                if (truckNumber != 0)
                {
                    long rfIdff = (long)_db.tbl_truckMapping.Where(x => x.truckFKId == truckNumber).FirstOrDefault().rfif_fkId;
                    string RFIDSss = _db.tbl_rfidDetails.Where(x => x.pkid == rfIdff).FirstOrDefault().RFIDNUMBER;
                    var rfidlist = _db.tbl_truckMapping.Where(x => x.truckFKId == truckNumber).ToList();

                    List<tbl_storemachineData> la = _db.tbl_storemachineData.Where(x => EntityFunctions.TruncateTime(x.dateTimeMachine) >= dt1 && EntityFunctions.TruncateTime(x.dateTimeMachine) <= dt2).ToList();

                    List<tbl_storemachineData> abc = new List<tbl_storemachineData>();

                    foreach (var item in la)
                    {
                        foreach (var ceta in rfidlist)
                        {
                            if (item.truckRFIDIN == ceta.truckRFID || item.truckRFIDOUT == ceta.truckRFID)
                            {
                                abc.Add(item);
                            }
                        }
                    }
                    foreach (var item in abc.OrderByDescending(x => x.dateTimeMachine))
                    {
                        actualimportdata _obj = new actualimportdata();
                        _obj.pkid = item.pkid;
                        _obj.ScaleId = item.ScaleId;

                        if (item.truckRFIDIN != null && item.truckRFIDIN != "")
                        {


                            //_obj.truckNo = (from rfId1 in _db.tbl_rfidDetails
                            //                where rfId1.RFIDNUMBER == item.truckRFIDIN 
                            //                join truckMap in _db.tbl_truckMapping on rfId1.pkid equals truckMap.rfif_fkId
                            //                join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
                            //                select new { truckdetails.truckNo }).FirstOrDefault().truckNo;

                            _obj.truckNo = _db.tbl_truckMapping.Where(x => x.truckRFID == item.truckRFIDIN).FirstOrDefault().truckNo ?? "";

                        }
                        else
                        {

                            //_obj.truckNo = (from rfId1 in _db.tbl_rfidDetails
                            //                where rfId1.RFIDNUMBER == item.truckRFIDOUT
                            //                join truckMap in _db.tbl_truckMapping on rfId1.pkid equals truckMap.rfif_fkId
                            //                join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
                            //                select new { truckdetails.truckNo }).FirstOrDefault().truckNo;

                            _obj.truckNo = _db.tbl_truckMapping.Where(x => x.truckRFID == item.truckRFIDOUT).FirstOrDefault().truckNo ?? "";

                        }

                        _obj.netweight = item.netWeight.ToString();
                        _obj.tareWeight = item.fairWeight.ToString();
                        _obj.grossWeight = item.grossWeight.ToString();
                        _obj.status = item.status;
                        _obj.dateTimeMachine = (DateTime)item.dateTimeMachine;

                        importData.Add(_obj);


                    }
                    DataTable _table = new DataTable();
                    _table.TableName = "GeneralReport";
                    _table.Columns.Add("S.No.", typeof(string));
                    _table.Columns.Add("Sale Id", typeof(string));
                    _table.Columns.Add("truck No", typeof(string));
                    _table.Columns.Add("Tare Weight", typeof(string));
                    _table.Columns.Add("Gross Weight", typeof(string));
                    _table.Columns.Add("Net Weight", typeof(string));
                    _table.Columns.Add("Date Time", typeof(string));
                    _table.Columns.Add("Status", typeof(string));
                    long srno = 1;
                    foreach (var data in importData)
                    {
                        if (data.netweight != "0.00")
                        {
                            _table.Rows.Add(srno, data.ScaleId, data.truckNo, data.tareWeight, data.grossWeight, data.netweight,
                                data.dateTimeMachine, data.status);
                        }
                        else
                        {
                            _table.Rows.Add(srno, data.ScaleId, data.truckNo, data.tareWeight, data.grossWeight, "ERROR",
                               data.dateTimeMachine, data.status);
                        }
                        srno++;
                    }
                    // create excel 
                    gv.DataSource = _table;
                    gv.DataBind();
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename=truckReport.xls");
                    Response.ContentType = "application/ms-excel";
                    Response.Charset = "";
                    StringWriter sw = new StringWriter();
                    HtmlTextWriter htw = new HtmlTextWriter(sw);
                    gv.RenderControl(htw);
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
                else
                {
                    var v = (from b in _db.tbl_storemachineData
                             where EntityFunctions.TruncateTime(b.dateTimeMachine) >= dt1 && EntityFunctions.TruncateTime(b.dateTimeMachine) <= dt2
                             orderby b.dateTimeMachine
                             select new
                             {
                                 pkid = b.pkid,
                                 ScaleId = b.ScaleId,
                                 truckIn = b.truckRFIDIN ?? "",
                                 truckOut = b.truckRFIDOUT ?? "",
                                 fairWeight = b.fairWeight,
                                 grossWeight = b.grossWeight,
                                 netweight = b.netWeight,
                                 dateTimeMachine = b.dateTimeMachine,
                                 status = b.status
                             }).Select(x => new
                             {
                                 pkid = x.pkid,
                                 ScaleId = x.ScaleId,
                                 truckIn = x.truckIn,
                                 truckOut = x.truckOut,
                                 fairWeight = x.fairWeight,
                                 grossWeight = x.grossWeight,
                                 netweight = x.netweight,
                                 dateTimeMachine = x.dateTimeMachine,
                                 status = x.status
                             }).ToList();
                    foreach (var item in v.OrderByDescending(x => x.dateTimeMachine))
                    {
                        actualimportdata _obj = new actualimportdata();
                        _obj.pkid = item.pkid;
                        _obj.ScaleId = item.ScaleId;

                        if (item.truckIn != null && item.truckIn != "")
                        {

                            _obj.truckNo = (from rfId in _db.tbl_rfidDetails
                                            where rfId.RFIDNUMBER == item.truckIn
                                            join truckMap in _db.tbl_truckMapping on rfId.pkid equals truckMap.rfif_fkId
                                            join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
                                            select new { truckdetails.truckNo }).FirstOrDefault().truckNo;

                        }
                        else
                        {

                            _obj.truckNo = (from rfId in _db.tbl_rfidDetails
                                            where rfId.RFIDNUMBER == item.truckOut
                                            join truckMap in _db.tbl_truckMapping on rfId.pkid equals truckMap.rfif_fkId
                                            join truckdetails in _db.tbl_TruckDetails on truckMap.truckFKId equals truckdetails.pkid
                                            select new { truckdetails.truckNo }).FirstOrDefault().truckNo;

                        }

                        _obj.netweight = item.netweight.ToString();
                        _obj.tareWeight = item.fairWeight.ToString();
                        _obj.grossWeight = item.grossWeight.ToString();
                        _obj.status = item.status;
                        _obj.dateTimeMachine = (DateTime)item.dateTimeMachine;

                        importData.Add(_obj);


                    }
                    DataTable _table = new DataTable();
                    _table.TableName = "GeneralReport";
                    _table.Columns.Add("S.No.", typeof(string));
                    _table.Columns.Add("Sale Id", typeof(string));
                    _table.Columns.Add("truck No", typeof(string));
                    _table.Columns.Add("Tare Weight", typeof(string));
                    _table.Columns.Add("Gross Weight", typeof(string));
                    _table.Columns.Add("Net Weight", typeof(string));
                    _table.Columns.Add("Date Time", typeof(string));
                    _table.Columns.Add("Status", typeof(string));
                    long srno = 1;
                    foreach (var data in importData)
                    {
                        if (data.netweight != "0.00")
                        {
                            _table.Rows.Add(srno, data.ScaleId, data.truckNo, data.tareWeight, data.grossWeight, data.netweight,
                                data.dateTimeMachine, data.status);
                        }
                        else
                        {
                            _table.Rows.Add(srno, data.ScaleId, data.truckNo, data.tareWeight, data.grossWeight, "ERROR",
                               data.dateTimeMachine, data.status);
                        }
                        srno++;
                    }
                    // create excel 
                    gv.DataSource = _table;
                    gv.DataBind();
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename=truckReport.xls");
                    Response.ContentType = "application/ms-excel";
                    Response.Charset = "";
                    StringWriter sw = new StringWriter();
                    HtmlTextWriter htw = new HtmlTextWriter(sw);
                    gv.RenderControl(htw);
                    Response.Output.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }
                return View();
            }
            catch
            {
                ViewBag.msg = "RFID OR Operator Not Mapped...";
                return View();
            }
        }

    }

}

