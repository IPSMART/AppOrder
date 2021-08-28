using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace Improvar.Models
{
    public class HELPLIST
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code1 { get; set; }
        public string Name1 { get; set; }
        public string Description1 { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Date { get; set; }
    }
}