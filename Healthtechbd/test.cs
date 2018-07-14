namespace Healthtechbd
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("wpf_healthtechbd.tests")]
    public partial class test
    {
        public int id { get; set; }

        [Required]
        [StringLength(222)]
        public string name { get; set; }

        public bool status { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime created { get; set; }
    }
}
