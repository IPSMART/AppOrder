using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class RetailOutletEntry : Permission
    {
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public bool Checked { get; set; }
        public string DSTBRGSTNO { get; set; }
        public string DSTBRAREA { get; set; }
        public M_RETAIL M_RETAIL { get; set; }
        public MS_STATE MS_STATE { get; set; }
        public List<MRETAILLINK> MRETAILLINK { get; set; }
        public string STATENM { get; set; }
        public M_RETAIL_LINK M_RETAIL_LINK { get; set; }
        public List<ListState> ListState { get; set; }
        public List<ListCountry> ListCountry { get; set; }
        public bool IsAPIEnabled { get; set; }
        public List<DropDown_list_Distributor> DropDown_list_Distributor { get; set; }
        public string Dstbrslcd { get; set; }
        public string Dstbrslnm { get; set; }
        public List<ListDistributor> ListDistributor { get; set; }


    }
    public class ListState
        {
            public string text { get; set; }
            public string value { get; set; }
        }
    public class ListCountry
    {
        public string text { get; set; }
        public string value { get; set; }
    }


}