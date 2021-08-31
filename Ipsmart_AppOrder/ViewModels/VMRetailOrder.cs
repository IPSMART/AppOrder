using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class VMRetailOrder
    {
        public List<ImageView> ImageView { get; set; }
        public bool Checked { get; set; }
        public string RetailerCode { get; set; }
        public string RetailerName { get; set; }
        public string RetailerPin { get; set; }
        public string RetailerGstno { get; set; }
        public string RetailerCity { get; set; }

        public string SelectedRetailerCode { get; set; }
        public string SelectedRetailerName { get; set; }
        public string SelectedRetailerPin { get; set; }
        public string SelectedRetailerGstno { get; set; }
        public string SelectedRetailerCity { get; set; }

        public string GEOLONGITUDE { get; set; }
        public string GEOLATITUDE { get; set; }
        //public string GEOADDRESS { get; set; }
        public string ITEMDETAIL_JSTR { get; set; }

    }

    public class ImageView
    {
        public string Url { get; set; }
        public string Desc { get; set; }
        public string ITCD { get; set; }
        public string SIZES { get; set; }
    }

    public class APP_ITEMLIST
    {
        public string itcd { get; set; }
        public string sizes { get; set; }
    }
}