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

                    string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
                    DataTable tbl = new DataTable();
                    string sql = "";
                    sql = "select distinct a.slcd, a.slnm,nvl(a.SLAREA,a.DISTRICT)SLAREA ";
                    sql += "from " + scmf + ".M_SUBLEG a, " + scmf + ".m_cntrl_hdr b, " + scmf + ".M_SUBLEG_LINK c ";
                    sql += "where a.m_autono=b.m_autono(+) and a.slcd=c.slcd(+) and c.LINKCD in ('D','A')  ";
                    sql += "and nvl(b.inactive_tag,'N')='N' ";
                    sql += "order by slnm ";
                    tbl = masterHelp.SQLquery(sql);

                    VE.ListDistributor = (from DataRow a in tbl.Rows
                                          select new ListDistributor()
                                          {
                                              value = a["SLCD"].retStr(),
                                              text = a["SLNM"].retStr() + GCS + a["SLAREA"].retStr(),
                                          }).ToList();
                    VE.ListRetailer = new List<ListRetailer>();

                    sql = "select distinct a.BRANDCD, a.BRANDNM ";
                    sql += "from " + scm + ".M_BRAND a, " + scm + ".m_cntrl_hdr b ";
                    sql += "where a.m_autono=b.m_autono(+)  ";
                    sql += "and nvl(b.inactive_tag,'N')='N' ";
                    sql += "order by BRANDNM ";
                    tbl = masterHelp.SQLquery(sql);

                    VE.ListBrand = (from DataRow a in tbl.Rows
                                    select new ListBrand()
                                    {
                                        value = a["BRANDCD"].retStr(),
                                        text = a["BRANDNM"].retStr(),
                                    }).ToList();


                    sql = "select distinct a.ITGRPCD, a.ITGRPNM ";
                    sql += "from " + scm + ".M_GROUP a, " + scm + ".m_cntrl_hdr b ";
                    sql += "where a.m_autono=b.m_autono(+)  ";
                    sql += "and nvl(b.inactive_tag,'N')='N' ";
                    sql += "order by ITGRPNM ";
                    tbl = masterHelp.SQLquery(sql);

                    VE.ListGroup = (from DataRow a in tbl.Rows
                                    select new ListGroup()
                                    {
                                        value = a["ITGRPCD"].retStr(),
                                        text = a["ITGRPNM"].retStr(),
                                    }).ToList();

                    sql = "select distinct a.COLLCD, a.COLLNM ";
                    sql += "from " + scm + ".M_COLLECTION a, " + scm + ".m_cntrl_hdr b ";
                    sql += "where a.m_autono=b.m_autono(+)  ";
                    sql += "and nvl(b.inactive_tag,'N')='N' ";
                    sql += "order by COLLNM ";
                    tbl = masterHelp.SQLquery(sql);

                    VE.ListCollection = (from DataRow a in tbl.Rows
                                         select new ListCollection()
                                         {
                                             value = a["COLLCD"].retStr(),
                                             text = a["COLLNM"].retStr(),
                                         }).ToList();

                    //string brand = "CHOC";// "REVO";
                    //string scm = CommVar.CurSchema(UNQSNO);
                    //string fscm = CommVar.FinSchema(UNQSNO);
                    //string comp = CommVar.Compcd(UNQSNO);
                    //string loc = CommVar.Loccd(UNQSNO);
                    //string doccd = "";

                    //string sql = "";
                    //sql += " select a.m_autono,a.itcd,a.styleno, listagg(C.SIZECD, ',') within group (order by a.itcd) as sizes";
                    //sql += " from " + CommVar.CurSchema(UNQSNO) + ".m_sitem a, " + CommVar.CurSchema(UNQSNO) + ".m_group b, " + CommVar.CurSchema(UNQSNO) + ".m_sitem_size c";
                    //sql += " where a.itgrpcd = b.itgrpcd and a.itcd = c.itcd and b.brandcd = '" + brand + "'";
                    //sql += " group by  a.m_autono,a.itcd,a.styleno";


                    //var dt = masterHelp.SQLquery(sql);
                    //List<ImageView> ImageViewlst = new List<ViewModels.ImageView>();
                    //foreach (DataRow dr in dt.Rows)
                    //{
                    //    ImageView objImageView = new ImageView();
                    //    objImageView.ITCD = dr["ITCD"].ToString();
                    //    objImageView.Desc = dr["styleno"].ToString();
                    //    objImageView.SIZES = dr["sizes"].ToString();
                    //    //objImageView.Desc = dr["desc"].ToString();
                    //    var img = Cn.GetUploadImage(scm, dr["m_autono"].retInt());
                    //    if (img.Count > 0)
                    //    {
                    //        var DBImgString = img[0].DOC_FILE;
                    //        var ImageName = img[0].DOC_FILE_NAME;
                    //        var extension = Path.GetExtension(ImageName);
                    //        string filename = objImageView.ITCD + "_0" + extension;
                    //        var folderpath = CommVar.LocalUploadDocPath(filename);
                    //        var link = Cn.SaveImage(DBImgString, folderpath);
                    //        var path = CommVar.WebUploadDocURL(filename);
                    //        objImageView.Url = path;
                    //        ImageViewlst.Add(objImageView);
                    //    }
                    //}
                    //VE.ImageView = ImageViewlst;
                    //ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    //   GetUploadImage(string schema, long AutoNO)
                    VE.DefaultView = true;
                    return View(VE);

                }
            }

            catch (Exception ex)
            {
                //AmountTypeMasterEntry VE = new AmountTypeMasterEntry();
                //VE.DefaultView = false;
                //VE.DefaultDay = 0;
                //ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
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
                string sql = "";
                sql = "select distinct a.RTLCD, a.RTLNM,a.LANDMARK ";
                sql += "from " + scm + ".M_RETAIL a, " + scm + ".m_cntrl_hdr b, " + scm + ".M_RETAIL_LINK c ";
                sql += "where a.m_autono=b.m_autono(+) and a.RTLCD=c.RTLCD(+) and c.slcd ='" + Distributor + "' ";
                sql += "and nvl(b.inactive_tag,'N')='N' ";
                sql += "order by RTLNM ";
                DataTable tbl = masterHelp.SQLquery(sql);

                VE.ListRetailer = (from DataRow a in tbl.Rows
                                   select new ListRetailer()
                                   {
                                       value = a["RTLCD"].retStr(),
                                       text = a["RTLNM"].retStr() + GCS + a["LANDMARK"].retStr(),
                                   }).ToList();

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
            ind.T_RETAILORDER = TRETAILORDER;

            if (TempData["printparameter"] != null)
            {
                TempData.Remove("printparameter");
            }
            TempData["printparameter"] = ind;
            return Content("");
        }     

        public string GetAddress(string lat, string lng)
        {
            try
            {
                string datastring = "";
                lat = "22.555"; lng = "88.258";
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

