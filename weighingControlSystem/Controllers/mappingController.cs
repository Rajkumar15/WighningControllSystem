using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using weighingControlSystem.Models.DAL;
using System.Linq.Dynamic;
using itechDll;
using weighingControlSystem.Models.BAL;
namespace weighingControlSystem.Controllers
{
    public class mappingController : AsyncController
    {
        //GET: /mapping/
        [Authorize]
        [HttpGet]
        public ActionResult truckMapping(int _id = 0)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            ViewBag.trucklist = new SelectList(_db.tbl_TruckDetails.ToList(), "pkid", "truckNo");
            ViewBag.RFIDList = new SelectList(_db.tbl_rfidDetails.ToList(), "pkid", "RFIDNUMBER");

            if (_id > 0)
            {

                return View(_db.tbl_truckMapping.Where(x => x.pkid == _id).FirstOrDefault());
            }
            else
            {
                return View();

            }


        }
        [Authorize]
        [HttpPost]
        public ActionResult truckMapping(tbl_truckMapping model)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            ViewBag.trucklist = new SelectList(_db.tbl_TruckDetails.ToList(), "pkid", "truckNo");
            ViewBag.RFIDList = new SelectList(_db.tbl_rfidDetails.ToList(), "pkid", "RFIDNUMBER");
            model.truckNo = _db.tbl_TruckDetails.Where(x => x.pkid == model.truckFKId).FirstOrDefault().truckNo;
            model.truckRFID = _db.tbl_rfidDetails.Where(x => x.pkid == model.rfif_fkId).FirstOrDefault().RFIDNUMBER;
            if (model.pkid > 0)
            {

                var truckDTL = _db.tbl_truckMapping.Where(x => x.truckRFID == model.truckRFID).ToList();

                if (truckDTL.Count() == 0)
                {
                    _db.Entry(model).State = System.Data.EntityState.Modified;
                    _db.SaveChanges();
                    ViewBag.alert = "Mapping Modified......";
                    ModelState.Clear();
                    return View();

                }
                else
                {

                    ViewBag.alert = "RFID Already Mapped With Another Truck......";
                    return View();
                }
            }
            else
            {
                var truckDTL = _db.tbl_truckMapping.Where(x => x.truckRFID == model.truckRFID).ToList();

                if (truckDTL.Count() == 0)
                {
                    _db.tbl_truckMapping.Add(model);
                    _db.SaveChanges();
                    ViewBag.alert = "Mapping saved......";
                    ModelState.Clear();
                    return View();

                }
                else
                {

                    ViewBag.alert = "RFID Already Mapped With Another Truck.....";
                    return View();

                }

            }
        }


        public ActionResult truckMappingList()
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
                var v = (from a in _db.tbl_truckMapping.Select(x => new { x.pkid, x.truckNo, x.truckRFID, x.comments }) select a);
                //SORT
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    v = (from b in _db.tbl_truckMapping.Where(x => x.truckNo.Contains(search) || x.truckRFID.Contains(search)).Select(x => new { x.pkid, x.truckNo, x.truckRFID, x.comments }) select b);
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
        [HttpGet]
        public ActionResult Contractor(int _id = 0)
        {
            if (_id > 0)
            {
                weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();

                return View(_db.tbl_contractor.Where(x => x.pkid == _id).FirstOrDefault());

            }
            else
            {
                return View();
            }

        }


        public ActionResult ContarctorList()
        {

            return View();
        }


        [Authorize]
        [HttpPost]
        public ActionResult Contractor(tbl_contractor model)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            if (model.pkid > 0)
            {
                _db.Entry(model).State = System.Data.EntityState.Modified;
                _db.SaveChanges();
                ViewBag.alert = "Contractor Modified....";
                ModelState.Clear();
                return View();

            }
            else
            {

                _db.tbl_contractor.Add(model);
                _db.SaveChanges();
                ViewBag.alert = "Contractor Modified....";
                ModelState.Clear();
                return View();
            }


        }

        public ActionResult contractorList()
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
                var v = (from a in _db.tbl_contractor.Select(x => new { x.pkid, x.contratorName, x.Contact, x.email, x.Address }) select a);
                //SORT
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    v = (from b in _db.tbl_contractor.Where(x => x.contratorName.Contains(search) || x.Contact.Contains(search) || x.email.Contains(search) || x.Address.Contains(search)).Select(x => new { x.pkid, x.contratorName, x.Contact, x.email, x.Address }) select b);
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
        [HttpGet]
        public ActionResult operatorShift(int _id = 0)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            if (_id > 0)
            {
                return View(_db.tbl_operatorshift.Where(x => x.pkid == _id).FirstOrDefault());
            }
            else
            {

                return View();
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult operatorShift(tbl_operatorshift model)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            if (model.pkid > 0)
            {

                _db.Entry(model).State = System.Data.EntityState.Modified;
                _db.SaveChanges();
                ViewBag.alert = "Shift Modified.....";
                ModelState.Clear();
                return View();
            }
            else
            {

                _db.tbl_operatorshift.Add(model);
                _db.SaveChanges();
                ModelState.Clear();
                ViewBag.alert = "Shift Saved.....";
                return View();

            }
        }


        public ActionResult shiftList()
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
                var v = (from a in _db.tbl_operatorshift.Select(x => new { x.pkid, x.shiftname, x.shiftstsrtTime, x.shiftEndTime }) select a);
                //SORT
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    v = (from b in _db.tbl_operatorshift.Where(x => x.shiftname.Contains(search)).Select(x => new { x.pkid, x.shiftname, x.shiftstsrtTime, x.shiftEndTime }) select b);
                }
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.OrderBy(sortColumn, sortColumnDir);
                    //v = v.OrderBy(x=>x.orderfkid);
                }
                recordsTotal = v.Count();
                var data = v.Skip(skip).Take(pageSize).ToList();
                List<Shift> newList = new List<Shift>();
                foreach (var item in data)
                {
                    Shift obj = new Shift();
                    obj.pkid = item.pkid;
                    obj.shiftname = item.shiftname;
                    TimeSpan ts = new TimeSpan();
                    ts = (TimeSpan)item.shiftstsrtTime;
                    obj.shiftstsrtTime = ts.Hours + ":" + ts.Minutes + ":" + ts.Milliseconds;
                    ts = new TimeSpan();
                    ts = (TimeSpan)item.shiftEndTime;
                    obj.shiftEndTime = ts.Hours + ":" + ts.Minutes + ":" + ts.Milliseconds;
                    newList.Add(obj);

                }
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = newList }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            { Commonfunction.LogError(e, HttpContext.Server.MapPath("~/ErrorLog.txt")); return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = "" }, JsonRequestBehavior.AllowGet); }




        }

        [Authorize]
        [HttpGet]
        public ActionResult operatormapping(int _id = 0)
        {
            if (_id == 0)
            {

                weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
                ViewBag.operatorList = new SelectList(_db.tbl_operatorDetails.ToList(), "pkid", "OperatorName");
                ViewBag.shift = new SelectList(_db.tbl_operatorshift.ToList(), "pkid", "shiftname");
                ViewBag.operatorRFIDLsit = new SelectList(_db.tbl_rfidDetails.ToList(), "pkid", "RFIDNUMBER");
                ViewBag.truck = new SelectList(_db.tbl_TruckDetails.ToList(), "pkid", "truckNo");
                TimeSpan time1 = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString());
                TimeSpan time2 = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString());

                if (_db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= time1 && x.shiftEndTime >= time2).Count() > 0)
                {
                    ViewBag.shifId = _db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= time1 && x.shiftEndTime >= time2).FirstOrDefault().pkid;
                }
                return View();
            }
            else
            {

                weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
                ViewBag.operatorList = new SelectList(_db.tbl_operatorDetails.ToList(), "pkid", "OperatorName");
                ViewBag.shift = new SelectList(_db.tbl_operatorshift.ToList(), "pkid", "shiftname");
                ViewBag.operatorRFIDLsit = new SelectList(_db.tbl_rfidDetails.ToList(), "pkid", "RFIDNUMBER");
                ViewBag.truck = new SelectList(_db.tbl_TruckDetails.ToList(), "pkid", "truckNo");
                TimeSpan time1 = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString());
                TimeSpan time2 = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString());

                if (_db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= time1 && x.shiftEndTime >= time2).Count() > 0)
                {
                    ViewBag.shifId = _db.tbl_operatorMapping.Where(x => x.pkid == _id).FirstOrDefault().shift_fkid;
                }
                return View(_db.tbl_operatorMapping.Where(x => x.pkid == _id).FirstOrDefault());
            }





        }

        [Authorize]
        [HttpPost]
        public ActionResult operatormapping(tbl_operatorMapping model)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            ViewBag.operatorList = new SelectList(_db.tbl_operatorDetails.ToList(), "pkid", "OperatorName");
            ViewBag.shift = new SelectList(_db.tbl_operatorshift.ToList(), "pkid", "shiftname");
            ViewBag.truck = new SelectList(_db.tbl_TruckDetails.ToList(), "pkid", "truckNo");
            //ViewBag.shifId = _db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= System.DateTime.Now.TimeOfDay && x.shiftEndTime <= System.DateTime.Now.TimeOfDay).FirstOrDefault().pkid;
            ViewBag.operatorRFIDLsit = new SelectList(_db.tbl_rfidDetails.ToList(), "pkid", "RFIDNUMBER");
            //var truck = _db.tbl_truckMapping.Where(n => n.truckRFID == model.operatorFKID).Count();
            //  model.operatorFKID = _db.tbl_rfidDetails.Where(x => x.pkid == model.rfiDfkId).FirstOrDefault().RFIDNUMBER;
            int count = _db.tbl_operatorMapping.Where(x => x.shift_fkid == model.shift_fkid && x.operator_fkId == model.operator_fkId && x.workingdate == model.workingdate).Count();
            if (_db.tbl_operatorMapping.Where(x => x.shift_fkid == model.shift_fkid && x.operator_fkId == model.operator_fkId && x.workingdate == model.workingdate && x.rfiDfkId == model.rfiDfkId).Count() == 0)
            {
                if (_db.tbl_truckMapping.Where(n => n.truckRFID == model.operatorFKID).Count() == 0)
                {
                    if (model.pkid > 0)
                    {
                        _db.Entry(model).State = System.Data.EntityState.Modified;
                        _db.SaveChanges();
                        ModelState.Clear();
                        ViewBag.alert = "Operator Modified.....";
                        return View();
                    }
                    else
                    {

                        _db.tbl_operatorMapping.Add(model);
                        _db.SaveChanges();
                        ModelState.Clear();
                        ViewBag.alert = "Operator saved..... ";
                        return View();
                    }

                }
                else
                {

                    ViewBag.alert = "Invalid Operator RFID ";
                    return View();
                }


            }
            else
            {

                ViewBag.alert = "Operator already exist for this shift ";
                return View();

            }
        }
        public ActionResult operatorshiftList()
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
                var v = (from a in _db.tbl_operatorMapping
                         select new
                         {

                             pkid = a.pkid,
                             operatorName = _db.tbl_operatorDetails.Where(x => x.pkid == a.operator_fkId).FirstOrDefault().OperatorName,
                             truckNo = _db.tbl_TruckDetails.Where(x => x.pkid == a.rfiDfkId).FirstOrDefault().truckNo,
                             shiftName = _db.tbl_operatorshift.Where(x => x.pkid == a.shift_fkid).FirstOrDefault().shiftname,
                             date = a.workingdate
                         });
                //SORT
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    v = (from a in _db.tbl_operatorMapping
                         select new
                         {
                             pkid = a.pkid,
                             operatorName = _db.tbl_operatorDetails.Where(x => x.pkid == a.operator_fkId).FirstOrDefault().OperatorName,
                             truckNo = _db.tbl_TruckDetails.Where(x => x.pkid == a.rfiDfkId).FirstOrDefault().truckNo,
                             shiftName = _db.tbl_operatorshift.Where(x => x.pkid == a.shift_fkid).FirstOrDefault().shiftname,
                             date = a.workingdate

                         }).Where(x => x.operatorName.Contains(search) || x.truckNo.Contains(search));
                }
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.OrderBy(sortColumn, sortColumnDir);
                    //v = v.OrderBy(x=>x.orderfkid);
                }
                recordsTotal = v.Count();
                var data = v.Skip(skip).Take(pageSize).ToList();

                List<OperatorList> opelist = new List<OperatorList>();
                foreach (var item in data)
                {
                    OperatorList _obj = new OperatorList();
                    _obj.pkid = item.pkid;
                    _obj.shiftName = item.shiftName;
                    _obj.truckNo = item.truckNo;
                    _obj.operatorName = item.operatorName;
                    _obj.date = Convert.ToDateTime(item.date).ToShortDateString();
                    opelist.Add(_obj);

                }

                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = opelist }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            { Commonfunction.LogError(e, HttpContext.Server.MapPath("~/ErrorLog.txt")); return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = "" }, JsonRequestBehavior.AllowGet); }



        }

        [Authorize]
        [HttpGet]
        public ActionResult truckDetails(int _id = 0)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            if (_id > 0)
            {
                return View(_db.tbl_TruckDetails.Where(x => x.pkid == _id).FirstOrDefault());
            }
            else
            {

                return View();
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult truckDetails(tbl_TruckDetails model)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();

            if (model.pkid > 0)
            {
                _db.Entry(model).State = System.Data.EntityState.Modified;
                _db.SaveChanges();
                ViewBag.alert = "Truck Details Modifire....";
                ModelState.Clear();
                return View();
            }
            else
            {
                _db.tbl_TruckDetails.Add(model);
                _db.SaveChanges();
                ViewBag.alert = "Truck Details Saved....";
                ModelState.Clear();
                return View();
            }
        }

        public ActionResult getTrucklist()
        {
            return View();
        }


        public ActionResult truckDetaisList()
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
                var v = (from a in _db.tbl_TruckDetails.Select(x => new { x.pkid, x.truckOwner, x.truckNo, x.Contact, x.email, x.address }) select a);
                //SORT
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    v = (from b in _db.tbl_TruckDetails.Where(x => x.truckNo.Contains(search) || x.truckOwner.Contains(search) || x.email.Contains(search)).Select(x => new { x.pkid, x.truckOwner, x.truckNo, x.Contact, x.email, x.address }) select b);
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
        [HttpGet]
        public ActionResult RFIDDetails(int _id = 0)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            if (_id > 0)
            { return View(_db.tbl_rfidDetails.Where(x => x.pkid == _id).FirstOrDefault()); }
            else
            {

                return View();
            }

        }

        [Authorize]
        [HttpPost]
        public ActionResult RFIDDetails(tbl_rfidDetails model)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            if (model.pkid > 0)
            {
                _db.Entry(model).State = System.Data.EntityState.Modified;
                _db.SaveChanges();
                ViewBag.alert = "RFID details modified....";
                ModelState.Clear();
                return View();
            }
            else
            {
                var data = _db.tbl_rfidDetails.Where(x => x.RFIDNUMBER == model.RFIDNUMBER).ToList();
                if (data.Count() == 0)
                {
                    _db.tbl_rfidDetails.Add(model);
                    _db.SaveChanges();
                    ViewBag.alert = "RFID details saved....";
                }
                else
                {
                    ViewBag.alert = "RFID Already Exist Please Change RFID Number....";
                }
                ModelState.Clear();
                return View();

            }
        }

        public ActionResult rfidList()
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
                var v = (from a in _db.tbl_rfidDetails.Select(x => new { x.pkid, x.RFIDNUMBER, x.comments }) select a);
                //SORT
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    v = (from b in _db.tbl_rfidDetails.Where(x => x.RFIDNUMBER.Contains(search)).Select(x => new { x.pkid, x.RFIDNUMBER, x.comments }) select b);
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

        [HttpGet]
        [Authorize]
        public ActionResult operatorDtails(int _id = 0)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            if (_id > 0)
            {

                return View(_db.tbl_operatorDetails.Where(x => x.pkid == _id).FirstOrDefault());
            }
            else
            {

                return View();
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult operatorDtails(tbl_operatorDetails model)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            if (model.pkid > 0)
            {
                _db.Entry(model).State = System.Data.EntityState.Modified;
                _db.SaveChanges();
                ViewBag.alert = "Operator details modified..";
                ModelState.Clear();
                return View();
            }
            else
            {
                _db.tbl_operatorDetails.Add(model);
                _db.SaveChanges();
                ViewBag.alert = "Operator details saved..";
                ModelState.Clear();
                return View();
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult operatoList()
        {
            return View();
        }

        public ActionResult GetOperatorLsit()
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
                var v = (from a in _db.tbl_operatorDetails.Select(x => new { x.pkid, x.OperatorName, x.opEmail, x.Contact, x.Address }) select a);
                //SORT
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    v = (from b in _db.tbl_operatorDetails.Where(x => x.OperatorName.Contains(search) || x.Address.Contains(search) || x.opEmail.Contains(search)).Select(x => new { x.pkid, x.OperatorName, x.opEmail, x.Contact, x.Address }) select b);
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

        [HttpGet]
        [Authorize]
        public ActionResult operatorMaapingWithContactor(int _id = 0)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            ViewBag.contarctor = new SelectList(_db.tbl_contractor.ToList(), "pkid", "contratorName");
            ViewBag.operatorlist = new SelectList(_db.tbl_operatorDetails.ToList(), "pkid", "OperatorName");
            if (_id > 0)
            {
                return View(_db.tbl_operatorMapiingWithContactor.Where(x => x.pkid == _id).FirstOrDefault());
            }
            else
            {
                return View();
            }


        }

        [HttpPost]
        [Authorize]
        public ActionResult operatorMaapingWithContactor(tbl_operatorMapiingWithContactor model)
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();

            ViewBag.contarctor = new SelectList(_db.tbl_contractor.ToList(), "pkid", "contratorName");
            ViewBag.operatorlist = new SelectList(_db.tbl_operatorDetails.ToList(), "pkid", "OperatorName");

            if (model.pkid > 0)
            {
                if (_db.tbl_operatorMapiingWithContactor.Where(x => x.operatorid == model.operatorid && x.contactorId == model.contactorId).Count() == 0)
                {
                    model.operatorname = _db.tbl_operatorDetails.Where(x => x.pkid == model.operatorid).FirstOrDefault().OperatorName;
                    model.contactorName = _db.tbl_contractor.Where(x => x.pkid == model.contactorId).FirstOrDefault().contratorName;
                    _db.Entry(model).State = System.Data.EntityState.Modified;
                    _db.SaveChanges();
                    ViewBag.alert = "Operator mapping modified...";
                    return View();
                }
                else
                {

                    ViewBag.alert = "Operator mapped already exist......";
                }


            }
            else
            {

                if (_db.tbl_operatorMapiingWithContactor.Where(x => x.operatorid == model.operatorid && x.contactorId == model.contactorId).Count() == 0)
                {
                    model.operatorname = _db.tbl_operatorDetails.Where(x => x.pkid == model.operatorid).FirstOrDefault().OperatorName;
                    model.contactorName = _db.tbl_contractor.Where(x => x.pkid == model.contactorId).FirstOrDefault().contratorName;
                    model.cdate = System.DateTime.Now.Date;
                    _db.tbl_operatorMapiingWithContactor.Add(model);
                    _db.SaveChanges();
                }
                else
                {

                    ViewBag.alert = "Operator mapped already exist......";
                }
            }

            return View();
        }


        public ActionResult operatotContractorMappingList()
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
                var v = (from a in _db.tbl_operatorMapiingWithContactor.Select(x => new { x.pkid, x.operatorname, x.contactorName }) select a);
                //SORT
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    v = (from b in _db.tbl_operatorMapiingWithContactor.Where(x => x.operatorname.Contains(search) || x.contactorName.Contains(search)).Select(x => new { x.pkid, x.operatorname, x.contactorName }) select b);
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
    }
}
