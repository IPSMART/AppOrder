using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Collections;
using System.Security.Cryptography;

namespace Improvar.Controllers
{
    public class MultiviewerController : Controller
    {
        // GET: Multiviewer
        MasterHelpFa masterHelpFa = new MasterHelpFa();
        MasterHelp masterHelp = new MasterHelp();
        Connection Cn = new Connection();
        string CS = null;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult multiVu()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        private bool hasChildMenutbl(DataTable dt, string MENU_ID)
        {
            if (dt.Select("MENU_PARENT_ID='" + MENU_ID + "'").Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public ActionResult MenuCall(string code)
        {
            bool masterdisable = false;
            bool trandisable = false;
            string sql = "";
            sql = "select masterdisable from  sys_cnfg";
            DataTable rsTmp = masterHelp.SQLquery(sql);
            if (rsTmp.Rows.Count > 0)
            {
                if (rsTmp.Rows[0]["masterdisable"].ToString() == "Y") masterdisable = true;
            }
            if (Session["MIRROR_TAG"].ToString() == "Y")
            {
                trandisable = true;
            }
            string fld1 = "p.user_right,", fld2 = "";
            if (trandisable == true)
            {
                fld2 = " and (m.menu_type <> 'O' or menu_type is null) ";
            }
            if (masterdisable == true) { fld1 = "decode(m.menu_type,'M',trim(translate(p.USER_RIGHT,'AED','   ')),p.user_right) user_right,"; }
            if (trandisable == true) { fld1 = "decode(m.menu_type,'E',trim(translate(p.USER_RIGHT,'AED','   ')),p.user_right) user_right,"; }
            if (trandisable == true && masterdisable == true) { fld1 = "decode(m.menu_type,'M',trim(translate(p.USER_RIGHT,'AED','   ')),'E',trim(translate(p.USER_RIGHT,'AED','   ')),p.user_right) user_right,"; }

            sql = "Select m.MENU_ID,m.MENU_NAME,m.MENU_INDEX,m.MENU_PARENT_ID,m.menu_find_id,m.MENU_PROGCALL,"
               + " m.MENU_ID||'~'||m.MENU_INDEX||'~'||nvl(m.MENU_PROGCALL,'Notset')||'~'||m.menu_date_option||'~'||m.menu_type||'~'||p.E_DAY||'~'||p.D_DAY  as MENU_PERMISSIONID, ";
            sql += fld1;
            sql += "p.E_DAY,p.A_DAY,'Y' AS PERDOTNETMENU,'Y' AS ATVDOTNETMENU,p.D_DAY,m.MENU_ORDER_CODE,m.MENU_DOCCD,m.MENU_PARA  menu_para, ";
            sql += "(select case  when exists(select 1 from appl_menu_fav f where  m.menu_id=f.menu_id and m.menu_index=f.menu_index and m.module_code='"
                + CommVar.ModuleCode() + "' and user_id='" + CommVar.UserID() + "') then 'Y'  else 'N' end from dual) as menu_fav ";
            sql += " from " + CommVar.CurSchema(UNQSNO) + ".M_USR_ACS p , APPL_MENU m ";
            sql += " where p.MENU_NAME=m.MENU_ID and p.MENU_INDEX=m.MENU_INDEX and p.USER_ID='" + CommVar.UserID() + "' and m.module_code='" + CommVar.ModuleCode() + "' and upper(MENU_FIND_ID)='" + code.ToUpper() + "' AND ";
            sql += "p.compcd = '" + CommVar.Compcd(UNQSNO) + "' and p.loccd = '" + CommVar.Loccd(UNQSNO) + "' and p.schema_name = '" + CommVar.CurSchema(UNQSNO) + "' " + fld2;
            http://localhost:53704/TS_DOCAUTH/TS_DOCAUTH/?op=V&MNUDET="++"&US=Q0hFTUtPTEsyMDE5NDA3MTM=&DC=&MP=&param_SQL=yBwYqtpmZzR9rkMFTUWqjJGfAsaCOXPV2iioWExH6xw=
            //LinkUrl = "winopen('" + MENU_PROGCALL + "','" + enc_MENU_DETAIL + "','" + enc_MENU_DOCCD + "','" + enc_MENU_PARA + "')";
            //sql = "select menu_id,menu_name,menu_index,menu_type,menu_date_option from appl_menu where menu_progcall='" + MENU_PROGCALL + "' AND MODULE_CODE='"+MODULE_CODE+"'";
            DataTable DT = masterHelp.SQLquery(sql);

            if (DT.Rows.Count > 0)
            {
                var mnu = (from DataRow DR in DT.Rows
                           select new
                           {
                               MENU_PROGCALL = DR["MENU_PROGCALL"].retStr(),
                               MENU_PARA = DR["MENU_PARA"].retStr(),
                               menu_name = DR["menu_name"].retStr(),
                               MENU_DOCCD = DR["MENU_DOCCD"].retStr(),
                               user_right = DR["user_right"].retStr(),
                               MENU_PERMISSIONID = DR["MENU_PERMISSIONID"].retStr(),
                           }).FirstOrDefault();
                string enc_MENU_DETAIL = Cn.Encrypt_URL((mnu.MENU_PERMISSIONID + "~" + mnu.user_right));
                string enc_MENU_DOCCD = Cn.Encrypt_URL(mnu.MENU_DOCCD);
                string enc_MENU_PARA = Cn.Encrypt_URL(mnu.MENU_PARA);
                var URL = "../" + mnu.MENU_PROGCALL + "/" + mnu.MENU_PROGCALL + "/?op=V&MNUDET=" + enc_MENU_DETAIL + "&US=" + Cn.Encrypt_URL(UNQSNO) + "&DC=" + enc_MENU_DOCCD + "&MP=" + enc_MENU_PARA + "";
                return Content(URL);
            }
            else
            {
                return Content("0");
            }
        }
        public string addFavorite(string MENU_ID, string MENU_INDEX)
        {
            string schema = Cn.Getschema;
            string MODULE_CODE = Session["ModuleCode"].ToString();
            string Userid = Session["UR_ID"].ToString();
            int MENU_iNDX = Convert.ToInt32(MENU_INDEX);
            bool flag = false;

            try
            {
                CS = Cn.GetConnectionString();
                Cn.con = new OracleConnection(CS);
                if (Cn.con.State == ConnectionState.Closed)
                {
                    Cn.con.Open();
                }
                var transaction = Cn.con.BeginTransaction();
                string Query = "";
                string schm = Cn.Getschema;
                Query = "select * from " + schm + ".appl_menu_fav where user_id='" + Userid + "' and module_code='" + MODULE_CODE + "' and menu_id='" + MENU_ID + "' and menu_index=" + MENU_INDEX + "";

                DataTable rsTmp = masterHelp.SQLquery(Query);
                if (rsTmp.Rows.Count == 0)
                {
                    Query = "insert into " + schema + ".APPL_MENU_FAV(USER_ID,mODULE_CODE,MENU_iD,MENU_INDEX) VALUES('" + Userid + "','" + MODULE_CODE + "','" + MENU_ID + "'," + MENU_INDEX + ")";
                    flag = true;
                }
                else
                {
                    Query = "delete from " + schm + ".appl_menu_fav where user_id='" + Userid + "' and module_code='" + MODULE_CODE + "' and menu_id='" + MENU_ID + "' and menu_index=" + MENU_INDEX + "";
                }
                Cn.com = new OracleCommand(Query, Cn.con);
                Cn.com.ExecuteNonQuery();
                transaction.Commit();
                Cn.con.Close();
                if (flag == true) return "Added in Favorite Menu  [Re-login Needed]";
                else return "Removed From Favorite Menu  [Re-login Needed]";
            }
            catch (Exception ex)
            {
                Cn.con.Close();
                Cn.SaveException(ex, "");
                return ex.ToString();
            }
            finally
            {
                Cn.con.Close();
            }
            return "";
        }

        //public dynamic UpdateDeshBoard(string BoardCode, string Permission, string callFrom, Menu Mnu)
        public dynamic UpdateDeshBoard(Menu Mnu, string BoardCode)
        {
            try
            {
                string DB = CommVar.CurSchema(UNQSNO);
                string DBF = CommVar.FinSchema(UNQSNO);
                List<DashboardDetails> DashboardDetails = (List<DashboardDetails>)Session["DashboardDetails"];
                if (BoardCode != "ALL") { DashboardDetails = DashboardDetails.Where(D => D.BoardCode == BoardCode).ToList(); }
                foreach (DashboardDetails det in DashboardDetails)
                {
                    #region Dash_Authorise
                    if (det.BoardCode == "TS_DOCAUTH")
                    {
                        det.RefreshedTime = System.DateTime.Now.ToLongTimeString();
                        //string SQL = "select c.dname,b.doccd,d.doctype,count(b.doccd)total_docno,listagg(b.docno, ',' ) within group (order by b.docno)docno from ";
                        //SQL = SQL + "" + DB + ".t_cntrl_doc_pass a," + DB + ".t_cntrl_hdr b," + DB + ".m_dtype c," + DB + ".m_doctype d," + DBF + ".m_sign_auth e, ";
                        //SQL = SQL + "" + DBF + ".m_subleg f where a.autono = b.autono and a.authcd = e.authcd and b.doccd = d.doccd and d.doctype = c.dcd and b.slcd = f.slcd(+) ";
                        //SQL = SQL + "and e.usrid = '" + Session["UR_ID"].ToString() + "' group by c.dname,b.doccd,d.doctype ";

                        string SQL = "select d.docnm dname, b.doccd, 0 total_docno, d.doctype, b.docno from ";
                        SQL += DB + ".t_cntrl_doc_pass a," + DB + ".t_cntrl_hdr b," + DB + ".m_dtype c,";
                        SQL += DB + ".m_doctype d," + DBF + ".m_sign_auth e, " + DBF + ".m_subleg f ";
                        SQL += "where a.autono = b.autono and a.authcd = e.authcd and b.doccd = d.doccd and ";
                        SQL += "d.doctype = c.dcd and b.slcd = f.slcd(+) and nvl(b.cancel,'N')='N' and b.compcd='" + CommVar.Compcd(UNQSNO) + "' and ";
                        SQL += "e.usrid = '" + Session["UR_ID"].ToString() + "' ";
                        SQL += " and a.autono not in (select autono from " + DB + ".T_TXNSTATUS where STSTYPE = 'N') ";
                        SQL += "order by dname, doccd, doctype, docno ";
                        DataTable tbltmp = masterHelp.SQLquery(SQL);

                        DataTable tbl = new DataTable();
                        tbl.Columns.Add("dname", typeof(string));
                        tbl.Columns.Add("doccd", typeof(string));
                        tbl.Columns.Add("total_docno", typeof(int));
                        tbl.Columns.Add("doctype", typeof(string));
                        tbl.Columns.Add("docno", typeof(string));
                        tbl.Columns.Add("URL", typeof(string));

                        string DashURL = Cn.CreateMenuUrl("TS_DOCAUTH", "", "");

                        int rno = 0, totreco = 0;
                        int i = 0, maxR = tbltmp.Rows.Count - 1;
                        while (i <= maxR)
                        {
                            string doccd = tbltmp.Rows[i]["doccd"].retStr();
                            int treco = 0; string docno = "";
                            while (tbltmp.Rows[i]["doccd"].retStr() == doccd)
                            {
                                if (docno != "") docno += ",";
                                docno += tbltmp.Rows[i]["docno"];
                                i++;
                                treco++;
                                if (i > maxR) break;
                            }
                            tbl.Rows.Add("");
                            rno = tbl.Rows.Count - 1;
                            tbl.Rows[rno]["dname"] = tbltmp.Rows[i - 1]["dname"];
                            tbl.Rows[rno]["doccd"] = tbltmp.Rows[i - 1]["doccd"];
                            tbl.Rows[rno]["total_docno"] = treco;
                            tbl.Rows[rno]["doctype"] = tbltmp.Rows[i - 1]["doctype"];
                            tbl.Rows[rno]["docno"] = docno;
                            tbl.Rows[rno]["URL"] = DashURL;
                            totreco = totreco + treco;
                        }
                        for (int rowIndex = 0; rowIndex < tbl.Rows.Count; rowIndex++)
                        {
                            tbl.Rows[rowIndex]["URL"] = DashURL;
                        }
                        if (tbl.Rows.Count > 1)
                        {
                            DataRow drow = tbl.NewRow();
                            drow["dname"] = "View All";
                            drow["doccd"] = string.Join(", ", tbl.AsEnumerable().Select(a => a.Field<string>("doccd")));
                            drow["doctype"] = string.Join(",", tbl.AsEnumerable().Select(a => a.Field<string>("doctype")));
                            drow["total_docno"] = totreco;// tbl.AsEnumerable().Sum(a => a.Field<decimal?>("total_docno") ?? 0);
                            drow["docno"] = string.Join(",", tbl.AsEnumerable().Select(a => a.Field<string>("docno")));
                            drow["URL"] = DashURL;
                            tbl.Rows.InsertAt(drow, 0);
                        }
                        det.DataTable = tbl;
                        det.Permission = det.Permission;
                    }
                    #endregion
                    #region FINANCE
                    #endregion
                    #region INVENTORY
                    if (det.BoardCode == "INVBOARD1")
                    {
                        det.RefreshedTime = System.DateTime.Now.ToLongTimeString();
                        string str = "";
                        str += "select deptcd,deptdescn, nvl(MAX(DECODE(days1, 'Less15', pending)),0) AS Less15, nvl(MAX(DECODE(days1, '15-30', pending)), 0) AS GreterThan15to30,nvl(MAX(DECODE(days1, '31-45', pending)), 0) AS GreterThen30to45,nvl(MAX(DECODE(days1, 'more than 45', pending)), 0) AS MoreThan45 from (select deptcd, deptdescn, days1, count(days) as pending from ";
                        str += "(select deptcd, deptdescn, count(deptcd)totalitem, doccd, docno, docdt, days, (case when days < 15 then 'Less15' when days >= 15 and days <= 30 then '15-30' when days >= 31 and days <= 45 then '31-45' else 'more than 45' end) days1 from (select deptcd, doccd, docno, docdt, (trunc(sysdate) - docdt1)days, itcd, itdescn, uom, deptdescn, slcd, qnty, rate, amt, slnm, grnqty, cancqty from ";
                        str += " (select b.deptcd, a.doccd, a.docno, to_char(a.docdt, 'dd/mm/yyyy') docdt, a.docdt docdt1, b.itcd, c.itdescn, b.uom, d.deptdescn, a.slcd, f.cancel, b.slno, nvl(b.qnty, 0) qnty, b.rate, nvl(b.amt, 0) + nvl(b.line_amt, 0) + nvl(b.fix_amt, 0) amt, e.slnm, nvl((select sum(qnty)"
                            + " from " + DB + ".t_grn_dtl x, " + DB + ".t_cntrl_hdr y  where x.autono = y.autono and x.po_type || x.po_docno || x.po_docdt || x.slitcd || x.mslno = b.doccd || b.docno || b.docdt || b.slitcd || b.slno  and y.compcd = '" + CommVar.Compcd(UNQSNO) + "' and y.loccd = '" + CommVar.Loccd(UNQSNO) + "' and nvl(y.cancel, 'N') = 'N'), 0) grnqty, nvl((select sum(cancqty) "
                            + " from " + DB + ".t_doc_canc x, " + DB + ".t_cntrl_hdr y  where x.autono = y.autono and x.doc_doccd || x.doc_docno || x.doc_docdt || x.slno = b.doccd || b.docno || b.docdt || b.slno  and y.compcd = '" + CommVar.Compcd(UNQSNO) + "' and y.loccd = '" + CommVar.Loccd(UNQSNO) + "' and nvl(y.cancel, 'N') = 'N'), 0) cancqty  from "
                            + DB + ".t_pord a, " + DB + ".t_pord_dtl b, " + DB + ".m_item c, " + DB + ".m_dept d, " + DBF + ".m_subleg e, " + DB + ".t_cntrl_hdr f  where a.autono = b.autono and b.itcd = c.itcd(+) and b.deptcd = d.deptcd and a.autono = f.autono and f.compcd = '" + CommVar.Compcd(UNQSNO) + "' and f.loccd = '" + CommVar.Loccd(UNQSNO) + "'  and a.docdt <= sysdate and a.slcd = e.slcd) ";
                        str += " where doccd is not null  and nvl(qnty,0) -nvl(grnqty, 0) - nvl(cancqty, 0) > 0  and nvl(cancel,'N') = 'N' order by deptcd,docdt1,docno,slno)  group by deptcd,deptdescn,doccd,docno,docdt,days order by deptcd) group by deptcd,deptdescn,days1 order by deptcd) group by deptcd,deptdescn order by deptcd";




                        string SQL = "select doccd,docno,po_amt from " + DB + ".t_pord where rownum<=10 ";
                        DataTable tbl = new DataTable();
                        tbl = masterHelp.SQLquery(SQL);
                        det.DataTable = tbl;
                    }
                    if (det.BoardCode == "INVBOARD2")
                    {
                        det.RefreshedTime = System.DateTime.Now.ToLongTimeString();

                        string str = "";
                        str += "select deptcd,deptdescn, nvl(MAX(DECODE(days1, 'Less15', pending)),0) AS Less15, nvl(MAX(DECODE(days1, '15-30', pending)), 0) AS GreterThan15to30,nvl(MAX(DECODE(days1, '31-45', pending)), 0) AS GreterThen30to45,nvl(MAX(DECODE(days1, 'more than 45', pending)), 0) AS MoreThan45 from (select deptcd, deptdescn, days1, count(days) as pending from (select deptcd, deptdescn, count(deptcd)totalitem, doccd, docno, docdt, days, (case when days < 15 then 'Less15' when days >= 15 and days <= 30 then '15-30' when days >= 31 and days <= 45 then '31-45' else 'more than 45' end) days1 from ";
                        str += " (select deptcd, doccd, docno, docdt, docdt1, (trunc(sysdate) - docdt1)days, itcd, itdescn, uom, deptdescn, qnty, rate, amt, cancel, poqty, cancqty, grnqty from (select a.deptcd, a.doccd, a.docno, to_char(a.docdt, 'dd/mm/yyyy') docdt, a.docdt docdt1, b.itcd, c.itdescn, b.uom, d.deptdescn, nvl(b.qnty, 0) qnty, b.rate, nvl(b.amt, 0) amt, e.cancel, b.slno, nvl((select sum(qnty) from " + DB + ".t_pord_dtl x, " + DB + ".t_cntrl_hdr y  where x.autono = y.autono and x.ind_type || x.ind_docno || x.ind_docdt || x.slitcd = b.doccd || b.docno || b.docdt || b.slitcd  and y.compcd = '" + CommVar.Compcd(UNQSNO) + "' and y.loccd = '" + CommVar.Loccd(UNQSNO) + "' and nvl(y.cancel, 'N') = 'N'), 0) poqty, nvl((select sum(qnty) from " + DB + ".t_grn_dtl x, " + DB + ".t_cntrl_hdr y  where x.autono = y.autono and x.ind_type || x.ind_docno || x.ind_docdt || x.slitcd || x.mslno = b.doccd || b.docno || b.docdt || b.slitcd || b.slno  and y.compcd = '" + CommVar.Compcd(UNQSNO) + "' and y.loccd = '" + CommVar.Loccd(UNQSNO) + "' and nvl(y.cancel, 'N') = 'N' and x.po_type is null), 0) grnqty, nvl((select sum(cancqty) from " + DB + ".t_doc_canc x, " + DB + ".t_cntrl_hdr y  where x.autono = y.autono and x.doc_doccd || x.doc_docno || x.doc_docdt || x.slno = b.doccd || b.docno || b.docdt || b.slno  and y.compcd = '" + CommVar.Compcd(UNQSNO) + "' and y.loccd = '" + CommVar.Loccd(UNQSNO) + "' and nvl(y.cancel, 'N') = 'N'), 0) cancqty from " + DB + ".t_preq a, " + DB + ".t_preq_dtl b, " + DB + ".m_item c, " + DB + ".m_dept d, " + DB + ".t_cntrl_hdr e  where a.autono = b.autono and b.itcd = c.itcd(+) and a.deptcd = d.deptcd  and a.docdt <= sysdate  and a.autono = e.autono and e.compcd = '" + CommVar.Compcd(UNQSNO) + "' and e.loccd = '" + CommVar.Loccd(UNQSNO) + "' ) where doccd is not null  and nvl(qnty,0) -nvl(poqty, 0) - nvl(grnqty, 0) - nvl(cancqty, 0) > 0  and nvl(cancel,'N') = 'N' ";
                        str += " order by deptcd,docdt1,docno,slno) group by deptcd,deptdescn,doccd,docno,docdt,days order by deptcd) group by deptcd,deptdescn,days1 order by deptcd) group by deptcd,deptdescn order by deptcd";

                        string SQL = "select AUTONO,docno from " + DB + ".t_preq where rownum<=20 ";
                        DataTable tbl = new DataTable();
                        tbl = masterHelp.SQLquery(SQL);
                        det.DataTable = tbl;
                    }
                    #endregion
                    #region PAYROLL
                    else if (det.BoardCode == "DASH_PAY_APPROVAL")
                    {
                        det.RefreshedTime = System.DateTime.Now.ToLongTimeString();
                        var data = "";
                        DataTable tbl = new DataTable();
                        tbl.Columns.Add("Name", typeof(string));
                        tbl.Columns.Add("Total", typeof(string));
                        tbl.Columns.Add("URL", typeof(string));
                        string[] check = det.Permission.Split('/');
                        if (check[0].ToString() == "1")
                        {
                            data = "select c.empcd from " + CommVar.PaySchema(UNQSNO) + ".T_DLY_SATN c," + CommVar.PaySchema(UNQSNO) + ".M_EMPMAS e where c.EMPCD = e.EMPCD  ";
                            data += "and c.EMPCD not in (select w.EMPCD from " + CommVar.PaySchema(UNQSNO) + ".T_DLY_ATN w where w.EMPCD = c.EMPCD and w.ATNDT = c.ATNDT)  ";
                            var dt = masterHelp.SQLquery(data);
                            DataRow finh = tbl.NewRow();
                            finh["Name"] = "Attendance approval pending";
                            finh["Total"] = tbl.AsEnumerable().Count();
                            finh["URL"] = Cn.CreateMenuUrl("T_App_Atten", "", "");
                            tbl.Rows.Add(finh);
                        }
                        if (check[1].ToString() == "1")
                        {
                            data = "select a.AUTONO from " + CommVar.PaySchema(UNQSNO) + ".T_SLEAVE a where a.TRNS is null and a.FAUTONO is null  ";
                            var dt = masterHelp.SQLquery(data);
                            DataRow finh = tbl.NewRow();
                            finh["Name"] = "Leave application approval pending";
                            finh["Total"] = tbl.AsEnumerable().Count();
                            finh["URL"] = Cn.CreateMenuUrl("T_App_Leave", "", "");
                            tbl.Rows.Add(finh);
                        }
                        if (check[2].ToString() == "1")
                        {
                            data = "select a.AUTONO from " + CommVar.PaySchema(UNQSNO) + ".T_ATNREGFRM a where a.AUTONOAPP is null  ";
                            var dt = masterHelp.SQLquery(data);
                            DataRow finh = tbl.NewRow();
                            finh["Name"] = "Attendance regularization approval pending";
                            finh["Total"] = tbl.AsEnumerable().Count();
                            finh["URL"] = Cn.CreateMenuUrl("T_Attn_Regul", "", "");
                            tbl.Rows.Add(finh);
                        }
                        det.DataTable = tbl;

                    }
                    #endregion
                    #region SALES
                    #endregion


                }
                Mnu.DashboardList = DashboardDetails;
                return PartialView("_Dashboard", Mnu);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }

        public ActionResult SaveAttendanceData(string GPSLAT, string GPSLOT)
        {
            try
            {
                string msg = "";
                if (GPSLAT.retStr() != "")
                {
                    var currentaddress = masterHelp.GetAddress(GPSLAT, GPSLOT);
                    string sql = "INSERT INTO IMPROVAR.USER_APP_LOG (USER_ID, MOD_NM, SESSION_NO, LOGDT, LOGGEO, LOGGEONAME, FLAG1) ";
                    sql += "VALUES('" + CommVar.UserName() + "','" + Module.Module_Code + "','" + CommVar.SessionNo() + "',sysdate,'" + GPSLAT + "-" + GPSLOT + "','" + currentaddress + "','" + Cn.GetStaticIp() + "') ";
                    var res = masterHelp.SQLNonQuery(sql);
                    msg = "Attendance Saved";
                }
                else
                {
                    msg = "Location is required.";
                    ModelState.Clear();
                }
                return Content(msg);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }

    }
}