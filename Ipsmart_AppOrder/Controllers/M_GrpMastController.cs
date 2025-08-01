﻿using System;
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

                    ViewBag.formname = "M_GrpMast";
                    VE.UNQSNO_ENCRYPTED = Cn.Encrypt_URL(UNQSNO);
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
                sql = "";
                sql += " select distinct RETLRCD,RETLRNM,RETLRPIN,RETLRCITY,RETLRGSTNO,a.DSTBRSLCD,b.slnm DSTBRSLnm  from  " + scm + ".M_RetailOutlet a, " + scmf + ".m_subleg b " + Environment.NewLine;
                sql += "where  A.DSTBRSLCD=b.slcd " + Environment.NewLine;
                if (RetailerName.retStr() != "") sql += " and upper(RETLRNM) like '%" + RetailerName.retStr().ToUpper() + "%' ";
                if (RetailerPin.retStr() != "") sql += "and RETLRPIN like '%" + RetailerPin.retStr() + "%' ";
                if (RetailerGstno.retStr() != "") sql += "and upper(RETLRGSTNO) like '%" + RetailerGstno.retStr().ToUpper() + "%' ";
                if (RetailerCity.retStr() != "") sql += "and upper(RETLRCITY) like '%" + RetailerCity.retStr().ToUpper() + "%' ";
                var txndt = masterHelp.SQLquery(sql);
                retlOrdrlst = (from DataRow dr in txndt.Rows
                               select new VMRetailOrder
                               {
                                   RetailerCode = dr["RETLRCD"].ToString(),
                                   RetailerGstno = dr["RETLRGSTNO"].ToString(),
                                   RetailerCity = dr["RETLRCITY"].retDateStr(),
                                   RetailerName = dr["RETLRNM"].ToString(),
                                   RetailerPin = dr["RETLRPIN"].ToString(),
                                   Dstbrslcd = dr["DSTBRSLCD"].ToString(),
                                   Dstbrslnm = dr["DSTBRSLnm"].ToString(),
                               }).Take(5).ToList();
                
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
                return address;
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
                try
                {
                    var address = GetAddress("", "");
                    string DefaultAction = "A";
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode");
                    if (DefaultAction == "A")
                    {
                        T_RETAILORDER MREASON = new T_RETAILORDER();
                        MREASON.CLCD = CommVar.ClientCode(UNQSNO);//DTYPE=RETAILORD
                        string DOCPATTERN = "", DOCCD = "SORDR", DOCNO = ""; int EMD_NO = 0;
                        DateTime DOCDT = System.DateTime.Now;
                        string Ddate = Convert.ToString(DOCDT);
                        string auto_no = ""; string Month = "", YR_CD = CommVar.YearCode(UNQSNO);
                        if (DefaultAction == "A")
                        {
                            EMD_NO = 0;
                            //  DOCCD = VE.T_TXN.DOCCD;
                            DOCNO = Cn.MaxDocNumber(DOCCD, Ddate);

                            DOCPATTERN = Cn.DocPattern(Convert.ToInt32(DOCNO), DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                            auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), DOCNO, DOCCD, Ddate, "", "", YR_CD);
                            MREASON.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                            Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                        }
                        //else
                        //{
                        //    MREASON.AUTONO = VE.AUTONO;
                        //    var MAXEMDNO = (from p in DB.T_CNTRL_HDR where p.AUTONO == MREASON.AUTONO select p.EMD_NO).Max();
                        //    if (MAXEMDNO == null)
                        //    {
                        //        MREASON.EMD_NO = 0;
                        //    }
                        //    else
                        //    {
                        //        MREASON.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                        //    }
                        //    Month = VE.T_CNTRL_HDR.MNTHCD;
                        //    MREASON.EMD_NO = Convert.ToInt16((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                        //    DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                        //    MREASON.DTAG = "E";
                        //}
                        MREASON.SLCD = VE.Dstbrslcd;
                        //List<APP_ITEMLIST> KARTIEMS = new List<ViewModels.APP_ITEMLIST>();
                        //var KARTIEMS1 = new APP_ITEMLIST() { itcd = "F008477", sizes = new List<APP_SIZEDTL>() { new APP_SIZEDTL() { qnty = "323", sizecd = "S" } } };
                        //KARTIEMS.Add(KARTIEMS1);
                        List<APP_ITEMLIST> aPP_ITEMLIST = JsonConvert.DeserializeObject<List<APP_ITEMLIST>>(VE.ITEMDETAIL_JSTR);
                        int slno = 0;
                        foreach (var v in aPP_ITEMLIST)
                        {
                            T_RETAILORDERDTL TRETAILORDERDTL = new T_RETAILORDERDTL();
                            slno++;
                            TRETAILORDERDTL.CLCD = MREASON.CLCD;
                            TRETAILORDERDTL.EMD_NO = MREASON.EMD_NO;
                            TRETAILORDERDTL.AUTONO = MREASON.AUTONO;
                            TRETAILORDERDTL.ITCD = v.itcd;
                            TRETAILORDERDTL.SLNO = slno.retShort();
                            var sizes = v.sizes.retStr().Split(',');
                            foreach (var sizeq in sizes)
                            {
                                var sqn = sizeq.retStr().Split('=');
                                if (sqn.Length > 1)
                                {
                                    TRETAILORDERDTL.SIZECD = sqn[0];
                                    TRETAILORDERDTL.QNTY = sqn[1].retDbl();
                                }
                                DB.T_RETAILORDERDTL.Add(TRETAILORDERDTL);
                            }
                        }
                        //T_CNTRL_HDR MCH = Cn.T_CNTRL_HDR(VE.Checked, "M_GrpMast", MREASON.AUTONO.retInt(), VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                        T_CNTRL_HDR MCH = Cn.T_CONTROL_HDR(DOCCD, DOCDT, DOCNO, MREASON.AUTONO, Month, DOCPATTERN, DefaultAction, CommVar.CurSchema(UNQSNO), "", MREASON.SLCD, 0, "", YR_CD);
                        if (DefaultAction == "A")
                        {
                            DB.T_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                            DB.T_RETAILORDER.Add(MREASON);
                        }
                        else if (DefaultAction == "E")
                        {
                            DB.Entry(MREASON).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
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
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
            return null;
        }
        public ActionResult GetBrandDetails(VMRetailOrder VE)
        {
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
            string sql = "";
           
            string BrandCode = VE.BrandCode.retStr().ToUpper().Trim();
            string BrandName = VE.BrandName.retStr().ToUpper().Trim();
            
            sql = "";
            sql += "select distinct a.BRANDCD,a.BRANDNM ";
            sql += "from " + scm + ".M_BRAND a, " + scm + ".m_cntrl_hdr c, " + scm + ".m_cntrl_loca d ";
            sql += "where a.m_autono=c.m_autono(+) and a.m_autono=d.m_autono(+) ";
            if (BrandCode.retStr() != "") sql += "and  upper(a.BRANDCD) LIKE '%" + BrandCode + "%'  ";
            if (BrandName.retStr() != "") sql += "and  upper(a.BRANDNM) LIKE '%" + BrandName + "%' ";
            sql += "and (d.compcd='" + COM + "' or d.compcd is null) and (d.loccd='" + LOC + "' or d.loccd is null) and ";
            sql += "nvl(c.inactive_tag,'N') = 'N' ";
            sql += "order by BRANDNM,BRANDCD";
            DataTable tbl = masterHelp.SQLquery(sql);
            if (tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["BRANDNM"] + "</td><td>" + tbl.Rows[i]["BRANDCD"] + " </td></tr>");
                }
                var hdr = "Brand Name" + Cn.GCS() + "Brand Code";
                return PartialView("_Help2", (masterHelp.Generate_help(hdr, SB.ToString())));

            }
            else
            {
                if (tbl.Rows.Count > 0)
                {
                    string str = masterHelp.ToReturnFieldValues("", tbl);
                    return Content(str);
                }
                else
                {
                    return Content("Invalid Brand Code ! Please Enter a Valid Brand Code !!");
                }
            }
        }

        public ActionResult GetGrpDetails(VMRetailOrder VE)
        {
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
            string sql = "";

            string GroupCode = VE.GroupCode.retStr().ToUpper().Trim();
            string GroupName = VE.GroupName.retStr().ToUpper().Trim();

            sql = "";
            sql += "select distinct a.ITGRPCD,a.ITGRPNM ";
            sql += "from " + scm + ".M_GROUP a, " + scm + ".m_cntrl_hdr c, " + scm + ".m_cntrl_loca d ";
            sql += "where a.m_autono=c.m_autono(+) and a.m_autono=d.m_autono(+) ";
            if (GroupCode.retStr() != "") sql += "and  upper(a.ITGRPCD) LIKE '%" + GroupCode + "%'  ";
            if (GroupName.retStr() != "") sql += "and  upper(a.ITGRPNM) LIKE '%" + GroupName + "%' ";
            sql += "and (d.compcd='" + COM + "' or d.compcd is null) and (d.loccd='" + LOC + "' or d.loccd is null) and ";
            sql += "nvl(c.inactive_tag,'N') = 'N' ";
            sql += "order by ITGRPNM,ITGRPCD";
            DataTable tbl = masterHelp.SQLquery(sql);
            if (tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    SB.Append("<tr><td>" + tbl.Rows[i]["ITGRPNM"] + "</td><td>" + tbl.Rows[i]["ITGRPCD"] + " </td></tr>");
                }
                var hdr = "Group Name" + Cn.GCS() + "Group Code";
                return PartialView("_Help2", (masterHelp.Generate_help(hdr, SB.ToString())));

            }
            else
            {
                if (tbl.Rows.Count > 0)
                {
                    string str = masterHelp.ToReturnFieldValues("", tbl);
                    return Content(str);
                }
                else
                {
                    return Content("Invalid Group Code ! Please Enter a Valid Group Code !!");
                }
            }
        }
        public ActionResult GetItem(VMRetailOrder VE)
        {
            try
            {
                string brand = VE.BrandCode;
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
                return PartialView("_M_GrpMast_Main", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }

    }
}

