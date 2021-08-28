//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using Improvar.Models;
//using Improvar.ViewModels;
//using System.Collections;
//using System.Data;
//using Oracle.ManagedDataAccess.Client;
//using System.Net;
//using System.Net.Sockets;


//namespace Improvar.Controllers
//{
//    public class DOCPASS_AUTHController : Controller
//    {
//        string CS = null;
//        Connection Cn = new Connection();
//        MasterHelp masterHelp = new MasterHelp();
//        string UNQSNO = CommVar.getQueryStringUNQSNO();
//        M_SIGN_AUTH sl;
//        M_DESIGNATION sDESG;
//        USER_APPL sl2;
//        M_CNTRL_HDR sll;
//        M_EMPMAS sEMP;

//        // GET: DOCPASS_AUTH
//        public ActionResult DOCPASS_AUTH(string op = "", string key = "", int Nindex = 0, string searchValue = "", string searchValue1 = "", long searchMautono = 0)

//        {
//            if (Session["UR_ID"] == null)
//            {
//                return RedirectToAction("Login", "Login");
//            }
//            else
//            {
//                ViewBag.formname = "Passing Authority";
//                string SCHEMA = Cn.Getschema;
//                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
//                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), SCHEMA);
//                PassingAuthorityEntry VE = new PassingAuthorityEntry(); Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
//                VE.DefaultAction = op;
//                if (op.Length != 0)
//                {
//                    VE.IndexKey = (from p in DB.M_SIGN_AUTH orderby p.AUTHCD select new IndexKey() { Navikey = p.AUTHCD }).ToList();


//                    VE.DropDown_list = (from i in DB1.USER_APPL
//                               select new DropDown_list()
//                               {
//                                   value = i.USER_ID,
//                                   text = i.USER_NAME
//                               }).OrderBy(s => s.value).ToList();


//                    if (op == "E" || op == "D" || op == "V")
//                    {
//                        var MDT = (from j in DB.M_SIGN_AUTH
//                                   join o in DB.M_CNTRL_HDR on j.M_AUTONO equals (o.M_AUTONO)
//                                   where (o.M_AUTONO == j.M_AUTONO)
//                                   select new Searching()
//                                   {
//                                       Col1 = j.AUTHCD,
//                                       Col2 = j.AUTHNM,
//                                       Col11 = o.M_AUTONO
//                                   }).OrderBy(s => s.Col1).ToList();
//                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
//                        string JR = javaScriptSerializer.Serialize(MDT);
//                        //VE.JsonString = JR;
//                        //VE.Header = " P.A. Code" + Cn.GCS() + " P.A. Name" + Cn.GCS() + "AUTO NO";
//                        //VE.NativHeader = "Col1" + Cn.GCS() + "Col2" + Cn.GCS() + "Col11";
//                        //VE.hidecolumn = "2";
//                        //VE.returnindex = "0";

//                        if (searchValue.Length != 0)
//                        {
//                            VE.Index = Nindex;
//                            VE = Navigation(VE, DB, 0, searchValue);
//                        }
//                        else
//                        {
//                            if (key == "F")
//                            {
//                                VE.Index = 0;
//                                VE = Navigation(VE, DB, 0, searchValue);
//                            }
//                            else if (key == "" || key == "L")
//                            {
//                                VE.Index = VE.IndexKey.Count - 1;
//                                VE = Navigation(VE, DB, VE.IndexKey.Count - 1, searchValue);
//                            }
//                            else if (key == "P")
//                            {
//                                Nindex -= 1;
//                                if (Nindex < 0)
//                                {
//                                    Nindex = 0;
//                                }
//                                VE.Index = Nindex;
//                                VE = Navigation(VE, DB, Nindex, searchValue);
//                            }
//                            else if (key == "N")
//                            {
//                                Nindex += 1;
//                                if (Nindex > VE.IndexKey.Count - 1)
//                                {
//                                    Nindex = VE.IndexKey.Count - 1;
//                                }
//                                VE.Index = Nindex;
//                                VE = Navigation(VE, DB, Nindex, searchValue);
//                            }
//                        }
//                        VE.M_SIGN_AUTH = sl;
//                        VE.M_DESIGNATION = sDESG;
//                        VE.M_CNTRL_HDR = sll;
//                        VE.USER_APPL = sl2;
//                        VE.M_EMPMAS = sEMP;
//                    }
//                    return View(VE);
//                }
//                else
//                {
//                    VE.DefaultView = false;
//                    VE.DefaultDay = 0;
//                    return View(VE);
//                }
//            }
//        }
  
                   
//        public PassingAuthorityEntry Navigation(PassingAuthorityEntry VE, ImprovarDB DB, int index, string searchValue)
//        {
//            ImprovarDB DBP = new ImprovarDB(Cn.GetConnectionString(), CommVar.PaySchema(UNQSNO));
//            sl = new M_SIGN_AUTH(); sll = new M_CNTRL_HDR();sEMP = new M_EMPMAS(); sDESG = new M_DESIGNATION();
//            if (VE.IndexKey.Count != 0)
//            {
//                string[] aa = null;
//                if (searchValue.Length == 0)
//                {
//                    aa = VE.IndexKey[index].Navikey.Split(Convert.ToChar(Cn.GCS()));
//                }
//                else
//                {
//                    aa = searchValue.Split(Convert.ToChar(Cn.GCS()));
//                }
//                sl = DB.M_SIGN_AUTH.Find(aa[0]);
//                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
//                sEMP = DBP.M_EMPMAS.Find(sl.EMPID);
//                sDESG = DB.M_DESIGNATION.Find(sl.DESIGCD);
//                if (sll.INACTIVE_TAG == "Y")
//                {
//                    VE.Checked = true;
//                }
//                else
//                {
//                    VE.Checked = false;
//                }
//            }

//            return VE;
//        }

//        public ActionResult SearchPannelData()
//        {
//            try
//            {
//                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
//                var DocpassAuth = (from j in DB.M_SIGN_AUTH
//                                   join k in DB.M_DESIGNATION on j.DESIGCD equals k.DESIGCD
//                                   orderby j.AUTHCD
//                                   select new
//                                   {
//                                       AUTHCD = j.AUTHCD,
//                                       AUTHNM = j.AUTHNM,
//                                       USRID = j.USRID,
//                                       DESIGCD = j.DESIGCD,
//                                       DESIGNM = k.DESIGNM,
//                                       EMPID = j.EMPID,
//                                   }).ToList();
//                System.Text.StringBuilder SB = new System.Text.StringBuilder();
//                var hdr = "Authority Code" + Cn.GCS() + "Authority Code" + Cn.GCS() + "User Id" + Cn.GCS() + "Epmployee Code" + Cn.GCS() + "Designation";
//                for (int j = 0; j <= DocpassAuth.Count - 1; j++)
//                {
//                    SB.Append("<tr><td>" + DocpassAuth[j].AUTHCD + "</td><td>" + DocpassAuth[j].AUTHNM + "</td><td>" + DocpassAuth[j].USRID + "</td><td>" + DocpassAuth[j].EMPID + "</td><td>" + DocpassAuth[j].DESIGNM + "[" + DocpassAuth[j].DESIGCD + "]" + "</td></tr>");
//                }
//                return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "0" + Cn.GCS() + "1" + Cn.GCS() + "2" + Cn.GCS() + "3" + Cn.GCS() + "4"));
//            }
//            catch (Exception ex)
//            {
//                Cn.SaveException(ex, "");
//                return Content(ex.Message + ex.InnerException);
//            }
//        }

//        //public ActionResult EmployeeCode(string val)
//        //{
            
//        //       ImprovarDB DBP = new ImprovarDB(Cn.GetConnectionString(), CommVar.PaySchema(UNQSNO));
//        //    var query = (from c in DBP.M_EMPMAS where (c.EMPCD == val) select c);
//        //    if (query.Any())
//        //    {
//        //        string str = "";
//        //        foreach (var i in query)
//        //        {
//        //            str = i.EMPCD + "/" + i.ENM;
//        //        }
//        //        return Content(str);
//        //    }
//        //    else
//        //    {
//        //        return Content("0");
//        //    }
//        //}
//        public ActionResult GetEmployeeCode(string val)
//        {
//            if (val==null) { return PartialView("_Help2", masterHelp.EMPCD_help(val)); }
//            else {
//                string str = masterHelp.EMPCD_help(val);
//                return Content(str);
//            }

//        }
//        public ActionResult GetDesigCode(string val)
//        {
//            if (val == null) { return PartialView("_Help2", masterHelp.DESIGCD_help(val)); }
//            else {
//                string str = masterHelp.DESIGCD_help(val);
//                return Content(str);
//            }
           
//        }
//        //public ActionResult DesigCode(string val)
//        //{
//        //    var UNQSNO = Cn.getQueryStringUNQSNO();
//        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
//        //    var query = (from c in DB.M_DESIGNATION where (c.DESIGCD == val) select c);
//        //    if (query.Any())
//        //    {
//        //        string str = "";
//        //        foreach (var i in query)
//        //        {
//        //            str = i.DESIGCD + "/" + i.DESIGNM;
//        //        }
//        //        return Content(str);
//        //    }
//        //    else
//        //    {
//        //        return Content("0");
//        //    }
//        //}
//        public ActionResult SAVE(FormCollection FC, PassingAuthorityEntry VE)
//        {
//            var UNQSNO = Cn.getQueryStringUNQSNO();
//            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
//            using (var transaction = DB.Database.BeginTransaction())
//            {
//                try
//                {
//                    if (VE.DefaultAction == "A")
//                    {
//                        M_SIGN_AUTH MSIGNAUTH = new M_SIGN_AUTH();

//                        var MAXAUTHCD = DB.M_SIGN_AUTH.Max(a => a.AUTHCD);
//                        if (MAXAUTHCD == null)
//                        {
//                            MSIGNAUTH.AUTHCD = "00001";
//                        }
//                        else
//                        {
//                            int MXPARTYCD = Convert.ToInt32(MAXAUTHCD);
//                            MAXAUTHCD = Convert.ToString(MXPARTYCD + 1);
//                            string code = MAXAUTHCD.ToString().PadLeft(5, '0');
//                            MSIGNAUTH.AUTHCD = code.ToString();
//                        }
//                        MSIGNAUTH.AUTHNM = VE.M_SIGN_AUTH.AUTHNM;
//                        MSIGNAUTH.EMPID = VE.M_SIGN_AUTH.EMPID;
//                        var MAXMAUTONO = DB.M_CNTRL_HDR.Select(p => p.M_AUTONO).DefaultIfEmpty(0).Max();

//                        if (MAXMAUTONO == 0)
//                        {
//                            MSIGNAUTH.M_AUTONO = 1;
//                        }
//                        else
//                        {
//                            MAXMAUTONO = MAXMAUTONO + 1;
//                            MSIGNAUTH.M_AUTONO = (MAXMAUTONO);
//                        }
//                        MSIGNAUTH.DESIGCD = VE.M_SIGN_AUTH.DESIGCD;
//                        MSIGNAUTH.CLCD = CommVar.ClientCode(UNQSNO);
//                        var query = (from c in DB.M_DESIGNATION where (c.DESIGCD == MSIGNAUTH.DESIGCD) select c);
//                        if (query.Any())
//                        {
//                            MSIGNAUTH.USRID = FC["usrid"].ToString();

//                            M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SIGN_AUTH", MSIGNAUTH.M_AUTONO.Value, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
//                            DB.M_SIGN_AUTH.Add(MSIGNAUTH);
//                            DB.M_CNTRL_HDR.Add(MCH);
//                            DB.SaveChanges();
//                            transaction.Commit();
//                            return Content("1");
//                        }
//                        else
//                        {
//                            return Content("Invalid 'Designation Code' ! Please Enter a Valid 'Designation Code'.");
//                        }
//                    }

//                    else if (VE.DefaultAction == "E")
//                    {
//                        M_SIGN_AUTH MSIGNAUTH = new M_SIGN_AUTH();
//                        MSIGNAUTH.M_AUTONO = VE.M_SIGN_AUTH.M_AUTONO;
//                        MSIGNAUTH.AUTHCD = VE.M_SIGN_AUTH.AUTHCD;
//                        MSIGNAUTH.AUTHNM = VE.M_SIGN_AUTH.AUTHNM;
//                        MSIGNAUTH.DESIGCD = VE.M_SIGN_AUTH.DESIGCD;
//                        MSIGNAUTH.EMPID = VE.M_SIGN_AUTH.EMPID;
//                        MSIGNAUTH.USRID = FC["usrid"].ToString();
//                        MSIGNAUTH.CLCD = CommVar.ClientCode(UNQSNO);

//                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SIGN_AUTH", MSIGNAUTH.M_AUTONO.Value, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
//                        DB.Entry(MSIGNAUTH).State = System.Data.Entity.EntityState.Modified;
//                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
//                        DB.SaveChanges();
//                        transaction.Commit();
//                        return Content("2");
//                    }
//                    else if (VE.DefaultAction == "V")
//                    {
//                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SIGN_AUTH", VE.M_SIGN_AUTH.M_AUTONO.Value, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
//                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
//                        DB.SaveChanges();
//                        DB.M_SIGN_AUTH.Where(x => x.M_AUTONO == VE.M_SIGN_AUTH.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
//                        DB.M_SIGN_AUTH.RemoveRange(DB.M_SIGN_AUTH.Where(x => x.M_AUTONO == VE.M_SIGN_AUTH.M_AUTONO));
//                        DB.SaveChanges();
//                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SIGN_AUTH.M_AUTONO));
//                        DB.SaveChanges();
//                        transaction.Commit();
//                        return Content("3");
//                    }
//                    else
//                    {
//                        return Content("");
//                    }

//                }
//                catch (Exception ex)
//                {
//                    Cn.SaveException(ex, "");
//                    return Content(ex.Message + " " + ex.InnerException);
//                }
//            }

//        }

//    }
//}