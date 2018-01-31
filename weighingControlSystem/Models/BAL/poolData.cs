using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using weighingControlSystem.Models.DAL;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Data;
using itechDll;
using System.IO;
namespace weighingControlSystem.Models.BAL
{
    public class poolData
    {

        public void start()
        {
            weighingcontrolSystemEntities _db = new weighingcontrolSystemEntities();
            foreach (var server in _db.tbl_RemoteConnection.ToList())
            {

#region

     string connectionstring = "Data Source=" + server.serverName + ";Initial Catalog=" + server.dataBaseName + ";User ID=" + server.userName + ";Password=" + server.password + ";";
            //string connectionString=_db.tbl_accessConnection.FirstOrDefault().accessconnection;

            SqlConnection con = new SqlConnection(connectionstring);
                     //OleDbConnection con= new OleDbConnection(connectionstring);
            SqlCommand cmd;
           string date1="";
           #region
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

                    DateTime dt = DateTime.Now.Date;
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
                                            smd.outNetWeight = smd.grossWeight;
                                            smd.outPkid = smd.machinePkid;
                                            smd.netWeight = 0;

                                        }

                                        if (smd.truckRFIDOUT != "" && smd.truckRFIDOUT != null && smd.status == "OUT")
                                        {
                                            try
                                            {
                                                long maxId = _db.tbl_storemachineData.Where(x => x.truckRFIDIN == smd.truckRFIDOUT && x.outPkid == null).Max(x => x.pkid);
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
                                                else 
                                                {
                                                    _db.tbl_storemachineData.Add(smd);
                                                    _db.SaveChanges();
                                                }
                                            }
                                            catch
                                            {


                                            }
                                        }
                                     
                                    }
                                }


                            }


                        }



                    }
                }

                catch (Exception ex)
                {
                    //itechDll.Commonfunction.LogError(ex, @"~/Errorlog.txt");
                    Commonfunction.LogError(ex, Path.Combine("~/ErrorLog.txt"));

                }
           #endregion
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
                        // shift Time
                        TimeSpan time1 = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString());
                        TimeSpan time2 = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString());
                        if (_db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= time1 && x.shiftEndTime >= time2).Count() > 0)
                        {
                            smd.shiftTime = _db.tbl_operatorshift.Where(x => x.shiftstsrtTime <= time1 && x.shiftEndTime >= time2).FirstOrDefault().pkid;
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

                                    if (_db.tbl_storemachineData.Where(x => x.truckRFIDIN == smd.truckRFIDOUT && x.outPkid == null).Count() > 0)
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
                                        smd.outNetWeight = smd.grossWeight;
                                        smd.outPkid = smd.machinePkid;
                                        smd.netWeight = 0;

                                    }

                                    if (smd.truckRFIDOUT != "" && smd.truckRFIDOUT != null && smd.status == "OUT")
                                    {
                                        try
                                        {
                                            long maxId = 0;
                                            try
                                            {
                                                maxId = _db.tbl_storemachineData.Where(x => x.truckRFIDIN == smd.truckRFIDOUT).Max(x => x.pkid);
                                            }
                                            catch { }
                                            if (maxId > 0)
                                            {
                                                var updateData = _db.tbl_storemachineData.Where(x => x.pkid == maxId).FirstOrDefault();
                                                // masterid = Convert.ToInt32(ds.Tables[0].Rows[i][0]);
                                                updateData.outPkid = Convert.ToInt32(ds.Tables[0].Rows[i][0].ToString().Trim());
                                                updateData.material = ds.Tables[0].Rows[i][5].ToString().Trim();
                                                //updateData.truckRFIDIN = ds.Tables[0].Rows[i][2].ToString().Trim();
                                                try
                                                {
                                                    updateData.outGrossWeight = Convert.ToDecimal(ds.Tables[0].Rows[i][6].ToString().Trim());
                                                }
                                                catch
                                                {
                                                    //cmd.CommandText = "SELECT top 1 TgrossWeight FROM [TBForce].[dbo].[tbl_WeighingMachineFill] WHERE pkid < (" + masterid + ") and RFID1 = " + updateData.truckRFIDIN + "ORDER BY pkid DESC";
                                                    //cmd.CommandType = CommandType.Text;
                                                    //cmd.Connection = con;
                                                    //con.Open();
                                                    //returnValue = cmd.ExecuteScalar();
                                                    //con.Close();
                                                    //smd.grossWeight = Convert.ToDecimal(returnValue);
                                                }
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
                                            else
                                            {
                                                _db.tbl_storemachineData.Add(smd);
                                                _db.SaveChanges();
                                            }
                                        }
                                        catch
                                        {


                                        }
                                    }
                                }
                                else
                                {
                                   
                                }
                            }
                        }
                        else
                        {
                            if (smd.grossWeight > 50)
                            {
                                smd.truckRFIDOUT = "0088";
                                smd.status = "OUT";
                                smd.netWeight = smd.grossWeight;
                                smd.fairWeight = smd.grossWeight;
                                smd.outGrossWeight = smd.grossWeight;
                                smd.outNetWeight = smd.grossWeight;
                                smd.outPkid = smd.machinePkid;
                                _db.tbl_storemachineData.Add(smd);
                                _db.SaveChanges();
                            }
                            else
                            {
                                smd.truckRFIDIN = "0088";
                                smd.status = "IN";
                                smd.netWeight = 0;
                                smd.fairWeight = smd.grossWeight;
                                _db.tbl_storemachineData.Add(smd);
                                _db.SaveChanges();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Commonfunction.LogError(ex, Path.Combine("~/ErrorLog.txt"));
                }
            }
        }
            #endregion
            }

        }
    }
