using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_tag")]
    public partial class SocialTag
    {
        public SocialTag()
        {
            SocialPostTags = new HashSet<SocialPostTag>();
            SocialUserActionWithTags = new HashSet<SocialUserActionWithTag>();
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Required]
        [Column("tag")]
        [StringLength(20)]
        public string Tag { get; set; }
        [Required]
        [Column("describe")]
        [StringLength(100)]
        public string Describe { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string Status { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; set; }
        [Column("last_modified_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastModifiedTimestamp { get; set; }
        [InverseProperty(nameof(SocialPostTag.Tag))]
        public virtual ICollection<SocialPostTag> SocialPostTags { get; set; }

        [InverseProperty(nameof(SocialUserActionWithTag.Tag))]
        public virtual ICollection<SocialUserActionWithTag> SocialUserActionWithTags { get; set; }
    }
}
