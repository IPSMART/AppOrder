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
using System.Configuration;
using CrystalDecisions.CrystalReports.Engine;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;

namespace Improvar.Controllers
{
    public class T_SLM_DLYREPController : Controller
    {
        Connection Cn = new Connection(); string sql = "";
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDown_Help = new DropDownHelp();

        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_Indent
        public ActionResult T_SLM_DLYREP(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {//k
            VM_SLMDLYREP VE = new VM_SLMDLYREP();
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ViewBag.formname = "Salesmen Daily Acitivity";
                    ViewBag.Title = "Salesmen Daily Acitivity";
                    VE.UNQSNO_ENCRYPTED = Cn.Encrypt_URL(UNQSNO);
                    VE.DocumentType = Cn.DOCTYPE1("SLMDA");

                    string GCS = Cn.GCS();
                    string[] linkcd = { "D", "A" };

                    string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmp = CommVar.PaySchema(UNQSNO);
                    string tdt = System.DateTime.Now.Date.retDateStr();
                    string uid = CommVar.UserID();
                    DataTable tbl = new DataTable();
                    VE.DropDown_list_REPAUTONO = new List<DropDown_list_REPAUTONO>();
                    T_CNTRL_HDR tcnrl = new T_CNTRL_HDR();
                    //tcnrl.DOCDT = System.DateTime.Now;
                    //if (VE.DocumentType != null && VE.DocumentType.Count() > 0)
                    //{
                    //    tcnrl.DOCCD = VE.DocumentType[0].value;
                    //}
                    VE.T_CNTRL_HDR = tcnrl;

                    VE.SLMSLCD = Salesfunc.GetSalesman(tdt, uid);
                    VE.MODETRAVEL = MODETRAVEL();
                    VE.BOOKUOM = BOOKUOM();

                    string sql1 = "select distinct a.autono, c.docno, a.docdt " + Environment.NewLine;
                    sql1 += "from " + scm + ".T_SLM_DLYREP_HDR a, " + scm + ".T_SLM_DLYREP b, " + scm + ".t_cntrl_hdr c " + Environment.NewLine;
                    sql1 += "where a.autono = b.autono(+) and a.autono = c.autono(+) and c.compcd = " + COM.retSqlformat() + "" + Environment.NewLine;
                    sql1 += "and a.slmslcd = " + VE.SLMSLCD.retSqlformat() + "" + Environment.NewLine;
                    DataTable tbl1 = masterHelp.SQLquery(sql1);

                    if (tbl1 != null && tbl1.Rows.Count > 0)
                    {
                        VE.DropDown_list_REPAUTONO = (from DataRow a in tbl1.Rows
                                                      select new DropDown_list_REPAUTONO()
                                                      {
                                                          value = a["autono"].retStr(),
                                                          text = a["docno"].retStr(),
                                                      }).OrderBy(x => x.value).ToList();
                    }
                    else
                    {
                        VE.DropDown_list_REPAUTONO = new List<DropDown_list_REPAUTONO>();
                    }


                    T_SLM_DLYREP_HDR TSLMDLYREPHDR = new T_SLM_DLYREP_HDR();
                    TSLMDLYREPHDR.DOCDT = System.DateTime.Now;
                    VE.T_SLM_DLYREP_HDR = TSLMDLYREPHDR;

                    List<TSLMDLYREP> SLMDLYREP = new List<TSLMDLYREP>();
                    for (int i = 0; i < 10; i++)
                    {
                        TSLMDLYREP SLMDLY_REP = new TSLMDLYREP();
                        SLMDLY_REP.SLNO = Convert.ToByte(i + 1);


                        SLMDLYREP.Add(SLMDLY_REP);
                    }
                    VE.TSLMDLYREP = SLMDLYREP;


                    VE.DefaultView = true;

                    return View(VE);

                }
            }

            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                ViewBag.ErrorMessage = ex.Message;
                VE.Add = "N";
                return View(VE);
            }
        }
        public ActionResult SAVE(FormCollection FC, VM_SLMDLYREP VE)
        {
            //Cn.getQueryString(VE);
            if (VE.REPAUTONO.retStr() == "")
            {
                VE.DefaultAction = "A";
            }
            else
            {
                VE.DefaultAction = "E";
            }
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            //Oracle Queries
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            string dbsql = "";
            string[] dbsql1;


            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;
            //
            DB.Configuration.ValidateOnSaveEnabled = false;
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    string ContentFlg = "";
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        T_SLM_DLYREP_HDR TSLMDLYREPHDR = new T_SLM_DLYREP_HDR();
                        T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                        string auto_no = "";
                        string tdt = System.DateTime.Now.Date.retDateStr();
                        string uid = CommVar.UserID();
                        TCH.DOCDT = VE.T_SLM_DLYREP_HDR.DOCDT;
                        string Ddate = TCH.DOCDT.retDateStr();
                        TSLMDLYREPHDR.CLCD = CommVar.ClientCode(UNQSNO);
                        string docno = ""; string Month = "", DOCCD = "";

                        VE.DocumentType = Cn.DOCTYPE1("SLMDA");
                        if (VE.DocumentType != null && VE.DocumentType.Count() > 0)
                        {
                            VE.T_CNTRL_HDR.DOCCD = VE.DocumentType[0].value;
                        }
                        if (VE.T_CNTRL_HDR.DOCCD.retStr() == "")
                        {
                            ContentFlg = "Plese Create Doc. Type for Salesmen Daily Acitivity"; goto dbnotsave;
                        }
                        if (VE.DefaultAction == "A")
                        {

                            TSLMDLYREPHDR.EMD_NO = 0;
                            DOCCD = VE.T_CNTRL_HDR.DOCCD;
                            TCH.DOCONLYNO = Cn.MaxDocNumber_Web(DOCCD, Ddate);
                            TCH.DOCNO = Cn.DocPattern_Web(Convert.ToInt32(TCH.DOCONLYNO), DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                            auto_no = Cn.Autonumber_Transaction_Web(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TCH.DOCONLYNO, DOCCD, Ddate);
                            TSLMDLYREPHDR.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                            Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();

                        }
                        else
                        {
                            var docData = (from p in DB.T_CNTRL_HDR
                                           where p.AUTONO == VE.REPAUTONO
                                           select new
                                           {
                                               p.DOCONLYNO,
                                               p.MNTHCD,
                                               p.DOCNO,
                                           }).FirstOrDefault();

                            DOCCD = VE.T_CNTRL_HDR.DOCCD;
                            TCH.DOCONLYNO = docData.DOCONLYNO;
                            TCH.DOCNO = docData.DOCNO;
                            TSLMDLYREPHDR.AUTONO = VE.REPAUTONO;
                            Month = docData.MNTHCD;

                            var MAXEMDNO = (from p in DB.T_CNTRL_HDR where p.AUTONO == TSLMDLYREPHDR.AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null) { TSLMDLYREPHDR.EMD_NO = 0; } else { TSLMDLYREPHDR.EMD_NO = Convert.ToInt16(MAXEMDNO + 1); }
                        }
                        TSLMDLYREPHDR.DOCDT = VE.T_SLM_DLYREP_HDR.DOCDT;
                        TSLMDLYREPHDR.SLMSLCD = VE.SLMSLCD.retStr();
                        //TSLMDLYREPHDR.DOCREM = VE.T_SLM_DLYREP_HDR.DOCREM.retStr();
                        if (VE.DefaultAction == "E")
                        {
                            dbsql = masterHelp.TblUpdt("T_SLM_DLYREP", TSLMDLYREPHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        }

                        //----------------------------------------------------------//
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TSLMDLYREPHDR.AUTONO, VE.DefaultAction, "S", Month, DOCCD, TCH.DOCNO, TCH.DOCDT.retStr(), TSLMDLYREPHDR.EMD_NO.retShort(), TCH.DOCONLYNO, Convert.ToDouble(TCH.DOCONLYNO), null, null, null, null, VE.T_CNTRL_HDR.DOCAMT.retDbl());
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                        dbsql = masterHelp.RetModeltoSql(TSLMDLYREPHDR, VE.DefaultAction);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        int COUNTER = 0;

                        for (int i = 0; i <= VE.TSLMDLYREP.Count - 1; i++)
                        {
                            if (VE.TSLMDLYREP[i].DTD.retStr() != "")
                            {
                                COUNTER = COUNTER + 1;
                                T_SLM_DLYREP TSLMDLYREP = new T_SLM_DLYREP();
                                TSLMDLYREP.EMD_NO = TSLMDLYREPHDR.EMD_NO;
                                TSLMDLYREP.CLCD = TSLMDLYREPHDR.CLCD;
                                TSLMDLYREP.DTAG = TSLMDLYREPHDR.DTAG;

                                TSLMDLYREP.AUTONO = TSLMDLYREPHDR.AUTONO;
                                TSLMDLYREP.SLNO = VE.TSLMDLYREP[i].SLNO;
                                TSLMDLYREP.DTD = VE.TSLMDLYREP[i].DTD;
                                TSLMDLYREP.PLFROM = VE.TSLMDLYREP[i].PLFROM;
                                TSLMDLYREP.PLTO = VE.TSLMDLYREP[i].PLTO;
                                TSLMDLYREP.MODETRAVEL = VE.TSLMDLYREP[i].MODETRAVEL;
                                TSLMDLYREP.KMUPDN = VE.TSLMDLYREP[i].KMUPDN;
                                TSLMDLYREP.CONVSTR = VE.TSLMDLYREP[i].CONVSTR;
                                TSLMDLYREP.CONVAMT = VE.TSLMDLYREP[i].CONVAMT;
                                TSLMDLYREP.TAAMT = VE.TSLMDLYREP[i].TAAMT;
                                TSLMDLYREP.DAAMT = VE.TSLMDLYREP[i].DAAMT;
                                TSLMDLYREP.BOOKQTY = VE.TSLMDLYREP[i].BOOKQTY;
                                TSLMDLYREP.BOOKUOM = VE.TSLMDLYREP[i].BOOKUOM;
                                TSLMDLYREP.REMK = VE.TSLMDLYREP[i].REMK;
                                dbsql = masterHelp.RetModeltoSql(TSLMDLYREP);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                        if (COUNTER == 0)
                        {
                            ContentFlg = "Select Ctg to save"; goto dbnotsave;
                        }

                        if (VE.DefaultAction == "A")
                        {
                            ContentFlg = "1~(Physical Stock No. " + TCH.DOCNO + ")";
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            ContentFlg = "2";
                        }
                        transaction.Commit();
                        OraTrans.Commit();
                        OraCon.Dispose();
                        return Content(ContentFlg);
                    }
                    else if (VE.DefaultAction == "V")
                    {
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_SLM_DLYREP_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", VE.T_SLM_DLYREP_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", VE.T_SLM_DLYREP_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("T_DEPT_PHYSTKDTL", VE.T_SLM_DLYREP_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("T_SLM_DLYREP_HDR", VE.T_SLM_DLYREP_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_SLM_DLYREP_HDR.AUTONO, "D", "S", null, null, null, VE.T_CNTRL_HDR.DOCDT.retStr(), null, null, null);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();


                        ModelState.Clear();
                        transaction.Commit();
                        OraTrans.Commit();
                        OraCon.Dispose();
                        return Content("3");
                    }
                    else
                    {
                        return Content("");
                    }
                    goto dbok;
                    dbnotsave:;
                    transaction.Rollback();
                    OraTrans.Rollback();
                    OraCon.Dispose();
                    return Content(ContentFlg);
                    dbok:;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    OraTrans.Rollback();
                    OraCon.Dispose();
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
        public ActionResult FillGrid(VM_SLMDLYREP VE, string REPAUTONO)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO);
                string Scm = CommVar.CurSchema(UNQSNO); string Scmf = CommVar.FinSchema(UNQSNO);
                string sql1 = "";
                string tdt = System.DateTime.Now.Date.retDateStr();
                string uid = CommVar.UserID();
                var GRID_DATA = "";

                sql1 = "select a.autono,b.slno,a.docrem,c.docno, a.docdt, b.itmctg, b.dtls, b.qnty, b.amt, b.distslcd, b.rtlcd, b.brandcd " + Environment.NewLine;
                sql1 += "from " + Scm + ".T_SLM_DLYREP_HDR a, " + Scm + ".T_SLM_DLYREP b, " + Scm + ".t_cntrl_hdr c " + Environment.NewLine;
                sql1 += "where a.autono = b.autono(+) and a.autono = c.autono(+) and c.compcd = " + COM.retSqlformat() + "" + Environment.NewLine;
                sql1 += "and a.autono = " + VE.REPAUTONO.retSqlformat() + "" + Environment.NewLine;
                sql1 += "order by b.slno " + Environment.NewLine;
                DataTable Record = masterHelp.SQLquery(sql1);

                if (Record.Rows.Count == 0)
                {
                    return Content("<center><h1>No Record Found !</h1></center>");
                }

                string DOCDT = Record.Rows[0]["DOCDT"].retDateStr();
                string DOCREM = Record.Rows[0]["DOCREM"].retStr();

                VE.TSLMDLYREP = (from DataRow dr in Record.Rows
                                 select new TSLMDLYREP()
                                 {
                                     SLNO = Convert.ToByte(dr["SLNO"].retInt()),
                                     DTD = Convert.ToDateTime(dr["DTD"].retStr()),
                                     PLFROM = dr["PLFROM"].retStr(),
                                     PLTO = dr["PLTO"].retStr(),
                                     MODETRAVEL = dr["MODETRAVEL"].retStr(),
                                     KMUPDN = dr["KMUPDN"].retShort(),
                                     CONVSTR = dr["CONVSTR"].retStr(),
                                     CONVAMT = dr["CONVAMT"].retDbl(),
                                     TAAMT = dr["TAAMT"].retDbl(),
                                     DAAMT = dr["DAAMT"].retDbl(),
                                     BOOKQTY = dr["BOOKQTY"].retDbl(),
                                     BOOKUOM = dr["BOOKUOM"].retStr(),
                                     REMK = dr["REMK"].retStr(),
                                 }).ToList();

                VE.MODETRAVEL = MODETRAVEL();
                VE.BOOKUOM = BOOKUOM();

                ModelState.Clear();
                VE.DefaultAction = "A";
                VE.DefaultView = true;
                GRID_DATA = RenderRazorViewToString(ControllerContext, "_T_SLM_DLYREP", VE);
                return Content(DOCDT + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + DOCREM + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + GRID_DATA);
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }
        }

        public static string RenderRazorViewToString(ControllerContext controllerContext, string viewName, object model)
        {
            controllerContext.Controller.ViewData.Model = model;

            using (var stringWriter = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var viewContext = new ViewContext(controllerContext, viewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, stringWriter);
                viewResult.View.Render(viewContext, stringWriter);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
                return stringWriter.GetStringBuilder().ToString();
            }
        }

        public ActionResult GetItemData(string itcd, string deptcd, string docdt)
        {
            try
            {
                string str = "";
                //DataTable dt = Salesfunc.GetPhysicalStock(deptcd, itcd.retSqlformat());
                //if (dt != null && dt.Rows.Count > 0)
                //{
                //    str += masterHelp.ToReturnFieldValues("", dt);
                //}
                //double pqnty = 0, wasqnty = 0, othqnty = 0;
                //DataTable proddata = Salesfunc.GetProduction(docdt, deptcd, itcd);
                //if (proddata != null && proddata.Rows.Count > 0)
                //{
                //    pqnty = (from DataRow dr in proddata.Rows
                //             where dr["itcd"].retStr() == itcd
                //             select dr["prodqnty"].retDbl()).Sum();

                //    wasqnty = (from DataRow dr in proddata.Rows
                //               where dr["itcd"].retStr() == itcd
                //               select dr["WASQNTY"].retDbl()).Sum();
                //}
                //DataTable depttrnfdata = Salesfunc.GetInterDeptTrnf(docdt, deptcd, itcd);
                //if (depttrnfdata != null && depttrnfdata.Rows.Count > 0)
                //{
                //    othqnty = (from DataRow dr in depttrnfdata.Rows
                //               where dr["itcd"].retStr() == itcd
                //               select dr["qnty"].retDbl()).Sum();

                //}
                //str += "^PRODQNTY=^" + pqnty + Cn.GCS();
                //str += "^WASQNTY=^" + wasqnty + Cn.GCS();
                //str += "^OTHQNTY=^" + othqnty + Cn.GCS();
                return Content(str);
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }

        }
        public ActionResult DeleteRow(VM_SLMDLYREP VE, int SerialNo)
        {
            try
            {
                List<TSLMDLYREP> TSLMDLYREP = new List<TSLMDLYREP>();
                int count = 0;
                for (int i = 0; i <= VE.TSLMDLYREP.Count - 1; i++)
                {
                    if (VE.TSLMDLYREP[i].SLNO != SerialNo)
                    {
                        count += 1;
                        TSLMDLYREP item = new TSLMDLYREP();
                        item = VE.TSLMDLYREP[i];
                        item.SLNO = Convert.ToByte(count);
                        TSLMDLYREP.Add(item);
                    }
                }
                VE.TSLMDLYREP = TSLMDLYREP;

                VE.MODETRAVEL = MODETRAVEL();
                VE.BOOKUOM = BOOKUOM();
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SLM_DLYREP", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddRow(VM_SLMDLYREP VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                if (VE.TSLMDLYREP == null)
                {
                    List<TSLMDLYREP> TXNDTL_HEAD = new List<TSLMDLYREP>();

                    TSLMDLYREP DTL = new TSLMDLYREP();
                    DTL.SLNO = 1;
                    TXNDTL_HEAD.Add(DTL);
                    VE.TSLMDLYREP = TXNDTL_HEAD;

                }
                else
                {
                    List<TSLMDLYREP> TXNDTL_HEAD = new List<TSLMDLYREP>();
                    for (int i = 0; i <= VE.TSLMDLYREP.Count - 1; i++)
                    {
                        TSLMDLYREP MIB = new TSLMDLYREP();
                        MIB = VE.TSLMDLYREP[i];
                        TXNDTL_HEAD.Add(MIB);
                    }

                    TSLMDLYREP MIB1 = new TSLMDLYREP();
                    MIB1.SLNO = Convert.ToByte(Convert.ToByte(VE.TSLMDLYREP.Max(a => Convert.ToInt32(a.SLNO))) + 1);
                    TXNDTL_HEAD.Add(MIB1);
                    VE.TSLMDLYREP = TXNDTL_HEAD;
                }

                VE.MODETRAVEL = MODETRAVEL();
                VE.BOOKUOM = BOOKUOM();

                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SLM_DLYREP", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public List<BOOKUOM> BOOKUOM()
        {
            List<BOOKUOM> DropDownlist = new List<BOOKUOM>();
            BOOKUOM BOOKUOM0 = new BOOKUOM();
            BOOKUOM0.Value = "BOX";
            BOOKUOM0.Text = "BOX";
            DropDownlist.Add(BOOKUOM0);
            BOOKUOM BOOKUOM1 = new BOOKUOM();
            BOOKUOM1.Value = "SET";
            BOOKUOM1.Text = "SET";
            DropDownlist.Add(BOOKUOM1);
            BOOKUOM BOOKUOM2 = new BOOKUOM();
            BOOKUOM2.Value = "PCS";
            BOOKUOM2.Text = "PCS";
            DropDownlist.Add(BOOKUOM2);
            return DropDownlist;
        }
        public List<TRANSMODE> MODETRAVEL()
        {
            List<TRANSMODE> DTYP = new List<TRANSMODE>();
            TRANSMODE DTYP3 = new TRANSMODE();
            DTYP3.Text = "Road";
            DTYP3.Value = "RO";
            DTYP.Add(DTYP3);
            TRANSMODE DTYP2 = new TRANSMODE();
            DTYP2.Text = "Rail";
            DTYP2.Value = "RA";
            DTYP.Add(DTYP2);
            TRANSMODE DTYP1 = new TRANSMODE();
            DTYP1.Text = "Air";
            DTYP1.Value = "AI";
            DTYP.Add(DTYP1);
            TRANSMODE DTYP4 = new TRANSMODE();
            DTYP4.Text = "Ship";
            DTYP4.Value = "SH";
            DTYP.Add(DTYP4);
            return DTYP;
        }
        public List<ListRetailer> GetRetailerforSelection(string SLMSLCD = "")
        {
            string GCS = Cn.GCS();
            List<ListRetailer> sllist = new List<ListRetailer>();
            string sql = "", scm = CommVar.CurSchema(UNQSNO);

            sql = "";
            sql += "select a.rtlcd, a.rtlnm, a.landmark " + Environment.NewLine;
            sql += "from " + scm + ".m_retail a," + scm + ".m_cntrl_hdr b " + Environment.NewLine;
            sql += "where a.m_autono = b.m_autono(+) and nvl(b.inactive_tag, 'N') = 'N'  " + Environment.NewLine;
            sql += "and a.SLMSLCD = '" + SLMSLCD.retStr() + "'  " + Environment.NewLine;
            sql += "order by rtlnm " + Environment.NewLine;
            DataTable tbl = masterHelp.SQLquery(sql);

            sllist = (from DataRow a in tbl.Rows
                      select new ListRetailer()
                      {
                          value = a["RTLCD"].retStr(),
                          text = a["RTLNM"].retStr() + GCS + a["LANDMARK"].retStr(),
                      }).ToList();
            return sllist;
        }


    }
}

