using Improvar.Models;
using Improvar.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace Improvar.Controllers
{
    public class M_RetailOutletController : Controller
    {
        Connection Cn = new Connection(); MasterHelp Master_Help = new MasterHelp(); M_RETAILOUTLET sl; M_CNTRL_HDR sll; M_CNTRL_HDR_REM MCHR;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_RetailOutlet
        public ActionResult M_RetailOutlet(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Retail";
                    ViewBag.Title = "Retail";
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    RetailOutletEntry VE = new RetailOutletEntry();
                    Cn.getQueryString(VE);
                    Cn.ValidateMenuPermission(VE);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    //if (op.Length != 0)
                    //{
                    VE.IndexKey = (from p in DB.M_RETAILOUTLET orderby p.M_AUTONO select new IndexKey() { Navikey = p.RETLRCD }).ToList();
                    if (op == "E" || op == "D" || op == "V")
                    {
                        if (searchValue.Length != 0)
                        {
                            VE.Index = Nindex;
                            VE = Navigation(VE, DB, 0, searchValue);
                        }
                        else
                        {
                            if (key == "F")
                            {
                                VE.Index = 0;
                                VE = Navigation(VE, DB, 0, searchValue);
                            }
                            else if (key == "" || key == "L")
                            {
                                VE.Index = VE.IndexKey.Count - 1;
                                VE = Navigation(VE, DB, VE.IndexKey.Count - 1, searchValue);
                            }
                            else if (key == "P")
                            {
                                Nindex -= 1;
                                if (Nindex < 0)
                                {
                                    Nindex = 0;
                                }
                                VE.Index = Nindex;
                                VE = Navigation(VE, DB, Nindex, searchValue);
                            }
                            else if (key == "N")
                            {
                                Nindex += 1;
                                if (Nindex > VE.IndexKey.Count - 1)
                                {
                                    Nindex = VE.IndexKey.Count - 1;
                                }
                                VE.Index = Nindex;
                                VE = Navigation(VE, DB, Nindex, searchValue);
                            }
                        }
                        VE.M_RETAILOUTLET = sl;
                        VE.M_CNTRL_HDR = sll;
                        VE.M_CNTRL_HDR_REM = MCHR;
                    }
                    if (op.ToString() == "A")
                    {
                        List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                        UploadDOC UPL = new UploadDOC();
                        UPL.DocumentType = doctP;
                        UploadDOC1.Add(UPL);
                        VE.UploadDOC = UploadDOC1;
                    }
                    VE.DefaultView = true;
                    return View(VE);
                    //}
                    //else
                    //{
                    //    VE.DefaultView = false;
                    //    VE.DefaultDay = 0;
                    //    return View(VE);
                    //}
                }
            }

            catch (Exception ex)
            {
                RetailOutletEntry VE = new RetailOutletEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public RetailOutletEntry Navigation(RetailOutletEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_RETAILOUTLET(); sll = new M_CNTRL_HDR();
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            //var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
            if (VE.IndexKey.Count != 0)
            {
                string[] aa = null;
                if (searchValue.Length == 0)
                {
                    aa = VE.IndexKey[index].Navikey.Split(Convert.ToChar(Cn.GCS()));
                }
                else
                {
                    aa = searchValue.Split(Convert.ToChar(Cn.GCS()));
                }
                sl = DB.M_RETAILOUTLET.Find(aa[0].Trim());
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Checked = true;
                }
                else
                {
                    VE.Checked = false;
                }
                if (sl.DSTBRSLCD != null) VE.DSTBRSLNM = (from i in DBF.M_SUBLEG where i.SLCD == sl.DSTBRSLCD select i.SLNM).FirstOrDefault();
                //if (sl.PPURECD != null) VE.PPURENM = (from i in DB.M_RETAILOUTLET where i.RETLRCD == sl.PPURECD select i.RETLRNM).FirstOrDefault();
                //MCHR = Cn.GetMasterReamrks(CommVar.CurSchema(UNQSNO), Convert.ToInt32(sl.M_AUTONO));
                //VE.UploadDOC = Cn.GetUploadImage(CommVar.CurSchema(UNQSNO).ToString(), Convert.ToInt32(sl.M_AUTONO));
                //if (sl.RAWITCD.retStr() != "")
                //{
                //    VE.RAWITNM = (from a in DB.M_ITEM where a.ITCD == sl.RAWITCD select a).FirstOrDefault().ITNM;
                //}
                //if (VE.UploadDOC.Count == 0)
                //{
                //    List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                //    UploadDOC UPL = new UploadDOC();
                //    UPL.DocumentType = doctP;
                //    UploadDOC1.Add(UPL);
                //    VE.UploadDOC = UploadDOC1;
                //}
            }
            //else
            //{
            //    List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
            //    UploadDOC UPL = new UploadDOC();
            //    UPL.DocumentType = doctP;
            //    UploadDOC1.Add(UPL);
            //    VE.UploadDOC = UploadDOC1;
            //}
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            var MDT = (from j in DB.M_RETAILOUTLET
                       join p in DB.M_CNTRL_HDR on j.M_AUTONO equals (p.M_AUTONO)
                       where (p.M_AUTONO == j.M_AUTONO)
                       select new
                       {
                           RETLRCD = j.RETLRCD,
                           RETLRNM = j.RETLRNM
                       }).Distinct().OrderBy(s => s.RETLRCD).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Retail Code" + Cn.GCS() + "Retail Name";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].RETLRCD + "</td><td>" + MDT[j].RETLRNM + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "0"));
        }
        //public ActionResult AddDOCRow(DeptMasterEntry VE)
        //{
        //    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), "IMPROVAR");
        //    var doctP = (from i in DB1.MS_DOCCTG
        //                 select new DocumentType()
        //                 {
        //                     value = i.DOC_CTG,
        //                     text = i.DOC_CTG
        //                 }).OrderBy(s => s.text).ToList();
        //    if (VE.UploadDOC == null)
        //    {
        //        List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
        //        UploadDOC MLI = new UploadDOC();
        //        MLI.DocumentType = doctP;
        //        MLocIFSC1.Add(MLI);
        //        VE.UploadDOC = MLocIFSC1;
        //    }
        //    else
        //    {
        //        List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
        //        for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
        //        {
        //            UploadDOC MLI = new UploadDOC();
        //            MLI = VE.UploadDOC[i];
        //            MLI.DocumentType = doctP;
        //            MLocIFSC1.Add(MLI);
        //        }
        //        UploadDOC MLI1 = new UploadDOC();
        //        MLI1.DocumentType = doctP;
        //        MLocIFSC1.Add(MLI1);
        //        VE.UploadDOC = MLocIFSC1;
        //    }
        //    VE.DefaultView = true;
        //    return PartialView("_UPLOADDOCUMENTS", VE);

        //}
        //public ActionResult DeleteDOCRow(DeptMasterEntry VE)
        //{
        //    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), "IMPROVAR");
        //    var doctP = (from i in DB1.MS_DOCCTG
        //                 select new DocumentType()
        //                 {
        //                     value = i.DOC_CTG,
        //                     text = i.DOC_CTG
        //                 }).OrderBy(s => s.text).ToList();
        //    List<UploadDOC> LOCAIFSC = new List<UploadDOC>();
        //    int count = 0;
        //    for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
        //    {
        //        if (VE.UploadDOC[i].chk == false)
        //        {
        //            count += 1;
        //            UploadDOC IFSC = new UploadDOC();
        //            IFSC = VE.UploadDOC[i];
        //            IFSC.DocumentType = doctP;
        //            LOCAIFSC.Add(IFSC);
        //        }
        //    }
        //    VE.UploadDOC = LOCAIFSC;
        //    ModelState.Clear();
        //    VE.DefaultView = true;
        //    return PartialView("_UPLOADDOCUMENTS", VE);

        //}
        public ActionResult GetPartyDetails(RetailOutletEntry VE)
        {
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
            string sql = "";
            string LINK_CD = "D";
            string linkcd = LINK_CD.retSqlformat();
            //string valsrch = val.ToUpper().Trim();
            string slcd = VE.M_RETAILOUTLET.DSTBRSLCD.retStr().ToUpper().Trim();
            string slnm = VE.DSTBRSLNM.retStr().ToUpper().Trim();
            string gstno = VE.DSTBRGSTNO.retStr().ToUpper().Trim();
            string area = VE.DSTBRAREA.retStr().ToUpper().Trim();

            sql = "";
            sql += "select distinct a.slcd, a.slnm, a.gstno, nvl(a.slarea,a.district) slarea,a.statecd,a.district,a.tcsappl,a.panno ";
            sql += "from " + scmf + ".m_subleg a, " + scmf + ".m_subleg_link b, " + scmf + ".m_cntrl_hdr c, " + scmf + ".m_cntrl_loca d ";
            sql += "where a.slcd=b.slcd(+) and a.m_autono=c.m_autono(+) and a.m_autono=d.m_autono(+) ";
            if (slcd.retStr() != "") sql += "and  upper(a.slcd) = '" + slcd + "'  ";
            if (slnm.retStr() != "") sql += "and  upper(a.slnm) = '" + slnm + "' ";
            if (gstno.retStr() != "") sql += "and upper(a.gstno) = '" + gstno + "' ";
            if (area.retStr() != "") sql += "and upper(nvl(a.slarea,a.district)) = '" + area + "' ";
            //if (GLCD.retStr() != "") sql += "f.glcd = '" + GLCD + "' and ";
            if (linkcd != "") sql += "and b.linkcd in (" + linkcd + ")  ";
            sql += "and (d.compcd='" + COM + "' or d.compcd is null) and (d.loccd='" + LOC + "' or d.loccd is null) and ";
            sql += "nvl(c.inactive_tag,'N') = 'N' ";
            sql += "order by slnm,slcd";
            DataTable tbl = Master_Help.SQLquery(sql);
            if (tbl.Rows.Count > 1)
            {
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    //SB.Append("<tr><td>" + tbl.Rows[i]["slnm"] + "</td><td>" + tbl.Rows[i]["slcd"] + " </td><td>" + tbl.Rows[i]["gstno"] + " </td><td>" + tbl.Rows[i]["slarea"] + " </td><td>" + tbl.Rows[i]["statecd"] + " </td></tr>");
                    SB.Append("<tr><td>" + tbl.Rows[i]["slnm"] + "</td><td>" + tbl.Rows[i]["slcd"] + " </td></tr>");
                }
                //var hdr = "" + CAPTION + " Name" + Cn.GCS() + "" + CAPTION + " Code" + Cn.GCS() + "GST Number" + Cn.GCS() + "Area" + Cn.GCS() + "State Code";
                var hdr = "Distributor Name" + Cn.GCS() + "Distributor Code";
                return PartialView("_Help2", (Master_Help.Generate_help(hdr, SB.ToString())));

            }
            else
            {
                if (tbl.Rows.Count > 0)
                {
                    string str = Master_Help.ToReturnFieldValues("", tbl);
                    return Content(str);
                }
                else
                {
                    return Content("Invalid Distributor Code ! Please Enter a Valid Distributor Code !!");
                }
            }
        }
        public ActionResult SAVE(FormCollection FC, RetailOutletEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_RETAILOUTLET MRETAILOUTLET = new M_RETAILOUTLET();
                        MRETAILOUTLET.CLCD = CommVar.ClientCode(UNQSNO);
                        string NAME = VE.M_RETAILOUTLET.RETLRNM;
                        string NAME_CHAR = NAME.Substring(0, 1).ToUpper().Trim();
                        if (VE.DefaultAction == "A")
                        {
                            MRETAILOUTLET.EMD_NO = 0;
                            MRETAILOUTLET.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO));
                            var MAXJOBCD = DB.M_RETAILOUTLET.Where(a => a.RETLRCD.Substring(0, 1) == NAME_CHAR).Max(a => a.RETLRCD);
                            if (MAXJOBCD == null)
                            {
                                string R = NAME_CHAR + "0000001";
                                MRETAILOUTLET.RETLRCD = R.ToString();
                            }
                            else
                            {
                                string s = MAXJOBCD;
                                string digits = new string(s.Where(char.IsDigit).ToArray());
                                string letters = new string(s.Where(char.IsLetter).ToArray());
                                int number;
                                if (!int.TryParse(digits, out number))                   //int.Parse would do the job since only digits are selected
                                {
                                    Console.WriteLine("Something weired happened");
                                }
                                string newStr = letters + (++number).ToString("D7");
                                MRETAILOUTLET.RETLRCD = newStr.ToString();
                            }
                        }
                        else
                        {
                            MRETAILOUTLET.RETLRCD = VE.M_RETAILOUTLET.RETLRCD;
                            MRETAILOUTLET.M_AUTONO = VE.M_RETAILOUTLET.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MRETAILOUTLET.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MRETAILOUTLET.EMD_NO = 0;
                            }
                            else
                            {
                                MRETAILOUTLET.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                            //DB.M_CNTRL_HDR_REM.Where(x => x.M_AUTONO == MRETAILOUTLET.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            //DB.M_CNTRL_HDR_REM.RemoveRange(DB.M_CNTRL_HDR_REM.Where(x => x.M_AUTONO == MRETAILOUTLET.M_AUTONO));
                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == MRETAILOUTLET.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == MRETAILOUTLET.M_AUTONO));

                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == MRETAILOUTLET.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == MRETAILOUTLET.M_AUTONO));
                        }

                        MRETAILOUTLET.RETLRNM = VE.M_RETAILOUTLET.RETLRNM;
                        MRETAILOUTLET.RETLRADD1 = VE.M_RETAILOUTLET.RETLRADD1;
                        MRETAILOUTLET.RETLRADD2 = VE.M_RETAILOUTLET.RETLRADD2;
                        MRETAILOUTLET.RETLRPIN = VE.M_RETAILOUTLET.RETLRPIN;
                        MRETAILOUTLET.RETLRCITY = VE.M_RETAILOUTLET.RETLRCITY;
                        MRETAILOUTLET.RETLRLOCALITY = VE.M_RETAILOUTLET.RETLRLOCALITY;
                        MRETAILOUTLET.RETLRGSTNO = VE.M_RETAILOUTLET.RETLRGSTNO;
                        MRETAILOUTLET.REMARKS = VE.M_RETAILOUTLET.REMARKS;
                        MRETAILOUTLET.DSTBRSLCD = VE.M_RETAILOUTLET.DSTBRSLCD;
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_RETAILOUTLET", MRETAILOUTLET.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                            DB.M_RETAILOUTLET.Add(MRETAILOUTLET);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MRETAILOUTLET).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        if (VE.UploadDOC != null)
                        {
                            var img = Cn.SaveUploadImage("M_RETAILOUTLET", VE.UploadDOC, Convert.ToInt32(MRETAILOUTLET.M_AUTONO), MRETAILOUTLET.EMD_NO.Value);
                            DB.M_CNTRL_HDR_DOC.AddRange(img.Item1);
                            DB.M_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                        }
                        if (VE.M_CNTRL_HDR_REM != null && VE.M_CNTRL_HDR_REM.DOCREM != null)// add REMARKS
                        {
                            var NOTE = Cn.SAVEMASTERREMARKS(VE.M_CNTRL_HDR_REM, Convert.ToInt32(MRETAILOUTLET.M_AUTONO), MRETAILOUTLET.CLCD, MRETAILOUTLET.EMD_NO.Value);

                            if (NOTE.Item1.Count != 0)
                            {
                                DB.M_CNTRL_HDR_REM.AddRange(NOTE.Item1);

                            }
                        }
                        DB.SaveChanges();
                        ModelState.Clear();
                        transaction.Commit();

                        string ContentFlg = "";
                        if (VE.DefaultAction == "A")
                        {
                            ContentFlg = "1";
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            ContentFlg = "2";
                        }
                        return Content(ContentFlg);

                    }
                    else if (VE.DefaultAction == "V")
                    {
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_RETAILOUTLET", VE.M_RETAILOUTLET.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_RETAILOUTLET.Where(x => x.M_AUTONO == VE.M_RETAILOUTLET.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_RETAILOUTLET.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_RETAILOUTLET.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        //DB.M_CNTRL_HDR_REM.Where(x => x.M_AUTONO == VE.M_RETAILOUTLET.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_RETAILOUTLET.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_RETAILOUTLET.M_AUTONO));
                        DB.SaveChanges();
                        //DB.M_CNTRL_HDR_REM.RemoveRange(DB.M_CNTRL_HDR_REM.Where(x => x.M_AUTONO == VE.M_RETAILOUTLET.M_AUTONO));
                        //DB.SaveChanges();
                        DB.M_RETAILOUTLET.RemoveRange(DB.M_RETAILOUTLET.Where(x => x.M_AUTONO == VE.M_RETAILOUTLET.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_RETAILOUTLET.M_AUTONO));
                        DB.SaveChanges();
                        ModelState.Clear();
                        transaction.Commit();
                        return Content("3");
                    }
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
        }
    }
}