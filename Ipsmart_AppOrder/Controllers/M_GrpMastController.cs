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
                    sql = " select a.m_autono,a.itcd,a.styleno from " + CommVar.CurSchema(UNQSNO) + ".m_sitem a, " + CommVar.CurSchema(UNQSNO) + ".m_group b ";
                    sql += "where a.itgrpcd = b.itgrpcd and b.brandcd = '" + brand + "'";
                    var dt = masterHelp.SQLquery(sql);
                    List<ImageView> ImageViewlst = new List<ViewModels.ImageView>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        ImageView objImageView = new ImageView();
                        objImageView.ITCD = dr["ITCD"].ToString();
                        objImageView.Desc = dr["styleno"].ToString();
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
                                   RetailerCode = dr["RETOUTNM"].ToString(),
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
        public ActionResult SAVE(FormCollection FC, VMRetailOrder VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".M_CNTRL_HDR in  row share mode");
                    //if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    //{
                    //    //    M_GrpMast MREASON = new M_GrpMast();
                    //    //    MREASON.CLCD = CommVar.ClientCode(UNQSNO);
                    //    //    if (VE.DefaultAction == "A")
                    //    //    {
                    //    //        MREASON.EMD_NO = 0;
                    //    //        MREASON.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO));
                    //    //        string txtITGRPNM = VE.M_GrpMast.REASONNM;
                    //    //        string txtst = txtITGRPNM.Substring(0, 1);
                    //    //        var MAXJOBCD = DB.M_GrpMast.Where(a => a.REASONCD.Substring(0, 1) == txtst).Max(a => a.REASONCD);
                    //    //        if (MAXJOBCD == null)
                    //    //        {
                    //    //            string txt = VE.M_GrpMast.REASONNM;
                    //    //            string stxt = txt.Substring(0, 1);
                    //    //            string R = stxt + "01";
                    //    //            MREASON.REASONCD = R.ToString();
                    //    //        }
                    //    //        else
                    //    //        {
                    //    //            string maxSLst = MAXJOBCD.Substring(0, 1);
                    //    //            if (maxSLst == txtst)
                    //    //            {
                    //    //                string s = MAXJOBCD;
                    //    //                string digits = new string(s.Where(char.IsDigit).ToArray());
                    //    //                string letters = new string(s.Where(char.IsLetter).ToArray());
                    //    //                int number;
                    //    //                if (!int.TryParse(digits, out number))   //int.Parse would do the job since only digits are selected
                    //    //                {
                    //    //                    Console.WriteLine("Something weired happened");
                    //    //                }
                    //    //                string newStr = letters + (++number).ToString("D2");
                    //    //                MREASON.REASONCD = newStr.ToString();
                    //    //            }
                    //    //            //else
                    //    //            //{
                    //    //            //    string txt = VE.M_GrpMast.REASONNM;
                    //    //            //    string stxt = txt.Substring(0, 1);
                    //    //            //    string R = stxt + "01";
                    //    //            //    MREASON.REASONCD = R.ToString();
                    //    //            //}
                    //    //        }

                    //    //    }
                    //    //    else
                    //    //    {
                    //    //        MREASON.REASONCD = VE.M_GrpMast.REASONCD;
                    //    //        MREASON.M_AUTONO = VE.M_GrpMast.M_AUTONO;
                    //    //        var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MREASON.M_AUTONO select p.EMD_NO).Max();
                    //    //        if (MAXEMDNO == null)
                    //    //        {
                    //    //            MREASON.EMD_NO = 0;
                    //    //        }
                    //    //        else
                    //    //        {
                    //    //            MREASON.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                    //    //        }
                    //    //    }
                    //    //    MREASON.REASONNM = VE.M_GrpMast.REASONNM;
                    //    //    M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_GrpMast", MREASON.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                    //    //    if (VE.DefaultAction == "A")
                    //    //    {
                    //    //        DB.M_CNTRL_HDR.Add(MCH);
                    //    //        DB.SaveChanges();
                    //    //        DB.M_GrpMast.Add(MREASON);
                    //    //    }
                    //    //    else if (VE.DefaultAction == "E")
                    //    //    {
                    //    //        DB.Entry(MREASON).State = System.Data.Entity.EntityState.Modified;
                    //    //        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                    //    //    }
                    //    //    DB.SaveChanges();
                    //    //    ModelState.Clear();
                    //    //    transaction.Commit();

                    //    //    string ContentFlg = "";
                    //    //    if (VE.DefaultAction == "A")
                    //    //    {
                    //    //        ContentFlg = "1";
                    //    //    }
                    //    //    else if (VE.DefaultAction == "E")
                    //    //    {
                    //    //        ContentFlg = "2";
                    //    //    }
                    //    //    return Content(ContentFlg);

                    //    //}
                    //    //else if (VE.DefaultAction == "V")
                    //    //{
                    //    //    M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_GrpMast", VE.M_GrpMast.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                    //    //    DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                    //    //    DB.SaveChanges();

                    //    //    DB.M_GrpMast.Where(x => x.M_AUTONO == VE.M_GrpMast.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                    //    //    DB.SaveChanges();

                    //    //    DB.M_GrpMast.RemoveRange(DB.M_GrpMast.Where(x => x.M_AUTONO == VE.M_GrpMast.M_AUTONO));
                    //    //    DB.SaveChanges();
                    //    //    DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_GrpMast.M_AUTONO));
                    //    //    DB.SaveChanges();
                    //    //    ModelState.Clear();
                    //    //    transaction.Commit();
                    //    //    return Content("3");
                    //    //}
                    //    //else
                    //    //{
                    //    //    return Content("");
                    //}
                }
                catch (Exception ex)
                {
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
            return null;
        }
    }
}

