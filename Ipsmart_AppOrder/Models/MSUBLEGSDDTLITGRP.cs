using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
namespace Improvar.Models
{
    public class MSUBLEGSDDTLITGRP
    {
        public short? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string SLCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string COMPCD { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(4)]
        public string LOCCD { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(4)]
        public string ITGRPCD { get; set; }

        [StringLength(4)]
        public string PRCCD { get; set; }

        [StringLength(4)]
        public string DISCRTCD { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? CRLIMIT { get; set; }

        public short? CRDAYS { get; set; }

        public long M_AUTONO { get; set; }
        public bool Checked { get; set; }
        public short SLNO { get; set; }
        [StringLength(40)]
        public string SLNM { get; set; }
        [StringLength(400)]
        public string ADD1 { get; set; }
        [StringLength(2)]
        public string STATECD { get; set; }
        [StringLength(15)]
        public string GSTNO { get; set; }
        [StringLength(30)]
        public string TAXGRPNM { get; set; }
        [StringLength(4)]
        public string PARTYGRP { get; set; }
        [StringLength(40)]
        public string TRSLNM { get; set; }
        [StringLength(4)]
        public string TAXGRPCD { get; set; }
        [StringLength(8)]
        public string TRSLCD { get; set; }
        [StringLength(20)]
        public string SAPCD { get; set; }
        public string ITGRPNM { get; set; }
        public string DISCRTNM { get; set; }
        public string PRCNM { get; set; }


    }
}