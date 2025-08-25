using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class VMRetailOrder : Permission
    {
        public List<ImageView> ImageView { get; set; }
        public bool Checked { get; set; }
        public string RetailerCode { get; set; }
        public string RetailerName { get; set; }
        public string RetailerPin { get; set; }
        public string RetailerGstno { get; set; }
        public string RetailerCity { get; set; }

        public string Dstbrslcd { get; set; }
        public string Dstbrslnm { get; set; }
        public string SelectedRetailerCode { get; set; }
        public string SelectedRetailerName { get; set; }
        public string SelectedRetailerPin { get; set; }
        public string SelectedRetailerGstno { get; set; }
        public string SelectedRetailerCity { get; set; }

        public string GEOLONGITUDE { get; set; }
        public string GEOLATITUDE { get; set; }
        //public string GEOADDRESS { get; set; }
        public string ITEMDETAIL_JSTR { get; set; }
        public string[] BrandCode { get; set; }
        public string BrandName { get; set; }
        public string[] GroupCode { get; set; }
        public string GroupName { get; set; }
        public string[] CollCode { get; set; }
        public string CollName { get; set; }
        public List<ListDistributor> ListDistributor { get; set; }
        public List<ListRetailer> ListRetailer { get; set; }
        public List<ListBrand> ListBrand { get; set; }
        public List<ListGroup> ListGroup { get; set; }
        public List<ListCollection> ListCollection { get; set; }
        [StringLength(8)]
        public string SLMSLCD { get; set; }

    }

    public class ImageView
    {
        public string Url { get; set; }
        public string Desc { get; set; }
        public string ITCD { get; set; }
        public string SIZES { get; set; }
        public short PCSPERSET { get; set; }
        public string MIXSIZE { get; set; }
        public double SIZE_COUNT { get; set; }
        public short PCSPERBOX { get; set; }
        [StringLength(50)]
        public string ITREM { get; set; }
    }

    public class APP_ITEMLIST
    {
        public string itcd { get; set; }
        public string sizes { get; set; }
        public string itrem { get; set; }
    }
    public class ListDistributor
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class ListRetailer
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class ListBrand
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class ListGroup
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class ListCollection
    {
        public string text { get; set; }
        public string value { get; set; }
    }
}