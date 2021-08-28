namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SIGN_AUTH")]
    public partial class M_SIGN_AUTH
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
        [StringLength(5)]
        public string AUTHCD { get; set; }

        [StringLength(40)]
        public string AUTHNM { get; set; }
                
        [StringLength(1)]
        public string DEPT { get; set; }

        public long M_AUTONO { get; set; }
    
    }
}
