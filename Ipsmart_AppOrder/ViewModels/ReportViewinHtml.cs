using System.Collections.Generic;
using Improvar.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class ReportViewinHtml : Permission
    {
        public List<DropDown_list> DropDown_list { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }
        public List<DropDown_list3> DropDown_list3 { get; set; }
        public bool Checkbox1 { get; set; }
        public bool Checkbox2 { get; set; }
        public bool Checkbox3 { get; set; }
        public bool Checkbox4 { get; set; }
        public bool Checkbox5 { get; set; }
        public bool Checkbox6 { get; set; }
        public bool Checkbox7 { get; set; }
        public bool Checkbox8 { get; set; }
        public bool Checkbox9 { get; set; }
        public bool Checkbox10 { get; set; }
        public bool Checkbox11 { get; set; }
        public bool Checkbox12 { get; set; }
        public bool Checkbox13 { get; set; }
        public bool Checkbox14 { get; set; }
        public bool Checkbox15 { get; set; }
        public string DOCNO { get; set; }
        public string FDOCNO { get; set; }
        public string TDOCNO { get; set; }
        public string DOCDT { get; set; }
        public string DOCCD { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string FDT { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string TDT { get; set; }
        public string TEXTBOX1 { get; set; }
        public string TEXTBOX2 { get; set; }
        public string TEXTBOX3 { get; set; }
        public string TEXTBOX4 { get; set; }
        public string TEXTBOX5 { get; set; }
        public string TEXTBOX6 { get; set; }
        public string TEXTBOX7 { get; set; }
        public string TEXTBOX8 { get; set; }
        public string Slnm { get; set; }
        public string Compnm { get; set; }
        public string Glnm { get; set; }
        public string Docnm { get; set; }
        public string Itgrpnm { get; set; }
        public string SelAutono { get; set; }
        public string CLASS1CD { get; set; }
        public string CLASS2CD { get; set; }
        public List<DropDown_list_GLCD> DropDown_list_GLCD { get; set; }
        public List<DropDown_list_SLCD> DropDown_list_SLCD { get; set; }
        public List<DropDown_list_AGSLCD> DropDown_list_AGSLCD { get; set; }
        public List<DropDown_list_SLMSLCD> DropDown_list_SLMSLCD { get; set; }
        public List<DropDown_list_TXN> DropDown_list_TXN { get; set; }
        public List<DropDown_list_Class1> DropDown_list_Class1 { get; set; }
        public string SubLeg_Grp { get; set; }
        public List<DropDown_list_SubLegGrp> DropDown_list_SubLegGrp { get; set; }
    }
}