using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Newtonsoft.Json;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Interface;
using DatabaseAccess.Common.Status;


#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_comment")]
    public class SocialComment : BaseModel
    {
        [Key]
        [Column("id")]
        public long Id { get; private set; }
        [Column("parent_id")]
        public long? ParentId { get; set; }
        [Column("post_id")]
        public long PostId { get; set; }
        [Column("owner")]
        public Guid Owner { get; set; }
        [Required]
        [Column("content")]
        public string Content { get; set; }
        [NotMapped]
        public int Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => BaseStatus.StatusToString(Status, EntityStatus.SocialCommentStatus);
            set => Status = BaseStatus.StatusFromString(value, EntityStatus.SocialCommentStatus);
        }
        [Column("search_vector")]
        public NpgsqlTsVector SearchVector { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; private set; }
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
        
        public SocialComment()
        {
            InverseParent = new HashSet<SocialComment>();
            SocialReports = new HashSet<SocialReport>();
            SocialUserActionWithComments = new HashSet<SocialUserActionWithComment>();

            __ModelName = "SocialComment";
            CreatedTimestamp = DateTime.UtcNow;
            Status = SocialCommentStatus.Created;
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = string.Empty;
            try {
                var parser = (ParserModels.ParserSocialComment)Parser;
                ParentId = parser.parent_id;
                PostId = parser.post_id;
                Owner = parser.owner;
                
                return true;
            } catch (Exception ex) {
                Error = ex.ToString();
                return false;
            }
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>()
            {
                { "id", Id },
                { "parent_id", ParentId },
                { "post_id", PostId },
                { "owner", Owner },
                { "content", Content },
                { "status", StatusStr },
                { "last_modified_timestamp", LastModifiedTimestamp},
                { "created_timestamp", CreatedTimestamp},
#if DEBUG
                { "__ModelName", __ModelName }
#endif
            };
            return true;
        }
    }
}
