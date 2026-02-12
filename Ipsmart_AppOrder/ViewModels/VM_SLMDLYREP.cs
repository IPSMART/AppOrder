using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class VM_SLMDLYREP : Permission
    {
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public T_CNTRL_HDR_REM T_CNTRL_HDR_REM { get; set; }
        public T_SLM_DLYREP_HDR T_SLM_DLYREP_HDR { get; set; }        
        public T_SLM_DLYREP T_SLM_DLYREP { get; set; }
        public string GONM { get; set; }
        public double TOTAL_QNTY { get; set; }
        public double? TOTAL_ADJAMT { get; set; }
        public string CTGTYP { get; set; }
        public string TOLOCNM { get; set; }
        public string TCOMPNM { get; set; }
        public List<TSLMDLYREP> TSLMDLYREP { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public string GEOLONGITUDE { get; set; }
        public string GEOLATITUDE { get; set; }
        public List<DropDown_list> DropDown_list { get; set; }       
        public string DEPTCD { get; set; }
        public string TMPLNAME { get; set; }
        public List<Database_Combo2> Database_Combo2 { get; set; }
        public List<ListDistributor> ListDistributor { get; set; }
        public List<ListBrand> ListBrand { get; set; }
        public List<ListRetailer> ListRetailer { get; set; }
        public string SLMSLCD { get; set; }
        public string REPAUTONO { get; set; }
        public List<DropDown_list_REPAUTONO> DropDown_list_REPAUTONO { get; set; }
        
    }
}