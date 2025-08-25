namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_RETAIL")]
    public partial class M_RETAIL
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
        [StringLength(8)]
        public string RTLCD { get; set; }

        [Required]
        [StringLength(60)]
        public string RTLNM { get; set; }

        [StringLength(15)]
        public string GSTNO { get; set; }

        [StringLength(10)]
        public string PAN { get; set; }

        [StringLength(60)]
        public string ADD1 { get; set; }

        [StringLength(60)]
        public string ADD2 { get; set; }

        [StringLength(60)]
        public string ADD3 { get; set; }

        [StringLength(60)]
        public string ADD4 { get; set; }

        [StringLength(50)]
        public string CITY { get; set; }

        [StringLength(6)]
        public string PIN { get; set; }

        [Required]
        [StringLength(2)]
        public string STATECD { get; set; }

        [StringLength(50)]
        public string COUNTRY { get; set; }

        [StringLength(2)]
        public string CNCD { get; set; }

        [StringLength(60)]
        public string LANDMARK { get; set; }

        [StringLength(11)]
        public string REGMOBILE { get; set; }

        [StringLength(11)]
        public string REGWHATSAPPNO { get; set; }

        [StringLength(100)]
        public string REGEMAIL { get; set; }

        [StringLength(60)]
        public string CPERSON { get; set; }

        [StringLength(11)]
        public string CMOB1 { get; set; }

        [StringLength(11)]
        public string CMOB2 { get; set; }

        [StringLength(1000)]
        public string REMARKS { get; set; }

        public double? GPSLAT { get; set; }

        public double? GPSLOT { get; set; }

        public long M_AUTONO { get; set; }
        [StringLength(500)]
        public string GPSNM { get; set; }
        [StringLength(8)]
        public string SLMSLCD { get; set; }
    }
}
