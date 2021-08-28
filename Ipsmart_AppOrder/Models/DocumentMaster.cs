using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Improvar.Models

{
    public class DocumentMaster :Permission
    {
        [StringLength(2)]
        [Display(Name = "Doc. Code")]
        public string DOCCD { get; set; }

        [Display(Name = "Doc. Description")]

        [StringLength(35)]
        public string DOCDES { get; set; }

        [Display(Name = "Doc. Prefix")]

        [StringLength(4)]
        public string DOCPREFIX { get; set; }

        [Display(Name = "Doc. Type")]

        public string DOCJRNL { get; set; }

        [Display(Name = "Doc. Numbering")]

        public string DOCTYPE { get; set; }

        [Display(Name = "Doc. Footer")]

        [StringLength(100)]
        public string DOCFTR { get; set; }

        [Display(Name = "Doc.No. Max Length")]
        public byte? DOCNOMAXLENGTH { get; set; }

        [Display(Name = "Doc. No. W/O Zero")]
        public string DOCNOWOZERO { get; set; }

        [Display(Name = "Doc. No. Pattern")]
        public string DOCNOPATTERN { get; set; }

      
    }
}