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

namespace Improvar.Controllers
{
    public class T_RetailerOrderFilterController : Controller
    {
        Connection Cn = new Connection(); string sql = "";
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();

        M_CNTRL_HDR sll; M_GENLEG sGEN;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_RetailerOrderFilter
        public ActionResult T_RetailerOrderFilter(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {//k
            VMRetailOrder VE = new VMRetailOrder();
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
                    ViewBag.formname = "ORDER TAKEN FROM RETAILER";
                    ViewBag.Title = "Order";
                    VE.UNQSNO_ENCRYPTED = Cn.Encrypt_URL(UNQSNO);

                    string GCS = Cn.GCS();
                    string[] linkcd = { "D", "A" };

                    string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmp = CommVar.PaySchema(UNQSNO);
                    string tdt = System.DateTime.Now.Date.retDateStr();
                    string uid = CommVar.UserID();
                    DataTable tbl = new DataTable();

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
                    tbl = masterHelp.SQLquery(sql);
                    if (tbl != null && tbl.Rows.Count > 0)
                    {
                        VE.SLMSLCD = tbl.Rows[0]["slmslcd"].retStr();
                    }
                    if (VE.SLMSLCD.retStr() != "")
                    {
                        sql = "";
                        sql += "select a.slmslcd, a.DISTSLCD , b.slnm DISTSLnm, nvl(b.slarea, b.district) slarea from " + Environment.NewLine;
                        sql += "" + scm + ".m_slsmn_agent a," + scmf + ".m_subleg b " + Environment.NewLine;
                        sql += "where a.DISTSLCD  = b.slcd(+) " + Environment.NewLine;
                        sql += "and a.effdt=(select a.effdt from " + Environment.NewLine;
                        sql += "(select a.slmslcd, a.effdt, " + Environment.NewLine;
                        sql += "row_number() over(partition by a.slmslcd order by a.effdt desc) rno " + Environment.NewLine;
                        sql += "from " + scm + ".m_slsmn_agent a " + Environment.NewLine;
                        sql += "where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and " + Environment.NewLine;
                        sql += "a.slmslcd = '" + VE.SLMSLCD + "') a " + Environment.NewLine;
                        sql += "where rno = 1 )  " + Environment.NewLine;
                        sql += "and a.slmslcd = '" + VE.SLMSLCD + "' " + Environment.NewLine;
                        sql += "order by slnm " + Environment.NewLine;

                        tbl = masterHelp.SQLquery(sql);

                        VE.ListDistributor = (from DataRow a in tbl.Rows
                                              select new ListDistributor()
                                              {
                                                  value = a["DISTSLCD"].retStr(),
                                                  text = a["DISTSLnm"].retStr() + GCS + a["SLAREA"].retStr(),
                                              }).ToList();
                        VE.ListRetailer = new List<ListRetailer>();

                        sql = "";
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
                        tbl = masterHelp.SQLquery(sql);

                        VE.ListBrand = (from DataRow a in tbl.Rows
                                        select new ListBrand()
                                        {
                                            value = a["BRANDCD"].retStr(),
                                            text = a["BRANDNM"].retStr(),
                                        }).ToList();

                        VE.ListGroup = new List<ListGroup>();
                        VE.ListCollection = new List<ListCollection>();

                        //sql = "";
                        //sql += "select distinct a.COLLCD, a.COLLNM " + Environment.NewLine;
                        //sql += "from " + scm + ".M_COLLECTION a, " + scm + ".m_cntrl_hdr b, " + scm + ".m_sitem c, " + scm + ".m_itemorder d " + Environment.NewLine;
                        //sql += "where a.m_autono = b.m_autono(+) and nvl(b.inactive_tag, 'N')= 'N' and a.collcd = c.collcd(+) and c.itcd = d.itcd " + Environment.NewLine;
                        //sql += "order by COLLNM " + Environment.NewLine;
                        //tbl = masterHelp.SQLquery(sql);

                        //VE.ListCollection = (from DataRow a in tbl.Rows
                        //                     select new ListCollection()
                        //                     {
                        //                         value = a["COLLCD"].retStr(),
                        //                         text = a["COLLNM"].retStr(),
                        //                     }).ToList();
                        if (TempData["DISTSLCD"].retStr() != "")
                        {
                            VE.Dstbrslcd = TempData["DISTSLCD"].retStr();
                            VE.Dstbrslnm = DBF.M_SUBLEG.Find(VE.Dstbrslcd).SLNM;
                        }
                        if (TempData["RTLCD"].retStr() != "")
                        {
                            sql = "";
                            sql += "select a.rtlcd, a.rtlnm, a.landmark from " + Environment.NewLine;
                            sql += "(select a.rtlcd, c.rtlnm, c.landmark, " + Environment.NewLine;
                            sql += "row_number() over(partition by a.rtlcd order by a.effdt desc) rno " + Environment.NewLine;
                            sql += "from " + scm + ".m_retail_link a, " + scm + ".m_cntrl_hdr b, " + scm + ".m_retail c " + Environment.NewLine;
                            sql += "where a.m_autono = b.m_autono(+) and a.rtlcd = c.rtlcd(+) and nvl(b.inactive_tag, 'N') = 'N' and " + Environment.NewLine;
                            sql += "a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and " + Environment.NewLine;
                            sql += "a.slcd = '" + VE.Dstbrslcd + "' ) a " + Environment.NewLine;
                            sql += "  where rno = 1 " + Environment.NewLine;
                            sql += "order by rtlnm " + Environment.NewLine;

                            tbl = masterHelp.SQLquery(sql);
                            if (tbl != null && tbl.Rows.Count > 0)
                            {
                                VE.ListRetailer = (from DataRow a in tbl.Rows
                                                   select new ListRetailer()
                                                   {
                                                       value = a["RTLCD"].retStr(),
                                                       text = a["RTLNM"].retStr() + GCS + a["LANDMARK"].retStr(),
                                                   }).ToList();
                            }
                            VE.RetailerCode = TempData["RTLCD"].retStr();
                            VE.RetailerName = DB.M_RETAIL.Find(VE.RetailerCode).RTLNM;
                        }
                        //TempData.Keep();
                    }
                    else
                    {
                        VE.ListDistributor = new List<ListDistributor>();
                        VE.ListRetailer = new List<ListRetailer>();
                        VE.ListBrand = new List<ListBrand>();
                        VE.ListGroup = new List<ListGroup>();
                        VE.ListCollection = new List<ListCollection>();
                    }
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
        public JsonResult BindRetailerData(string Distributor)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            try
            {
                VMRetailOrder VE = new VMRetailOrder();
                string GCS = Cn.GCS();

                string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
                string tdt = System.DateTime.Now.Date.retDateStr();
                string sql = "";
                sql += "select a.rtlcd, a.rtlnm, a.landmark, a.REGEMAIL, a.REGWHATSAPPNO from " + Environment.NewLine;
                sql += "(select a.rtlcd, c.rtlnm, c.landmark, nvl(c.REGWHATSAPPNO,c.REGMOBILE)REGWHATSAPPNO, c.REGEMAIL, " + Environment.NewLine;
                sql += "row_number() over(partition by a.rtlcd order by a.effdt desc) rno " + Environment.NewLine;
                sql += "from " + scm + ".m_retail_link a, " + scm + ".m_cntrl_hdr b, " + scm + ".m_retail c " + Environment.NewLine;
                sql += "where a.m_autono = b.m_autono(+) and a.rtlcd = c.rtlcd(+) and nvl(b.inactive_tag, 'N') = 'N' and " + Environment.NewLine;
                sql += "a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and " + Environment.NewLine;
                sql += "a.slcd = '" + Distributor + "' ) a " + Environment.NewLine;
                sql += "  where rno = 1 " + Environment.NewLine;
                sql += "order by rtlnm " + Environment.NewLine;

                DataTable tbl = masterHelp.SQLquery(sql);
                if (tbl != null && tbl.Rows.Count > 0)
                {
                    VE.ListRetailer = (from DataRow a in tbl.Rows
                                       select new ListRetailer()
                                       {
                                           value = a["RTLCD"].retStr(),
                                           text = a["RTLNM"].retStr() + GCS + a["LANDMARK"].retStr() + GCS + a["REGEMAIL"].retStr() + GCS + a["REGWHATSAPPNO"].retStr(),
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
        public JsonResult BindGroupData(VMRetailOrder VE)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            try
            {
                string brandcd = "", collcd = "";
                if (VE.BrandCode != null)
                {
                    brandcd = VE.BrandCode.retSqlfromStrarray();
                }
                if (VE.CollCode != null)
                {
                    collcd = VE.CollCode.retSqlfromStrarray();
                }

                string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
                string sql = "";
                sql += "select distinct a.ITGRPCD, a.ITGRPNM " + Environment.NewLine;
                sql += "from " + scm + ".M_GROUP a, " + scm + ".m_cntrl_hdr b, " + scm + ".m_sitem c, " + scm + ".m_itemorder d, " + scm + ".m_group e " + Environment.NewLine;
                sql += "where a.m_autono = b.m_autono(+)  and nvl(b.inactive_tag, 'N')= 'N' and " + Environment.NewLine;
                sql += "e.brandcd IN (" + brandcd + ") and " + Environment.NewLine;
                if (collcd.retStr() != "") sql += "c.collcd IN (" + collcd + ") and " + Environment.NewLine;
                sql += "a.itgrpcd = c.itgrpcd(+) and c.itcd = d.itcd and c.itgrpcd=e.itgrpcd(+) " + Environment.NewLine;
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
        public JsonResult BindCollectionData(VMRetailOrder VE)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            try
            {
                string brandcd = "";
                if (VE.BrandCode != null)
                {
                    brandcd = VE.BrandCode.retSqlfromStrarray();
                }

                string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
                string sql = "";
                sql = "";
                sql += "select distinct a.COLLCD, a.COLLNM " + Environment.NewLine;
                sql += "from " + scm + ".M_COLLECTION a, " + scm + ".m_cntrl_hdr b, " + scm + ".m_sitem c, " + scm + ".m_itemorder d, " + scm + ".m_group e " + Environment.NewLine;
                sql += "where a.m_autono = b.m_autono(+) and nvl(b.inactive_tag, 'N')= 'N' and a.collcd = c.collcd(+) and c.itcd = d.itcd and c.itgrpcd=e.itgrpcd(+) " + Environment.NewLine;
                sql += "and e.brandcd in (" + brandcd + ") " + Environment.NewLine;
                sql += "order by COLLNM " + Environment.NewLine;
                DataTable tbl = masterHelp.SQLquery(sql);

                if (tbl != null && tbl.Rows.Count > 0)
                {
                    VE.ListCollection = (from DataRow a in tbl.Rows
                                         select new ListCollection()
                                         {
                                             value = a["COLLCD"].retStr(),
                                             text = a["COLLNM"].retStr(),
                                         }).ToList();
                }
                else
                {
                    VE.ListCollection = new List<ListCollection>();
                }


                ModelState.Clear();
                return Json(VE.ListCollection, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                dic.Add("message", ex.Message + ex.InnerException);
                Cn.SaveException(ex, "");
            }
            return Json(dic, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetItem(VMRetailOrder VE)
        {
            try
            {
                //TransactionRetailOrder ind = new TransactionRetailOrder();
                //ind.Dstbrslnm = VE.Dstbrslnm.Split(Convert.ToChar(Cn.GCS()))[0];
                //ind.RetailerName = VE.RetailerName.Split(Convert.ToChar(Cn.GCS()))[0];
                //ind.BrandCode = VE.BrandCode;
                //ind.BrandName = VE.BrandName;
                //ind.GroupCode = VE.GroupCode;
                //ind.GroupName = VE.GroupName;
                //ind.CollCode = VE.CollCode;
                //ind.CollName = VE.CollName;

                //T_RETAILORDER TRETAILORDER = new T_RETAILORDER();
                //TRETAILORDER.SLCD = VE.Dstbrslcd;
                //TRETAILORDER.RTLCD = VE.RetailerCode;
                //TRETAILORDER.SLMSLCD = VE.SLMSLCD;
                //ind.T_RETAILORDER = TRETAILORDER;

                //if (TempData["OrderFilter"] != null)
                //{
                //    TempData.Remove("OrderFilter");
                //}
                //TempData["OrderFilter"] = ind;

                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                ViewBag.DistributorName = VE.Dstbrslnm.Split(Convert.ToChar(Cn.GCS()))[0];
                ViewBag.RetailerName = VE.RetailerName.Split(Convert.ToChar(Cn.GCS()))[0];
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


                string brand = VE.BrandCode.retSqlfromStrarray();
                string scm = CommVar.CurSchema(UNQSNO);
                string fscm = CommVar.FinSchema(UNQSNO);
                string comp = CommVar.Compcd(UNQSNO);
                string loc = CommVar.Loccd(UNQSNO);
                string doccd = "";

                string sql = "";
                sql += " select a.m_autono,a.itcd,a.styleno, listagg(C.SIZECD, ',') within group (order by a.itcd,d.PRINT_SEQ) as sizes,nvl(a.PCSPERSET,0)PCSPERSET,a.MIXSIZE, " + Environment.NewLine;
                sql += "count(C.SIZECD)SIZE_COUNT,nvl(a.PCSPERBOX,0) PCSPERBOX " + Environment.NewLine;
                sql += " from " + CommVar.CurSchema(UNQSNO) + ".m_sitem a, " + CommVar.CurSchema(UNQSNO) + ".m_group b, " + CommVar.CurSchema(UNQSNO) + ".m_sitem_size c, " + CommVar.CurSchema(UNQSNO) + ".M_SIZE d, " + CommVar.CurSchema(UNQSNO) + ".m_itemorder e " + Environment.NewLine;
                sql += " where a.itgrpcd = b.itgrpcd and a.itcd = c.itcd and C.SIZECD=d.SIZECD(+) and a.itcd=e.itcd and " + Environment.NewLine;
                sql += " a.m_autono in (select m_autono from " + CommVar.CurSchema(UNQSNO) + ".M_CNTRL_HDR_DOC ) " + Environment.NewLine;
                if (VE.BrandCode != null) sql += "and b.brandcd in(" + VE.BrandCode.retSqlfromStrarray() + ") " + Environment.NewLine;
                if (VE.GroupCode != null) sql += "and a.itgrpcd in(" + VE.GroupCode.retSqlfromStrarray() + ") " + Environment.NewLine;
                if (VE.CollCode != null) sql += "and a.collcd in(" + VE.CollCode.retSqlfromStrarray() + ") " + Environment.NewLine;
                sql += " group by  a.m_autono,a.itcd,a.styleno,nvl(a.PCSPERSET,0),a.MIXSIZE,nvl(a.PCSPERBOX,0)  " + Environment.NewLine;


                var dt = masterHelp.SQLquery(sql);
                List<ImageView> ImageViewlst = new List<ViewModels.ImageView>();
                foreach (DataRow dr in dt.Rows)
                {
                    string itcd = dr["ITCD"].ToString();

                    string ITEMIMGPATH = @ConfigurationManager.AppSettings["ITEMIMGPATH"].retStr();
                    string physicalPath = ITEMIMGPATH + "/ItemImages/"; //@"D:" + "/ItemImages/";

                    DirectoryInfo di = new DirectoryInfo(physicalPath);
                    var totalfiles = di.GetFiles(itcd + "_*")
                                    .Select(file => new { filenm = file.Name }).ToList();

                    if (totalfiles != null && totalfiles.Count > 0)
                    {
                        foreach (var file in totalfiles)
                        {
                            ImageView objImageView = new ImageView();
                            objImageView.ITCD = dr["ITCD"].ToString();
                            objImageView.Desc = dr["styleno"].ToString();
                            objImageView.SIZES = dr["sizes"].ToString();
                            objImageView.PCSPERSET = dr["PCSPERSET"].retShort();
                            objImageView.MIXSIZE = dr["MIXSIZE"].ToString();
                            objImageView.SIZE_COUNT = dr["SIZE_COUNT"].retDbl();
                            objImageView.PCSPERBOX = dr["PCSPERBOX"].retShort();


                            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority); // http://000.000.000.00
                            string imageUrl = baseUrl + "/ItemImages/" + file.filenm.retStr();

                            objImageView.Url = imageUrl;
                            ImageViewlst.Add(objImageView);
                        }
                    }

                }
                VE.ImageView = ImageViewlst;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_RetailerOrder_Main", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult OpenRetailMaster(VMRetailOrder TSP)
        {
            RetailOutletEntry ind = new RetailOutletEntry();
            ind.Dstbrslcd = TSP.Dstbrslcd;

            M_RETAIL TRETAILORDER = new M_RETAIL();
            TRETAILORDER.SLMSLCD = TSP.SLMSLCD;
            ind.M_RETAIL = TRETAILORDER;

            if (TempData["OrderFilterRetail"] != null)
            {
                TempData.Remove("OrderFilterRetail");
            }
            TempData["OrderFilterRetail"] = ind;

            return Content("");
        }

        public ActionResult SAVE(FormCollection FC, VMRetailOrder VE, string RTLAUTONO)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    string DefaultAction = "A";
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".T_RETAILORDER in  row share mode");
                    if (DefaultAction == "A")
                    {
                        int slno = 0;

                        T_RETAILORDER TRETAILORDER = new T_RETAILORDER();
                        TRETAILORDER.CLCD = CommVar.ClientCode(UNQSNO);

                        if (RTLAUTONO.retStr() != "")
                        {
                            TRETAILORDER.AUTONO = RTLAUTONO;
                            var MAXEMDNO = (from p in DB.T_RETAILORDER where p.AUTONO == TRETAILORDER.AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                TRETAILORDER.EMD_NO = 0;
                            }
                            else
                            {
                                TRETAILORDER.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                            slno = (from p in DB.T_RETAILORDERDTL where p.AUTONO == TRETAILORDER.AUTONO select p.SLNO).Max();
                        }
                        else
                        {
                            TRETAILORDER.DOCDT = System.DateTime.Now.Date;
                            string Ddate = Convert.ToString(TRETAILORDER.DOCDT);

                            if (DefaultAction == "A")
                            {
                                TRETAILORDER.EMD_NO = 0;
                                string DOCNO = Cn.MaxDocNumber(Ddate, "T_RETAILORDER", "", true);
                                TRETAILORDER.VCHRNO = DOCNO.Split(Convert.ToChar(Cn.GCS()))[0].retInt();
                                TRETAILORDER.MNTHCD = DOCNO.Split(Convert.ToChar(Cn.GCS()))[1].ToString();

                                TRETAILORDER.DOCNO = Cn.DocPattern(TRETAILORDER.VCHRNO.retDbl(), TRETAILORDER.MNTHCD);
                                TRETAILORDER.AUTONO = "RTL" + TRETAILORDER.MNTHCD + VE.T_RETAILORDER.SLCD + TRETAILORDER.VCHRNO.retStr().PadLeft(5, '0');

                            }
                            else
                            {
                                var MAXEMDNO = (from p in DB.T_RETAILORDER where p.AUTONO == TRETAILORDER.AUTONO select p.EMD_NO).Max();
                                if (MAXEMDNO == null)
                                {
                                    TRETAILORDER.EMD_NO = 0;
                                }
                                else
                                {
                                    TRETAILORDER.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                                }
                                TRETAILORDER.VCHRNO = VE.T_RETAILORDER.VCHRNO;
                                TRETAILORDER.DOCNO = VE.T_RETAILORDER.AUTONO;
                                TRETAILORDER.AUTONO = VE.T_RETAILORDER.AUTONO;
                                TRETAILORDER.MNTHCD = VE.T_RETAILORDER.MNTHCD;
                                TRETAILORDER.DTAG = "E";
                            }
                            TRETAILORDER.RTLCD = VE.T_RETAILORDER.RTLCD;
                            TRETAILORDER.SLCD = VE.T_RETAILORDER.SLCD;
                            TRETAILORDER.SLMSLCD = VE.SLMSLCD;// VE.T_RETAILORDER.SLMSLCD;
                            TRETAILORDER.DOCAMT = VE.T_RETAILORDER.DOCAMT;

                            TRETAILORDER.USR_ID = CommVar.UserID();
                            TRETAILORDER.USR_ENTDT = System.DateTime.Now;
                            TRETAILORDER.USR_SIP = Cn.GetStaticIp();

                            //TRETAILORDER.LM_USR_ID = CommVar.UserID();
                            //TRETAILORDER.LM_USR_ENTDT = System.DateTime.Now;
                            //TRETAILORDER.LM_USR_SIP = Cn.GetStaticIp();
                            //TRETAILORDER.LM_REM = "";

                            //TRETAILORDER.DEL_USR_ID = CommVar.UserID();
                            //TRETAILORDER.DEL_USR_ENTDT = System.DateTime.Now;
                            //TRETAILORDER.DEL_USR_SIP =Cn.GetStaticIp();
                            //TRETAILORDER.DEL_REM = "";

                            //TRETAILORDER.CANCEL = "Y";
                            //TRETAILORDER.CANC_REM = "";
                            //TRETAILORDER.CANC_USR_ID = CommVar.UserID();
                            //TRETAILORDER.CANC_USR_ENTDT = System.DateTime.Now;
                            //TRETAILORDER.CANC_USR_SIP =Cn.GetStaticIp();

                            TRETAILORDER.GPSLAT = VE.T_RETAILORDER.GPSLAT;
                            TRETAILORDER.GPSLOT = VE.T_RETAILORDER.GPSLOT;
                            TRETAILORDER.DOCREM = VE.T_RETAILORDER.DOCREM;
                            TRETAILORDER.GPSNM = GetAddress(VE.T_RETAILORDER.GPSLAT.retStr(), VE.T_RETAILORDER.GPSLOT.retStr());


                            if (DefaultAction == "A")
                            {
                                DB.T_RETAILORDER.Add(TRETAILORDER);
                            }
                            else if (DefaultAction == "E")
                            {
                                DB.Entry(TRETAILORDER).State = System.Data.Entity.EntityState.Modified;
                            }
                        }

                        List<APP_ITEMLIST> aPP_ITEMLIST = JsonConvert.DeserializeObject<List<APP_ITEMLIST>>(VE.ITEMDETAIL_JSTR);
                        foreach (var v in aPP_ITEMLIST)
                        {
                            var sizes = v.sizes.retStr().Split(',');
                            foreach (var sizeq in sizes)
                            {
                                var sqn = sizeq.retStr().Split('=');
                                if (sqn.Length > 1)
                                {
                                    slno++;
                                    T_RETAILORDERDTL TRETAILORDERDTL = new T_RETAILORDERDTL();
                                    TRETAILORDERDTL.CLCD = TRETAILORDER.CLCD;
                                    TRETAILORDERDTL.EMD_NO = TRETAILORDER.EMD_NO;
                                    TRETAILORDERDTL.AUTONO = TRETAILORDER.AUTONO;
                                    TRETAILORDERDTL.DTAG = TRETAILORDER.DTAG;
                                    TRETAILORDERDTL.ITCD = v.itcd;
                                    TRETAILORDERDTL.SLNO = slno.retShort();
                                    TRETAILORDERDTL.SIZECD = sqn[0];
                                    TRETAILORDERDTL.QNTY = sqn[1].retDbl();
                                    TRETAILORDERDTL.ITREM = v.itrem;
                                    DB.T_RETAILORDERDTL.Add(TRETAILORDERDTL);
                                }
                            }
                        }


                        DB.SaveChanges();
                        ModelState.Clear();
                        transaction.Commit();

                        string ContentFlg = "";
                        if (DefaultAction == "A")
                        {
                            string emailmsg = SendEmailWhatsapp(TRETAILORDER.AUTONO);
                            if (RTLAUTONO.retStr() == "")
                            {
                                ContentFlg = "1~(Order No. " + TRETAILORDER.DOCNO + ")" + emailmsg;

                            }
                            else
                            {
                                ContentFlg = "2~(Order No. " + TRETAILORDER.DOCNO + ")" + emailmsg;
                            }
                        }
                        else if (DefaultAction == "E")
                        {
                            ContentFlg = "2";
                        }
                        return Content(ContentFlg);

                    }
                    //else if (DefaultAction == "V")
                    //{
                    //    T_CNTRL_HDR MCH = Cn.T_CONTROL_HDR(VE.T_CNTRL_HDR.DOCCD, VE.T_CNTRL_HDR.DOCDT, VE.T_CNTRL_HDR.DOCNO, VE.T_RETAILORDER.AUTONO, VE.T_CNTRL_HDR.MNTHCD, VE.T_CNTRL_HDR.DOCNO, "D", CommVar.CurSchema(UNQSNO), "", VE.T_RETAILORDER.SLCD, 0, "", VE.T_CNTRL_HDR.YR_CD);
                    //    DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                    //    DB.SaveChanges();

                    //    DB.T_RETAILORDER.Where(x => x.AUTONO == VE.T_RETAILORDER.AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                    //    DB.SaveChanges();

                    //    DB.T_RETAILORDER.RemoveRange(DB.T_RETAILORDER.Where(x => x.AUTONO == VE.T_RETAILORDER.AUTONO));
                    //    DB.SaveChanges();
                    //    DB.T_CNTRL_HDR.RemoveRange(DB.T_CNTRL_HDR.Where(x => x.AUTONO == VE.T_RETAILORDER.AUTONO));
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
                }
            }
            return null;
        }
        public ActionResult ChkForMerge(VMRetailOrder VE)
        {
            try
            {
                string msg = "";

                string RTLCD = VE.T_RETAILORDER.RTLCD;
                string SLCD = VE.T_RETAILORDER.SLCD;
                string docdt = System.DateTime.Now.Date.retDateStr();
                string scm = CommVar.SaleSchema(UNQSNO);
                string sql = "";
                sql += "select autono,USR_ENTDT from " + scm + ".T_RETAILORDER where RTLCD='" + RTLCD + "' and SLCD='" + SLCD + "' and docdt=to_date('" + docdt + "','dd/mm/yyyy') ";
                sql += "order by USR_ENTDT desc ";
                DataTable dt = masterHelp.SQLquery(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    msg += "^MSG=^Y" + Cn.GCS();
                    msg += "^AUTONO=^" + dt.Rows[0]["autono"] + Cn.GCS();

                }
                return Content(msg);
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
                sql += Environment.NewLine + "select a.autono, a.docno,''doconlyno,''vchrno, a.docdt,''doccd, a.slcd, ''agslcd, ''trslcd, ''crslcd, ''slmslcd, ''prccd, ''prceffdt, ";
                sql += Environment.NewLine + "''discrtcd, ''discrteffdt, ''docth, ''scmnm, ''prcnm, ''splnote,''cournm,''destn,''agslnm, h.itnm, h.styleno, ";
                sql += Environment.NewLine + "''district, ''trslnm,''crslnm,''totbox,''toset,b.itcd,''freestk,''rate,''ordqnty,i.pcsperbox,i.pcsperset,i.colrperset, ";
                sql += Environment.NewLine + " a.usr_id,a.usr_entdt,''ordamt, ''delvtypedsc, ''rateprint,''slno,''stylno,''stktype, ";
                sql += Environment.NewLine + "''docth1, ''docth2, ''docth3, ''paytrmcd, ''paytrmnm, ''delvins, ''duedays, ''cod, ''prefno, ''prefdt, ";

                sql += Environment.NewLine + "a.RTLCD,d.RTLNM,a.slcd,e.slnm,e.add1 sladd1,e.add3 sladd2,e.add3 sladd3,e.add4 sladd4,e.add5 sladd5, ";
                sql += Environment.NewLine + "e.add6 sladd6,e.add7 sladd7,e.state slstate,e.REGEMAILID,e.PANNO slpanno,e.TANNO sltanno,e.REGEMAILID slemail, ";
                sql += Environment.NewLine + "e.REGMOBILE slmobile,e.GSTNO slgstno,d.SLMSLCD,f.slnm SLMSLNM,d.add1,d.add2,d.add3,d.add4,d.landmark,d.city,d.pin,g.statenm, ";
                sql += Environment.NewLine + "e.DISTRICT sldistrict,e.PIN slpin, d.GSTNO, d.REGMOBILE, d.REGEMAIL,b.SIZECD,b.QNTY,d.pan,nvl(d.REGWHATSAPPNO,d.REGMOBILE)REGWHATSAPPNO,k.BRANDNM from ";

                sql += Environment.NewLine + scm + ".T_RETAILORDER a, " + scm + ".T_RETAILORDERDTL b, " + scm + ".t_cntrl_hdr c, " + scm + ".M_RETAIL d, ";
                sql += Environment.NewLine + scmf + ".m_subleg e, " + scmf + ".m_subleg f, " + scm + ".m_sitem h, " + csm + ".ms_state g, " + scm + ".M_SITEM i, " + scm + ".M_GROUP j, " + scm + ".M_BRAND k ";
                sql += Environment.NewLine + "where a.autono=b.autono(+) and a.autono=c.autono(+) and a.RTLCD=d.RTLCD(+) and a.slcd=e.slcd(+) and d.SLMSLCD=f.slcd(+) ";
                sql += Environment.NewLine + " and b.itcd = h.itcd(+) and d.STATECD = g.STATECD(+) and b.itcd = i.itcd(+) and i.itgrpcd=j.itgrpcd(+) and j.brandcd = k.brandcd(+) ";
                sql += Environment.NewLine + "and a.autono in ('" + autonum + "') ";
                sql += Environment.NewLine + "order by a.docdt,c.doconlyno ";
                rstbl = masterHelp.SQLquery(sql);

                string AUTO_NO = string.Join(",", (from DataRow dr in rstbl.Rows select "'" + dr["autono"].ToString() + "'").Distinct());

                DataTable IR = new DataTable();

                IR.Columns.Add("docno", typeof(string), "");
                IR.Columns.Add("docdt", typeof(string), "");
                IR.Columns.Add("slnm", typeof(string), "");
                IR.Columns.Add("slcd", typeof(string), "");
                IR.Columns.Add("trslnm", typeof(string), "");
                IR.Columns.Add("trslcd", typeof(string), "");
                IR.Columns.Add("cournm", typeof(string), "");
                IR.Columns.Add("destn", typeof(string), "");
                IR.Columns.Add("agslnm", typeof(string), "");
                IR.Columns.Add("agslcd", typeof(string), "");
                IR.Columns.Add("slmslnm", typeof(string), "");
                IR.Columns.Add("slmslcd", typeof(string), "");
                IR.Columns.Add("prcnm", typeof(string), "");
                IR.Columns.Add("rem", typeof(string), "");
                IR.Columns.Add("splnote", typeof(string), "");
                IR.Columns.Add("docth1", typeof(string), "");
                IR.Columns.Add("docth2", typeof(string), "");
                IR.Columns.Add("docth3", typeof(string), "");
                IR.Columns.Add("scmnm", typeof(string), "");
                IR.Columns.Add("totbox", typeof(string), "");
                IR.Columns.Add("toset", typeof(string), "");
                IR.Columns.Add("ordamt", typeof(double), "");
                IR.Columns.Add("delvtypedsc", typeof(string), "");
                //extra
                IR.Columns.Add("rateprint", typeof(string), "");
                IR.Columns.Add("crslcd", typeof(string), "");
                IR.Columns.Add("prccd", typeof(string), "");
                IR.Columns.Add("prceffdt", typeof(string), "");
                IR.Columns.Add("discrtcd", typeof(string), "");
                IR.Columns.Add("discrteffdt", typeof(string), "");
                IR.Columns.Add("sldistrict", typeof(string), "");
                IR.Columns.Add("crslnm", typeof(string), "");
                IR.Columns.Add("pcstyle", typeof(string), "");
                IR.Columns.Add("usr_id", typeof(string), "");
                IR.Columns.Add("usr_entdt", typeof(string), "");
                IR.Columns.Add("paytrmcd", typeof(string), "");
                IR.Columns.Add("paytrmnm", typeof(string), "");
                IR.Columns.Add("duedays", typeof(string), "");
                //details
                IR.Columns.Add("slno", typeof(double), "");
                IR.Columns.Add("styleno", typeof(string), "");
                IR.Columns.Add("itnm", typeof(string), "");
                IR.Columns.Add("stktype", typeof(string), "");
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
                IR.Columns.Add("prefno", typeof(string), "");
                IR.Columns.Add("prefdt", typeof(string), "");
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
                IR.Columns.Add("autono", typeof(string), "");
                IR.Columns.Add("QNTY", typeof(double), "");
                IR.Columns.Add("REGWHATSAPPNO", typeof(string), "");

                Int32 maxR = rstbl.Rows.Count - 1;
                Int32 i = 0; double partytotos = 0, totbox = 0, totset = 0, approxvalue = 0;
                string billno = "", slcd = "";
                int slno = 0;
                while (i <= maxR)
                {
                    string autono = rstbl.Rows[i]["autono"].ToString();
                    string docdt = rstbl.Rows[i]["docdt"].ToString().retDateStr();
                    billno = rstbl.Rows[i]["docno"].ToString();
                    slcd = rstbl.Rows[i]["RTLCD"].ToString();
                    double tset = 0;


                    DataRow Row1 = IR.NewRow();
                    Row1["docno"] = rstbl.Rows[i]["docno"].ToString();
                    /* Row1["docdt"] = prndt; rstbl.Rows[i]["docdt"].ToString().Remove(10);*/
                    Row1["docdt"] = rstbl.Rows[i]["docdt"].ToString().Remove(10);
                    Row1["slnm"] = rstbl.Rows[i]["slnm"].ToString();
                    Row1["slcd"] = rstbl.Rows[i]["slcd"].ToString();
                    Row1["trslnm"] = rstbl.Rows[i]["trslnm"].ToString();
                    Row1["trslcd"] = rstbl.Rows[i]["trslcd"].ToString();
                    Row1["destn"] = rstbl.Rows[i]["district"].ToString();
                    Row1["agslnm"] = rstbl.Rows[i]["agslnm"].ToString();
                    Row1["agslcd"] = rstbl.Rows[i]["agslcd"].ToString();
                    Row1["slmslnm"] = rstbl.Rows[i]["SLMSLNM"].ToString();
                    Row1["slmslcd"] = rstbl.Rows[i]["slmslcd"].ToString();
                    Row1["cournm"] = rstbl.Rows[i]["cournm"].ToString();
                    Row1["delvtypedsc"] = Salesfunc.retDelvTypeDesc(rstbl.Rows[i]["cod"].ToString());
                    Row1["prcnm"] = rstbl.Rows[i]["prcnm"].ToString();
                    Row1["prceffdt"] = rstbl.Rows[i]["prceffdt"].ToString().retDateStr();
                    Row1["docth1"] = Salesfunc.retDocTh(rstbl.Rows[i]["docth"].ToString()) + " " + rstbl.Rows[i]["docth1"].ToString();
                    Row1["docth2"] = rstbl.Rows[i]["docth2"].ToString();
                    Row1["docth3"] = rstbl.Rows[i]["docth3"].ToString();
                    Row1["scmnm"] = rstbl.Rows[i]["scmnm"].ToString();
                    Row1["ordamt"] = approxvalue;
                    //extra
                    Row1["crslcd"] = rstbl.Rows[i]["crslcd"].ToString();
                    Row1["prccd"] = rstbl.Rows[i]["prccd"].ToString();
                    Row1["discrtcd"] = rstbl.Rows[i]["discrtcd"].ToString();
                    Row1["discrteffdt"] = rstbl.Rows[i]["discrteffdt"].ToString();
                    Row1["sldistrict"] = " " + rstbl.Rows[i]["sldistrict"].ToString() + " - " + rstbl.Rows[i]["slpin"].ToString() + ", " + rstbl.Rows[i]["slstate"].ToString();
                    Row1["crslnm"] = rstbl.Rows[i]["crslnm"].ToString();
                    Row1["RTLREGEMAIL"] = rstbl.Rows[i]["REGEMAIL"].ToString();
                    Row1["DISREGEMAILID"] = rstbl.Rows[i]["REGEMAILID"].ToString();
                    Row1["usr_id"] = rstbl.Rows[i]["usr_id"].ToString();
                    Row1["usr_entdt"] = rstbl.Rows[i]["usr_entdt"].ToString();
                    Row1["paytrmcd"] = rstbl.Rows[i]["paytrmcd"].ToString();
                    Row1["paytrmnm"] = rstbl.Rows[i]["paytrmnm"].ToString();
                    Row1["rem"] = "";
                    Row1["splnote"] = rstbl.Rows[i]["splnote"].ToString();
                    Row1["duedays"] = rstbl.Rows[i]["duedays"].ToString();
                    Row1["autono"] = rstbl.Rows[i]["autono"].ToString();
                    Row1["QNTY"] = rstbl.Rows[i]["QNTY"].ToString();
                    Row1["REGWHATSAPPNO"] = rstbl.Rows[i]["REGWHATSAPPNO"].ToString();
                    //details table
                    slno++;
                    Row1["slno"] = slno;
                    Row1["styleno"] = rstbl.Rows[i]["styleno"];
                    Row1["itnm"] = rstbl.Rows[i]["itnm"].ToString();
                    Row1["stktype"] = rstbl.Rows[i]["stktype"].ToString();
                    Row1["prefno"] = rstbl.Rows[i]["prefno"].ToString();
                    Row1["prefdt"] = rstbl.Rows[i]["prefdt"].ToString();
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
                    Row1["brandnm"] = rstbl.Rows[i]["brandnm"].ToString();
                    Row1["CITY"] = rstbl.Rows[i]["CITY"].ToString();
                    Row1["PIN"] = rstbl.Rows[i]["PIN"].ToString();
                    Row1["STATENM"] = rstbl.Rows[i]["STATENM"].ToString();
                    Row1["GSTNO"] = rstbl.Rows[i]["GSTNO"].ToString();
                    Row1["PAN"] = rstbl.Rows[i]["pan"].ToString();
                    Row1["REGMOBILE"] = rstbl.Rows[i]["REGMOBILE"].ToString();



                    string check1 = rstbl.Rows[i]["itcd"].ToString() + rstbl.Rows[i]["freestk"].ToString();
                    string pcstyle = "", sizes = "", boxes = "";
                    double tbox = 0, tpcs = 0, rate = 0, ordqnty = 0, chkpcs = 0;

                    rate = (rstbl.Rows[i]["rate"]).retDbl();
                    ordqnty = (rstbl.Rows[i]["ordqnty"]).retDbl();

                    pcstyle = rstbl.Rows[i]["pcsperbox"].ToString() + "/" + rstbl.Rows[i]["pcsperset"].ToString() + "/" + rstbl.Rows[i]["colrperset"].ToString();

                    while (rstbl.Rows[i]["itcd"].ToString() + rstbl.Rows[i]["freestk"].ToString() == check1)
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
                string rptfile = "Rep_Ord.rpt";
                rptname = "~/Report/" + rptfile;
                ReportDocument reportdocument = new ReportDocument();

                var rsemailid = (from DataRow dr in IR.Rows
                                 select new
                                 {
                                     email = dr["RTLREGEMAIL"],
                                     slcd = dr["slcd"],
                                     //watsapp = dr["REGWHATSAPPNO"]
                                     regmno = dr["REGWHATSAPPNO"],
                                     autono = dr["autono"],

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
                            if (rsemailid1.Rows[iz]["DISREGEMAILID"].ToString().retStr() != "") ccemailid = "";// rsemailid1.Rows[iz]["DISREGEMAILID"].ToString();
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


                        reportdocument.Load(Server.MapPath("~/Report/Rep_Ord.rpt"));
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
                            bool emailsent = EmailControl.SendHtmlFormattedEmail(rsemailid[z].email.ToString(), "Order Copy", "SalesOrder.htm", emlaryBody, attchmail, ccemailid);
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

                        if (rsemailid[z].regmno.ToString() != "")
                        {
                            SMS sms = new SMS();
                            List<string> sendmsg = sms.WHATSAPPMessContectGen(slcd, "APPORDW", smsaryMsg);
                            msgresult = sms.WHATSAPPsend(rsemailid[z].regmno.ToString(), sendmsg[0], sendmsg[1], pdffilenm, imgfilenm);
                            string[] msgretval = msgresult.Split('=');
                            if (msgretval[0].retStr() == "")
                            {
                                sendmobno = rsemailid[z].regmno.ToString();
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
                return (ex.Message + ex.InnerException);
            }


        }


        public string GetAddress(string lat, string lng)
        {
            try
            {
                string datastring = "";
                //lat = "22.555"; lng = "88.258";
                var url = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&sensor=true&key=AIzaSyBDxBcnd3Jf8nDInK1xxCSvtRwSiWB4mck";
                WebRequest rqst = HttpWebRequest.Create(url);
                using (HttpWebResponse rspns = (HttpWebResponse)rqst.GetResponse())
                {
                    Stream strm = (Stream)rspns.GetResponseStream();
                    StreamReader strmrdr = new StreamReader(strm);
                    datastring = strmrdr.ReadToEnd();
                    strm.Close();
                    strmrdr.Close();
                    rspns.Close();
                }
                GeoLocation geoLocation = JsonConvert.DeserializeObject<GeoLocation>(datastring);
                var address = geoLocation.results[0].formatted_address;
                return address;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return "";
            }
        }
        public class AddressComponent
        {
            public string long_name { get; set; }
            public string short_name { get; set; }
            public List<string> types { get; set; }
        }

        public class Result
        {
            public List<AddressComponent> address_components { get; set; }
            public string formatted_address { get; set; }
        }

        public class GeoLocation
        {
            public List<Result> results { get; set; }
            public string status { get; set; }
        }
    }
}

