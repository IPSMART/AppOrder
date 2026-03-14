using System;
using System.Linq;
using System.Data;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;

namespace Improvar.Controllers
{
    public class Rep_RetailOutletController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string jobcd = "", jobnm = "", repname = "";
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_GL
        public ActionResult Rep_RetailOutlet()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ReportViewinHtml VE = new ReportViewinHtml();
                    //Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    jobcd = VE.MENU_PARA;
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmp = CommVar.PaySchema(UNQSNO);
                    ViewBag.formname = "Retailer Register";
                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.Checkbox1 = false; //show summary
                    VE.Checkbox2 = true; //Show Party
                    VE.TEXTBOX2 = "P"; //Calc on Box/Pcs/Sets;
                    string comcd = CommVar.Compcd(UNQSNO);
                    string location = CommVar.Loccd(UNQSNO);
                    string tdt = System.DateTime.Now.Date.retDateStr();
                    string uid = CommVar.UserID();
                    VE.SLMSLCD = GetSalesman(tdt, uid);

                    VE.ListSalesman = DropDownHelp.GetSalesmanforSelection();
                    VE.ListDistributor = DropDownHelp.GetDistributorforSelection(tdt, VE.SLMSLCD.retSqlformat());
                    string sql = "";
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
                    DataTable tbl = MasterHelp.SQLquery(sql);

                    VE.ListBrand = (from DataRow a in tbl.Rows
                                    select new ListBrand()
                                    {
                                        value = a["BRANDCD"].retStr(),
                                        text = a["BRANDNM"].retStr(),
                                    }).ToList();
                    VE.List_State = DropDownHelp.GetStateforSelection();
                    VE.List_City = DropDownHelp.GetCityforSelection();

                    VE.DefaultView = true;
                    VE.ExitMode = 1;
                    VE.DefaultDay = 0;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                ReportViewinHtml VE = new ReportViewinHtml();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        //public ActionResult GetMtrlJobDetails(string val)
        //{
        //    try
        //    {
        //        if (val == null)
        //        {
        //            return PartialView("_Help2", MasterHelp.MTRL_JOB_HELP(val));
        //        }
        //        else
        //        {
        //            string str = MasterHelp.MTRL_JOB_HELP(val);
        //            return Content(str);
        //        }
        //    }
        //    catch (Exception Ex)
        //    {
        //        Cn.SaveException(Ex, "");
        //        return Content(Ex.Message + Ex.InnerException);
        //    }
        //}
        [HttpPost]
        public ActionResult Rep_RetailOutlet(ReportViewinHtml VE, FormCollection FC)
        {
            string ModuleCode = Module.Module_Code;
            try
            {
                //Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));

                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), csm = CommVar.CommSchema(), usr_id = CommVar.UserID();
                string fdt, tdt = "", city = "", brandcd = "", statecd = "", distslcd = "", slmslcd = "";
                
                bool showitem = false, showsumm = VE.Checkbox1, showparty = VE.Checkbox2;

                tdt = System.DateTime.Now.Date.retDateStr();
                string uid = CommVar.UserID();
                var SLMSLCD = GetSalesman(tdt, uid).retSqlformat();

                string stkdrcr = VE.MENU_PARA;

                if (VE.slcd != null)
                {
                    slmslcd = VE.slcd.retSqlfromStrarray();
                }
                if (VE.distributor != null)
                {
                    distslcd = VE.distributor.retSqlfromStrarray();
                }
                if (VE.BrandCode != null)
                {
                    brandcd = VE.BrandCode.retSqlfromStrarray();
                }

                if (VE.State != null)
                {
                    statecd = VE.State.retSqlfromStrarray();
                }
                if (VE.City != null)
                {
                    city = VE.City.retSqlfromStrarray();
                }

                string sql = "";
                sql += "select b.m_autono, a.slmslcd, g.slnm slmslnm, a.agslcd, f.slnm agslnm, a.distslcd, e.slnm distslnm, " + Environment.NewLine;
                sql += "b.rtlcd, b.rtlnm, b.ADD1, b.ADD2, b.ADD3, b.ADD4, b.city, b.PIN, b.STATECD, b.COUNTRY, b.LANDMARK, " + Environment.NewLine;
                sql += "b.REGMOBILE, b.REGWHATSAPPNO, b.REGEMAIL, b.CPERSON, NVL(b.CMOB1, NVL(b.CMOB2, '')) CPERSONNO, b.REMARKS, b.GPSLAT, b.GPSLOT, " + Environment.NewLine;
                sql += "b.GPSNM, a.SLMSLCD, b.GSTNO, b.PAN, d.statenm from " + Environment.NewLine;
                sql += "(select a.slmslcd, a.effdt, b.agslcd, b.distslcd from " + Environment.NewLine;
                sql += "(select slmslcd, effdt from( " + Environment.NewLine;
                sql += "select a.slmslcd, a.effdt, " + Environment.NewLine;
                sql += "row_number() over(partition by a.slmslcd order by a.effdt desc) rno " + Environment.NewLine;
                sql += "from "+scm+".m_slsmn_hdr a ) where rno = 1) a, " + Environment.NewLine;
                sql += "(select a.slmslcd, a.effdt, a.agslcd, a.distslcd " + Environment.NewLine;
                sql += "from "+scm+".m_slsmn_agent a ) b " + Environment.NewLine;
                sql += "where a.slmslcd = b.slmslcd(+) and a.effdt = b.effdt(+) " + Environment.NewLine;
                sql += "and a.slmslcd || a.effdt in ( " + Environment.NewLine;
                sql += "select a.slmslcd || a.effdt from "+scm+".m_slsmn_brand a " + Environment.NewLine;
                if (brandcd.retStr() != "") sql += "where  a.BRANDCD in (" + brandcd + ") " + Environment.NewLine;
                sql += ") ) a, " + Environment.NewLine;
                sql += ""+scm+".m_retail b, "+scm+".m_cntrl_hdr c, improvar.ms_state d, " + Environment.NewLine;
                sql += ""+scmf+".m_subleg e, "+scmf+".m_subleg f, "+scmf+".m_subleg g, "+scm+".m_retail_link h " + Environment.NewLine;
                sql += "where a.slmslcd = b.slmslcd and b.m_autono = c.m_autono(+) and b.statecd = d.statecd(+) and a.slmslcd = g.slcd(+) " + Environment.NewLine;
                sql += "and a.distslcd = e.slcd(+) and a.agslcd = f.slcd(+) " + Environment.NewLine;
                sql += "and c.m_autono = h.m_autono and a.distslcd = h.slcd(+) " + Environment.NewLine;
                sql += "and  a.slmslcd in (" + SLMSLCD + ") ";
                if (statecd.retStr() != "") sql += Environment.NewLine + "and  b.STATECD in (" + statecd + ") ";
                if (city.retStr() != "") sql += Environment.NewLine + "and  b.city in (" + city + ") ";
                if (slmslcd.retStr() != "") sql += Environment.NewLine + "and  a.slmslcd in (" + slmslcd + ") ";
                if (distslcd.retStr() != "") sql += Environment.NewLine + "and  a.distslcd in (" + distslcd + ") ";
                sql += "order by a.slmslcd, g.slnm ,a.agslcd, f.slnm , a.distslcd, e.slnm ,b.rtlcd, b.rtlnm " + Environment.NewLine;
                DataTable tbl = MasterHelp.SQLquery(sql);
                if (tbl.Rows.Count == 0) return Content("No Records");

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                DataTable IR = new DataTable("mstrep");
                HC.RepStart(IR, 3);

                HC.GetPrintHeader(IR, "rtlnm", "string", "c,16", "Retailer");
                HC.GetPrintHeader(IR, "addrs", "string", "c,40", "Address");
                HC.GetPrintHeader(IR, "city", "string", "c,16", "City");
                HC.GetPrintHeader(IR, "PIN", "string", "c,16", "Pin Code");
                HC.GetPrintHeader(IR, "statenm", "string", "c,16", "State");
                HC.GetPrintHeader(IR, "cname", "string", "c,16", "Country");
                HC.GetPrintHeader(IR, "mobileno", "string", "c,16", "Mobile Nunber");
                HC.GetPrintHeader(IR, "whatsno", "string", "c,16", "Whatsapp Number");
                HC.GetPrintHeader(IR, "email", "string", "c,16", "Email ID");
                HC.GetPrintHeader(IR, "CPERSON", "string", "c,16", "Contact Person");
                HC.GetPrintHeader(IR, "CPERSONNO", "string", "c,16", "Contact Person No.");

                Int32 maxR = 0, i = 0, rNo = 0;
                i = 0; maxR = tbl.Rows.Count - 1;
                while (i <= maxR)
                {
                    string distributor = tbl.Rows[i]["distslcd"].ToString();
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["Dammy"] = "[ " + tbl.Rows[i]["distslnm"] + "  " + "] " + distributor;
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:18px;";

                    while (tbl.Rows[i]["distslcd"].ToString() == distributor)
                    {
                        DataRow dr = IR.NewRow();
                        dr["rtlnm"] = tbl.Rows[i]["rtlnm"].retStr() + "[" + tbl.Rows[i]["rtlcd"].retStr() + "]";
                        dr["addrs"] = tbl.Rows[i]["ADD1"].retStr() + " " + tbl.Rows[i]["ADD2"].retStr() + " " + tbl.Rows[i]["ADD3"].retStr() + " " + tbl.Rows[i]["ADD4"].retStr();
                        dr["city"] = tbl.Rows[i]["city"].retStr();
                        dr["PIN"] = tbl.Rows[i]["PIN"].retStr();
                        dr["statenm"] = tbl.Rows[i]["statenm"].retStr();
                        dr["cname"] = tbl.Rows[i]["COUNTRY"].retStr();
                        dr["mobileno"] = tbl.Rows[i]["REGMOBILE"].retStr();
                        dr["whatsno"] = tbl.Rows[i]["REGWHATSAPPNO"].retStr();
                        dr["email"] = tbl.Rows[i]["REGEMAIL"].retStr();
                        dr["CPERSON"] = tbl.Rows[i]["CPERSON"].retStr();
                        dr["CPERSONNO"] = tbl.Rows[i]["CPERSONNO"].retStr();
                        i++;
                        IR.Rows.Add(dr);
                        if (i > maxR) break;
                    }
                    if (i > maxR) break;
                }
                string repname = CommFunc.retRepname("RETAILER REGISTER");
                PV = HC.ShowReport(IR, repname, "RETAILER REGISTER");
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public string GetSalesman(string tdt, string uid)
        {
            string SLMSLCD = "";
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmp = CommVar.PaySchema(UNQSNO);

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
            DataTable tbl = MasterHelp.SQLquery(sql);
            if (tbl != null && tbl.Rows.Count > 0)
            {
                SLMSLCD = tbl.Rows[0]["slmslcd"].retStr();
            }
            return SLMSLCD;

        }
        public string retsizemaxmin(string sizecdgrp)
        {
            string chkval = sizecdgrp.Replace("^", "");
            string rval = "";
            string[] chk1 = chkval.Split(',');
            rval = chk1[0];
            if (chk1.Count() > 1)
            {
                rval = rval + "-" + chk1[chk1.Count() - 1];
            }
            if (rval == "") rval = sizecdgrp;
            return rval;
        }
    }
}