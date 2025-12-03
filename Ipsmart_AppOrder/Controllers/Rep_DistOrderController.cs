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
    public class Rep_DistOrderController : Controller
    {
        Connection Cn = new Connection(); string sql = "";
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc salesfunc = new Salesfunc();
        DropDownHelp dropDownHelp = new DropDownHelp();

        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_DistOrder
        public ActionResult Rep_DistOrder(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {//k
            ReportViewinHtml VE = new ReportViewinHtml();
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ViewBag.formname = "DISTRIBUTOR ORDER REGISTER";
                    ViewBag.Title = "Order";
                    VE.UNQSNO_ENCRYPTED = Cn.Encrypt_URL(UNQSNO);

                    string GCS = Cn.GCS();
                    string[] linkcd = { "D", "A" };

                    string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmp = CommVar.PaySchema(UNQSNO);
                    string tdt = System.DateTime.Now.Date.retDateStr();
                    string uid = CommVar.UserID();
                    DataTable tbl = new DataTable();

                    VE.SLMSLCD = salesfunc.GetSalesman(tdt, uid);

                    if (VE.SLMSLCD.retStr() != "")
                    {
                        VE.ListDistributor = dropDownHelp.GetDistributorforSelection(tdt, VE.SLMSLCD.retSqlformat());
                        VE.ListBrand = dropDownHelp.GetBrandforSelection(tdt, VE.SLMSLCD.retSqlformat());
                        VE.ListCollection = dropDownHelp.GetCollectionforSelection();
                    }
                    else
                    {
                        VE.ListDistributor = new List<ListDistributor>();
                        VE.ListBrand = new List<ListBrand>();
                        VE.ListCollection = new List<ListCollection>();
                    }
                    VE.ListRetailer = new List<ListRetailer>();
                    VE.ListGroup = new List<ListGroup>();


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
        public JsonResult BindRetailerData(string[] Distributor)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            try
            {
                string slcd = "";
                if (Distributor != null)
                {
                    slcd = Distributor.retSqlfromStrarray();
                }
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
                sql += "a.slcd in (" + slcd + ") ) a " + Environment.NewLine;
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
        public JsonResult BindGroupData(ReportViewinHtml VE)
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
        [HttpPost]
        public ActionResult Rep_DistOrder(ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), csm = CommVar.CommSchema(), usr_id = CommVar.UserID();
                string Dstbrslcd = "", RetailerCode = "", BrandCode = "", GroupCode = "", CollCode = "";
                if (VE.Dstbrslcd != null)
                {
                    Dstbrslcd = VE.Dstbrslcd.retSqlfromStrarray();
                }
                if (VE.RetailerCode != null)
                {
                    RetailerCode = VE.RetailerCode.retSqlfromStrarray();
                }
                if (VE.BrandCode != null)
                {
                    BrandCode = VE.BrandCode.retSqlfromStrarray();
                }
                if (VE.GroupCode != null)
                {
                    GroupCode = VE.GroupCode.retSqlfromStrarray();
                }
                if (VE.CollCode != null)
                {
                    CollCode = VE.CollCode.retSqlfromStrarray();
                }


                string sql = "";
                sql += Environment.NewLine + "select ''doconlyno,''vchrno,''doccd,''agslcd, ''trslcd, ''crslcd,  ''prccd, ''prceffdt, ";
                sql += Environment.NewLine + "''discrtcd, ''discrteffdt, ''docth, ''scmnm, ''prcnm, ''splnote,''cournm,''destn,''agslnm, ";
                sql += Environment.NewLine + "''district, ''trslnm,''crslnm,''totbox,''toset,''freestk,''rate,''ordqnty, ";
                sql += Environment.NewLine + " ''ordamt, ''delvtypedsc, ''rateprint,''slno,''stylno,''stktype, ";
                sql += Environment.NewLine + "''docth1, ''docth2, ''docth3, ''paytrmcd, ''paytrmnm, ''delvins, ''duedays, ''cod, ''prefno, ''prefdt, ";

                sql += Environment.NewLine + " a.autono, a.docno, a.docdt,a.VCHRNO, a.slcd,h.itnm, h.styleno,b.itcd,h.pcsperbox,h.pcsperset,h.colrperset,a.usr_id,a.usr_entdt, ";
                sql += Environment.NewLine + "m.RTLCD,n.RTLNM,a.slcd,e.slnm,e.add1 sladd1,e.add3 sladd2,e.add3 sladd3,e.add4 sladd4,e.add5 sladd5, ";
                sql += Environment.NewLine + "e.add6 sladd6,e.add7 sladd7,e.state slstate,e.REGEMAILID,e.PANNO slpanno,e.TANNO sltanno,e.REGEMAILID slemail, ";
                sql += Environment.NewLine + "e.REGMOBILE slmobile,e.GSTNO slgstno,a.SLMSLCD,f.slnm SLMSLNM,n.add1,n.add2,n.add3,n.add4,n.landmark,n.city,n.pin,g.statenm, ";
                sql += Environment.NewLine + "e.DISTRICT sldistrict,e.PIN slpin, n.GSTNO, n.REGMOBILE, n.REGEMAIL,b.SIZECD,b.QNTY,n.pan,n.REGWHATSAPPNO,j.brandcd,k.BRANDNM,h.uomcd, ";
                sql += Environment.NewLine + "l.RTLAUTONO,m.docno rtldocno,m.docdt rtldocdt from ";

                sql += Environment.NewLine + scm + ".T_DISTORDER a, " + scm + ".T_DISTORDERDTL b, " + scm + ".t_cntrl_hdr c, ";
                sql += Environment.NewLine + scmf + ".m_subleg e, " + scmf + ".m_subleg f, " + scm + ".m_sitem h, " + csm + ".ms_state g, " + scm + ".M_GROUP j, " + scm + ".M_BRAND k, ";
                sql += Environment.NewLine + scm + ".T_DISTORDLINK l, " + scm + ".T_RETAILORDER m, " + scm + ".M_RETAIL n ";
                sql += Environment.NewLine + "where a.autono=b.autono(+) and a.autono=c.autono(+) and a.slcd=e.slcd(+) and a.SLMSLCD=f.slcd(+) ";
                sql += Environment.NewLine + " and b.itcd = h.itcd(+) and n.STATECD = g.STATECD(+) and h.itgrpcd=j.itgrpcd(+) and j.brandcd = k.brandcd(+) ";
                sql += Environment.NewLine + " and b.autono=l.autono(+) and b.slno=l.slno(+) and l.RTLAUTONO=m.autono(+) and m.RTLCD=n.RTLCD(+) and a.SLMSLCD='" + VE.SLMSLCD + "' ";
                if (Dstbrslcd.retStr() != "") sql += Environment.NewLine + "and  a.slcd in (" + Dstbrslcd + ") ";
                if (RetailerCode.retStr() != "") sql += Environment.NewLine + "and  m.RTLCD in (" + RetailerCode + ") ";
                if (BrandCode.retStr() != "") sql += Environment.NewLine + "and  j.brandcd in (" + BrandCode + ") ";
                if (GroupCode.retStr() != "") sql += Environment.NewLine + "and  h.itgrpcd in (" + GroupCode + ") ";
                if (CollCode.retStr() != "") sql += Environment.NewLine + "and  h.COLLCD in (" + CollCode + ") ";
                sql += Environment.NewLine + "order by a.docdt,c.doconlyno,a.autono,l.RTLAUTONO,b.itcd,b.SIZECD ";

                DataTable tbl = masterHelp.SQLquery(sql);
                if (tbl.Rows.Count == 0) return Content("No Records");

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                DataTable IR = new DataTable("mstrep");
                HC.RepStart(IR, 3);

                HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Doc Date");
                HC.GetPrintHeader(IR, "docno", "string", "c,16", "Doc No");
                HC.GetPrintHeader(IR, "rtldocdt", "string", "c,10", "Rtl. Doc Date");
                HC.GetPrintHeader(IR, "rtldocno", "string", "c,16", "Rtl. Doc No");
                HC.GetPrintHeader(IR, "SLCD", "string", "c,10", "Distributor Code");
                HC.GetPrintHeader(IR, "SLnm", "string", "c,35", "Distributor Name");
                HC.GetPrintHeader(IR, "RTLCD", "string", "c,10", "Retailer Code");
                HC.GetPrintHeader(IR, "RTLnm", "string", "c,35", "Retailer Name");
                HC.GetPrintHeader(IR, "itcd", "string", "c,10", "Item Code");
                HC.GetPrintHeader(IR, "styleno", "string", "c,15", "Style No");
                HC.GetPrintHeader(IR, "itnm", "string", "c,25", "Item");
                HC.GetPrintHeader(IR, "pcsperbox", "double", "n,4", "P/Box");
                HC.GetPrintHeader(IR, "pcsperset", "double", "n,4", "P/Set");
                HC.GetPrintHeader(IR, "uom", "string", "c,4", "uom");
                HC.GetPrintHeader(IR, "qnty", "double", "n,12,2", "Qnty");
                HC.GetPrintHeader(IR, "sizedsp", "string", "c,50", "Size details");

                Int32 maxR = 0, i = 0, rNo = 0;
                i = 0; maxR = tbl.Rows.Count - 1;
                while (i <= maxR)
                {



                    string autono = tbl.Rows[i]["autono"].ToString();

                    while (tbl.Rows[i]["autono"].ToString() == autono)
                    {
                        DataRow dr = IR.NewRow();
                        dr["docdt"] = tbl.Rows[i]["docdt"].retDateStr();
                        dr["docno"] = tbl.Rows[i]["docno"];
                        dr["rtldocdt"] = tbl.Rows[i]["rtldocdt"].retDateStr();
                        dr["rtldocno"] = tbl.Rows[i]["rtldocno"];
                        dr["SLCD"] = tbl.Rows[i]["SLCD"];
                        dr["SLnm"] = tbl.Rows[i]["SLnm"];
                        dr["RTLCD"] = tbl.Rows[i]["RTLCD"];
                        dr["RTLnm"] = tbl.Rows[i]["RTLnm"];
                        dr["itcd"] = tbl.Rows[i]["itcd"];
                        dr["styleno"] = "<b>" + tbl.Rows[i]["styleno"] + "</b>";
                        dr["itnm"] = tbl.Rows[i]["itnm"];
                        dr["pcsperbox"] = tbl.Rows[i]["pcsperbox"].retDbl();
                        dr["pcsperset"] = tbl.Rows[i]["pcsperset"].retDbl();
                        dr["uom"] = tbl.Rows[i]["uomcd"];
                        dr["Flag"] = " class='grid_td'";

                        string itcd = tbl.Rows[i]["itcd"].ToString();
                        string RTLAUTONO = tbl.Rows[i]["RTLAUTONO"].ToString();
                        string sizes = ""; double tqnty = 0;
                        while (tbl.Rows[i]["autono"].ToString() == autono && tbl.Rows[i]["itcd"].ToString() == itcd && tbl.Rows[i]["RTLAUTONO"].ToString() == RTLAUTONO)
                        {
                            double qnty = 0;
                            string size = tbl.Rows[i]["sizecd"].ToString();
                            while (tbl.Rows[i]["autono"].ToString() == autono && tbl.Rows[i]["itcd"].ToString() == itcd && tbl.Rows[i]["RTLAUTONO"].ToString() == RTLAUTONO && tbl.Rows[i]["sizecd"].ToString() == size)
                            {
                                qnty += tbl.Rows[i]["qnty"].retDbl();
                                tqnty += tbl.Rows[i]["qnty"].retDbl();
                                i++;
                                if (i > maxR) break;
                            }
                            if (sizes != "") sizes += ", ";
                            sizes += salesfunc.retsizemaxmin(size) + "=" + qnty.ToString();
                            if (i > maxR) break;
                        }
                        dr["sizedsp"] = sizes;
                        dr["qnty"] = tqnty.retDbl();

                        IR.Rows.Add(dr);
                        if (i > maxR) break;
                    }
                    if (i > maxR) break;
                }
                string repname = CommFunc.retRepname("DISTRIBUTOR ORDER REGISTER");
                PV = HC.ShowReport(IR, repname, "DISTRIBUTOR ORDER REGISTER");
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }

    }
}

