namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SYSCNFG")]
    public partial class M_SYSCNFG
    {
        [Key]
        public bool SLNO { get; set; }

        [StringLength(15)]
        public string PROP1CAP { get; set; }

        [StringLength(3)]
        public string PROP1FLDTYPE { get; set; }

        [StringLength(15)]
        public string PROP2CAP { get; set; }

        [StringLength(3)]
        public string PROP2FLDTYPE { get; set; }

        [StringLength(15)]
        public string PROP3CAP { get; set; }

        [StringLength(3)]
        public string PROP3FLDTYPE { get; set; }

        [StringLength(15)]
        public string PROP4CAP { get; set; }

        [StringLength(3)]
        public string PROP4FLDTYPE { get; set; }
        [StringLength(15)]
        public string PROP5CAP { get; set; }

        [StringLength(3)]
        public string PROP5FLDTYPE { get; set; }
        [StringLength(15)]
        public string PROP6CAP { get; set; }

        [StringLength(3)]
        public string PROP6FLDTYPE { get; set; }
        [StringLength(4)]
        public string PKGTYPE { get; set; }
    }
}
