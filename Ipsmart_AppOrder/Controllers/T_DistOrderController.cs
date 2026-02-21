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
using System.Web.Hosting;
using System.Configuration;
using CrystalDecisions.CrystalReports.Engine;
using System.Text.RegularExpressions;

namespace Improvar.Controllers
{
    public class T_DistOrderController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_DistOrder
        public ActionResult T_DistOrder(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {//k
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    TransactionDistOrder VE;
                    if (TempData["DistOrderFilter"] == null)
                    {
                        VE = new TransactionDistOrder();
                    }
                    else
                    {
                        VE = (TransactionDistOrder)TempData["DistOrderFilter"];
                        TempData.Keep();
                    }
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                    ViewBag.DistributorName = VE.Dstbrslnm;
                    ViewBag.RetailerName = VE.RetailerName;
                    if (VE.BrandName.retStr() != "")
                    {
                        ViewBag.BrandName = "[" + VE.BrandName + "]";
                    }
                    if (VE.GroupName.retStr() != "")
                    {
                        ViewBag.GroupName = "[" + VE.GroupName + "]";
                    }
                    if (VE.CollName.retStr() != "")
                    {
                        ViewBag.CollectionName = "[" + VE.CollName + "]";
                    }

                    var dt = Salesfunc.GetPendingOrder(VE.T_DISTORDER.SLCD.retSqlformat(), VE.BrandCode.retSqlfromStrarray());
                    //dt.Columns.Add("SET", typeof(double), "");
                    //dt.Columns.Add("BOX", typeof(double), "");
                    //for (int i = 0; i <= dt.Rows.Count - 1; i++)
                    //{
                    //    double qnty = dt.Rows[i]["QNTY"].retDbl();
                    //    dt.Rows[i]["SET"] = Salesfunc.ConvPcstoSet(qnty, dt.Rows[i]["PCSPERSET"].retDbl());
                    //    dt.Rows[i]["BOX"] = Salesfunc.ConvPcstoBox(qnty, dt.Rows[i]["PCSPERBOX"].retDbl());
                    //}
                    VE.ListPendOrd = (from DataRow dr in dt.Rows
                                      group dr by new
                                      {
                                          RTLCD = dr["RTLCD"].retStr(),
                                          RTLAUTONO = dr["AUTONO"].retStr(),
                                          RTLNM = dr["RTLNM"].retStr(),
                                          RTLAREA = dr["landmark"].retStr() + dr["city"].retStr(),
                                          BRANDNM = dr["BRANDNM"].retStr(),
                                          BRANDCD = dr["BRANDCD"].retStr(),
                                      } into X

                                      select new ListPendOrd
                                      {
                                          RTLCD = X.Key.RTLCD.retStr(),
                                          RTLAUTONO = X.Key.RTLAUTONO.retStr(),
                                          RTLNM = X.Key.RTLNM.retStr() + " [" + X.Key.RTLAREA.retStr() + "]",
                                          RTLAREA = X.Key.RTLAREA.retStr(),
                                          BRANDNM = X.Key.BRANDNM.retStr(),
                                          BRANDCD = X.Key.BRANDCD.retStr(),
                                          QNTY = X.Sum(Z => Z.Field<double>("QNTY").retDbl()),
                                          //SET = X.Sum(Z => Z.Field<double>("SET").retDbl()),
                                          //BOX = X.Sum(Z => Z.Field<double>("BOX").retDbl()),
                                      }).ToList();

                    for (int i = 0; i <= VE.ListPendOrd.Count() - 1; i++)
                    {
                        VE.ListPendOrd[i].SLNO = (i + 1).retShort();
                        string RTLAUTONO = VE.ListPendOrd[i].RTLAUTONO;
                        string BRANDCD = VE.ListPendOrd[i].BRANDCD;

                        VE.ListPendOrdPopup = (from DataRow dr in dt.Rows
                                               where dr["AUTONO"].retStr() == RTLAUTONO && dr["BRANDCD"].retStr() == BRANDCD
                                               group dr by new
                                               {
                                                   STYLENO = dr["STYLENO"].retStr(),
                                                   RTLAUTONO = dr["AUTONO"].retStr(),
                                                   ITCD = dr["ITCD"].retStr(),
                                                   PCSPERBOX = dr["PCSPERBOX"].retDbl(),
                                                   PCSPERSET = dr["PCSPERSET"].retDbl(),
                                                   //SIZECD = dr["SIZECD"].retStr(),
                                                   ITREM = dr["ITREM"].retStr(),
                                                   MIXSIZE = dr["MIXSIZE"].retStr(),
                                               } into X

                                               select new ListPendOrdPopup
                                               {
                                                   ParentSerialNo = (i + 1).retShort(),
                                                   STYLENO = X.Key.STYLENO.retStr(),
                                                   RTLAUTONO = X.Key.RTLAUTONO.retStr(),
                                                   ITCD = X.Key.ITCD.retStr(),
                                                   PCSPERBOX = X.Key.PCSPERBOX.retDbl(),
                                                   PCSPERSET = X.Key.PCSPERSET.retDbl(),
                                                   ITREM = X.Key.ITREM.retStr(),
                                                   MIXSIZE = X.Key.MIXSIZE.retStr(),
                                                   SIZEDET = string.Join(",", X.GroupBy(z => z["SIZECD"].retStr()).Select(g => $"{g.Key}={g.Sum(z => z["QNTY"].retDbl())}")),
                                                   ALLSIZES = string.Join(",", X.GroupBy(z => z["SIZECD"].retStr()).Select(g => $"{g.Key}")),
                                                   TRTLQNTY = X.Sum(Z => Z.Field<double>("QNTY").retDbl()),
                                               }).OrderBy(a => a.ITCD).ToList();
                        double set = 0, box = 0;
                        for (int j = 0; j <= VE.ListPendOrdPopup.Count - 1; j++)
                        {
                            string ITCD = VE.ListPendOrdPopup[j].ITCD;
                            VE.ListPendOrdPopup[j].SLNO = (j + 1).retShort();
                            VE.ListPendOrdPopup[j].TRTLBOX = Salesfunc.ConvPcstoBox(VE.ListPendOrdPopup[j].TRTLQNTY, VE.ListPendOrdPopup[j].PCSPERBOX);
                            VE.ListPendOrdPopup[j].TRTLSET = Salesfunc.ConvPcstoSet(VE.ListPendOrdPopup[j].TRTLQNTY, VE.ListPendOrdPopup[j].PCSPERSET);

                            VE.ListPendOrdPopup[j].SET = VE.ListPendOrdPopup[j].TRTLSET;
                            VE.ListPendOrdPopup[j].QNTY = VE.ListPendOrdPopup[j].TRTLQNTY;
                            VE.ListPendOrdPopup[j].SIZE_COUNT = (from a in DB.M_SITEM_SIZE where a.ITCD == ITCD select a.SIZECD).Count();

                            set += VE.ListPendOrdPopup[j].TRTLSET;
                            box += VE.ListPendOrdPopup[j].TRTLBOX;

                        }
                        VE.ListPendOrd[i].SET = set;
                        VE.ListPendOrd[i].BOX = box;
                        VE.ListPendOrd[i].ORDDET = "Box=" + VE.ListPendOrd[i].BOX + ", Set=" + VE.ListPendOrd[i].SET;

                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        string JR = javaScriptSerializer.Serialize(VE.ListPendOrdPopup);
                        VE.ListPendOrd[i].ChildData = JR;
                    }

                    return View(VE);

                }
            }

            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult SAVE(FormCollection FC, TransactionDistOrder VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    string DefaultAction = "A";
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".T_DISTORDER in  row share mode");
                    if (DefaultAction == "A")
                    {
                        T_DISTORDER TDISTORDER = new T_DISTORDER();
                        TDISTORDER.CLCD = CommVar.ClientCode(UNQSNO);
                        TDISTORDER.DOCDT = System.DateTime.Now.Date;
                        string Ddate = Convert.ToString(TDISTORDER.DOCDT);

                        if (DefaultAction == "A")
                        {
                            TDISTORDER.EMD_NO = 0;
                            string DOCNO = Cn.MaxDocNumber(Ddate, "T_DISTORDER");
                            TDISTORDER.VCHRNO = DOCNO.Split(Convert.ToChar(Cn.GCS()))[0].retInt();
                            TDISTORDER.MNTHCD = DOCNO.Split(Convert.ToChar(Cn.GCS()))[1].ToString();

                            TDISTORDER.DOCNO = DocPattern(TDISTORDER.VCHRNO.retDbl(), TDISTORDER.MNTHCD);
                            TDISTORDER.AUTONO = "DST" + VE.T_DISTORDER.SLCD + TDISTORDER.VCHRNO.retStr().PadLeft(5, '0');

                        }
                        else
                        {
                            var MAXEMDNO = (from p in DB.T_DISTORDER where p.AUTONO == TDISTORDER.AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                TDISTORDER.EMD_NO = 0;
                            }
                            else
                            {
                                TDISTORDER.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                            TDISTORDER.VCHRNO = VE.T_DISTORDER.VCHRNO;
                            TDISTORDER.DOCNO = VE.T_DISTORDER.AUTONO;
                            TDISTORDER.AUTONO = VE.T_DISTORDER.AUTONO;
                            TDISTORDER.MNTHCD = VE.T_DISTORDER.MNTHCD;
                            TDISTORDER.DTAG = "E";
                        }
                        TDISTORDER.RTLCD = VE.T_DISTORDER.RTLCD;
                        TDISTORDER.SLCD = VE.T_DISTORDER.SLCD;
                        TDISTORDER.SLMSLCD = VE.T_DISTORDER.SLMSLCD;
                        TDISTORDER.DOCAMT = VE.T_DISTORDER.DOCAMT;

                        TDISTORDER.USR_ID = CommVar.UserID();
                        TDISTORDER.USR_ENTDT = System.DateTime.Now;
                        TDISTORDER.USR_SIP = Cn.GetStaticIp();

                        //TDISTORDER.LM_USR_ID = CommVar.UserID();
                        //TDISTORDER.LM_USR_ENTDT = System.DateTime.Now;
                        //TDISTORDER.LM_USR_SIP = Cn.GetStaticIp();
                        //TDISTORDER.LM_REM = "";

                        //TDISTORDER.DEL_USR_ID = CommVar.UserID();
                        //TDISTORDER.DEL_USR_ENTDT = System.DateTime.Now;
                        //TDISTORDER.DEL_USR_SIP =Cn.GetStaticIp();
                        //TDISTORDER.DEL_REM = "";

                        //TDISTORDER.CANCEL = "Y";
                        //TDISTORDER.CANC_REM = "";
                        //TDISTORDER.CANC_USR_ID = CommVar.UserID();
                        //TDISTORDER.CANC_USR_ENTDT = System.DateTime.Now;
                        //TDISTORDER.CANC_USR_SIP =Cn.GetStaticIp();

                        TDISTORDER.GPSLAT = VE.T_DISTORDER.GPSLAT;
                        TDISTORDER.GPSLOT = VE.T_DISTORDER.GPSLOT;
                        TDISTORDER.DOCREM = VE.T_DISTORDER.DOCREM;
                        TDISTORDER.GPSNM = masterHelp.GetAddress(VE.T_DISTORDER.GPSLAT.retStr(), VE.T_DISTORDER.GPSLOT.retStr());


                        if (DefaultAction == "A")
                        {
                            DB.T_DISTORDER.Add(TDISTORDER);
                        }
                        else if (DefaultAction == "E")
                        {
                            DB.Entry(TDISTORDER).State = System.Data.Entity.EntityState.Modified;
                        }
                        int slno = 0;
                        for (int i = 0; i <= VE.ListPendOrd.Count - 1; i++)
                        {
                            if (VE.ListPendOrd[i].CheckedORDSKIP == true)
                            {
                                if (VE.ListPendOrd[i].ORDSKIPREASON.retStr() == "")
                                {
                                    transaction.Rollback();
                                    return Content("Enter Reason for Skip Order");
                                }
                                slno++;
                                T_DISTORDLINK TDISTORDLINK = new T_DISTORDLINK();
                                TDISTORDLINK.CLCD = TDISTORDER.CLCD;
                                TDISTORDLINK.EMD_NO = TDISTORDER.EMD_NO;
                                TDISTORDLINK.AUTONO = TDISTORDER.AUTONO;
                                TDISTORDLINK.DTAG = TDISTORDER.DTAG;
                                TDISTORDLINK.RTLAUTONO = VE.ListPendOrd[i].RTLAUTONO;
                                TDISTORDLINK.SLNO = slno.retShort();
                                TDISTORDLINK.ORDSKIPREASON = VE.ListPendOrd[i].ORDSKIPREASON;
                                DB.T_DISTORDLINK.Add(TDISTORDLINK);
                            }
                            else
                            {
                                if (VE.ListPendOrd[i].ChildData != null && VE.ListPendOrd[i].ChildData != "[]")
                                {
                                    string data = VE.ListPendOrd[i].ChildData;
                                    var helpM = new List<Improvar.Models.ListPendOrdPopup>();
                                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                    helpM = javaScriptSerializer.Deserialize<List<Improvar.Models.ListPendOrdPopup>>(data);
                                    for (int j = 0; j <= helpM.Count - 1; j++)
                                    {
                                        var sizes = helpM[j].SIZEDET.retStr().Split(',');
                                        foreach (var sizeq in sizes)
                                        {
                                            var sqn = sizeq.retStr().Split('=');
                                            if (sqn.Length > 1)
                                            {
                                                if (sqn[1].retDbl() != 0)
                                                {
                                                    slno++;

                                                    T_DISTORDERDTL TDISTORDERDTL = new T_DISTORDERDTL();
                                                    TDISTORDERDTL.CLCD = TDISTORDER.CLCD;
                                                    TDISTORDERDTL.EMD_NO = TDISTORDER.EMD_NO;
                                                    TDISTORDERDTL.AUTONO = TDISTORDER.AUTONO;
                                                    TDISTORDERDTL.DTAG = TDISTORDER.DTAG;
                                                    TDISTORDERDTL.ITCD = helpM[j].ITCD;
                                                    TDISTORDERDTL.SLNO = slno.retShort();
                                                    TDISTORDERDTL.SIZECD = sqn[0];
                                                    TDISTORDERDTL.FREESTK = "";
                                                    TDISTORDERDTL.QNTY = sqn[1].retDbl();
                                                    TDISTORDERDTL.TRTLQNTY = helpM[j].TRTLQNTY;
                                                    TDISTORDERDTL.TSTKQNTY = helpM[j].TSTKQNTY;
                                                    DB.T_DISTORDERDTL.Add(TDISTORDERDTL);

                                                    T_DISTORDLINK TDISTORDLINK = new T_DISTORDLINK();
                                                    TDISTORDLINK.CLCD = TDISTORDER.CLCD;
                                                    TDISTORDLINK.EMD_NO = TDISTORDER.EMD_NO;
                                                    TDISTORDLINK.AUTONO = TDISTORDER.AUTONO;
                                                    TDISTORDLINK.DTAG = TDISTORDER.DTAG;
                                                    TDISTORDLINK.RTLAUTONO = helpM[j].RTLAUTONO;
                                                    TDISTORDLINK.SLNO = slno.retShort();
                                                    TDISTORDLINK.ORDSKIPREASON = helpM[j].ORDSKIPREASON;
                                                    DB.T_DISTORDLINK.Add(TDISTORDLINK);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                        }


                        DB.SaveChanges();
                        ModelState.Clear();
                        transaction.Commit();

                        masterHelp.SaveLocation(VE.T_DISTORDER.GPSLAT.retStr(), VE.T_DISTORDER.GPSLOT.retStr(), "DST" + Cn.GCS() + DefaultAction + Cn.GCS() + TDISTORDER.AUTONO);
                        string ContentFlg = "";
                        if (DefaultAction == "A")
                        {
                            string emailmsg = SendEmailWhatsapp(TDISTORDER.AUTONO);
                            ContentFlg = "1~(Distributor Order No. " + TDISTORDER.DOCNO + ")" + emailmsg;
                        }
                        else if (DefaultAction == "E")
                        {
                            ContentFlg = "2";
                        }
                        return Content(ContentFlg);

                    }
                    //else if (DefaultAction == "V")
                    //{
                    //    T_CNTRL_HDR MCH = Cn.T_CONTROL_HDR(VE.T_CNTRL_HDR.DOCCD, VE.T_CNTRL_HDR.DOCDT, VE.T_CNTRL_HDR.DOCNO, VE.T_DISTORDER.AUTONO, VE.T_CNTRL_HDR.MNTHCD, VE.T_CNTRL_HDR.DOCNO, "D", CommVar.CurSchema(UNQSNO), "", VE.T_DISTORDER.SLCD, 0, "", VE.T_CNTRL_HDR.YR_CD);
                    //    DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                    //    DB.SaveChanges();

                    //    DB.T_DISTORDER.Where(x => x.AUTONO == VE.T_DISTORDER.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                    //    DB.SaveChanges();

                    //    DB.T_DISTORDER.RemoveRange(DB.T_DISTORDER.Where(x => x.AUTONO == VE.T_DISTORDER.AUTONO));
                    //    DB.SaveChanges();
                    //    DB.T_CNTRL_HDR.RemoveRange(DB.T_CNTRL_HDR.Where(x => x.AUTONO == VE.T_DISTORDER.AUTONO));
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
                    transaction.Rollback();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                    //return Content("test");
                }
            }
            return null;
        }
        [HttpPost]
        public ActionResult T_DistOrder(TransactionRetailOrder VE, FormCollection FC, string submitbutton)
        {
            try
            {
                return SendEmailWhatsapp("DSTDA0008200001", true);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public dynamic SendEmailWhatsapp(string autonum, bool onlyprint = false)
        {
            try
            {
                string path_Save = "C:\\Ipsmart\\Temp";
                string id = "_" + System.DateTime.Now.ToString("yyMMddHHmmss").retStr();

                DataTable rstbl;
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), csm = CommVar.CommSchema(), usr_id = CommVar.UserID();
                List<string> ExcelFileNm = new List<string>();

                string sql = "";
                sql += Environment.NewLine + "select ''doconlyno,''vchrno,''doccd,''agslcd, ''trslcd, ''crslcd,  ''prccd, ''prceffdt, ";
                sql += Environment.NewLine + "''discrtcd, ''discrteffdt, ''docth, ''scmnm, ''prcnm, ''splnote,''cournm,''destn,''agslnm, ";
                sql += Environment.NewLine + "''district, ''trslnm,''crslnm,''totbox,''toset,''freestk,''rate,''ordqnty, ";
                sql += Environment.NewLine + " ''ordamt, ''delvtypedsc, ''rateprint,''slno,''stylno,''stktype, ";
                sql += Environment.NewLine + "''docth1, ''docth2, ''docth3, ''paytrmcd, ''paytrmnm, ''delvins, ''duedays, ''cod, ''prefno, ''prefdt, ";

                sql += Environment.NewLine + " a.autono, a.docno, a.docdt,a.VCHRNO, a.slcd,h.itnm, h.styleno,b.itcd,i.pcsperbox,i.pcsperset,i.colrperset,a.usr_id,a.usr_entdt, ";
                sql += Environment.NewLine + "a.RTLCD,d.RTLNM,a.slcd,e.slnm,e.add1 sladd1,e.add3 sladd2,e.add3 sladd3,e.add4 sladd4,e.add5 sladd5, ";
                sql += Environment.NewLine + "e.add6 sladd6,e.add7 sladd7,e.state slstate,e.REGEMAILID,nvl(e.WHATSAPP_NO,e.REGMOBILE)DISWHATSAPPNO,e.PANNO slpanno,e.TANNO sltanno,e.REGEMAILID slemail, ";
                sql += Environment.NewLine + "e.REGMOBILE slmobile,e.GSTNO slgstno,a.SLMSLCD,f.slnm SLMSLNM,d.add1,d.add2,d.add3,d.add4,d.landmark,d.city,d.pin,g.statenm, ";
                sql += Environment.NewLine + "e.DISTRICT sldistrict,e.PIN slpin, d.GSTNO, d.REGMOBILE,nvl(d.REGWHATSAPPNO,d.REGMOBILE)REGWHATSAPPNO, d.REGEMAIL,b.SIZECD,b.QNTY,d.pan,j.brandcd,k.BRANDNM from ";

                sql += Environment.NewLine + scm + ".T_DISTORDER a, " + scm + ".T_DISTORDERDTL b, " + scm + ".t_cntrl_hdr c, " + scm + ".M_RETAIL d, ";
                sql += Environment.NewLine + scmf + ".m_subleg e, " + scmf + ".m_subleg f, " + scm + ".m_sitem h, " + csm + ".ms_state g, " + scm + ".M_SITEM i, " + scm + ".M_GROUP j, " + scm + ".M_BRAND k ";
                sql += Environment.NewLine + "where a.autono=b.autono(+) and a.autono=c.autono(+) and a.RTLCD=d.RTLCD(+) and a.slcd=e.slcd(+) and a.SLMSLCD=f.slcd(+) ";
                sql += Environment.NewLine + " and b.itcd = h.itcd(+) and d.STATECD = g.STATECD(+) and b.itcd = i.itcd(+) and i.itgrpcd=j.itgrpcd(+) and j.brandcd = k.brandcd(+) ";
                sql += Environment.NewLine + "and a.autono in ('" + autonum + "') ";
                sql += Environment.NewLine + "order by a.docdt,a.VCHRNO,k.BRANDNM,h.styleno,b.itcd ";
                rstbl = masterHelp.SQLquery(sql);

                string AUTO_NO = string.Join(",", (from DataRow dr in rstbl.Rows select "'" + dr["autono"].ToString() + "'").Distinct());

                DataTable IR = new DataTable();

                IR.Columns.Add("docno", typeof(string), "");
                IR.Columns.Add("docdt", typeof(string), "");
                IR.Columns.Add("slnm", typeof(string), "");
                IR.Columns.Add("slcd", typeof(string), "");

                IR.Columns.Add("destn", typeof(string), "");
                IR.Columns.Add("slmslnm", typeof(string), "");
                IR.Columns.Add("slmslcd", typeof(string), "");
                IR.Columns.Add("rem", typeof(string), "");
                IR.Columns.Add("totbox", typeof(string), "");
                IR.Columns.Add("toset", typeof(string), "");
                IR.Columns.Add("ordamt", typeof(double), "");
                IR.Columns.Add("delvtypedsc", typeof(string), "");
                //extra
                IR.Columns.Add("rateprint", typeof(string), "");
                IR.Columns.Add("sldistrict", typeof(string), "");
                IR.Columns.Add("pcstyle", typeof(string), "");
                IR.Columns.Add("usr_id", typeof(string), "");
                IR.Columns.Add("usr_entdt", typeof(string), "");
                //details
                IR.Columns.Add("slno", typeof(double), "");
                IR.Columns.Add("styleno", typeof(string), "");
                IR.Columns.Add("itnm", typeof(string), "");
                IR.Columns.Add("SIZECD", typeof(string), "");
                IR.Columns.Add("boxpcs", typeof(string), "");
                IR.Columns.Add("tbox", typeof(double), "");
                IR.Columns.Add("tset", typeof(double), "");
                IR.Columns.Add("tpcs", typeof(double), "");
                IR.Columns.Add("rate", typeof(double), "");
                IR.Columns.Add("obldt1", typeof(string), "");
                IR.Columns.Add("oblno1", typeof(string), "");
                IR.Columns.Add("oblamt1", typeof(string), "");
                IR.Columns.Add("osamt1", typeof(string), "");
                IR.Columns.Add("obldt2", typeof(string), "");
                IR.Columns.Add("oblno2", typeof(string), "");
                IR.Columns.Add("oblamt2", typeof(string), "");
                IR.Columns.Add("osamt2", typeof(string), "");
                IR.Columns.Add("totos", typeof(string), "");
                IR.Columns.Add("RTLNM", typeof(string), "");
                IR.Columns.Add("ADD1", typeof(string), "");
                IR.Columns.Add("ADD2", typeof(string), "");
                IR.Columns.Add("ADD3", typeof(string), "");
                IR.Columns.Add("ADD4", typeof(string), "");
                IR.Columns.Add("sladd", typeof(string), "");
                IR.Columns.Add("sladd1", typeof(string), "");
                IR.Columns.Add("sladd2", typeof(string), "");
                IR.Columns.Add("sladd3", typeof(string), "");
                IR.Columns.Add("sladd4", typeof(string), "");
                IR.Columns.Add("sladd5", typeof(string), "");
                IR.Columns.Add("sladd6", typeof(string), "");
                IR.Columns.Add("sladd7", typeof(string), "");
                IR.Columns.Add("slstate", typeof(string), "");
                IR.Columns.Add("slmobile", typeof(string), "");
                IR.Columns.Add("slemail", typeof(string), "");
                IR.Columns.Add("sltanno", typeof(string), "");
                IR.Columns.Add("slpanno", typeof(string), "");
                IR.Columns.Add("slgstno", typeof(string), "");
                IR.Columns.Add("brandnm", typeof(string), "");
                IR.Columns.Add("LANDMARK", typeof(string), "");
                IR.Columns.Add("CITY", typeof(string), "");
                IR.Columns.Add("PIN", typeof(string), "");
                IR.Columns.Add("STATENM", typeof(string), "");
                IR.Columns.Add("GSTNO", typeof(string), "");
                IR.Columns.Add("PAN", typeof(string), "");
                IR.Columns.Add("REGMOBILE", typeof(string), "");
                IR.Columns.Add("RTLREGEMAIL", typeof(string), "");
                IR.Columns.Add("DISREGEMAILID", typeof(string), "");
                IR.Columns.Add("DISWHATSAPPNO", typeof(string), "");
                IR.Columns.Add("autono", typeof(string), "");
                IR.Columns.Add("QNTY", typeof(double), "");
                IR.Columns.Add("REGWHATSAPPNO", typeof(string), "");
                IR.Columns.Add("compnm", typeof(string), "");
                IR.Columns.Add("compadd", typeof(string), "");
                IR.Columns.Add("compcommu", typeof(string), "");
                IR.Columns.Add("compstat", typeof(string), "");
                IR.Columns.Add("compREGEMAILID", typeof(string), "");
                IR.Columns.Add("compREGWHATSAPPNO", typeof(string), "");
                IR.Columns.Add("locaadd", typeof(string), "");
                IR.Columns.Add("locacommu", typeof(string), "");
                IR.Columns.Add("locastat", typeof(string), "");
                IR.Columns.Add("legalname", typeof(string), "");
                IR.Columns.Add("corpadd", typeof(string), "");
                IR.Columns.Add("corpcommu", typeof(string), "");
                IR.Columns.Add("brandcd", typeof(string), "");

                Int32 maxR = rstbl.Rows.Count - 1;
                Int32 i = 0; double partytotos = 0, totbox = 0, totset = 0, approxvalue = 0;
                string billno = "", slcd = "";
                int slno = 0;


                while (i <= maxR)
                {
                    string brandcd = rstbl.Rows[i]["brandcd"].ToString();
                    DataTable comdet = Salesfunc.GetComcdByBrand(rstbl.Rows[i]["brandcd"].ToString());
                    string compcd = comdet.Rows[0]["compcd"].retStr();
                    string loccd = comdet.Rows[0]["loccd"].retStr();
                    string compdtl = "", compemail = "";
                    compdtl = Salesfunc.retCompAddress("", compemail, compcd, loccd);

                    while (rstbl.Rows[i]["brandcd"].ToString() == brandcd)
                    {
                        string autono = rstbl.Rows[i]["autono"].ToString();
                        string docdt = rstbl.Rows[i]["docdt"].ToString().retDateStr();
                        billno = rstbl.Rows[i]["docno"].ToString();
                        slcd = rstbl.Rows[i]["RTLCD"].ToString();
                        double tset = 0;

                        DataRow Row1 = IR.NewRow();
                        Row1["brandcd"] = rstbl.Rows[i]["brandcd"].ToString();
                        Row1["brandnm"] = rstbl.Rows[i]["brandnm"].ToString();

                        Row1["compnm"] = compdtl.retCompValue("compnm");
                        Row1["compadd"] = compdtl.retCompValue("compadd");
                        Row1["compcommu"] = compdtl.retCompValue("compcommu");
                        Row1["compstat"] = compdtl.retCompValue("compstat");
                        Row1["locaadd"] = compdtl.retCompValue("locaadd");
                        Row1["locacommu"] = compdtl.retCompValue("locacommu");
                        Row1["locastat"] = compdtl.retCompValue("locastat");
                        Row1["legalname"] = compdtl.retCompValue("legalname");
                        Row1["corpadd"] = compdtl.retCompValue("corpadd");
                        Row1["corpcommu"] = compdtl.retCompValue("corpcommu");
                        Row1["compREGEMAILID"] = comdet.Rows[0]["REGEMAIL"].retStr();
                        Row1["compREGWHATSAPPNO"] = comdet.Rows[0]["REGMOBILE"].retStr();
                        Row1["docno"] = rstbl.Rows[i]["docno"].ToString();
                        /* Row1["docdt"] = prndt; rstbl.Rows[i]["docdt"].ToString().Remove(10);*/
                        Row1["docdt"] = rstbl.Rows[i]["docdt"].ToString().Remove(10);
                        Row1["slnm"] = rstbl.Rows[i]["slnm"].ToString();
                        Row1["slcd"] = rstbl.Rows[i]["slcd"].ToString();
                        Row1["slmslnm"] = rstbl.Rows[i]["SLMSLNM"].ToString();
                        Row1["slmslcd"] = rstbl.Rows[i]["slmslcd"].ToString();
                        Row1["ordamt"] = approxvalue;
                        //extra
                        Row1["sldistrict"] = " " + rstbl.Rows[i]["sldistrict"].ToString() + " - " + rstbl.Rows[i]["slpin"].ToString() + ", " + rstbl.Rows[i]["slstate"].ToString();
                        Row1["RTLREGEMAIL"] = rstbl.Rows[i]["REGEMAIL"].ToString();
                        Row1["DISREGEMAILID"] = rstbl.Rows[i]["REGEMAILID"].ToString();
                        Row1["DISWHATSAPPNO"] = rstbl.Rows[i]["DISWHATSAPPNO"].ToString();
                        Row1["usr_id"] = rstbl.Rows[i]["usr_id"].ToString();
                        Row1["usr_entdt"] = rstbl.Rows[i]["usr_entdt"].ToString();
                        Row1["rem"] = "";
                        Row1["autono"] = rstbl.Rows[i]["autono"].ToString();
                        Row1["QNTY"] = rstbl.Rows[i]["QNTY"].ToString();
                        //Row1["REGWHATSAPPNO"] = comdet.Rows[0]["REGMOBILE"].retStr();// rstbl.Rows[i]["REGWHATSAPPNO"].ToString();
                        //details table
                        slno++;
                        Row1["slno"] = slno;
                        Row1["styleno"] = rstbl.Rows[i]["styleno"];
                        Row1["itnm"] = rstbl.Rows[i]["itnm"].ToString();
                        Row1["rateprint"] = "Y";
                        //last table 
                        Row1["RTLNM"] = rstbl.Rows[i]["RTLNM"].ToString();
                        Row1["ADD1"] = rstbl.Rows[i]["ADD1"].ToString();
                        Row1["ADD2"] = rstbl.Rows[i]["ADD2"].ToString();
                        Row1["ADD3"] = rstbl.Rows[i]["ADD3"].ToString();
                        Row1["ADD4"] = rstbl.Rows[i]["ADD4"].ToString();

                        Row1["sladd1"] = rstbl.Rows[i]["sladd1"].ToString();
                        Row1["sladd2"] = rstbl.Rows[i]["sladd2"].ToString();
                        Row1["sladd3"] = rstbl.Rows[i]["sladd3"].ToString();
                        Row1["sladd4"] = rstbl.Rows[i]["sladd4"].ToString();
                        Row1["sladd5"] = rstbl.Rows[i]["sladd5"].ToString();
                        Row1["sladd6"] = rstbl.Rows[i]["sladd6"].ToString();
                        Row1["sladd7"] = rstbl.Rows[i]["sladd7"].ToString();
                        Row1["sladd"] = rstbl.Rows[i]["sladd1"].ToString() + " " + rstbl.Rows[i]["sladd2"].ToString() + " " + rstbl.Rows[i]["sladd3"].ToString() + " " + rstbl.Rows[i]["sladd4"].ToString() + " " + rstbl.Rows[i]["sladd5"].ToString() + " ";
                        //if (!string.IsNullOrEmpty(add))
                        //{
                        //Row1["sladd"] = add;
                        //}
                        //else
                        //{
                        //    Row1["sladd"] = "";
                        //}
                        Row1["slstate"] = rstbl.Rows[i]["slstate"].ToString();
                        Row1["slmobile"] = rstbl.Rows[i]["slmobile"].ToString();
                        Row1["slemail"] = rstbl.Rows[i]["slemail"].ToString();
                        //Row1["sltanno"] = rstbl.Rows[i]["sltanno"].ToString();
                        string tan = rstbl.Rows[i]["sltanno"].ToString();
                        if (!string.IsNullOrEmpty(tan))
                        {
                            Row1["sltanno"] = "TAN#" + tan;
                        }
                        else
                        {
                            Row1["sltanno"] = "";
                        }
                        //Row1["slpanno"] = rstbl.Rows[i]["slpanno"].ToString();
                        string pan = rstbl.Rows[i]["slpanno"].ToString();
                        if (!string.IsNullOrEmpty(pan))
                        {
                            Row1["slpanno"] = "PAN#" + pan;
                        }
                        else
                        {
                            Row1["slpanno"] = "";
                        }
                        Row1["slgstno"] = rstbl.Rows[i]["slgstno"].ToString();

                        Row1["LANDMARK"] = rstbl.Rows[i]["LANDMARK"].ToString();
                        Row1["CITY"] = rstbl.Rows[i]["CITY"].ToString();
                        Row1["PIN"] = rstbl.Rows[i]["PIN"].ToString();
                        Row1["STATENM"] = rstbl.Rows[i]["STATENM"].ToString();
                        Row1["GSTNO"] = rstbl.Rows[i]["GSTNO"].ToString();
                        Row1["PAN"] = rstbl.Rows[i]["pan"].ToString();
                        Row1["REGMOBILE"] = rstbl.Rows[i]["REGMOBILE"].ToString();



                        string check1 = rstbl.Rows[i]["itcd"].ToString();
                        string pcstyle = "", sizes = "", boxes = "";
                        double tbox = 0, tpcs = 0, rate = 0, ordqnty = 0, chkpcs = 0;

                        rate = (rstbl.Rows[i]["rate"]).retDbl();
                        ordqnty = (rstbl.Rows[i]["ordqnty"]).retDbl();

                        pcstyle = rstbl.Rows[i]["pcsperbox"].ToString() + "/" + rstbl.Rows[i]["pcsperset"].ToString() + "/" + rstbl.Rows[i]["colrperset"].ToString();

                        while (rstbl.Rows[i]["itcd"].ToString() == check1)
                        {
                            approxvalue += Math.Round(rstbl.Rows[i]["rate"].retDbl() * rstbl.Rows[i]["ordqnty"].retDbl(), 2);//new
                            string fld = "QNTY";

                            double dbboxes = Salesfunc.ConvPcstoBox((rstbl.Rows[i][fld]).retDbl(), (rstbl.Rows[i]["pcsperbox"]).retDbl());
                            if (boxes != "") boxes += "+";
                            if (sizes != "") sizes += ",";
                            sizes += rstbl.Rows[i]["SIZECD"] + "=" + dbboxes.ToString();
                            boxes += dbboxes.ToString();
                            tpcs = tpcs + (rstbl.Rows[i][fld]).retDbl();

                            chkpcs = chkpcs + (rstbl.Rows[i][fld]).retDbl();
                            i++;
                            if (i > maxR) break;
                        }
                        tbox = Salesfunc.ConvPcstoBox(chkpcs, (rstbl.Rows[i - 1]["pcsperbox"]).retDbl());

                        tset = Salesfunc.ConvPcstoSet(chkpcs, (rstbl.Rows[i - 1]["pcsperset"]).retDbl());//new
                        totbox += tbox;
                        totset += tset;//new

                        Row1["pcstyle"] = pcstyle;
                        Row1["SIZECD"] = sizes;
                        Row1["boxpcs"] = boxes;
                        Row1["tbox"] = tbox;
                        Row1["tset"] = tset;
                        Row1["tpcs"] = tpcs;
                        Row1["rate"] = rate;

                        Row1["totbox"] = totbox.ToString();
                        Row1["toset"] = totset.ToString();

                        IR.Rows.Add(Row1);
                        if (i > maxR) break;
                    }
                    if (i > maxR) break;
                }

                maxR = 0;
                string compaddress; string stremail = "";
                string grpemailid = "", rptname = "";
                compaddress = Salesfunc.retCompAddress("", grpemailid);
                stremail = compaddress.retCompValue("email");

                string ccemail = grpemailid;
                if (ccemail == "") ccemail = stremail;

                string complogo = Salesfunc.retCompLogo();
                EmailControl EmailControl = new EmailControl();

                string complogosrc = complogo;
                string compfixlogosrc = "c:\\improvar\\" + CommVar.Compcd(UNQSNO) + "fix.jpg";
                string sendemailids = "", ccemailid = "", msgresult = "", sendmobno = "";
                string rptfile = "Rep_DistOrd.rpt";
                rptname = "~/Report/" + rptfile;
                ReportDocument reportdocument = new ReportDocument();

                sql = "select b.SLMSLCD,b.SLNO, b.agslcd,c.slnm agslnm, b.DISTSLCD, d.slnm DISTSlnm,d.SLAREA DISTSLAREA,c.REGEMAILID from " + Environment.NewLine;
                sql += scm + ".M_SLSMN_AGENT b," + scmf + ".M_SUBLEG c," + scmf + ".M_SUBLEG d " + Environment.NewLine;
                sql += "where b.AGSLCD=C.SLCD and b.DISTSLCD=d.slcd(+) and b.EFFDT=( SELECT MAX(x.EFFDT)  " + Environment.NewLine;
                sql += "FROM " + scm + ".M_SLSMN_AGENT x  " + Environment.NewLine;
                sql += "WHERE x.SLMSLCD = b.SLMSLCD ) order by b.slno " + Environment.NewLine;
                DataTable tbl = masterHelp.SQLquery(sql);


                var rsemailid = (from DataRow dr in IR.Rows
                                 select new
                                 {
                                     email = dr["DISREGEMAILID"],
                                     slcd = dr["slcd"],
                                     regmno = dr["DISWHATSAPPNO"],
                                     autono = dr["autono"],
                                     compregmno = dr["compREGWHATSAPPNO"],
                                     SLMSLCD = dr["SLMSLCD"],

                                 }).Distinct().ToList();

                for (int z = 0; z < rsemailid.Count; z++)
                {

                    if (rsemailid[z].email.ToString() != "" || rsemailid[z].regmno.ToString() != "")
                    {
                        List<string> pdffilenm = new List<string>();
                        List<string> imgfilenm = new List<string>();

                        var queryq = from row in IR.AsEnumerable()
                                     where row.Field<string>("autono") == rsemailid[z].autono.ToString()
                                     select row;

                        var rsemailid1 = queryq.AsDataView().ToTable();


                        reportdocument.Load(Server.MapPath(rptname));

                        maxR = rsemailid1.Rows.Count - 1;
                        Int32 iz = 0;
                        string rtlnm = "", emlslcd = "", body = "", chkfld = "", chkfld1 = "", rtladd = "";
                        emlslcd = rsemailid[z].slcd.ToString();

                        while (iz <= maxR)
                        {
                            rtlnm = rsemailid1.Rows[iz]["RTLNM"].ToString();
                            rtladd = rsemailid1.Rows[iz]["add1"].ToString() + rsemailid1.Rows[iz]["add2"].ToString() + rsemailid1.Rows[iz]["add3"].ToString() + rsemailid1.Rows[iz]["add4"].ToString();
                            emlslcd = rsemailid1.Rows[iz]["slcd"].ToString();
                            body += "<tr>";
                            body += "<td>" + rsemailid1.Rows[iz]["docdt"] + "</td>";
                            body += "<td>" + rsemailid1.Rows[iz]["docno"] + "</td>";
                            body += "<td>" + rsemailid1.Rows[iz]["QNTY"] + "</td>";
                            body += "</tr>";
                            if (rsemailid1.Rows[iz]["compREGEMAILID"].ToString().retStr() != "") ccemailid = rsemailid1.Rows[iz]["compREGEMAILID"].ToString();
                            chkfld = rsemailid1.Rows[iz]["autono"].ToString().Substring(0, rsemailid1.Rows[iz]["autono"].ToString().Length - 1);

                            while (rsemailid1.Rows[iz]["autono"].ToString().Substring(0, rsemailid1.Rows[iz]["autono"].ToString().Length - 1) == chkfld)
                            {
                                iz++;
                                if (iz > maxR) break;
                            }
                        }

                        string sql1 = "";
                        string uid = CommVar.UserID();
                        ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                        string MOBILE = DB1.USER_APPL.Find(uid).MOBILE;
                        string ldt = rsemailid1.Rows[rsemailid1.Rows.Count - 1]["docdt"].ToString();


                        string blhead = "PARTY ORDER";
                        string compMobile = "";
                        string compEmail = "";
                        string legalname = compaddress.retCompValue("legalname").retStr() == "" ? "" : "(" + compaddress.retCompValue("legalname") + ")";


                        reportdocument.Load(Server.MapPath("~/Report/Rep_DistOrd.rpt"));
                        reportdocument.SetDataSource(IR);
                        reportdocument.SetParameterValue("partytotos", partytotos.ToString());
                        reportdocument.SetParameterValue("billheading", blhead);
                        reportdocument.SetParameterValue("compnm", compaddress.retCompValue("compnm"));
                        reportdocument.SetParameterValue("compadd", compaddress.retCompValue("compadd"));
                        reportdocument.SetParameterValue("compcommu", compaddress.retCompValue("compcommu"));
                        reportdocument.SetParameterValue("compstat", compaddress.retCompValue("compstat"));
                        reportdocument.SetParameterValue("locaadd", compaddress.retCompValue("locaadd"));
                        reportdocument.SetParameterValue("locacommu", compaddress.retCompValue("locacommu"));
                        reportdocument.SetParameterValue("locastat", compaddress.retCompValue("locastat"));
                        reportdocument.SetParameterValue("legalname", compaddress.retCompValue("legalname"));
                        reportdocument.SetParameterValue("corpadd", compaddress.retCompValue("corpadd"));
                        reportdocument.SetParameterValue("corpcommu", compaddress.retCompValue("corpcommu"));
                        Response.Buffer = false;
                        Response.ClearContent();
                        Response.ClearHeaders();

                        Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);
                        if (onlyprint == true)
                        {
                            reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
                            return new FileStreamResult(stream, "application/pdf");
                        }
                        if (!System.IO.Directory.Exists(path_Save)) { System.IO.Directory.CreateDirectory(path_Save); }
                        var edocno = (Regex.Replace(billno, @"[^0-9a-zA-Z_]+", ""));
                        path_Save = path_Save + "\\" + edocno + ".pdf";
                        if (System.IO.File.Exists(path_Save)) { System.IO.File.Delete(path_Save); }
                        reportdocument.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, path_Save);
                        reportdocument.Close(); reportdocument.Dispose(); GC.Collect();

                        //whatsapp save
                        string billpath_Save = "";

                        string filenm = edocno + id + ".pdf";
                        billpath_Save = Salesfunc.LocalWhatsappFilePath() + filenm;
                        pdffilenm.Add(filenm);

                        if (System.IO.File.Exists(billpath_Save))
                        {
                            System.IO.File.Delete(billpath_Save);
                        }
                        System.IO.File.Copy(path_Save, billpath_Save, true);


                        // email


                        List<System.Net.Mail.Attachment> attchmail = new List<System.Net.Mail.Attachment>();
                        attchmail.Add(new System.Net.Mail.Attachment(path_Save));
                        string template = "";
                        template = "Salebill_" + CommVar.Compcd(UNQSNO) + ".htm";
                        string filePath = Server.MapPath("~/Templates/Email/" + template + "");
                        if (!System.IO.File.Exists(filePath))
                        {
                            template = "Salebill_" + CommVar.ClientCode(UNQSNO) + ".htm";
                            filePath = Server.MapPath("~/Templates/Email/" + template + "");
                        }
                        if (!System.IO.File.Exists(filePath))
                        {
                            template = "Salebill.htm";
                        }

                        string[,] emlaryBody = new string[9, 2];

                        emlaryBody[0, 0] = "{rtlnm}"; emlaryBody[0, 1] = rtlnm;
                        emlaryBody[1, 0] = "{rtladd}"; emlaryBody[1, 1] = rtladd;
                        emlaryBody[2, 0] = "{tbody}"; emlaryBody[2, 1] = body;
                        emlaryBody[3, 0] = "{username}"; emlaryBody[3, 1] = usr_id;
                        emlaryBody[4, 0] = "{compname}"; emlaryBody[4, 1] = compaddress.retCompValue("compnm");//need to come from Web Config
                        emlaryBody[5, 0] = "{usermobno}"; emlaryBody[5, 1] = MOBILE;
                        emlaryBody[6, 0] = "{complogo}"; emlaryBody[6, 1] = complogosrc;
                        emlaryBody[7, 0] = "{compfixlogo}"; emlaryBody[7, 1] = compfixlogosrc;
                        emlaryBody[8, 0] = "{compmobno}"; emlaryBody[8, 1] = compMobile;
                        if (rsemailid[z].email.ToString() != "")
                        {
                            string distslcd = rsemailid[z].slcd.retStr();
                            string slmslcd = rsemailid[z].SLMSLCD.retStr();
                            string agmailid = string.Join(";", (from DataRow dr in tbl.Rows where dr["DISTSLCD"].retStr() == distslcd && dr["SLMSLCD"].retStr() == slmslcd && dr["REGEMAILID"].retStr() != "" select dr["REGEMAILID"].retStr()).Distinct());

                            if (agmailid.retStr() != "")
                            {
                                if (ccemailid.retStr() != "")
                                {
                                    ccemailid += ";";
                                }
                                ccemailid += agmailid;
                            }
                            // distributor,company,agent

                            bool emailsent = EmailControl.SendHtmlFormattedEmail(rsemailid[z].email.ToString(), "Order Copy", "DistOrder.htm", emlaryBody, attchmail, ccemailid);
                            if (emailsent == true) sendemailids = sendemailids + rsemailid[z].email.ToString() + ";"; else sendemailids = sendemailids + " not able to send on " + rsemailid[z].email.ToString();
                        }

                        string[,] smsaryMsg = new string[9, 2];
                        smsaryMsg[0, 0] = "&rtlnm&"; smsaryMsg[0, 1] = rtlnm;
                        smsaryMsg[1, 0] = "&rtladd&"; smsaryMsg[1, 1] = rtladd;
                        smsaryMsg[2, 0] = "&tbody&"; smsaryMsg[2, 1] = body;
                        smsaryMsg[3, 0] = "&username&"; smsaryMsg[3, 1] = usr_id;
                        smsaryMsg[4, 0] = "&compname&"; smsaryMsg[4, 1] = compaddress.retCompValue("compnm");//need to come from Web Config
                        smsaryMsg[5, 0] = "&usermobno&"; smsaryMsg[5, 1] = MOBILE;
                        smsaryMsg[6, 0] = "&complogo&"; smsaryMsg[6, 1] = complogosrc;
                        smsaryMsg[7, 0] = "&compfixlogo&"; smsaryMsg[7, 1] = compfixlogosrc;
                        smsaryMsg[8, 0] = "&compmobno&"; smsaryMsg[8, 1] = compMobile;



                        string mobno = rsemailid[z].regmno.ToString();
                        if (rsemailid[z].compregmno.retStr() != "")
                        {
                            if (mobno.retStr() != "")
                            {
                                mobno += ",";
                            }
                            mobno += rsemailid[z].compregmno.ToString();
                        }
                        if (mobno.retStr() != "")
                        {
                            SMS sms = new SMS();
                            List<string> sendmsg = sms.WHATSAPPMessContectGen(slcd, "APPORDW", smsaryMsg);
                            // distributor,company
                            msgresult = sms.WHATSAPPsend(mobno, sendmsg[0], sendmsg[1], pdffilenm, imgfilenm);
                            string[] msgretval = msgresult.Split('=');
                            if (msgretval[0].retStr() == "")
                            {
                                sendmobno = mobno.ToString();
                            }
                        }


                        if (System.IO.File.Exists(path_Save))
                        {
                            System.IO.File.Delete(path_Save);
                        }
                        if (pdffilenm != null && pdffilenm.Count() > 0)
                        {
                            for (int a = 0; a < pdffilenm.Count(); a++)
                            {
                                string delpath = Salesfunc.LocalWhatsappFilePath() + pdffilenm[a];
                                if (System.IO.File.Exists(delpath))
                                {
                                    System.IO.File.Delete(delpath);
                                }
                            }
                        }
                        if (imgfilenm != null && imgfilenm.Count() > 0)
                        {
                            for (int a = 0; a < imgfilenm.Count(); a++)
                            {
                                string delpath = Salesfunc.LocalWhatsappFilePath() + imgfilenm[a];
                                if (System.IO.File.Exists(delpath))
                                {
                                    System.IO.File.Delete(delpath);
                                }
                            }
                        }

                    }
                }
                string emailretmsg = "email : " + sendemailids + ", " + "CC email on : " + ccemailid + ", " + "Whatsapp : " + sendmobno;
                return emailretmsg;

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return ("");
            }


        }

        public ActionResult OpenPopupSize(TransactionDistOrder VE, int ParentSerialNo)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            try
            {
                ListPendOrd query = (from c in VE.ListPendOrd where (c.SLNO == ParentSerialNo) select c).SingleOrDefault();
                if (query != null)
                {
                    var helpM1 = new List<Improvar.Models.ListPendOrdPopup>();
                    var javaScriptSerializer1 = new System.Web.Script.Serialization.JavaScriptSerializer();
                    if (query.ChildData != null)
                    {
                        helpM1 = javaScriptSerializer1.Deserialize<List<Improvar.Models.ListPendOrdPopup>>(query.ChildData);
                    }
                    if (helpM1 != null)
                    {
                        if (helpM1.Count > 0)
                        {
                            VE.ListPendOrdPopup = helpM1;
                            if (VE.ListPendOrdPopup != null && VE.ListPendOrdPopup.Count > 0)
                            {
                                VE.ListPendOrdPopup[0].CheckedORDSKIP = query.CheckedORDSKIP == true ? "Y" : "N";
                            }
                        }
                    }
                }
                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_T_DistOrder_Popup", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }

        }
        public ActionResult ClosePopupSize(TransactionDistOrder VE, int ParentSerialNo)
        {
            ListPendOrd query = (from c in VE.ListPendOrd where (c.SLNO == ParentSerialNo) select c).SingleOrDefault();
            if (VE.ListPendOrdPopup != null)
            {
                double totbox = 0, totset = 0;
                foreach (var v in VE.ListPendOrdPopup)
                {
                    totbox += Salesfunc.ConvPcstoBox(v.QNTY, v.PCSPERBOX);
                    totset += Salesfunc.ConvPcstoSet(v.QNTY, v.PCSPERSET);
                }
                if (query != null)
                {

                    query.ORDDET = "Box=" + totbox + ",Set=" + totset;
                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string JR = javaScriptSerializer.Serialize(VE.ListPendOrdPopup);
                    query.ChildData = JR;
                }
            }
            else
            {
                query.ChildData = null;
            }
            VE.DefaultView = true;
            ModelState.Clear();
            return PartialView("_T_DistOrder_Main", VE);
        }


        public string DocPattern(double docno, string mnthcd)
        {

            string[] dfinyr = CommVar.FinPeriod(UNQSNO).Split('-');
            string finyr = "", yy = "";
            yy = dfinyr[0].ToString().Trim().Substring(8);
            if (yy == dfinyr[1].ToString().Trim().Substring(8)) finyr = yy;
            else finyr = yy + "-" + dfinyr[1].ToString().Trim().Substring(8);

            string newPattern = "HP/DOR/" + docno.retStr().PadLeft(5, '0') + "/" + finyr;

            return newPattern;
        }
        public ActionResult DeleteRow(TransactionDistOrder VE, int SerialNo)
        {
            try
            {
                List<ListPendOrd> ListPendOrd = new List<ListPendOrd>();
                int count = 0;
                for (int i = 0; i <= VE.ListPendOrd.Count - 1; i++)
                {
                    if (VE.ListPendOrd[i].SLNO != SerialNo)
                    {
                        count += 1;
                        ListPendOrd item = new ListPendOrd();
                        item = VE.ListPendOrd[i];
                        item.SLNO = Convert.ToInt16(count);
                        ListPendOrd.Add(item);
                    }
                }
                VE.ListPendOrd = ListPendOrd;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_DistOrder_Main", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }


    }
}

