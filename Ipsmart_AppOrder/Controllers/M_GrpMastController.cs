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
        {
            VMRetailOrder VE = new VMRetailOrder();
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {

                    ViewBag.formname = "M_GrpMast";
                    string brand = "REVO";
                    string scm = CommVar.CurSchema(UNQSNO);
                    string fscm = CommVar.FinSchema(UNQSNO);
                    string comp = CommVar.Compcd(UNQSNO);
                    string loc = CommVar.Loccd(UNQSNO);
                    string doccd = "";

                    string sql = "";
                    sql += " select a.m_autono,a.itcd,a.styleno, listagg(C.SIZECD, ',') within group (order by a.itcd) as sizes";
                    sql += " from " + CommVar.CurSchema(UNQSNO) + ".m_sitem a, " + CommVar.CurSchema(UNQSNO) + ".m_group b, " + CommVar.CurSchema(UNQSNO) + ".m_sitem_size c";
                    sql += " where a.itgrpcd = b.itgrpcd and a.itcd = c.itcd and b.brandcd = '" + brand + "'";
                    sql += " group by  a.m_autono,a.itcd,a.styleno";


                    var dt = masterHelp.SQLquery(sql);
                    List<ImageView> ImageViewlst = new List<ViewModels.ImageView>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        ImageView objImageView = new ImageView();
                        objImageView.ITCD = dr["ITCD"].ToString();
                        objImageView.Desc = dr["styleno"].ToString();
                        objImageView.SIZES = dr["sizes"].ToString();
                        //objImageView.Desc = dr["desc"].ToString();
                        var img = Cn.GetUploadImage(scm, dr["m_autono"].retInt());
                        if (img.Count > 0)
                        {
                            var DBImgString = img[0].DOC_FILE;
                            var ImageName = img[0].DOC_FILE_NAME;
                            var extension = Path.GetExtension(ImageName);
                            string filename = objImageView.ITCD + "_0" + extension;
                            var folderpath = CommVar.LocalUploadDocPath(filename);
                            var link = Cn.SaveImage(DBImgString, folderpath);
                            var path = CommVar.WebUploadDocURL(filename);
                            objImageView.Url = path;
                            ImageViewlst.Add(objImageView);
                        }
                    }
                    VE.ImageView = ImageViewlst;
                    //ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    //   GetUploadImage(string schema, long AutoNO)

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

        public JsonResult GetRetailerInfo(string RetailerName, string RetailerPin, string RetailerGstno, string RetailerCity)
        {
            List<VMRetailOrder> retlOrdrlst = new List<VMRetailOrder>();
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));


                sql += " select distinct * from  " + scm + ".M_RetailOutlet " + Environment.NewLine;
                var txndt = masterHelp.SQLquery(sql);
                retlOrdrlst = (from DataRow dr in txndt.Rows
                               select new VMRetailOrder
                               {
                                   RetailerCode = dr["RETOUTCD"].ToString(),
                                   RetailerGstno = dr["RETOUTNM"].ToString(),
                                   RetailerCity = dr["RETOUTNM"].retDateStr(),
                                   RetailerName = dr["RETOUTNM"].ToString(),
                               }).ToList();

            }
            catch (Exception ex)
            {
                //   dic.Add("message", ex.Message + ex.InnerException);
                Cn.SaveException(ex, "");
            }
            return Json(retlOrdrlst, JsonRequestBehavior.AllowGet);
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
                return datastring;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return "";
            }
        }
        public ActionResult SAVE(FormCollection FC, VMRetailOrder VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                //try
                //{
                //    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode");
                //    if (VE.DefaultAction == "A" || VE.DefaultAction == "E" || VE.DefaultAction == null)
                //    {
                //        T_RETAILORDER MREASON = new T_RETAILORDER();
                //        MREASON.CLCD = CommVar.ClientCode(UNQSNO);
                //        string DOCPATTERN = "", DOCCD = "SRETO", DOCNO = ""; int EMD_NO = 0;
                //        DateTime DOCDT = System.DateTime.Now;
                //        string Ddate = Convert.ToString(DOCDT);
                //        string auto_no = ""; string Month = "", YR_CD = CommVar.YearCode(UNQSNO);
                //        if (VE.DefaultAction == "A" || VE.DefaultAction == null)
                //        {
                //            EMD_NO = 0;
                //            //  DOCCD = VE.T_TXN.DOCCD;
                //            DOCNO = Cn.MaxDocNumber(DOCCD, Ddate);

                //            DOCPATTERN = Cn.DocPattern(Convert.ToInt32(DOCNO), DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                //            auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), DOCNO, DOCCD, Ddate, "", "", YR_CD);
                //            MREASON.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                //            Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                //        }
                //        else
                //        {
                //            MREASON.AUTONO = VE.AUTONO;
                //            var MAXEMDNO = (from p in DB.T_CNTRL_HDR where p.AUTONO == MREASON.AUTONO select p.EMD_NO).Max();
                //            if (MAXEMDNO == null)
                //            {
                //                MREASON.EMD_NO = 0;
                //            }
                //            else
                //            {
                //                MREASON.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                //            }
                //            Month = VE.T_CNTRL_HDR.MNTHCD;
                //            MREASON.EMD_NO = Convert.ToInt16((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                //            DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                //            MREASON.DTAG = "E";
                //        }
                //        MREASON.SLCD = "C00001";
                //        List<KARTIEMS> KARTIEMS = new List<ViewModels.KARTIEMS>();
                //        var KARTIEMS1 = new KARTIEMS() { itcd = "3456789dfgh", sizes = new List<SIZEDTL>() { new SIZEDTL() { qnty = "323", sizecd = "S" } } };
                //        KARTIEMS.Add(KARTIEMS1);
                //        T_RETAILORDERDTL TRETAILORDERDTL = new T_RETAILORDERDTL();
                //        int slno = 0;
                //        foreach (var v in KARTIEMS)
                //        {
                //            slno++;
                //            TRETAILORDERDTL.AUTONO = MREASON.AUTONO;
                //            TRETAILORDERDTL.ITCD = v.itcd;
                //            TRETAILORDERDTL.SLNO = slno.retShort();
                //            foreach (var v1 in v.sizes)
                //            {
                //                TRETAILORDERDTL.QNTY = v1.qnty.retDbl();
                //                TRETAILORDERDTL.SIZECD = v1.sizecd;
                //            }
                //            DB.T_RETAILORDERDTL.AddRange(TRETAILORDERDTL);
                //        }
                //        //T_CNTRL_HDR MCH = Cn.T_CNTRL_HDR(VE.Checked, "M_GrpMast", MREASON.AUTONO.retInt(), VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                //        T_CNTRL_HDR MCH = Cn.T_CONTROL_HDR(DOCCD, DOCDT, DOCNO, MREASON.AUTONO, Month, DOCPATTERN, "A", CommVar.CurSchema(UNQSNO), "", MREASON.SLCD, 0, "", YR_CD);
                //        if (VE.DefaultAction == "A" || VE.DefaultAction == null)
                //        {
                //            DB.T_CNTRL_HDR.Add(MCH);
                //            DB.SaveChanges();
                //            DB.T_RETAILORDER.Add(MREASON);
                //        }
                //        else if (VE.DefaultAction == "E")
                //        {
                //            DB.Entry(MREASON).State = System.Data.Entity.EntityState.Modified;
                //            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                //        }

                //        DB.SaveChanges();
                //        ModelState.Clear();
                //        transaction.Commit();

                //        string ContentFlg = "";
                //        if (VE.DefaultAction == "A" || VE.DefaultAction == null)
                //        {
                //            ContentFlg = "1";
                //        }
                //        else if (VE.DefaultAction == "E")
                //        {
                //            ContentFlg = "2";
                //        }
                //        return Content(ContentFlg);

                //    }
                //    else if (VE.DefaultAction == "V")
                //    {
                //        T_CNTRL_HDR MCH = Cn.T_CONTROL_HDR(VE.T_CNTRL_HDR.DOCCD, VE.T_CNTRL_HDR.DOCDT, VE.T_CNTRL_HDR.DOCNO, VE.T_RETAILORDER.AUTONO, VE.T_CNTRL_HDR.MNTHCD, VE.T_CNTRL_HDR.DOCNO, "D", CommVar.CurSchema(UNQSNO), "", VE.T_RETAILORDER.SLCD, 0, "", VE.T_CNTRL_HDR.YR_CD);
                //        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                //        DB.SaveChanges();

                //        DB.T_RETAILORDER.Where(x => x.AUTONO == VE.T_RETAILORDER.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                //        DB.SaveChanges();

                //        DB.T_RETAILORDER.RemoveRange(DB.T_RETAILORDER.Where(x => x.AUTONO == VE.T_RETAILORDER.AUTONO));
                //        DB.SaveChanges();
                //        DB.T_CNTRL_HDR.RemoveRange(DB.T_CNTRL_HDR.Where(x => x.AUTONO == VE.T_RETAILORDER.AUTONO));
                //        DB.SaveChanges();
                //        ModelState.Clear();
                //        transaction.Commit();
                //        return Content("3");
                //    }
                //    else
                //    {
                //        return Content("");
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Cn.SaveException(ex, "");
                //    return Content(ex.Message + ex.InnerException);
                //}
            }
            return null;
        }


    }
}

