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
    public class T_DistOrderFilterController : Controller
    {
        Connection Cn = new Connection(); string sql = "";
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc salesfunc = new Salesfunc();
        DropDownHelp dropDownHelp = new DropDownHelp();

        M_CNTRL_HDR sll; M_GENLEG sGEN;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_DistOrderFilter
        public ActionResult T_DistOrderFilter(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {//k
            VMDistOrder VE = new VMDistOrder();
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ViewBag.formname = "DISTRIBUTOR ORDER CONFIRMATION";
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
                    }
                    else
                    {
                        VE.ListDistributor = new List<ListDistributor>();
                        VE.ListBrand = new List<ListBrand>();
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

        public ActionResult GetItem(VMDistOrder TSP)
        {
            TransactionDistOrder ind = new TransactionDistOrder();
            ind.Dstbrslnm = TSP.Dstbrslnm.Split(Convert.ToChar(Cn.GCS()))[0];
            ind.BrandCode = TSP.BrandCode;
            ind.BrandName = TSP.BrandName;
            ind.GroupCode = TSP.GroupCode;
            ind.GroupName = TSP.GroupName;
            ind.CollCode = TSP.CollCode;
            ind.CollName = TSP.CollName;

            T_DISTORDER TDISTORDER = new T_DISTORDER();
            TDISTORDER.SLCD = TSP.Dstbrslcd;
            TDISTORDER.RTLCD = TSP.RetailerCode;
            TDISTORDER.SLMSLCD = TSP.SLMSLCD;
            ind.T_DISTORDER = TDISTORDER;

            if (TempData["DistOrderFilter"] != null)
            {
                TempData.Remove("DistOrderFilter");
            }
            TempData["DistOrderFilter"] = ind;
            return Content("");
        }


    }
}

