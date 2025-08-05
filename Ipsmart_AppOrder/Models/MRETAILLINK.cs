using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MRETAILLINK
    {
        public short? EMD_NO { get; set; }        
        public string CLCD { get; set; }        
        public string DTAG { get; set; }        
        public string TTAG { get; set; }
        public string RTLCD { get; set; }       
        public DateTime EFFDT { get; set; }
        public double SLNO { get; set; }        
        public string SLCD { get; set; }
        public string SLNM { get; set; }
        public long M_AUTONO { get; set; }
        public bool Checked { get; set; }
    }
}