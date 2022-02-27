using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace DatabaseAccess.tst
{
    [Table("social_user_right")]
    public partial class SocialUserRight
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("right_name")]
        [StringLength(50)]
        public string RightName { get; set; }
        [Required]
        [Column("display_name")]
        [StringLength(50)]
        public string DisplayName { get; set; }
        [Required]
        [Column("describe")]
        [StringLength(150)]
        public string Describe { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string Status { get; set; }
    }
}
