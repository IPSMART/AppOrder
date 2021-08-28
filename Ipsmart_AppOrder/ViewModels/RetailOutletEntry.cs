using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class RetailOutletEntry : Permission
    {
        public M_RETAILOUTLET M_RETAILOUTLET { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public bool Checked { get; set; }
    }
}