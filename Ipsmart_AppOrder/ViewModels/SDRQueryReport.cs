using System.Collections.Generic;
using Improvar.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class SDRQueryReport : Permission
    {
        public string LOGINAS { get; set; }
        public string USERQRY { get; set; }
        public string USERTYPE { get; set; }
        public string QUERYID { get; set; }
        public string QUERYNM { get; set; }
        public string REPHEADING { get; set; }
        public string REPDESIGN { get; set; }
        public string FILENAME { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string DATE1 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string DATE2 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string DATE3 { get; set; }
        [StringLength(100)]
        public string TEXT1 { get; set; }
        [StringLength(100)]
        public string TEXT2 { get; set; }
        [StringLength(100)]
        public string TEXT3 { get; set; }
        [StringLength(100)]
        public string TEXT4 { get; set; }
        public List<DropDown_list_SLCD> DropDown_list_SLCD { get; set; }
        public List<DropDown_list_GLCD> DropDown_list_GLCD { get; set; }
        public string OUTPUT { get; set; }
        public string DELEMETER { get; set; }
        public string VARBLUSED { get; set; }
        public string QUERYVAL { get; set; }
        public string[,] FIXVAR { get; set; }
        public M_SDR_QUERY  M_SDR_QUERY { get; set; }        
     public   List<SDR_USER> SDR_USER { get; set; }
    }
    public class SDR_USER
    {
        public bool Checked { get; set; }
        public int SLNO { get; set; }
        public string USERID { get; set; }
        public string USERNAME { get; set; }
    }
}