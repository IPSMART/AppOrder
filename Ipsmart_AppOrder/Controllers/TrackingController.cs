using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Improvar.Controllers
{
    public class TrackingController : Controller
    {
        MasterHelp masterHelp = new MasterHelp();
        Connection Cn = new Connection();

        // GET: Tracking
        //public ActionResult Index()
        //{
        //    return View();
        //}
        [HttpPost]
        public ActionResult SaveTracking(string apikey, string USERID, string MODULECD, string SESSIONNO, string LAT, string LNG, string CALLFRM)
        {
            try
            {
                // 🔐 API KEY CHECK
                if (string.IsNullOrEmpty(apikey) || apikey != "123456")
                {
                    return Json(new { status = "UNAUTHORIZED" });
                }

                // 🔎 Basic validation
                if (string.IsNullOrEmpty(USERID) ||
                    string.IsNullOrEmpty(LAT) ||
                    string.IsNullOrEmpty(LNG))
                {
                    return Json(new { status = "INVALID_DATA" });
                }
                decimal lat = 0, lng = 0;
                // 🔢 Validate coordinates
                if (!decimal.TryParse(LAT, out lat) ||
                    !decimal.TryParse(LNG, out lng))
                {
                    return Json(new { status = "INVALID_COORDINATE" });
                }

                // 🌍 Optional: Reject fake 0,0 location
                if (lat == 0 || lng == 0)
                {
                    return Json(new { status = "INVALID_LOCATION" });
                }

                // 💾 Save
                string res = masterHelp.SaveLocation(lat.ToString(), lng.ToString(), "T", USERID, MODULECD, SESSIONNO, CALLFRM);
                string filepath = @"C:/IPSMART/ErrorLog/" + "ERROR LOG " + DateTime.Today.ToString("yyyy-MM-dd") + ".txt";   //Text File Name

                //Cn.SaveTextFile(res, "", filepath);
                return Json(new { status = "OK" });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Json(new { status = "ERROR" });
            }
        }
    }
}