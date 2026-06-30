using System;
using System.Linq;
using System.Data;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class Rep_SLM_DLYREPController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string jobcd = "", jobnm = "", repname = "";
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_GL
        public ActionResult Rep_SLM_DLYREP()
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

                    ViewBag.formname = "Salesman Activity Report";
                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);

                    VE.ListSalesman = DropDownHelp.GetSalesmanforSelection();

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

        [HttpPost]
        public ActionResult Rep_SLM_DLYREP(ReportViewinHtml VE, FormCollection FC)
        {
            string ModuleCode = Module.Module_Code;
            try
            {
                //Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));

                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), csm = CommVar.CommSchema(), slmslcd = "";
                

                string fdt = VE.FDT;
                string tdt = VE.TDT;
                string tdt1 = System.DateTime.Now.Date.retDateStr();
                string uid = CommVar.UserID();
                slmslcd = GetSalesman(tdt1, uid).retSqlformat();
                if (slmslcd.retStr() == "")
                {
                    return Content("Please 'Salesmen Linkup with Agent/Brand' ");
                }
                string sql = "";

                sql += "select c.docno,c.docdt,b.AUTONO, b.SLMSLCD,d.slnm SLMSLNM, a.SLNO, a.DTD, a.PLFROM, a.PLTO, a.MODETRAVEL, a.KMUPDN, " + Environment.NewLine;
                sql += "a.CONVSTR, a.CONVAMT, a.TAAMT, a.DAAMT, a.BOOKQTY, a.BOOKUOM, a.REMK  " + Environment.NewLine;
                sql += " from " + Environment.NewLine;
                sql += "" + scm + ".T_SLM_DLYREP a, " + scm + ".T_SLM_DLYREP_HDR b, " + scm + ".t_cntrl_hdr c, " + Environment.NewLine;
                sql += "" + scmf + ".M_SUBLEG d " + Environment.NewLine;
                sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and b.SLMSLCD=d.SLCD(+)" + Environment.NewLine;
                sql += "and  b.slmslcd in (" + slmslcd + ") ";
                if (fdt.retStr() != "") sql += "and a.DTD >= to_date('" + fdt + "','dd/mm/yyyy')  " + Environment.NewLine;
                if (tdt.retStr() != "") sql += "and a.DTD <= to_date('" + tdt + "','dd/mm/yyyy')  " + Environment.NewLine;
                sql += "order by a.DTD,b.AUTONO,a.slno,c.docdt,b.SLMSLCD,d.slnm,a.DTD " + Environment.NewLine;
                DataTable tbl = MasterHelp.SQLquery(sql);
                if (tbl.Rows.Count == 0) return Content("No Records");

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                DataTable IR = new DataTable("mstrep");
                HC.RepStart(IR, 3);


                HC.GetPrintHeader(IR, "DTD", "string", "c,10", "Date");
                HC.GetPrintHeader(IR, "PLFROM", "string", "c,30", "Place;From");
                HC.GetPrintHeader(IR, "PLTO", "string", "c,30", "Place;To");
                HC.GetPrintHeader(IR, "KMUPDN", "double", "n,10", "Km.;(Up/Down)");
                HC.GetPrintHeader(IR, "CONVSTR", "string", "c,30", "Conv. fees;(Breakup)");
                HC.GetPrintHeader(IR, "CONVAMT", "double", "n,10,2", "Conv. fees");
                HC.GetPrintHeader(IR, "TAAMT", "double", "n,10,2", "T.A.");
                HC.GetPrintHeader(IR, "DAAMT", "double", "n,10,2", "D.A.");
                HC.GetPrintHeader(IR, "TOTCONV", "double", "n,10,2", "Total Conv.");
                HC.GetPrintHeader(IR, "BOOKQTY", "double", "n,10,2", "Booking;Qnty.");
                HC.GetPrintHeader(IR, "BOOKUOM", "string", "c,10", "Uom");
                HC.GetPrintHeader(IR, "REMK", "string", "c,30", "Remarks");

                double tamt = 0, tqnty = 0, tKMUPDN = 0, tCONVAMT = 0, tDAAMT = 0, tTCNVAMT = 0, tTAAMT = 0;

                double kmupdn = 0, convamt = 0, tottaamt = 0, taamt = 0, daamt = 0, bookqnty = 0;
                Int32 maxR = 0, i = 0, rNo = 0;
                i = 0; maxR = tbl.Rows.Count - 1;

                while (i <= maxR)
                {
                    string SLMSLCD = tbl.Rows[i]["SLMSLCD"].ToString();
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["Dammy"] = "[ " + SLMSLCD + "  " + "] " + tbl.Rows[i]["SLMSLNM"];
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";

                    while (tbl.Rows[i]["SLMSLCD"].ToString() == SLMSLCD)
                    {
                        string plfrom = "", plto = "", convstr = "";
                        double ttcnvamt = 0, ttqnty = 0, ttKMUPDN = 0, ttCONVAMT = 0, ttDAAMT = 0, ttTAAMT = 0;
                        string DTD = tbl.Rows[i]["DTD"].retDateStr();
                        double cnt = 0;
                        DataRow dr = IR.NewRow();
                        //dr["docdt"] = tbl.Rows[i]["docdt"].retDateStr();
                        //dr["docno"] = tbl.Rows[i]["docno"].retStr();
                        dr["DTD"] = tbl.Rows[i]["DTD"].retDateStr();
                        dr["BOOKUOM"] = tbl.Rows[i]["BOOKUOM"].retStr();
                        dr["REMK"] = tbl.Rows[i]["REMK"].retStr();
                        dr["PLFROM"] = tbl.Rows[i]["PLFROM"].retStr();
                        while (tbl.Rows[i]["SLMSLCD"].ToString() == SLMSLCD && tbl.Rows[i]["DTD"].retDateStr() == DTD)
                        {
                            if (cnt == 0)
                            {
                                plto = plto + tbl.Rows[i]["PLTO"].retStr();
                                convstr = convstr + tbl.Rows[i]["CONVSTR"].retStr();
                            }
                            else
                            {
                                if (tbl.Rows[i]["PLTO"].retStr() != "")
                                {
                                    plto = plto + "," + tbl.Rows[i]["PLTO"].retStr();
                                }
                                if (tbl.Rows[i]["CONVSTR"].retStr() != "")
                                {
                                    convstr = convstr + "+" + tbl.Rows[i]["CONVSTR"].retStr();
                                }
                            }

                            tamt += (tbl.Rows[i]["CONVAMT"].retDbl() + tbl.Rows[i]["TAAMT"].retDbl()).toRound();
                            tqnty += tbl.Rows[i]["BOOKQTY"].retDbl();
                            tKMUPDN += tbl.Rows[i]["KMUPDN"].retDbl();
                            tCONVAMT += tbl.Rows[i]["CONVAMT"].retDbl();
                            tDAAMT += tbl.Rows[i]["DAAMT"].retDbl();
                            tTAAMT += tbl.Rows[i]["TAAMT"].retDbl();
                            tTCNVAMT += (tbl.Rows[i]["CONVAMT"].retDbl() + tbl.Rows[i]["TAAMT"].retDbl().toRound() + tbl.Rows[i]["DAAMT"].retDbl());

                            ttcnvamt += (tbl.Rows[i]["CONVAMT"].retDbl() + tbl.Rows[i]["TAAMT"].retDbl().toRound() + tbl.Rows[i]["DAAMT"].retDbl());
                            ttqnty += tbl.Rows[i]["BOOKQTY"].retDbl();
                            ttKMUPDN += tbl.Rows[i]["KMUPDN"].retDbl();
                            ttCONVAMT += tbl.Rows[i]["CONVAMT"].retDbl();
                            ttDAAMT += tbl.Rows[i]["DAAMT"].retDbl();
                            ttTAAMT += tbl.Rows[i]["TAAMT"].retDbl();
                            cnt++;
                            i++;
                            if (i > maxR) break;
                        }

                        dr["PLTO"] = plto.retStr();
                        dr["CONVSTR"] = convstr.retStr();

                        dr["KMUPDN"] = ttKMUPDN.retDbl();
                        dr["CONVAMT"] = ttCONVAMT;
                        dr["TAAMT"] = ttTAAMT.retDbl();
                        dr["TOTCONV"] = ttcnvamt.retDbl();
                        dr["DAAMT"] = ttDAAMT.retDbl();
                        dr["BOOKQTY"] = ttqnty.retDbl();
                        IR.Rows.Add(dr);
                        if (i > maxR) break;
                    }

                    if (i > maxR) break;
                }
                DataRow dr2 = IR.NewRow();
                dr2["DTD"] = "Totals";
                dr2["BOOKQTY"] = tqnty;
                dr2["KMUPDN"] = tKMUPDN;
                dr2["CONVAMT"] = tCONVAMT;
                dr2["TOTCONV"] = tTCNVAMT;
                dr2["DAAMT"] = tDAAMT;
                dr2["TAAMT"] = tTAAMT;
                dr2["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                IR.Rows.Add(dr2);

                string repname = CommFunc.retRepname("Salesman Activity Register");
                PV = HC.ShowReport(IR, repname, "Salesman Activity Register from " + fdt + " to " + tdt);
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
    }
}