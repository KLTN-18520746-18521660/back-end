using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

#nullable disable

namespace DatabaseAccess.tst
{
    [Table("social_report")]
    public partial class SocialReport
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Column("post_id")]
        public long? PostId { get; set; }
        [Column("comment_id")]
        public long? CommentId { get; set; }
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

        [ForeignKey(nameof(CommentId))]
        [InverseProperty(nameof(SocialComment.SocialReports))]
        public virtual SocialComment Comment { get; set; }
        [ForeignKey(nameof(PostId))]
        [InverseProperty(nameof(SocialPost.SocialReports))]
        public virtual SocialPost Post { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialReports))]
        public virtual SocialUser User { get; set; }
    }
}
