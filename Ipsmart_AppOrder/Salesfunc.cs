using System;
using System.Collections.Generic;
using System.Linq;
using Improvar.Models;
using System.Data;
using Microsoft.Ajax.Utilities;
using System.Configuration;
namespace Improvar
{
    public class Salesfunc : MasterHelpFa
    {
        Connection Cn = new Connection();
        MasterHelpFa MasterHelpFa = new MasterHelpFa();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public DataTable GetSlcdDetails(string slcd, string docdt, string linkcd = "", string brandcd = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DataTable tbl = new DataTable();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (docdt == null) docdt = "";

            string sql = "";
            string itgrpcd = "";

            sql += "select z.slcd, b.taxgrpcd, a.agslcd, a.areacd, a.prccd, a.discrtcd, a.cod, a.gstno, a.docth, b.trslcd, b.courcd, nvl(c.agslcd,a.agslcd) agslcd, ";
            sql += "g.slnm, g.slarea, h.slnm agslnm, i.slnm trslnm, e.taxgrpnm, f.prcnm, g.panno, ";
            //sql += "(case when to_date('" + docdt + "', 'dd/mm/yyyy') < to_date('01/07/2021', 'dd/mm/yyyy') then nvl(g.tcsappl, 'Y') else  decode(nvl(g.tot194q, 'N'), 'Y', 'N', 'Y') end ) tcsappl, ";
            sql += "decode(nvl(k.tcsappl,'Y'),'N','N',nvl(g.tcsappl, 'N')) tcsappl,decode(nvl(k.TDSGOODSAPPL,'Y'),'N','N',nvl(g.tot194Q,'N')) tdsappl, ";
            //sql += "f.prcnm, "; // c.prcdesc, c.effdt, c.itmprccd, ";
            //sql += "nvl(a.crdays,0) crdays, nvl(a.crlimit,0) crlimit ";
            sql += "(case when  nvl(c.crdays,0)=0 then a.crdays else c.crdays end) crdays, nvl(a.crlimit,0) crlimit ";
            sql += "from ";

            sql += "(select a.slcd from " + scmf + ".m_subleg a where a.slcd='" + slcd + "' ) z, ";

            sql += "(select a.slcd, a.agslcd, a.areacd, a.prccd, a.discrtcd, nvl(a.crdays,0)crdays, a.crlimit, a.cod, a.docth, b.gstno ";
            sql += "from " + scm + ".m_subleg_com a, " + scmf + ".m_subleg b ";
            sql += "where b.slcd='" + slcd + "' and a.slcd=b.slcd(+) and (a.compcd='" + COM + "' or a.compcd is null) ) a, ";

            sql += "(select a.slcd, a.taxgrpcd, a.trslcd, a.courcd ";
            sql += "from " + scm + ".m_subleg_sddtl a, " + scmf + ".m_subleg b ";
            sql += "where b.slcd='" + slcd + "' and a.slcd=b.slcd(+) and (a.compcd='" + COM + "' or a.compcd is null) and (a.loccd='" + LOC + "'  or a.loccd is null) ) b, ";

            sql += "(select b.slcd, b.agslcd,nvl(b.crdays,0)crdays ";
            sql += "from " + scm + ".m_subleg_brand b ";
            sql += "where b.slcd='" + slcd + "' and b.brandcd='" + brandcd + "' and b.compcd='" + COM + "' ) c, ";

            //sql += "(select a.effdt, a.prccd, a.itmprccd, a.prcdesc from ";
            //sql += "(select a.effdt, a.prccd, a.itmprccd, a.prcdesc, ";
            //sql += "row_number() over (partition by a.prccd order by a.effdt desc) as rn ";
            //sql += "from " + scm + ".m_itemplist a ";
            //sql += "where a.itgrpcd='" + itgrpcd + "' ";
            //if (docdt != "") sql += "and a.effdt <= to_date('" + docdt.Substring(0, 10) + "','dd/mm/yyyy') ";
            //sql += ") a where a.rn=1) d, ";

            sql += "" + scmf + ".m_taxgrp e, " + scmf + ".m_prclst f, " + scmf + ".m_subleg g, " + scmf + ".m_subleg h, " + scmf + ".m_subleg i, " + scmf + ".m_subleg j, " + scmf + ".m_comp k ";
            sql += "where z.slcd=a.slcd(+) and z.slcd=b.slcd(+) and z.slcd=c.slcd(+) and ";
            sql += "b.taxgrpcd=e.taxgrpcd(+) and a.prccd=f.prccd(+) and ";
            sql += "z.slcd=g.slcd(+) and a.agslcd=h.slcd(+) and b.trslcd=i.slcd(+) and b.courcd=j.slcd(+) and k.compcd='" + COM + "' ";

            tbl = SQLquery(sql);

            return tbl;
        }

        public double ConvPcstoBox(double pcs, double pcsperbox)
        {
            double box = 0;
            double dbDzn, dbPcs, zDzn = 0;
            string txt1, txt2 = "";
            if (pcsperbox == 0)
                return 0;
            if (pcs == 0) dbPcs = 0; else dbPcs = Cn.Roundoff(pcs / pcsperbox, 2);

            txt1 = dbPcs.ToString("0.00");
            txt1 = txt1.Substring(0, txt1.Length - 3);
            txt2 = (pcs - Convert.ToDouble(txt1) * pcsperbox).ToString("0");

            if (pcsperbox != 10)
            {
                if (Convert.ToDouble(txt2) < 10) box = Convert.ToDouble(txt1 + ".0" + txt2.Substring(txt2.Length - 1));
                else box = Convert.ToDouble(txt1 + "." + txt2);
            }
            else
            {
                box = Convert.ToDouble(txt1 + ".0" + txt2.Substring(txt2.Length - 1));
            }
            return box;
        }
        public double ConvPcstoSet(double pcs, double pcsperset)
        {
            double box = 0;
            double dbDzn, dbPcs, zDzn = 0;
            string txt1, txt2 = "";
            if (pcsperset == 0)
                return 0;
            if (pcs == 0) dbPcs = 0; else dbPcs = Cn.Roundoff(pcs / pcsperset, 2);

            txt1 = dbPcs.ToString("0.00");
            txt1 = txt1.Substring(0, txt1.Length - 3);
            txt2 = (pcs - Convert.ToDouble(txt1) * pcsperset).ToString("0");

            if (pcsperset != 10)
            {
                if (Convert.ToDouble(txt2) < 10) box = Convert.ToDouble(txt1 + ".0" + txt2.Substring(txt2.Length - 1));
                else box = Convert.ToDouble(txt1 + "." + txt2);
            }
            else
            {
                box = Convert.ToDouble(txt1 + ".0" + txt2.Substring(txt2.Length - 1));
            }
            return box;
        }
        public String retDocTh(string val)
        {
            string rtval = "";
            switch (val)
            {
                case "D":
                    rtval = "DIRECT"; break;
                case "B":
                    rtval = "BANK"; break;
                case "A":
                    rtval = "AGENT"; break;
                case "H":
                    rtval = "HOLD"; break;
                case "P":
                    rtval = "AGST PYMT"; break;
                case "N":
                    rtval = "BRANCH"; break;
                case "O":
                    rtval = "OTHERS"; break;
                default:
                    rtval = "DIRECT"; break;
            }
            return rtval;
        }

        public string retDelvTypeDesc(string delvtype)
        {
            string rval = "";
            switch (delvtype)
            {
                case "C":
                    rval = "COD"; break;
                case "T":
                    rval = "TO PAY"; break;
                case "P":
                    rval = "PAID BILTY"; break;
                case "H":
                    rval = "HOLD BILTY"; break;
                default:
                    rval = ""; break;
            }
            return rval;
        }


        public double ConvBoxtoPcs(double box, double pcsperbox)
        {
            double pcs = 0;
            pcs = box * pcsperbox;
            return pcs;
        }


        public double ConvSettoPcs(double set, double pcsperset)
        {
            double pcs = 0;
            pcs = set * pcsperset;
            return pcs;
        }
        public DataTable GetPendatKar(string slcd = "", string docdt = "", string stylelike = "", string skipautono = "")
        {
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), scmi = CommVar.InvSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

            string sql = "select a.batchautono, a.batchslno, d.mtrljobcd, d.batchno, d.itcd, e.itnm, e.styleno, e.uomcd, e.pcsperbox, ";
            sql += " g.doccd, g.docno, g.docdt, d.slcd, j.slnm, ";
            sql += "d.colrcd,f.colrnm,d.colrnm shade, d.dia, d.gsm, d.gauge, d.texture, ";
            sql += "d.mchnname, d.fabtype, d.partcd, d.sizecd, h.partnm, i.sizenm, d.stktype, ";
            sql += "nvl(b.qnty, 0) qnty, nvl(c.qnty, 0) consqnty, nvl(b.qnty, 0) - nvl(c.qnty, 0) balqnty, ";
            sql += "nvl(b.nos, 0) nos, nvl(c.nos, 0) consnos, nvl(b.nos, 0) - nvl(c.nos, 0) balnos from ";

            sql += "(select a.batchautono, a.batchslno ";
            sql += "from " + scm + ".t_batchmst a) a, ";

            sql += "(select a.batchautono, a.batchslno, ";
            sql += "sum((case a.stkdrcr when 'C' then a.qnty when a.stkdrcr then a.qnty * -1 end)) qnty, ";
            sql += "sum((case a.stkdrcr when 'C' then a.nos when a.stkdrcr then a.nos * -1 end)) nos ";
            sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_txn b, " + scm + ".t_cntrl_hdr c ";
            sql += "where a.autono = b.autono(+) and a.autono = c.autono(+) and ";
            if (docdt.retStr() != "") sql += "c.docdt <= to_date('" + docdt + "', 'dd/mm/yyyy') and ";
            sql += "c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and nvl(c.cancel, 'N')= 'N' and ";
            sql += "b.doctag in ('JC','JU') and b.jobcd = 'CT' ";
            if (skipautono.retStr() != "") sql += "and a.autono <> '" + skipautono + "' ";
            sql += "group by a.batchautono, a.batchslno ) b, ";

            sql += "(select a.batchautono, a.batchslno, a.qnty, a.nos ";
            sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_txn b, " + scm + ".t_cntrl_hdr c ";
            sql += "where a.autono = b.autono(+) and a.autono = c.autono(+) and ";
            if (docdt.retStr() != "") sql += "c.docdt <= to_date('" + docdt + "', 'dd/mm/yyyy') and ";
            if (skipautono.retStr() != "") sql += "a.autono <> '" + skipautono + "' and ";
            sql += "c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and nvl(c.cancel, 'N')= 'N' and ";
            sql += "b.doctag in ('JR') and b.jobcd = 'CT' and a.slno >= 6000 and a.slno <= 6999 ) c, ";
            sql += "" + scm + ".t_batchmst d, " + scm + ".m_sitem e, " + scm + ".m_color f, " + scm + ".t_cntrl_hdr g, ";
            sql += "" + scm + ".m_parts h, " + scm + ".m_size i, " + scmf + ".m_subleg j ";
            sql += "where a.batchautono = b.batchautono(+) and a.batchslno = b.batchslno(+) and ";
            sql += "a.batchautono = c.batchautono(+) and a.batchslno = c.batchslno(+) and ";
            sql += "a.batchautono = d.batchautono(+) and a.batchslno = d.batchslno(+) and ";
            sql += "g.compcd = '" + COM + "' and ";
            sql += "nvl(b.qnty, 0)-nvl(c.qnty, 0) > 0 and ";
            sql += "d.itcd = e.itcd(+) and d.colrcd = f.colrcd(+) and d.autono = g.autono(+) and ";
            sql += "d.sizecd = i.sizecd(+) and d.partcd = h.partcd(+) and d.slcd = j.slcd(+) ";
            if (slcd.retStr() != "") sql += "and d.slcd = '" + slcd + "' and ";
            if (stylelike.retStr() != "") sql += "e.styleno like '%" + stylelike + "%' and ";

            DataTable tbl = SQLquery(sql);
            return tbl;

        }
        public double getTranRate(string trslcd, string docdt, string destn)
        {
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            string sql = "";


            sql += "select a.trslcd, a.destn, a.effdt, a.rate from ( ";
            sql += "select a.trslcd, a.destn, a.effdt, a.rate, ";
            sql += "row_number() over(partition by a.trslcd, a.destn order by a.effdt desc) as rn ";
            sql += "from " + scm + ".m_trandelv a ";
            sql += "where a.effdt <= to_date('" + docdt + "', 'dd/mm/yyyy') and a.compcd = '" + COM + "' and a.loccd = '" + LOC + "' ) a ";
            sql += "where a.rn = 1 and a.trslcd='" + trslcd + "' and a.destn='" + destn + "' ";
            sql += "order by trslcd, destn, effdt ";

            DataTable tbl = SQLquery(sql);
            double retRate = 0;

            if (tbl.Rows.Count == 1) retRate = tbl.Rows[0]["rate"].retDbl();
            return retRate;
        }
        public string CheckAutoreminder(string slcd, string docdt, string autono, string doccd = "")
        {
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            string sql = "", msg = "N";
            sql += "select a.ORDPYMTDAYSCALC,b.AUTHCD from " + scm + ".M_MGROUP_SPL a," + scm + ".M_DOC_AUTH b, ";
            sql += scm + ".M_DOCTYPE d ";
            sql += "where a.compcd='" + COM + "' and b.doccd=d.doccd and d.doctype='SDO' ";
            if (doccd.retStr() != "") sql += "and b.doccd='" + doccd + "' ";
            sql += "and ROWNUM=1 and a.ORDPYMTDAYSCALC>0 and b.AUTHCD is not null ";
            DataTable dt = MasterHelpFa.SQLquery(sql);

            if (dt != null && dt.Rows.Count > 0)
            {
                int chkday = 0;
                if (dt != null && dt.Rows.Count > 0) chkday = dt.Rows[0]["ORDPYMTDAYSCALC"].retInt();


                var ostbl = MasterHelpFa.GenOSTbl("", slcd.retSqlformat(), docdt, "", autono);

                var osamt = (from DataRow dr in ostbl.Rows
                             where (Convert.ToDateTime(docdt) - ((dr["lrdt"].retStr() == "" ? dr["bldt"].retStr() : dr["lrdt"].retStr()) == "" ? Convert.ToDateTime(dr["docdt"]) : Convert.ToDateTime(dr["lrdt"].retStr() == "" ? dr["bldt"].retStr() : dr["lrdt"].retStr()))).Days > chkday
                             && dr["drcr"].retStr() == "D"
                             select
                             dr["bal_amt"].retDbl()
                          ).Sum();

                var totosamt = (from DataRow dr in ostbl.Rows
                                select
                                dr["bal_amt"].retDbl()
                          ).Sum();

                if (osamt > 0 && dt.Rows[0]["AUTHCD"].retStr() != "")
                {
                    msg = "^OSAMT=^" + osamt + Cn.GCS();
                    msg += "^TOTALOSAMT=^" + totosamt + Cn.GCS();
                    msg += "^CHKDAY=^" + chkday + Cn.GCS();
                }
            }
            return msg;
        }
        public void insT_TXNSTATUS(string Auto_Number, string ststype, string flag1, string stsrem)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            Models.T_TXNSTATUS TCH = new Models.T_TXNSTATUS();

            var MAXEMDNO = (from p in DB.T_TXNSTATUS where (p.AUTONO == Auto_Number && p.FLAG1 == flag1 && p.STSTYPE == ststype) select p.EMD_NO).Max();
            short emdno = 0;
            if (MAXEMDNO == null) emdno = 0; else emdno = Convert.ToByte(MAXEMDNO + 1);

            var TCHOLD = (from i in DB.T_TXNSTATUS
                          where (i.AUTONO == Auto_Number && i.STSTYPE == ststype && i.FLAG1 == flag1)
                          select i).ToList();
            if (TCHOLD.Any())
            {
                DB.T_TXNSTATUS.Where(x => x.AUTONO == Auto_Number && x.FLAG1 == flag1 && x.STSTYPE == ststype).ToList().ForEach(x => { x.DTAG = "D"; });
                DB.T_TXNSTATUS.RemoveRange(DB.T_TXNSTATUS.Where(x => x.AUTONO == Auto_Number && x.FLAG1 == flag1 && x.STSTYPE == ststype));
            }
            TCH.AUTONO = Auto_Number;
            TCH.STSTYPE = ststype;
            TCH.FLAG1 = flag1;
            TCH.CLCD = CommVar.ClientCode(UNQSNO);
            TCH.STSREM = stsrem;
            TCH.USR_ID = System.Web.HttpContext.Current.Session["UR_ID"].ToString();
            TCH.USR_ENTDT = System.DateTime.Now;
            TCH.USR_LIP = Cn.GetIp();
            TCH.USR_SIP = Cn.GetStaticIp();
            TCH.USR_OS = null;
            TCH.USR_MNM = Cn.DetermineCompName(Cn.GetIp());  //GetMachin;
            TCH.DTAG = "";
            TCH.EMD_NO = emdno;

            DB.T_TXNSTATUS.Add(TCH);
            DB.SaveChanges();
            return;
        }

        public static string LocalWhatsappFilePath(string FileName = "")
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("~/Whatsapp/" + FileName);
            return path;
        }
        public string GetWhatsappFilePath()
        {
            string foldernm = System.Web.HttpContext.Current.Request.Url.Segments[1];
            string WebHostPath = @ConfigurationManager.AppSettings["WhtsappSendIp"].retStr() + "/" + foldernm + "Whatsapp/";
            return WebHostPath;
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
            DataTable tbl = SQLquery(sql);
            if (tbl != null && tbl.Rows.Count > 0)
            {
                SLMSLCD = tbl.Rows[0]["slmslcd"].retStr();
            }
            return SLMSLCD;

        }
        public DataTable GetDistributor(string tdt, string SLMSLCD)
        {
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmp = CommVar.PaySchema(UNQSNO);

            string sql = "";
            sql += "select a.slmslcd, a.DISTSLCD , b.slnm DISTSLnm, nvl(b.slarea, b.district) slarea, b.WHATSAPP_NO, b.regemailid from " + Environment.NewLine;
            sql += "" + scm + ".m_slsmn_agent a," + scmf + ".m_subleg b " + Environment.NewLine;
            sql += "where a.DISTSLCD  = b.slcd(+) " + Environment.NewLine;
            sql += "and a.effdt=(select a.effdt from " + Environment.NewLine;
            sql += "(select a.slmslcd, a.effdt, " + Environment.NewLine;
            sql += "row_number() over(partition by a.slmslcd order by a.effdt desc) rno " + Environment.NewLine;
            sql += "from " + scm + ".m_slsmn_agent a " + Environment.NewLine;
            sql += "where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and " + Environment.NewLine;
            sql += "a.slmslcd in (" + SLMSLCD + ")) a " + Environment.NewLine;
            sql += "where rno = 1 )  " + Environment.NewLine;
            sql += "and a.slmslcd in (" + SLMSLCD + ") " + Environment.NewLine;
            sql += "order by slnm " + Environment.NewLine;

            DataTable tbl = SQLquery(sql);
            return tbl;
        }
        public DataTable GetBrand(string tdt, string SLMSLCD)
        {
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmp = CommVar.PaySchema(UNQSNO);

            string sql = "";
            sql = "";
            sql += "select a.slmslcd, a.brandcd, b.brandnm from " + Environment.NewLine;
            sql += "" + scm + ".m_slsmn_brand a," + scm + ".m_brand b where a.effdt = " + Environment.NewLine;
            sql += "(select a.effdt from " + Environment.NewLine;
            sql += "(select a.slmslcd, a.brandcd, a.effdt, " + Environment.NewLine;
            sql += "row_number() over(partition by a.slmslcd order by a.effdt desc) rno " + Environment.NewLine;
            sql += "from " + scm + ".m_slsmn_brand a " + Environment.NewLine;
            sql += "where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and " + Environment.NewLine;
            sql += "a.slmslcd in (" + SLMSLCD + ")) a " + Environment.NewLine;
            sql += "where rno = 1 ) " + Environment.NewLine;
            sql += "and a.brandcd = b.brandcd(+) " + Environment.NewLine;
            sql += "and a.slmslcd in (" + SLMSLCD + ") " + Environment.NewLine;
            sql += "order by brandnm " + Environment.NewLine;

            DataTable tbl = SQLquery(sql);
            return tbl;
        }
        public DataTable GetPendingOrder(string distslcd, string brandcd, string rtlautono = "")
        {
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmp = CommVar.PaySchema(UNQSNO);

            string sql = "";
            sql += "select a.autono, a.slno, a.itcd, a.sizecd, a.qnty, a.itrem, a.freestk, " + Environment.NewLine;
            sql += "b.slcd, d.slnm, b.slmslcd, b.rtlcd, e.rtlnm, e.city, e.landmark, nvl(e.regwhatsappno, e.regmobile) regmobile, " + Environment.NewLine;
            sql += "f.brandcd, g.brandnm, c.styleno, c.pcsperbox, c.pcsperset,c.MIXSIZE,h.PRINT_SEQ,h.sizenm from " + Environment.NewLine;
            sql += "(select a.autono, a.slno, a.itcd, a.sizecd, a.qnty, a.itrem, a.freestk " + Environment.NewLine;
            sql += "from " + scm + ".t_retailorderdtl a, " + scm + ".t_distordlink b " + Environment.NewLine;
            sql += "where a.autono = b.rtlautono(+) and a.slno = b.slno(+) and " + Environment.NewLine;
            sql += "b.autono is null ) a, " + Environment.NewLine;
            sql += "" + scm + ".t_retailorder b, " + scm + ".m_sitem c, " + scmf + ".m_subleg d, " + scm + ".m_retail e, " + Environment.NewLine;
            sql += "" + scm + ".m_group f, " + scm + ".m_brand g , " + scm + ".m_size h " + Environment.NewLine;
            sql += "where a.autono = b.autono(+) and a.itcd = c.itcd(+) and b.slcd = d.slcd(+) and b.rtlcd = e.rtlcd(+) and " + Environment.NewLine;
            sql += "c.itgrpcd = f.itgrpcd(+) and f.brandcd = g.brandcd(+) and a.sizecd=h.sizecd(+) " + Environment.NewLine;
            if (distslcd.retStr() != "") sql += "and b.slcd in (" + distslcd + ")  " + Environment.NewLine;
            if (brandcd.retStr() != "") sql += "and f.brandcd in (" + brandcd + ")  " + Environment.NewLine;
            if (rtlautono.retStr() != "") sql += "and a.autono in (" + rtlautono + ")  " + Environment.NewLine;
            sql += "order by a.autono,h.PRINT_SEQ,h.sizenm " + Environment.NewLine;
            DataTable dt = SQLquery(sql);

            return dt;

        }
        public DataTable GetComcdByBrand(string brandcd)
        {
            string scm = CommVar.CurSchema(UNQSNO);
            string sql = "";
            sql += "select brandcd,compcd,loccd,REGMOBILE,REGEMAIL from " + scm + ".M_BRANDCOMP ";
            sql += "where brandcd ='" + brandcd + "' ";
            DataTable dt = SQLquery(sql);
            return dt;
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