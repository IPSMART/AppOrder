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
        Connection Cn = new Connection(); MasterHelp Master_Help = new MasterHelp(); M_RETAIL sl; M_CNTRL_HDR sll; M_CNTRL_HDR_REM MCHR;
        MasterHelpFa Master_HelpFa = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_RETAIL
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
                    ViewBag.formname = "Retail Outlet Master";
                    ViewBag.Title = "Retail Outlet Master";
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    RetailOutletEntry VE = new RetailOutletEntry();
                    string scm = CommVar.CommSchema();
                    Cn.getQueryString(VE);
                    Cn.ValidateMenuPermission(VE);
                    string GCS = Cn.GCS();

                    DataTable tbl = new DataTable();
                    string sql = "";
                    sql = "select distinct STATECD, STATENM ";
                    sql += "from " + scm + ".MS_STATE ";
                    sql += "order by STATENM ";
                    tbl = Master_Help.SQLquery(sql);

                    VE.ListState = (from DataRow a in tbl.Rows
                                    select new ListState()
                                    {
                                        value = a["STATECD"].retStr(),
                                        text = a["STATENM"].retStr() + GCS + a["STATECD"].retStr(),
                                    }).ToList();

                    DataTable tbl1 = new DataTable();
                    string sql1 = "";
                    sql1 = "select distinct CNCD, CNAME ";
                    sql1 += "from " + scm + ".MS_COUNTRY ";
                    sql1 += "order by CNAME ";
                    tbl = Master_Help.SQLquery(sql1);

                    VE.ListCountry = (from DataRow a in tbl.Rows
                                    select new ListCountry()
                                    {
                                        value = a["CNCD"].retStr(),
                                        text = a["CNAME"].retStr() + GCS + a["CNCD"].retStr(),
                                    }).ToList();

                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_RETAIL orderby p.M_AUTONO select new IndexKey() { Navikey = p.RTLCD }).ToList();
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
                        VE.M_RETAIL = sl;
                        VE.M_CNTRL_HDR = sll;
                        VE.M_CNTRL_HDR_REM = MCHR;

                            List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                            UploadDOC UPL = new UploadDOC();
                            UPL.DocumentType = doctP;
                            UploadDOC1.Add(UPL);
                            VE.UploadDOC = UploadDOC1;
                        }
                    if (op.ToString() == "A")
                    {
                            M_RETAIL MRETAIL = new M_RETAIL();
                            VE.M_RETAIL = MRETAIL;
                        List<MRETAILLINK> MRETAILLINK = new List<MRETAILLINK>();
                        for (int i = 0; i <= 4; i++)
                        {
                            MRETAILLINK DOCCD = new MRETAILLINK();
                            DOCCD.SLNO = Convert.ToByte(i + 1);
                            MRETAILLINK.Add(DOCCD);
                        }
                        VE.MRETAILLINK = MRETAILLINK;

                                             
                    }
                    VE.DefaultView = true;
                    return View(VE);
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                        return View(VE);
                    }
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
            sl = new M_RETAIL(); sll = new M_CNTRL_HDR();
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
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
                
                sl = DB.M_RETAIL.Find(aa[0].Trim());
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Checked = true;                   
                }
                else
                {
                    VE.Checked = false;
                }

                

                VE.UploadDOC = Cn.GetUploadImage(CommVar.CurSchema(UNQSNO).ToString(), Convert.ToInt32(sl.M_AUTONO));

                if (VE.UploadDOC.Count == 0)
                {
                    List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                    UploadDOC UPL = new UploadDOC();
                    UPL.DocumentType = doctP;
                    UploadDOC1.Add(UPL);
                    VE.UploadDOC = UploadDOC1;
                }
            }
            
            if (sl != null)
            {               
                string scm = CommVar.CurSchema(UNQSNO);
                string scmf = CommVar.FinSchema(UNQSNO);
                string sql = "select distinct a.M_AUTONO,a.SLNO,a.SLCD,a.EFFDT,b.SLNM from " + scm + ".M_RETAIL_LINK a," + scmf + ".m_subleg b ";
                sql += "where a.slcd=b.slcd and a.M_AUTONO='" + sl.M_AUTONO + "' order by a.M_AUTONO   ";
                DataTable tbl = Master_Help.SQLquery(sql);
                VE.MRETAILLINK = (from DataRow dr in tbl.Rows
                                         select new MRETAILLINK()
                                         {
                                             SLNO = dr["SLNO"].retDbl(),
                                             SLCD = dr["SLCD"].retStr(),
                                             SLNM = dr["SLNM"].retStr(),                                             
                                         }).ToList();
                               
                MCHR = Cn.GetMasterReamrks(CommVar.FinSchema(UNQSNO), Convert.ToInt32(sl.M_AUTONO));
                VE.UploadDOC = Cn.GetUploadImage(CommVar.FinSchema(UNQSNO).ToString(), Convert.ToInt32(sl.M_AUTONO));
            }

            return VE;
        }
        public ActionResult SearchPannelData()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            var MDT = (from j in DB.M_RETAIL
                       join p in DB.M_CNTRL_HDR on j.M_AUTONO equals (p.M_AUTONO)
                       where (p.M_AUTONO == j.M_AUTONO)
                       select new
                       {
                           RTLCD = j.RTLCD,
                           RTLNM = j.RTLNM
                       }).Distinct().OrderBy(s => s.RTLCD).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Retail Code" + Cn.GCS() + "Retail Name";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].RTLCD + "</td><td>" + MDT[j].RTLNM + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "0"));
        }       
        public ActionResult GetPartyDetails(RetailOutletEntry VE)
        {
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
            string sql = "";
            string LINK_CD = "D";
            string linkcd = LINK_CD.retSqlformat();
            //string valsrch = val.ToUpper().Trim();
            //string slcd = VE.M_RETAIL.DSTBRSLCD.retStr().ToUpper().Trim();
            string slnm = VE.DSTBRSLNM.retStr().ToUpper().Trim();
            string gstno = VE.DSTBRGSTNO.retStr().ToUpper().Trim();
            string area = VE.DSTBRAREA.retStr().ToUpper().Trim();

            sql = "";
            sql += "select distinct a.slcd, a.slnm, a.gstno, nvl(a.slarea,a.district) slarea,a.statecd,a.district,a.tcsappl,a.panno ";
            sql += "from " + scmf + ".m_subleg a, " + scmf + ".m_subleg_link b, " + scmf + ".m_cntrl_hdr c, " + scmf + ".m_cntrl_loca d ";
            sql += "where a.slcd=b.slcd(+) and a.m_autono=c.m_autono(+) and a.m_autono=d.m_autono(+) ";
            //if (slcd.retStr() != "") sql += "and  upper(a.slcd) LIKE '%" + slcd + "%'  ";
            if (slnm.retStr() != "") sql += "and  upper(a.slnm) LIKE '%" + slnm + "%' ";
            if (gstno.retStr() != "") sql += "and upper(a.gstno) like '%" + gstno + "%' ";
            if (area.retStr() != "") sql += "and upper(nvl(a.slarea,a.district)) like '%" + area + "%' ";
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
                    SB.Append("<tr><td>" + tbl.Rows[i]["slnm"] + "</td><td>" + tbl.Rows[i]["slcd"] + " </td></tr>");
                }
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
                        M_RETAIL MRETAILOUTLET = new M_RETAIL();
                        MRETAILOUTLET.CLCD = CommVar.ClientCode(UNQSNO);
                        string YEAR = CommVar.YearCode(UNQSNO);
                        string NAME_CHAR = VE.M_RETAIL.RTLNM.Substring(0, 1).ToUpper().Trim();
                        //string YEAR_CHAR = System.DateTime.Now.ToString("YY"); // YEAR.Substring(2, 2).retStr().Trim();
                        string YEAR_CHAR = YEAR.Substring(2, 2).retStr().Trim();  
                        if (VE.DefaultAction == "A")
                        {
                            MRETAILOUTLET.EMD_NO = 0;
                            MRETAILOUTLET.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO));                            
                            var MAXJOBCD = DB.M_RETAIL.Where(a => a.RTLCD.Substring(0, 3) == YEAR_CHAR+NAME_CHAR).Max(a => a.RTLCD);
                            if (MAXJOBCD == null)
                            {
                                string R = YEAR_CHAR + NAME_CHAR + "00001";
                                MRETAILOUTLET.RTLCD = R.ToString();
                            }
                            else
                            {
                                string s = MAXJOBCD;
                                string digits = new string(s.Where(char.IsDigit).ToArray()).Substring(3, 4).retStr().Trim();
                                string letters = new string(s.Where(char.IsLetter).ToArray());
                                int number;
                                //string yr = digits.Substring(0, 2).retStr().Trim();                                
                                if (!int.TryParse(digits, out number))                   //int.Parse would do the job since only digits are selected
                                {
                                    Console.WriteLine("Something weired happened");
                                }
                               
                                string newStr = YEAR_CHAR + letters + (++number).ToString().PadLeft(5, '0');
                                MRETAILOUTLET.RTLCD = newStr.ToString();
                            }
                        }
                        else
                        {
                            MRETAILOUTLET.RTLCD = VE.M_RETAIL.RTLCD;
                            MRETAILOUTLET.M_AUTONO = VE.M_RETAIL.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MRETAILOUTLET.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MRETAILOUTLET.EMD_NO = 0;
                            }
                            else
                            {
                                MRETAILOUTLET.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }

                            //DB.M_RETAIL.Where(x => x.M_AUTONO == MRETAILOUTLET.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            //DB.M_RETAIL.RemoveRange(DB.M_RETAIL.Where(x => x.M_AUTONO == MRETAILOUTLET.M_AUTONO));
                            DB.M_RETAIL_LINK.Where(x => x.M_AUTONO == MRETAILOUTLET.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_RETAIL_LINK.RemoveRange(DB.M_RETAIL_LINK.Where(x => x.M_AUTONO == MRETAILOUTLET.M_AUTONO));
                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == MRETAILOUTLET.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == MRETAILOUTLET.M_AUTONO));
                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == MRETAILOUTLET.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == MRETAILOUTLET.M_AUTONO));
                        }

                        //MRETAILOUTLET.RTLCD = VE.M_RETAIL.RTLCD;
                        MRETAILOUTLET.GPSLAT = VE.M_RETAIL.GPSLAT;
                        MRETAILOUTLET.GPSLOT = VE.M_RETAIL.GPSLOT;
                        MRETAILOUTLET.GSTNO = VE.M_RETAIL.GSTNO;
                        MRETAILOUTLET.PAN = VE.M_RETAIL.PAN;
                        MRETAILOUTLET.RTLNM = VE.M_RETAIL.RTLNM;
                        MRETAILOUTLET.ADD1 = VE.M_RETAIL.ADD1;
                        MRETAILOUTLET.ADD2 = VE.M_RETAIL.ADD2;
                        MRETAILOUTLET.ADD3 = VE.M_RETAIL.ADD3;
                        MRETAILOUTLET.ADD4 = VE.M_RETAIL.ADD4;
                        MRETAILOUTLET.LANDMARK = VE.M_RETAIL.LANDMARK;
                        MRETAILOUTLET.CITY = VE.M_RETAIL.CITY;
                        MRETAILOUTLET.PIN = VE.M_RETAIL.PIN;
                        MRETAILOUTLET.STATECD = VE.M_RETAIL.STATECD;                        
                        MRETAILOUTLET.CNCD = VE.M_RETAIL.CNCD;
                        MRETAILOUTLET.COUNTRY = VE.M_RETAIL.COUNTRY;
                        MRETAILOUTLET.REGMOBILE = VE.M_RETAIL.REGMOBILE;
                        MRETAILOUTLET.REGEMAIL = VE.M_RETAIL.REGEMAIL;
                        MRETAILOUTLET.CPERSON = VE.M_RETAIL.CPERSON;
                        MRETAILOUTLET.CMOB1 = VE.M_RETAIL.CMOB1;
                        MRETAILOUTLET.CMOB2 = VE.M_RETAIL.CMOB2;
                        MRETAILOUTLET.REMARKS = VE.M_RETAIL.REMARKS;
                        
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_RETAIL", MRETAILOUTLET.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                            DB.M_RETAIL.Add(MRETAILOUTLET);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MRETAILOUTLET).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                            MRETAILOUTLET.DTAG = "E";
                        }
                        if (VE.UploadDOC != null)
                        {
                            var img = Cn.SaveUploadImage("M_RETAIL", VE.UploadDOC, Convert.ToInt32(MRETAILOUTLET.M_AUTONO), MRETAILOUTLET.EMD_NO.Value);
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
                        if (VE.MRETAILLINK != null)
                        {
                            for (int i = 0; i <= VE.MRETAILLINK.Count - 1; i++)
                            {
                                if (VE.MRETAILLINK[i].SLNO.retDbl() != 0 && VE.MRETAILLINK[i].SLCD.retStr() != "")
                                {

                                    M_RETAIL_LINK RETAILLINK = new M_RETAIL_LINK();
                                    RETAILLINK.SLNO = VE.MRETAILLINK[i].SLNO;
                                    RETAILLINK.SLCD = VE.MRETAILLINK[i].SLCD;
                                    RETAILLINK.EFFDT = System.DateTime.Now;
                                    RETAILLINK.RTLCD = MRETAILOUTLET.RTLCD;
                                    RETAILLINK.CLCD = MRETAILOUTLET.CLCD;
                                    RETAILLINK.EMD_NO = MRETAILOUTLET.EMD_NO;
                                    RETAILLINK.M_AUTONO = MRETAILOUTLET.M_AUTONO;
                                    DB.M_RETAIL_LINK.Add(RETAILLINK);
                                }
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_RETAIL", VE.M_RETAIL.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_RETAIL.Where(x => x.M_AUTONO == VE.M_RETAIL.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_RETAIL.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_RETAIL.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_RETAIL_LINK.Where(x => x.M_AUTONO == VE.M_RETAIL.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_RETAIL.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_RETAIL.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_RETAIL.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_RETAIL.RemoveRange(DB.M_RETAIL.Where(x => x.M_AUTONO == VE.M_RETAIL.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_RETAIL_LINK.RemoveRange(DB.M_RETAIL_LINK.Where(x => x.M_AUTONO == VE.M_RETAIL.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_RETAIL.M_AUTONO));
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
        public ActionResult GetState(string State)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                return PartialView("_Help2", Master_HelpFa.STATECD_help(DB));
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetCountry()
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                return PartialView("_Help2", Master_HelpFa.CISDCD_help(DB));
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddDOCRow(RetailOutletEntry VE)
        {
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), "IMPROVAR");
            var doctP = (from i in DB1.MS_DOCCTG
                         select new DocumentType()
                         {
                             value = i.DOC_CTG,
                             text = i.DOC_CTG
                         }).OrderBy(s => s.text).ToList();
            if (VE.UploadDOC == null)
            {
                List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
                UploadDOC MLI = new UploadDOC();
                MLI.DocumentType = doctP;
                MLocIFSC1.Add(MLI);
                VE.UploadDOC = MLocIFSC1;
            }
            else
            {
                List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
                for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
                {
                    UploadDOC MLI = new UploadDOC();
                    MLI = VE.UploadDOC[i];
                    MLI.DocumentType = doctP;
                    MLocIFSC1.Add(MLI);
                }
                UploadDOC MLI1 = new UploadDOC();
                MLI1.DocumentType = doctP;
                MLocIFSC1.Add(MLI1);
                VE.UploadDOC = MLocIFSC1;
            }
            VE.DefaultView = true;
            return PartialView("_UPLOADDOCUMENTS", VE);

        }
        public ActionResult DeleteDOCRow(RetailOutletEntry VE)
        {
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), "IMPROVAR");
            var doctP = (from i in DB1.MS_DOCCTG
                         select new DocumentType()
                         {
                             value = i.DOC_CTG,
                             text = i.DOC_CTG
                         }).OrderBy(s => s.text).ToList();
            List<UploadDOC> LOCAIFSC = new List<UploadDOC>();
            int count = 0;
            for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
            {
                if (VE.UploadDOC[i].chk == false)
                {
                    count += 1;
                    UploadDOC IFSC = new UploadDOC();
                    IFSC = VE.UploadDOC[i];
                    IFSC.DocumentType = doctP;
                    LOCAIFSC.Add(IFSC);
                }
            }
            VE.UploadDOC = LOCAIFSC;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_UPLOADDOCUMENTS", VE);

        }
        public ActionResult AddRow(RetailOutletEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                if (VE.MRETAILLINK == null)
                {
                    List<MRETAILLINK> MRETAILLINK = new List<MRETAILLINK>();
                    MRETAILLINK DOCCD = new MRETAILLINK();
                    DOCCD.SLNO = 1;
                    MRETAILLINK.Add(DOCCD);
                    VE.MRETAILLINK = MRETAILLINK;
                }
                else
                {
                    List<MRETAILLINK> MRETAILLINK = new List<MRETAILLINK>();
                    for (int i = 0; i <= VE.MRETAILLINK.Count - 1; i++)
                    {
                        MRETAILLINK DOCCD = new MRETAILLINK();
                        DOCCD = VE.MRETAILLINK[i];
                        MRETAILLINK.Add(DOCCD);
                    }
                    MRETAILLINK DOCCD1 = new MRETAILLINK();
                    var max = VE.MRETAILLINK.Max(a => Convert.ToByte(a.SLNO));
                    int SRLNO = Convert.ToInt32(max) + 1;
                    DOCCD1.SLNO = Convert.ToByte(SRLNO);
                    MRETAILLINK.Add(DOCCD1);
                    VE.MRETAILLINK = MRETAILLINK;
                }
                VE.DefaultView = true;
                return PartialView("_M_Retail_Distributor", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + "<br/>" + ex.InnerException);
            }
        }
        public ActionResult DeleteRow(RetailOutletEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                List<MRETAILLINK> MRETAILLINK = new List<MRETAILLINK>();
                int count = 0;
                for (int i = 0; i <= VE.MRETAILLINK.Count - 1; i++)
                {
                    if (VE.MRETAILLINK[i].Checked == false)
                    {
                        count += 1;
                        MRETAILLINK DOCCD = new MRETAILLINK();
                        DOCCD = VE.MRETAILLINK[i];
                        DOCCD.SLNO = Convert.ToByte(count);
                        MRETAILLINK.Add(DOCCD);
                    }
                }
                VE.MRETAILLINK = MRETAILLINK;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_M_Retail_Distributor", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + "<br/>" + ex.InnerException);
            }

        }
        public ActionResult GetSubLedgerDetails(string val, string Code)
        {
            try
            {
                var str = Master_Help.SLCD_help(val, Code, "Jobber");
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
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