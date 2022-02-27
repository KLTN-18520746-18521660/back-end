using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

#nullable disable

namespace DatabaseAccess.tst
{
    [Table("social_comment")]
    public partial class SocialComment
    {
        public SocialComment()
        {
            InverseParent = new HashSet<SocialComment>();
            SocialReports = new HashSet<SocialReport>();
            SocialUserActionWithComments = new HashSet<SocialUserActionWithComment>();
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Column("parent_id")]
        public long? ParentId { get; set; }
        [Column("post_id")]
        public long PostId { get; set; }
        [Column("owner")]
        public Guid Owner { get; set; }
        [Required]
        [Column("content")]
        public string Content { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string Status { get; set; }
        [Column("search_vector")]
        public NpgsqlTsVector SearchVector { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; set; }
        [Column("last_modified_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastModifiedTimestamp { get; set; }

        [ForeignKey(nameof(Owner))]
        [InverseProperty(nameof(SocialUser.SocialComments))]
        public virtual SocialUser OwnerNavigation { get; set; }
        [ForeignKey(nameof(ParentId))]
        [InverseProperty(nameof(SocialComment.InverseParent))]
        public virtual SocialComment Parent { get; set; }
        [ForeignKey(nameof(PostId))]
        [InverseProperty(nameof(SocialPost.SocialComments))]
        public virtual SocialPost Post { get; set; }
        [InverseProperty(nameof(SocialComment.Parent))]
        public virtual ICollection<SocialComment> InverseParent { get; set; }
        [InverseProperty(nameof(SocialReport.Comment))]
        public virtual ICollection<SocialReport> SocialReports { get; set; }
        [InverseProperty(nameof(SocialUserActionWithComment.Comment))]
        public virtual ICollection<SocialUserActionWithComment> SocialUserActionWithComments { get; set; }
    }
}
