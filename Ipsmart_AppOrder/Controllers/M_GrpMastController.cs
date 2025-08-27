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

namespace Improvar.Controllers
{
    public class M_GrpMastController : Controller
    {
        Connection Cn = new Connection(); string sql = "";
        MasterHelp masterHelp = new MasterHelp();
        M_CNTRL_HDR sll; M_GENLEG sGEN;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_GrpMast
        public ActionResult M_GrpMast(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {//k
            VMRetailOrder VE = new VMRetailOrder();
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ViewBag.formname = "ORDER TAKEN FROM RETAILER";
                    ViewBag.Title = "Order";
                    VE.UNQSNO_ENCRYPTED = Cn.Encrypt_URL(UNQSNO);

                    string GCS = Cn.GCS();
                    string[] linkcd = { "D", "A" };

                    string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmp = CommVar.PaySchema(UNQSNO);
                    string tdt = System.DateTime.Now.Date.retDateStr();
                    string uid = CommVar.UserID();
                    DataTable tbl = new DataTable();

                    string sql = "";
                    sql += "select a.slmslcd, a.effdt, a.enm, b.agslcd, c.slnm agslnm from " + Environment.NewLine;
                    sql += "(select a.slmslcd, a.effdt, a.enm from( " + Environment.NewLine;
                    sql += "select a.slmslcd, a.effdt, b.enm, " + Environment.NewLine;
                    sql += "row_number() over(partition by a.slmslcd order by a.effdt desc) rno " + Environment.NewLine;
                    sql += "from " + scm + ".m_slsmn_hdr a, " + scmp + ".m_empmas b " + Environment.NewLine;
                    sql += "where a.slmslcd = b.empcd(+) and b.dol is null and " + Environment.NewLine;
                    sql += "a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and " + Environment.NewLine;
                    sql += "b.impvr_loginid = '" + uid + "') a " + Environment.NewLine;
                    sql += "where rno = 1) a, " + Environment.NewLine;
                    sql += " " + Environment.NewLine;
                    sql += "(select a.slmslcd, a.effdt, a.agslcd " + Environment.NewLine;
                    sql += "from " + scm + ".m_slsmn_agent a ) b, " + Environment.NewLine;
                    sql += "" + scmf + ".m_subleg c " + Environment.NewLine;
                    sql += "where a.slmslcd = b.slmslcd(+) and a.effdt = b.effdt(+) and b.agslcd = c.slcd(+) " + Environment.NewLine;
                    sql += "order by slmslcd, agslnm " + Environment.NewLine;
                    tbl = masterHelp.SQLquery(sql);
                    if (tbl != null && tbl.Rows.Count > 0)
                    {
                        VE.SLMSLCD = tbl.Rows[0]["slmslcd"].retStr();
                    }
                    if (VE.SLMSLCD.retStr() != "")
                    {
                        sql = "";
                        sql += "select a.slmslcd, a.DISTSLCD , b.slnm DISTSLnm, nvl(b.slarea, b.district) slarea from " + Environment.NewLine;
                        sql += "" + scm + ".m_slsmn_agent a," + scmf + ".m_subleg b " + Environment.NewLine;
                        sql += "where a.DISTSLCD  = b.slcd(+) " + Environment.NewLine;
                        sql += "and a.effdt=(select a.effdt from " + Environment.NewLine;
                        sql += "(select a.slmslcd, a.effdt, " + Environment.NewLine;
                        sql += "row_number() over(partition by a.slmslcd order by a.effdt desc) rno " + Environment.NewLine;
                        sql += "from " + scm + ".m_slsmn_agent a " + Environment.NewLine;
                        sql += "where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and " + Environment.NewLine;
                        sql += "a.slmslcd = '" + VE.SLMSLCD + "') a " + Environment.NewLine;
                        sql += "where rno = 1 )  " + Environment.NewLine;
                        sql += "and a.slmslcd = '" + VE.SLMSLCD + "' " + Environment.NewLine;
                        sql += "order by slnm " + Environment.NewLine;

                        tbl = masterHelp.SQLquery(sql);

                        VE.ListDistributor = (from DataRow a in tbl.Rows
                                              select new ListDistributor()
                                              {
                                                  value = a["DISTSLCD"].retStr(),
                                                  text = a["DISTSLnm"].retStr() + GCS + a["SLAREA"].retStr(),
                                              }).ToList();
                        VE.ListRetailer = new List<ListRetailer>();

                        sql = "";
                        sql += "select a.slmslcd, a.brandcd, b.brandnm from " + Environment.NewLine;
                        sql += "" + scm + ".m_slsmn_brand a," + scm + ".m_brand b where a.effdt = " + Environment.NewLine;
                        sql += "(select a.effdt from " + Environment.NewLine;
                        sql += "(select a.slmslcd, a.brandcd, a.effdt, " + Environment.NewLine;
                        sql += "row_number() over(partition by a.slmslcd order by a.effdt desc) rno " + Environment.NewLine;
                        sql += "from " + scm + ".m_slsmn_brand a " + Environment.NewLine;
                        sql += "where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and " + Environment.NewLine;
                        sql += "a.slmslcd = '" + VE.SLMSLCD + "') a " + Environment.NewLine;
                        sql += "where rno = 1 ) " + Environment.NewLine;
                        sql += "and a.brandcd = b.brandcd(+) " + Environment.NewLine;
                        sql += "and a.slmslcd = '" + VE.SLMSLCD + "' " + Environment.NewLine;
                        sql += "order by brandnm " + Environment.NewLine;
                        tbl = masterHelp.SQLquery(sql);

                        VE.ListBrand = (from DataRow a in tbl.Rows
                                        select new ListBrand()
                                        {
                                            value = a["BRANDCD"].retStr(),
                                            text = a["BRANDNM"].retStr(),
                                        }).ToList();

                        VE.ListGroup = new List<ListGroup>();

                        sql = "";
                        sql += "select distinct a.COLLCD, a.COLLNM " + Environment.NewLine;
                        sql += "from " + scm + ".M_COLLECTION a, " + scm + ".m_cntrl_hdr b, " + scm + ".m_sitem c, " + scm + ".m_itemorder d " + Environment.NewLine;
                        sql += "where a.m_autono = b.m_autono(+) and nvl(b.inactive_tag, 'N')= 'N' and a.collcd = c.collcd(+) and c.itcd = d.itcd " + Environment.NewLine;
                        sql += "order by COLLNM " + Environment.NewLine;
                        tbl = masterHelp.SQLquery(sql);

                        VE.ListCollection = (from DataRow a in tbl.Rows
                                             select new ListCollection()
                                             {
                                                 value = a["COLLCD"].retStr(),
                                                 text = a["COLLNM"].retStr(),
                                             }).ToList();
                    }
                    else
                    {
                        VE.ListDistributor = new List<ListDistributor>();
                        VE.ListRetailer = new List<ListRetailer>();
                        VE.ListBrand = new List<ListBrand>();
                        VE.ListGroup = new List<ListGroup>();
                        VE.ListCollection = new List<ListCollection>();
                    }
                    VE.DefaultView = true;
                    return View(VE);

                }
            }

            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public JsonResult BindRetailerData(string Distributor)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            try
            {
                VMRetailOrder VE = new VMRetailOrder();
                string GCS = Cn.GCS();

                string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
                string tdt = System.DateTime.Now.Date.retDateStr();
                string sql = "";
                sql += "select a.rtlcd, a.rtlnm, a.landmark from " + Environment.NewLine;
                sql += "(select a.rtlcd, c.rtlnm, c.landmark, " + Environment.NewLine;
                sql += "row_number() over(partition by a.rtlcd order by a.effdt desc) rno " + Environment.NewLine;
                sql += "from " + scm + ".m_retail_link a, " + scm + ".m_cntrl_hdr b, " + scm + ".m_retail c " + Environment.NewLine;
                sql += "where a.m_autono = b.m_autono(+) and a.rtlcd = c.rtlcd(+) and nvl(b.inactive_tag, 'N') = 'N' and " + Environment.NewLine;
                sql += "a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and " + Environment.NewLine;
                sql += "a.slcd = '" + Distributor + "' ) a " + Environment.NewLine;
                sql += "  where rno = 1 " + Environment.NewLine;
                sql += "order by rtlnm " + Environment.NewLine;

                DataTable tbl = masterHelp.SQLquery(sql);
                if (tbl != null && tbl.Rows.Count > 0)
                {
                    VE.ListRetailer = (from DataRow a in tbl.Rows
                                       select new ListRetailer()
                                       {
                                           value = a["RTLCD"].retStr(),
                                           text = a["RTLNM"].retStr() + GCS + a["LANDMARK"].retStr(),
                                       }).ToList();
                }
                else
                {
                    VE.ListRetailer = new List<ListRetailer>();
                }


                ModelState.Clear();
                return Json(VE.ListRetailer, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                dic.Add("message", ex.Message + ex.InnerException);
                Cn.SaveException(ex, "");
            }
            return Json(dic, JsonRequestBehavior.AllowGet);
        }
        public JsonResult BindGroupData(VMRetailOrder VE)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            try
            {
                string brandcd = "''";
                if (VE.BrandCode != null)
                {
                    brandcd = VE.BrandCode.retSqlfromStrarray();
                }

                string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
                string sql = "";
                sql += "select distinct a.ITGRPCD, a.ITGRPNM " + Environment.NewLine;
                sql += "from " + scm + ".M_GROUP a, " + scm + ".m_cntrl_hdr b, " + scm + ".m_sitem c, " + scm + ".m_itemorder d " + Environment.NewLine;
                sql += "where a.m_autono = b.m_autono(+)  and nvl(b.inactive_tag, 'N')= 'N' and " + Environment.NewLine;
                sql += "a.brandcd IN (" + brandcd + ") and " + Environment.NewLine;
                sql += "a.itgrpcd = c.itgrpcd(+) and c.itcd = d.itcd " + Environment.NewLine;
                sql += "order by ITGRPNM " + Environment.NewLine;

                DataTable tbl = masterHelp.SQLquery(sql);
                if (tbl != null && tbl.Rows.Count > 0)
                {
                    VE.ListGroup = (from DataRow a in tbl.Rows
                                    select new ListGroup()
                                    {
                                        value = a["ITGRPCD"].retStr(),
                                        text = a["ITGRPNM"].retStr(),
                                    }).ToList();
                }
                else
                {
                    VE.ListGroup = new List<ListGroup>();
                }


                ModelState.Clear();
                return Json(VE.ListGroup, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                dic.Add("message", ex.Message + ex.InnerException);
                Cn.SaveException(ex, "");
            }
            return Json(dic, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetItem(VMRetailOrder TSP)
        {
            TransactionRetailOrder ind = new TransactionRetailOrder();
            ind.Dstbrslnm = TSP.Dstbrslnm.Split(Convert.ToChar(Cn.GCS()))[0];
            ind.RetailerName = TSP.RetailerName.Split(Convert.ToChar(Cn.GCS()))[0];
            ind.BrandCode = TSP.BrandCode;
            ind.BrandName = TSP.BrandName;
            ind.GroupCode = TSP.GroupCode;
            ind.GroupName = TSP.GroupName;
            ind.CollCode = TSP.CollCode;
            ind.CollName = TSP.CollName;

            T_RETAILORDER TRETAILORDER = new T_RETAILORDER();
            TRETAILORDER.SLCD = TSP.Dstbrslcd;
            TRETAILORDER.RTLCD = TSP.RetailerCode;
            TRETAILORDER.SLMSLCD = TSP.SLMSLCD;
            ind.T_RETAILORDER = TRETAILORDER;

            if (TempData["OrderFilter"] != null)
            {
                TempData.Remove("OrderFilter");
            }
            TempData["OrderFilter"] = ind;
            return Content("");
        }
        public ActionResult OpenRetailMaster(VMRetailOrder TSP)
        {
            RetailOutletEntry ind = new RetailOutletEntry();
            ind.Dstbrslcd = TSP.Dstbrslcd;

            M_RETAIL TRETAILORDER = new M_RETAIL();
            TRETAILORDER.SLMSLCD = TSP.SLMSLCD;
            ind.M_RETAIL = TRETAILORDER;

            if (TempData["OrderFilterRetail"] != null)
            {
                TempData.Remove("OrderFilterRetail");
            }
            TempData["OrderFilterRetail"] = ind;
            return Content("");
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
    }
}

