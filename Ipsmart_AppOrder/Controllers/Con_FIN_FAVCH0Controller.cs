using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Improvar.ViewModels;
using Improvar.Models;
using System.Collections;
using System.Data;

namespace Improvar.Controllers
{
    public class Con_FIN_FAVCH0Controller : Controller
    {
        // GET: Con_FIN_FAVCH0
        Connection Cn = new Connection();
        public ActionResult Menu_FIN_FAVCH0(FormCollection FC, string op = "", string key = "")
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                Hashtable Permission = (Hashtable)Session["Permission"];
                string Permidetails = Permission[Session["menupermission"]].ToString();
                string[] PermiChild = Permidetails.Split('#');
                string AEDV = PermiChild[0];
                string Add = PermiChild[2];
                string edit = PermiChild[1];
                string dele = PermiChild[3];
                string loca1 = Session["CompanyLocationCode"].ToString().Trim();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Session["DatabaseSchemaName"].ToString());
                VoucherEntry VE = new VoucherEntry();
                VE.MSG = "";
                if (TempData["recsave"] != null)
                {
                    VE.MSG = TempData["recsave"].ToString();
                    TempData.Remove("recsave");
                }
                VE.Add = "";
                VE.Edit = "";
                VE.Delete = "";
                VE.View = "";
                for (int i = 0; i <= AEDV.Length - 1; i++)
                {
                    if (AEDV[i].ToString().Trim() == "A")
                    {
                        VE.Add = "A";
                    }
                    else if (AEDV[i].ToString().Trim() == "E")
                    {
                        VE.Edit = "E";
                    }
                    else if (AEDV[i].ToString().Trim() == "D")
                    {
                        VE.Delete = "D";
                    }
                    else if (AEDV[i].ToString().Trim() == "V")
                    {
                        VE.View = "V";
                    }
                    else if (AEDV[i].ToString().Trim() == "C")
                    {
                        VE.Check = "C";
                    }
                }
                VE.AddDay = Convert.ToInt32(Add);
                VE.EditDay = Convert.ToInt32(edit);
                VE.DeleteDay = Convert.ToInt32(dele);
                VE.ExitMode = 0;
                if (op.Length != 0)
                {
                    VE.DefaultView = true;
                    VE.ExitMode = 1;
                    if (op.ToUpper().Trim() == "A")
                    {
                        VE.DefaultDay = VE.AddDay;
                    }
                    else if (op.ToUpper().Trim() == "E")
                    {
                        VE.DefaultDay = VE.EditDay;
                    }
                    else if (op.ToUpper().Trim() == "D")
                    {
                        VE.DefaultDay = VE.DeleteDay;
                    }
                    else if (op.ToUpper().Trim() == "V")
                    {
                        VE.DefaultDay = 0;
                    }
                    //calender setting==============================================
                    string[] financialyeardate = Session["CompanyFinancial"].ToString().Split('-');
                    DateTime LastDate = DateTime.ParseExact(financialyeardate[1].Trim().ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    if (DateTime.Now.Date <= LastDate)
                    {
                        string day = DateTime.Now.Day.ToString().Length == 1 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
                        string mon = DateTime.Now.Month.ToString().Length == 1 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
                        VE.maxdate = day + "/" + mon + "/" + DateTime.Now.Year.ToString();
                        DateTime FirstDate = DateTime.ParseExact(VE.maxdate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        if (VE.DefaultDay == 0)
                        {
                            VE.mindate = financialyeardate[0].Trim().ToString();
                        }
                        else
                        {
                            DateTime FD = FirstDate.AddDays(-VE.DefaultDay);
                            DateTime Date = DateTime.ParseExact(financialyeardate[0].Trim().ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            if (FD >= Date)
                            {
                                VE.mindate = FD.Day.ToString() + "/" + FD.Month.ToString() + "/" + FD.Year.ToString();
                            }
                            else
                            {
                                VE.mindate = financialyeardate[0].Trim().ToString();
                            }
                        }
                    }
                    else
                    {
                        VE.maxdate = financialyeardate[1].Trim().ToString();
                        if (VE.DefaultDay == 0)
                        {
                            VE.mindate = financialyeardate[0].Trim().ToString();
                        }
                        else
                        {
                            DateTime LD = LastDate.AddDays(-VE.DefaultDay);
                            VE.mindate = LD.Day.ToString() + "/" + LD.Month.ToString() + "/" + LD.Year.ToString();
                        }
                    }
                    //==================================================end
                    VE.DefaultAction = op;
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
    }
}