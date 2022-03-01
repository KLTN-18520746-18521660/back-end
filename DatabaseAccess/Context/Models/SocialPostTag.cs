using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace DatabaseAccess.Context.Models
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

        [ForeignKey(nameof(PostId))]
        [InverseProperty(nameof(SocialPost.SocialPostTags))]
        public virtual SocialPost Post { get; set; }
        [ForeignKey(nameof(TagId))]
        [InverseProperty(nameof(SocialTag.SocialPostTags))]
        public virtual SocialTag Tag { get; set; }
    }
}
