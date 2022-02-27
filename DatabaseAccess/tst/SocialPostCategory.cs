using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace DatabaseAccess.tst
{
    [Table("social_post_category")]
    public partial class SocialPostCategory
    {
        [Key]
        [Column("post_id")]
        public long PostId { get; set; }
        [Key]
        [Column("category_id")]
        public long CategoryId { get; set; }
    }
}
