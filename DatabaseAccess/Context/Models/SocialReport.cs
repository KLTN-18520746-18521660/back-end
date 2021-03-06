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
    [Table("social_report")]
    public class SocialReport : BaseModel
    {
        [Key]
        [Column("id")]
        public long Id { get; private set; }
        [Column("user_id")]
        public Guid? UserId { get; set; }
        [Column("post_id")]
        public long? PostId { get; set; }
        [Column("comment_id")]
        public long? CommentId { get; set; }
        [Required]
        [Column("type")]
        public string Type { get; set; }
        [Column("report_type")]
        public string ReportType { get; set; }
        [Column("content")]
        public string Content { get; set; }
        [NotMapped]
        public EntityStatus Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => Status.ToString();
            set => Status = new EntityStatus(EntityStatusType.SocialReport, value);
        }
        [Column("reporter_id")]
        public Guid ReporterId { get; set; }
        [Column("search_vector")]
        public NpgsqlTsVector SearchVector { get; private set; }
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

        public SocialReport()
        {
            __ModelName = "SocialReport";
            CreatedTimestamp = DateTime.UtcNow;
            Status = new EntityStatus(EntityStatusType.SocialReport, StatusType.Pending);
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = string.Empty;
            try {
                var parser = (ParserModels.ParserSocialReport)Parser;
                Content = parser.content;
                ReportType = parser.report_type;

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
                { "user_id", UserId },
                { "post_id", PostId },
                { "comment_id", CommentId },
                { "content", Content },
                { "status", StatusStr },
                { "created_timestamp", CreatedTimestamp},
                { "last_modified_timestamp", LastModifiedTimestamp},
#if DEBUG
                { "__ModelName", __ModelName }
#endif
            };
            return true;
        }
    }
}
