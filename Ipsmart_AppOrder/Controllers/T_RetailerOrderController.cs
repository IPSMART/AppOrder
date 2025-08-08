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

namespace Improvar.Controllers
{
    public class T_RetailerOrderController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_RetailerOrder
        public ActionResult T_RetailerOrder(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {//k
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    TransactionRetailOrder VE;
                    if (TempData["printparameter"] == null)
                    {
                        VE = new TransactionRetailOrder();
                    }
                    else
                    {
                        VE = (TransactionRetailOrder)TempData["printparameter"];
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


                    string brand = VE.BrandCode.retSqlfromStrarray();
                    string scm = CommVar.CurSchema(UNQSNO);
                    string fscm = CommVar.FinSchema(UNQSNO);
                    string comp = CommVar.Compcd(UNQSNO);
                    string loc = CommVar.Loccd(UNQSNO);
                    string doccd = "";

                    string sql = "";
                    sql += " select a.m_autono,a.itcd,a.styleno, listagg(C.SIZECD, ',') within group (order by a.itcd) as sizes,nvl(a.PCSPERSET,0)PCSPERSET,a.MIXSIZE, " + Environment.NewLine;
                    sql += "count(C.SIZECD)SIZE_COUNT,nvl(a.PCSPERBOX,0) PCSPERBOX " + Environment.NewLine;
                    sql += " from " + CommVar.CurSchema(UNQSNO) + ".m_sitem a, " + CommVar.CurSchema(UNQSNO) + ".m_group b, " + CommVar.CurSchema(UNQSNO) + ".m_sitem_size c " + Environment.NewLine;
                    sql += " where a.itgrpcd = b.itgrpcd and a.itcd = c.itcd and " + Environment.NewLine;
                    sql += " a.m_autono in (select m_autono from " + CommVar.CurSchema(UNQSNO) + ".M_CNTRL_HDR_DOC ) " + Environment.NewLine;
                    if (VE.BrandCode != null) sql += "and b.brandcd in(" + VE.BrandCode.retSqlfromStrarray() + ") " + Environment.NewLine;
                    if (VE.GroupCode != null) sql += "and a.itgrpcd in(" + VE.GroupCode.retSqlfromStrarray() + ") " + Environment.NewLine;
                    if (VE.CollCode != null) sql += "and a.collcd in(" + VE.CollCode.retSqlfromStrarray() + ") " + Environment.NewLine;
                    sql += " group by  a.m_autono,a.itcd,a.styleno,nvl(a.PCSPERSET,0),a.MIXSIZE,nvl(a.PCSPERBOX,0)  " + Environment.NewLine;


                    var dt = masterHelp.SQLquery(sql);
                    List<ImageView> ImageViewlst = new List<ViewModels.ImageView>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        string itcd = dr["ITCD"].ToString();

                        string ITEMIMGPATH = @ConfigurationManager.AppSettings["ITEMIMGPATH"].retStr();
                        string physicalPath = ITEMIMGPATH + "/ItemImages/"; //@"D:" + "/ItemImages/";

                        DirectoryInfo di = new DirectoryInfo(physicalPath);
                        var totalfiles = di.GetFiles(itcd + "_*")
                                        .Select(file => new { filenm = file.Name }).ToList();

                        if (totalfiles != null && totalfiles.Count > 0)
                        {
                            foreach (var file in totalfiles)
                            {
                                ImageView objImageView = new ImageView();
                                objImageView.ITCD = dr["ITCD"].ToString();
                                objImageView.Desc = dr["styleno"].ToString();
                                objImageView.SIZES = dr["sizes"].ToString();
                                objImageView.PCSPERSET = dr["PCSPERSET"].retShort();
                                objImageView.MIXSIZE = dr["MIXSIZE"].ToString();
                                objImageView.SIZE_COUNT = dr["SIZE_COUNT"].retDbl();
                                objImageView.PCSPERBOX = dr["PCSPERBOX"].retShort();


                                string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority); // http://000.000.000.00
                                string imageUrl = baseUrl + "/ItemImages/" + file.filenm.retStr();

                                objImageView.Url = imageUrl;
                                ImageViewlst.Add(objImageView);
                            }
                        }

                    }
                    VE.ImageView = ImageViewlst;
                    return View(VE);

                }
            }

            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult SAVE(FormCollection FC, TransactionRetailOrder VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    string DefaultAction = "A";
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".T_RETAILORDER in  row share mode");
                    if (DefaultAction == "A")
                    {
                        T_RETAILORDER TRETAILORDER = new T_RETAILORDER();
                        TRETAILORDER.CLCD = CommVar.ClientCode(UNQSNO);
                        TRETAILORDER.DOCDT = System.DateTime.Now.Date;
                        string Ddate = Convert.ToString(TRETAILORDER.DOCDT);

                        if (DefaultAction == "A")
                        {
                            TRETAILORDER.EMD_NO = 0;
                            string DOCNO = Cn.MaxDocNumber(Ddate);
                            TRETAILORDER.VCHRNO = DOCNO.Split(Convert.ToChar(Cn.GCS()))[0].retInt();
                            TRETAILORDER.MNTHCD = DOCNO.Split(Convert.ToChar(Cn.GCS()))[1].ToString();

                            TRETAILORDER.DOCNO = Cn.DocPattern(TRETAILORDER.VCHRNO.retDbl(), TRETAILORDER.MNTHCD);
                            TRETAILORDER.AUTONO = VE.T_RETAILORDER.SLCD + TRETAILORDER.VCHRNO;

                        }
                        else
                        {
                            var MAXEMDNO = (from p in DB.T_RETAILORDER where p.AUTONO == TRETAILORDER.AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                TRETAILORDER.EMD_NO = 0;
                            }
                            else
                            {
                                TRETAILORDER.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                            TRETAILORDER.VCHRNO = VE.T_RETAILORDER.VCHRNO;
                            TRETAILORDER.DOCNO = VE.T_RETAILORDER.AUTONO;
                            TRETAILORDER.AUTONO = VE.T_RETAILORDER.AUTONO;
                            TRETAILORDER.MNTHCD = VE.T_RETAILORDER.MNTHCD;
                            TRETAILORDER.DTAG = "E";
                        }
                        TRETAILORDER.RTLCD = VE.T_RETAILORDER.RTLCD;
                        TRETAILORDER.SLCD = VE.T_RETAILORDER.SLCD;
                        TRETAILORDER.DOCAMT = VE.T_RETAILORDER.DOCAMT;

                        TRETAILORDER.USR_ID = CommVar.UserID();
                        TRETAILORDER.USR_ENTDT = System.DateTime.Now;
                        TRETAILORDER.USR_SIP = Cn.GetStaticIp();

                        //TRETAILORDER.LM_USR_ID = CommVar.UserID();
                        //TRETAILORDER.LM_USR_ENTDT = System.DateTime.Now;
                        //TRETAILORDER.LM_USR_SIP = Cn.GetStaticIp();
                        //TRETAILORDER.LM_REM = "";

                        //TRETAILORDER.DEL_USR_ID = CommVar.UserID();
                        //TRETAILORDER.DEL_USR_ENTDT = System.DateTime.Now;
                        //TRETAILORDER.DEL_USR_SIP =Cn.GetStaticIp();
                        //TRETAILORDER.DEL_REM = "";

                        //TRETAILORDER.CANCEL = "Y";
                        //TRETAILORDER.CANC_REM = "";
                        //TRETAILORDER.CANC_USR_ID = CommVar.UserID();
                        //TRETAILORDER.CANC_USR_ENTDT = System.DateTime.Now;
                        //TRETAILORDER.CANC_USR_SIP =Cn.GetStaticIp();

                        TRETAILORDER.GPSLAT = VE.T_RETAILORDER.GPSLAT;
                        TRETAILORDER.GPSLOT = VE.T_RETAILORDER.GPSLOT;
                        TRETAILORDER.DOCREM = VE.T_RETAILORDER.DOCREM;
                        TRETAILORDER.GPSNM = GetAddress(VE.T_RETAILORDER.GPSLAT.retStr(), VE.T_RETAILORDER.GPSLOT.retStr());


                        if (DefaultAction == "A")
                        {
                            DB.T_RETAILORDER.Add(TRETAILORDER);
                        }
                        else if (DefaultAction == "E")
                        {
                            DB.Entry(TRETAILORDER).State = System.Data.Entity.EntityState.Modified;
                        }
                        List<APP_ITEMLIST> aPP_ITEMLIST = JsonConvert.DeserializeObject<List<APP_ITEMLIST>>(VE.ITEMDETAIL_JSTR);
                        int slno = 0;
                        foreach (var v in aPP_ITEMLIST)
                        {
                            var sizes = v.sizes.retStr().Split(',');
                            foreach (var sizeq in sizes)
                            {
                                var sqn = sizeq.retStr().Split('=');
                                if (sqn.Length > 1)
                                {
                                    slno++;
                                    T_RETAILORDERDTL TRETAILORDERDTL = new T_RETAILORDERDTL();
                                    TRETAILORDERDTL.CLCD = TRETAILORDER.CLCD;
                                    TRETAILORDERDTL.EMD_NO = TRETAILORDER.EMD_NO;
                                    TRETAILORDERDTL.AUTONO = TRETAILORDER.AUTONO;
                                    TRETAILORDERDTL.DTAG = TRETAILORDER.DTAG;
                                    TRETAILORDERDTL.ITCD = v.itcd;
                                    TRETAILORDERDTL.SLNO = slno.retShort();
                                    TRETAILORDERDTL.SIZECD = sqn[0];
                                    TRETAILORDERDTL.QNTY = sqn[1].retDbl();
                                    DB.T_RETAILORDERDTL.Add(TRETAILORDERDTL);
                                }
                            }
                        }


                        DB.SaveChanges();
                        ModelState.Clear();
                        transaction.Commit();

                        string ContentFlg = "";
                        if (DefaultAction == "A")
                        {
                            ContentFlg = "1";
                        }
                        else if (DefaultAction == "E")
                        {
                            ContentFlg = "2";
                        }
                        return Content(ContentFlg);

                    }
                    //else if (DefaultAction == "V")
                    //{
                    //    T_CNTRL_HDR MCH = Cn.T_CONTROL_HDR(VE.T_CNTRL_HDR.DOCCD, VE.T_CNTRL_HDR.DOCDT, VE.T_CNTRL_HDR.DOCNO, VE.T_RETAILORDER.AUTONO, VE.T_CNTRL_HDR.MNTHCD, VE.T_CNTRL_HDR.DOCNO, "D", CommVar.CurSchema(UNQSNO), "", VE.T_RETAILORDER.SLCD, 0, "", VE.T_CNTRL_HDR.YR_CD);
                    //    DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                    //    DB.SaveChanges();

                    //    DB.T_RETAILORDER.Where(x => x.AUTONO == VE.T_RETAILORDER.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                    //    DB.SaveChanges();

                    //    DB.T_RETAILORDER.RemoveRange(DB.T_RETAILORDER.Where(x => x.AUTONO == VE.T_RETAILORDER.AUTONO));
                    //    DB.SaveChanges();
                    //    DB.T_CNTRL_HDR.RemoveRange(DB.T_CNTRL_HDR.Where(x => x.AUTONO == VE.T_RETAILORDER.AUTONO));
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

