using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace DatabaseAccess.tst
{
    [Table("social_post_tag")]
    public partial class SocialPostTag
    {
        [Key]
        [Column("post_id")]
        public long PostId { get; set; }
        [Key]
        [Column("tag_id")]
        public long TagId { get; set; }
    }
}
