using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Web.Hosting;
using System.Configuration;
using CrystalDecisions.CrystalReports.Engine;
using System.Text.RegularExpressions;

namespace Improvar.Controllers
{
    public class T_DistOrderController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_DistOrder
        public ActionResult T_DistOrder(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {//k
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    TransactionDistOrder VE;
                    if (TempData["DistOrderFilter"] == null)
                    {
                        VE = new TransactionDistOrder();
                    }
                    else
                    {
                        VE = (TransactionDistOrder)TempData["DistOrderFilter"];
                        TempData.Keep();
                    }
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                    ViewBag.DistributorName = VE.Dstbrslnm;
                    ViewBag.RetailerName = VE.RetailerName;
                    if (VE.BrandName.retStr() != "")
                    {
                        ViewBag.BrandName = "[" + VE.BrandName + "]";
                    }
                    if (VE.GroupName.retStr() != "")
                    {
                        ViewBag.GroupName = "[" + VE.GroupName + "]";
                    }
                    if (VE.CollName.retStr() != "")
                    {
                        ViewBag.CollectionName = "[" + VE.CollName + "]";
                    }

                    var dt = Salesfunc.GetPendingOrder(VE.T_DISTORDER.SLCD.retSqlformat(), VE.BrandCode.retSqlfromStrarray());
                    dt.Columns.Add("SET", typeof(double), "");
                    dt.Columns.Add("BOX", typeof(double), "");
                    for (int i = 0; i <= dt.Rows.Count - 1; i++)
                    {
                        double qnty = dt.Rows[i]["QNTY"].retDbl();
                        dt.Rows[i]["SET"] = Salesfunc.ConvPcstoSet(qnty, dt.Rows[i]["PCSPERSET"].retDbl());
                        dt.Rows[i]["BOX"] = Salesfunc.ConvPcstoBox(qnty, dt.Rows[i]["PCSPERBOX"].retDbl());
                    }
                    VE.ListPendOrd = (from DataRow dr in dt.Rows
                                      group dr by new
                                      {
                                          RTLCD = dr["RTLCD"].retStr(),
                                          RTLAUTONO = dr["AUTONO"].retStr(),
                                          RTLNM = dr["RTLNM"].retStr(),
                                          RTLAREA = dr["landmark"].retStr() + dr["city"].retStr(),
                                          BRANDNM = dr["BRANDNM"].retStr(),
                                          BRANDCD = dr["BRANDCD"].retStr(),
                                      } into X

                                      select new ListPendOrd
                                      {
                                          RTLCD = X.Key.RTLCD.retStr(),
                                          RTLAUTONO = X.Key.RTLAUTONO.retStr(),
                                          RTLNM = X.Key.RTLNM.retStr(),
                                          RTLAREA = X.Key.RTLAREA.retStr(),
                                          BRANDNM = X.Key.BRANDNM.retStr(),
                                          BRANDCD = X.Key.BRANDCD.retStr(),
                                          QNTY = X.Sum(Z => Z.Field<double>("QNTY").retDbl()),
                                          SET = X.Sum(Z => Z.Field<double>("SET").retDbl()),
                                          BOX = X.Sum(Z => Z.Field<double>("BOX").retDbl()),
                                      }).ToList();

                    for (int i = 0; i <= VE.ListPendOrd.Count() - 1; i++)
                    {
                        VE.ListPendOrd[i].SLNO = (i + 1).retShort();
                        VE.ListPendOrd[i].ORDDET = "Box=" + VE.ListPendOrd[i].BOX + ",Set=" + VE.ListPendOrd[i].SET;
                        string RTLAUTONO = VE.ListPendOrd[i].RTLAUTONO;
                        string BRANDCD = VE.ListPendOrd[i].BRANDCD;

                        VE.ListPendOrdPopup = (from DataRow dr in dt.Rows
                                               where dr["AUTONO"].retStr() == RTLAUTONO && dr["BRANDCD"].retStr() == BRANDCD
                                               select new ListPendOrdPopup()
                                               {
                                                   ParentSerialNo = (i + 1).retShort(),
                                                   STYLENO = dr["STYLENO"].retStr(),
                                                   RTLAUTONO = dr["AUTONO"].retStr(),
                                                   ITCD = dr["ITCD"].retStr(),
                                                   TRTLQNTY = dr["QNTY"].retDbl(),
                                                   PCSPERBOX = dr["PCSPERBOX"].retDbl(),
                                                   PCSPERSET = dr["PCSPERSET"].retDbl(),
                                                   SIZECD = dr["SIZECD"].retStr(),
                                                   ITREM = dr["ITREM"].retStr(),
                                                   MIXSIZE = dr["MIXSIZE"].retStr(),
                                               }).OrderBy(a => a.ITCD).ToList();

                        for (int j = 0; j <= VE.ListPendOrdPopup.Count - 1; j++)
                        {
                            string ITCD = VE.ListPendOrdPopup[j].ITCD;
                            VE.ListPendOrdPopup[j].SLNO = (j + 1).retShort();
                            VE.ListPendOrdPopup[j].TRTLBOX = Salesfunc.ConvPcstoBox(VE.ListPendOrdPopup[j].TRTLQNTY, VE.ListPendOrdPopup[j].PCSPERBOX);
                            VE.ListPendOrdPopup[j].TRTLSET = Salesfunc.ConvPcstoSet(VE.ListPendOrdPopup[j].TRTLQNTY, VE.ListPendOrdPopup[j].PCSPERSET);

                            VE.ListPendOrdPopup[j].SET = VE.ListPendOrdPopup[j].TRTLSET;
                            VE.ListPendOrdPopup[j].QNTY = VE.ListPendOrdPopup[j].TRTLQNTY;
                            VE.ListPendOrdPopup[j].SIZE_COUNT = (from a in DB.M_SITEM_SIZE where a.ITCD == ITCD select a.SIZECD).Count();
                        }
                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        string JR = javaScriptSerializer.Serialize(VE.ListPendOrdPopup);
                        VE.ListPendOrd[i].ChildData = JR;
                    }

                    return View(VE);

                }
            }

            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult SAVE(FormCollection FC, TransactionDistOrder VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    string DefaultAction = "A";
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".T_DISTORDER in  row share mode");
                    if (DefaultAction == "A")
                    {
                        T_DISTORDER TDISTORDER = new T_DISTORDER();
                        TDISTORDER.CLCD = CommVar.ClientCode(UNQSNO);
                        TDISTORDER.DOCDT = System.DateTime.Now.Date;
                        string Ddate = Convert.ToString(TDISTORDER.DOCDT);

                        if (DefaultAction == "A")
                        {
                            TDISTORDER.EMD_NO = 0;
                            string DOCNO = Cn.MaxDocNumber(Ddate);
                            TDISTORDER.VCHRNO = DOCNO.Split(Convert.ToChar(Cn.GCS()))[0].retInt();
                            TDISTORDER.MNTHCD = DOCNO.Split(Convert.ToChar(Cn.GCS()))[1].ToString();

                            TDISTORDER.DOCNO = DocPattern(TDISTORDER.VCHRNO.retDbl(), TDISTORDER.MNTHCD);
                            TDISTORDER.AUTONO = "DST" + VE.T_DISTORDER.SLCD + TDISTORDER.VCHRNO.retStr().PadLeft(5, '0');

                        }
                        else
                        {
                            var MAXEMDNO = (from p in DB.T_DISTORDER where p.AUTONO == TDISTORDER.AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                TDISTORDER.EMD_NO = 0;
                            }
                            else
                            {
                                TDISTORDER.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                            TDISTORDER.VCHRNO = VE.T_DISTORDER.VCHRNO;
                            TDISTORDER.DOCNO = VE.T_DISTORDER.AUTONO;
                            TDISTORDER.AUTONO = VE.T_DISTORDER.AUTONO;
                            TDISTORDER.MNTHCD = VE.T_DISTORDER.MNTHCD;
                            TDISTORDER.DTAG = "E";
                        }
                        TDISTORDER.RTLCD = VE.T_DISTORDER.RTLCD;
                        TDISTORDER.SLCD = VE.T_DISTORDER.SLCD;
                        TDISTORDER.SLMSLCD = VE.T_DISTORDER.SLMSLCD;
                        TDISTORDER.DOCAMT = VE.T_DISTORDER.DOCAMT;

                        TDISTORDER.USR_ID = CommVar.UserID();
                        TDISTORDER.USR_ENTDT = System.DateTime.Now;
                        TDISTORDER.USR_SIP = Cn.GetStaticIp();

                        //TDISTORDER.LM_USR_ID = CommVar.UserID();
                        //TDISTORDER.LM_USR_ENTDT = System.DateTime.Now;
                        //TDISTORDER.LM_USR_SIP = Cn.GetStaticIp();
                        //TDISTORDER.LM_REM = "";

                        //TDISTORDER.DEL_USR_ID = CommVar.UserID();
                        //TDISTORDER.DEL_USR_ENTDT = System.DateTime.Now;
                        //TDISTORDER.DEL_USR_SIP =Cn.GetStaticIp();
                        //TDISTORDER.DEL_REM = "";

                        //TDISTORDER.CANCEL = "Y";
                        //TDISTORDER.CANC_REM = "";
                        //TDISTORDER.CANC_USR_ID = CommVar.UserID();
                        //TDISTORDER.CANC_USR_ENTDT = System.DateTime.Now;
                        //TDISTORDER.CANC_USR_SIP =Cn.GetStaticIp();

                        TDISTORDER.GPSLAT = VE.T_DISTORDER.GPSLAT;
                        TDISTORDER.GPSLOT = VE.T_DISTORDER.GPSLOT;
                        TDISTORDER.DOCREM = VE.T_DISTORDER.DOCREM;
                        TDISTORDER.GPSNM = GetAddress(VE.T_DISTORDER.GPSLAT.retStr(), VE.T_DISTORDER.GPSLOT.retStr());


                        if (DefaultAction == "A")
                        {
                            DB.T_DISTORDER.Add(TDISTORDER);
                        }
                        else if (DefaultAction == "E")
                        {
                            DB.Entry(TDISTORDER).State = System.Data.Entity.EntityState.Modified;
                        }
                        int slno = 0;
                        for (int i = 0; i <= VE.ListPendOrd.Count - 1; i++)
                        {

                            if (VE.ListPendOrd[i].ChildData != null && VE.ListPendOrd[i].ChildData != "[]")
                            {
                                string data = VE.ListPendOrd[i].ChildData;
                                var helpM = new List<Improvar.Models.ListPendOrdPopup>();
                                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                helpM = javaScriptSerializer.Deserialize<List<Improvar.Models.ListPendOrdPopup>>(data);
                                for (int j = 0; j <= helpM.Count - 1; j++)
                                {
                                    if (helpM[j].QNTY != 0)
                                    {
                                        slno++;
                                        T_DISTORDERDTL TDISTORDERDTL = new T_DISTORDERDTL();
                                        TDISTORDERDTL.CLCD = TDISTORDER.CLCD;
                                        TDISTORDERDTL.EMD_NO = TDISTORDER.EMD_NO;
                                        TDISTORDERDTL.AUTONO = TDISTORDER.AUTONO;
                                        TDISTORDERDTL.DTAG = TDISTORDER.DTAG;
                                        TDISTORDERDTL.ITCD = helpM[j].ITCD;
                                        TDISTORDERDTL.SLNO = slno.retShort();
                                        TDISTORDERDTL.SIZECD = helpM[j].SIZECD;
                                        TDISTORDERDTL.FREESTK = "";
                                        TDISTORDERDTL.QNTY = helpM[j].QNTY;
                                        TDISTORDERDTL.TRTLQNTY = helpM[j].TRTLQNTY;
                                        TDISTORDERDTL.TSTKQNTY = helpM[j].TSTKQNTY;
                                        DB.T_DISTORDERDTL.Add(TDISTORDERDTL);


                                        T_DISTORDLINK TDISTORDLINK = new T_DISTORDLINK();
                                        TDISTORDLINK.CLCD = TDISTORDER.CLCD;
                                        TDISTORDLINK.EMD_NO = TDISTORDER.EMD_NO;
                                        TDISTORDLINK.AUTONO = TDISTORDER.AUTONO;
                                        TDISTORDLINK.DTAG = TDISTORDER.DTAG;
                                        TDISTORDLINK.RTLAUTONO = helpM[j].RTLAUTONO;
                                        TDISTORDLINK.SLNO = slno.retShort();
                                        TDISTORDLINK.ORDSKIPREASON = helpM[j].ORDSKIPREASON;
                                        DB.T_DISTORDLINK.Add(TDISTORDLINK);
                                    }
                                }


                            }
                        }


                        DB.SaveChanges();
                        ModelState.Clear();
                        transaction.Commit();

                        string ContentFlg = "";
                        if (DefaultAction == "A")
                        {
                            ContentFlg = "1~(Order No. " + TDISTORDER.DOCNO + ")";
                        }
                        else if (DefaultAction == "E")
                        {
                            ContentFlg = "2";
                        }
                        return Content(ContentFlg);

                    }
                    //else if (DefaultAction == "V")
                    //{
                    //    T_CNTRL_HDR MCH = Cn.T_CONTROL_HDR(VE.T_CNTRL_HDR.DOCCD, VE.T_CNTRL_HDR.DOCDT, VE.T_CNTRL_HDR.DOCNO, VE.T_DISTORDER.AUTONO, VE.T_CNTRL_HDR.MNTHCD, VE.T_CNTRL_HDR.DOCNO, "D", CommVar.CurSchema(UNQSNO), "", VE.T_DISTORDER.SLCD, 0, "", VE.T_CNTRL_HDR.YR_CD);
                    //    DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                    //    DB.SaveChanges();

                    //    DB.T_DISTORDER.Where(x => x.AUTONO == VE.T_DISTORDER.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                    //    DB.SaveChanges();

                    //    DB.T_DISTORDER.RemoveRange(DB.T_DISTORDER.Where(x => x.AUTONO == VE.T_DISTORDER.AUTONO));
                    //    DB.SaveChanges();
                    //    DB.T_CNTRL_HDR.RemoveRange(DB.T_CNTRL_HDR.Where(x => x.AUTONO == VE.T_DISTORDER.AUTONO));
                    //    DB.SaveChanges();
                    //    ModelState.Clear();
                    //    transaction.Commit();
                    //    return Content("3");
                    //}
                    else
                    {
                        return Content("");
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
            return null;
        }
        public ActionResult OpenPopupSize(TransactionDistOrder VE, int ParentSerialNo)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            try
            {
                //Cn.getQueryString(VE);
                ListPendOrd query = (from c in VE.ListPendOrd where (c.SLNO == ParentSerialNo) select c).SingleOrDefault();
                if (query != null)
                {
                    string brandcd = query.BRANDCD.retSqlformat();
                    string RTLAUTONO = query.RTLAUTONO.retSqlformat();
                    if (query.ChildData == null || query.ChildData == "[]")
                    {
                        DataTable tbl = Salesfunc.GetPendingOrder(VE.T_DISTORDER.SLCD.retSqlformat(), brandcd, RTLAUTONO);
                        if (tbl != null && tbl.Rows.Count > 0)
                        {
                            VE.ListPendOrdPopup = (from DataRow dr in tbl.Rows
                                                   select new ListPendOrdPopup()
                                                   {
                                                       ParentSerialNo = ParentSerialNo,
                                                       STYLENO = dr["STYLENO"].retStr(),
                                                       RTLAUTONO = dr["AUTONO"].retStr(),
                                                       ITCD = dr["ITCD"].retStr(),
                                                       TRTLQNTY = dr["QNTY"].retDbl(),
                                                       PCSPERBOX = dr["PCSPERBOX"].retDbl(),
                                                       PCSPERSET = dr["PCSPERSET"].retDbl(),
                                                       SIZECD = dr["SIZECD"].retStr(),
                                                       ITREM = dr["ITREM"].retStr(),
                                                       MIXSIZE = dr["MIXSIZE"].retStr(),
                                                   }).OrderBy(a => a.ITCD).ToList();

                            for (int i = 0; i <= VE.ListPendOrdPopup.Count - 1; i++)
                            {
                                string ITCD = VE.ListPendOrdPopup[i].ITCD;

                                VE.ListPendOrdPopup[i].SLNO = (i + 1).retShort();
                                VE.ListPendOrdPopup[i].TRTLBOX = Salesfunc.ConvPcstoBox(VE.ListPendOrdPopup[i].TRTLQNTY, VE.ListPendOrdPopup[i].PCSPERBOX);
                                VE.ListPendOrdPopup[i].TRTLSET = Salesfunc.ConvPcstoSet(VE.ListPendOrdPopup[i].TRTLQNTY, VE.ListPendOrdPopup[i].PCSPERSET);

                                VE.ListPendOrdPopup[i].SET = VE.ListPendOrdPopup[i].TRTLSET;
                                VE.ListPendOrdPopup[i].QNTY = VE.ListPendOrdPopup[i].TRTLQNTY;
                                VE.ListPendOrdPopup[i].SIZE_COUNT = (from a in DB.M_SITEM_SIZE where a.ITCD == ITCD select a.SIZECD).Count();
                            }
                        }
                        else
                        {
                            List<ListPendOrdPopup> ListPendOrdPopup = new List<ListPendOrdPopup>();
                            VE.ListPendOrdPopup = ListPendOrdPopup;
                        }


                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        string JR = javaScriptSerializer.Serialize(VE.ListPendOrdPopup);
                        query.ChildData = JR;


                    }
                    else
                    {
                        var helpM1 = new List<Improvar.Models.ListPendOrdPopup>();
                        var javaScriptSerializer1 = new System.Web.Script.Serialization.JavaScriptSerializer();
                        if (query.ChildData != null)
                        {
                            helpM1 = javaScriptSerializer1.Deserialize<List<Improvar.Models.ListPendOrdPopup>>(query.ChildData);
                        }
                        if (helpM1 != null)
                        {
                            if (helpM1.Count > 0)
                            {
                                VE.ListPendOrdPopup = helpM1;
                            }
                        }
                    }
                }
                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_T_DistOrder_Popup", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }

        }
        public ActionResult ClosePopupSize(TransactionDistOrder VE, int ParentSerialNo)
        {
            ListPendOrd query = (from c in VE.ListPendOrd where (c.SLNO == ParentSerialNo) select c).SingleOrDefault();
            if (VE.ListPendOrdPopup != null)
            {
                double totbox = 0, totset = 0;
                foreach (var v in VE.ListPendOrdPopup)
                {
                    totbox += Salesfunc.ConvPcstoBox(v.QNTY, v.PCSPERBOX);
                    totset += Salesfunc.ConvPcstoSet(v.QNTY, v.PCSPERSET);
                }
                if (query != null)
                {

                    query.ORDDET = "Box=" + totbox + ",Set=" + totset;
                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string JR = javaScriptSerializer.Serialize(VE.ListPendOrdPopup);
                    query.ChildData = JR;
                }
            }
            else
            {
                query.ChildData = null;
            }
            VE.DefaultView = true;
            ModelState.Clear();
            return PartialView("_T_DistOrder_Main", VE);
        }

        public string GetAddress(string lat, string lng)
        {
            try
            {
                string datastring = "";
                //lat = "22.555"; lng = "88.258";
                var url = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&sensor=true&key=AIzaSyBDxBcnd3Jf8nDInK1xxCSvtRwSiWB4mck";
                WebRequest rqst = HttpWebRequest.Create(url);
                using (HttpWebResponse rspns = (HttpWebResponse)rqst.GetResponse())
                {
                    Stream strm = (Stream)rspns.GetResponseStream();
                    StreamReader strmrdr = new StreamReader(strm);
                    datastring = strmrdr.ReadToEnd();
                    strm.Close();
                    strmrdr.Close();
                    rspns.Close();
                }
                GeoLocation geoLocation = JsonConvert.DeserializeObject<GeoLocation>(datastring);
                var address = geoLocation.results[0].formatted_address;
                return address;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return "";
            }
        }

        public class AddressComponent
        {
            public string long_name { get; set; }
            public string short_name { get; set; }
            public List<string> types { get; set; }
        }

        public class Result
        {
            public List<AddressComponent> address_components { get; set; }
            public string formatted_address { get; set; }
        }

        public class GeoLocation
        {
            public List<Result> results { get; set; }
            public string status { get; set; }
        }

        public string DocPattern(double docno, string mnthcd)
        {

            string[] dfinyr = CommVar.FinPeriod(UNQSNO).Split('-');
            string finyr = "", yy = "";
            yy = dfinyr[0].ToString().Trim().Substring(8);
            if (yy == dfinyr[1].ToString().Trim().Substring(8)) finyr = yy;
            else finyr = yy + "-" + dfinyr[1].ToString().Trim().Substring(8);

            string newPattern = "HP/DOR/" + docno.retStr().PadLeft(5, '0') + "/" + finyr;

            return newPattern;
        }


    }
}

