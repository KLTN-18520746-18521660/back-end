using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Interface;
using DatabaseAccess.Common.Status;
using Common;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    public enum CONTENT_TYPE {
        INVALID = 0,
        HTML = 1,
        MARKDOWN = 2,
    }
    [Table("social_post")]
    public class SocialPost : BaseModel
    {
        [Key]
        [Column("id")]
        public long Id { get; private set; }
        [Column("owner")]
        public Guid Owner { get; set; }
        [NotMapped]
        private string _title;
        [Required]
        [Column("title")]
        public string Title { 
            get => _title; 
            set {
                _title = value;
                Slug = Utils.GenerateSlug(value);
            }
        }
        [Required]
        [Column("slug")]
        public string Slug { get; set; }
        [Required]
        [Column("thumbnail")]
        public string Thumbnail { get; set; }
        [Required]
        [Column("views")]
        public int Views { get; set; }
        [Required]
        [Column("time_read")]
        public int TimeRead { get; set; }
        [NotMapped]
        public int Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => BaseStatus.StatusToString(Status, EntityStatus.SocialPostStatus);
            set => Status = BaseStatus.StatusFromString(value, EntityStatus.SocialPostStatus);
        }
        [Required]
        [Column("content_search")]
        public string ContentSearch { get; private set; }
        [NotMapped]
        private string _content;
        [Required]
        [Column("content")]
        public string Content {
            get => _content;
            set {
                _content = value;
                ContentSearch = Utils.TakeContentForSearchFromRawContent(value);
                if (ShortContent == default || ShortContent == string.Empty) {
                    ShortContent = Utils.TakeShortContentFromContentSearch(ContentSearch);
                }
            }
        }
        [Required]
        [Column("short_content")]
        public string ShortContent { get; set; }
        [NotMapped]
        public CONTENT_TYPE ContentType;
        [Required]
        [Column("content_type")]
        [StringLength(15)]
        public string ContenTypeStr {
            get => ContentTypeToString(ContentType);
            set => ContentType = StringToContentType(value);
        }
        [Column("search_vector")]
        public NpgsqlTsVector SearchVector { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; set; }
        [Column("last_modified_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastModifiedTimestamp { get; set; }

        [ForeignKey(nameof(Owner))]
        [InverseProperty(nameof(SocialUser.SocialPosts))]
        public virtual SocialUser OwnerNavigation { get; set; }
        [InverseProperty(nameof(SocialComment.Post))]
        public virtual ICollection<SocialComment> SocialComments { get; set; }
        [InverseProperty(nameof(SocialPostCategory.Post))]
        public virtual ICollection<SocialPostCategory> SocialPostCategories { get; set; }
        [InverseProperty(nameof(SocialPostTag.Post))]
        public virtual ICollection<SocialPostTag> SocialPostTags { get; set; }
        [InverseProperty(nameof(SocialReport.Post))]
        public virtual ICollection<SocialReport> SocialReports { get; set; }
        [InverseProperty(nameof(SocialUserActionWithPost.Post))]
        public virtual ICollection<SocialUserActionWithPost> SocialUserActionWithPosts { get; set; }
        
        public SocialPost()
        {
            SocialComments = new HashSet<SocialComment>();
            SocialReports = new HashSet<SocialReport>();
            SocialPostCategories = new HashSet<SocialPostCategory>();
            SocialPostTags = new HashSet<SocialPostTag>();
            SocialUserActionWithPosts = new HashSet<SocialUserActionWithPost>();

            CreatedTimestamp = DateTime.UtcNow;
            Status = SocialPostStatus.Pending;
        }
        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "";
            try {
                var parser = (ParserModels.ParserSocialPost)Parser;
                Title = parser.title;
                Thumbnail = parser.thumbnail;
                Content = parser.content;
                ContenTypeStr = parser.content_type;

                return true;
            } catch (Exception ex) {
                Error = ex.ToString();
                return false;
            }
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>
            {
                { "id", Id },
                { "owner", Owner },
                { "title", Title },
                { "slug", Slug },
                { "thumbnail", Thumbnail },
                { "views", Views },
                // { "comments", SocialComments.Count<SocialComment>(p => p.Status != SocialCommentStatus.Deleted) },
                // { "likes", SocialUserActionWithPosts.Count<SocialUserActionWithPost>(p => p.Actions.li) },
                { "content", Content },
                { "content_type", ContenTypeStr },
                { "status", StatusStr },
                { "created_timestamp", CreatedTimestamp },
                { "last_modified_timestamp", LastModifiedTimestamp },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }
        public static CONTENT_TYPE StringToContentType(string ContentType) {
            switch (ContentType) {
                case "HTML":
                    return CONTENT_TYPE.HTML;
                case "MARKDOWN":
                    return CONTENT_TYPE.MARKDOWN;
                default:
                    return CONTENT_TYPE.INVALID;
            }
        }
        public static string ContentTypeToString(CONTENT_TYPE ContentType) {
            switch (ContentType) {
                case CONTENT_TYPE.HTML:
                    return "HTML";
                case CONTENT_TYPE.MARKDOWN:
                    return "markdown";
                default:
                    return "Invalid content type.";
            }
        }
    }
}
