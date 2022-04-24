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
using DatabaseAccess.Common.Actions;

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
                if (Status == SocialPostStatus.Approved || Status == SocialPostStatus.Private) {
                    Slug = Utils.GenerateSlug(value, true);
                } else {
                    Slug = string.Empty;
                }
            }
        }
        [Required]
        [Column("slug")]
        public string Slug { get; set; } // only != empty when status is approve or private
        [Required]
        [Column("thumbnail")]
        public string Thumbnail { get; set; }
        [Required]
        [Column("views")]
        public int Views { get; set; }
        [NotMapped]
        public int Comments { get =>
            SocialComments.Count();
        }
        [NotMapped]
        public object[] Tags { get =>
            SocialPostTags
                .Where(e => e.Tag.StatusStr != BaseStatus.StatusToString(SocialTagStatus.Disabled, EntityStatus.SocialTagStatus))
                .Select(e => new {
                    tag = e.Tag.Tag,
                    name = e.Tag.Name
                }).ToArray();
        }
        [NotMapped]
        public object[] Categories { get =>
            SocialPostCategories
                .Where(e => 
                    e.Category.StatusStr != BaseStatus.StatusToString(SocialCategoryStatus.Disabled, EntityStatus.SocialCategoryStatus)
                )
                .Select(e => new {
                    name = e.Category.Name,
                    display_name = e.Category.DisplayName,
                    slug = e.Category.Slug
                }).ToArray();
        }
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
            set {
                Status = BaseStatus.StatusFromString(value, EntityStatus.SocialPostStatus);
                if ((Status == SocialPostStatus.Approved || Status == SocialPostStatus.Private) && Slug == string.Empty) {
                    Slug = Utils.GenerateSlug(this.Title, true);
                } else if ((Status != SocialPostStatus.Approved && Status != SocialPostStatus.Private) && Slug != string.Empty) {
                    Slug = string.Empty;
                }
            }
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

            __ModelName = "SocialPost";
            CreatedTimestamp = DateTime.UtcNow;
            Status = SocialPostStatus.Pending;
        }
        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = string.Empty;
            try {
                var parser = (ParserModels.ParserSocialPost)Parser;
                Title = parser.title;
                Thumbnail = parser.thumbnail;
                Content = parser.content;
                ContenTypeStr = parser.content_type;

                if (parser.short_content != default) {
                    ShortContent = parser.short_content;
                }
                if (parser.time_read != default) {
                    TimeRead = parser.time_read;
                }

                return true;
            } catch (Exception ex) {
                Error = ex.ToString();
                return false;
            }
        }

        public int CountLikes()
        {
            return SocialUserActionWithPosts.Count(p =>
                p.Actions.Count(a => a.action == EntityAction.ActionTypeToString(ActionType.Like)) > 0
            );
        }

        public int CountDisLikes()
        {
            return SocialUserActionWithPosts.Count(p =>
                p.Actions.Count(a => a.action == EntityAction.ActionTypeToString(ActionType.Like)) > 0
            );
        }

        public int CountVisited()
        {
            return SocialUserActionWithPosts.Count(p =>
                p.Actions.Count(a => a.action == EntityAction.ActionTypeToString(ActionType.Visited)) > 0
            );
        }

        public JObject GetPublicStatisticJsonObject(Guid SocialUserId = default)
        {
            var ret = new Dictionary<string, object>
            {
                { "views", Views },
                { "likes", CountLikes() },
                { "dislikes", CountDisLikes() },
                { "comments", Comments },
            };
            if (this.Owner == SocialUserId) {
                ret.Add("id", this.Id);
            }
            return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(ret));
        }

        public JObject GetPublicShortJsonObject(Guid SocialUserId = default)
        {
            var ret = new Dictionary<string, object>
            {
                {
                    "owner",
                    new JObject(){
                        { "user_name", this.OwnerNavigation.UserName },
                        { "display_name", this.OwnerNavigation.DisplayName },
                        { "avatar", this.OwnerNavigation.Avatar },
                        { "status", this.OwnerNavigation.StatusStr },
                    }
                },
                { "title", Title },
                { "slug", Slug },
                { "thumbnail", Thumbnail },
                { "time_read", TimeRead },
                { "views", Views },
                { "likes", CountLikes() },
                { "dislikes", CountDisLikes() },
                { "comments", Comments },
                { "tags", Tags },
                { "categories", Categories },
                { "short_content", ShortContent },
                { "status", StatusStr },
                { "created_timestamp", CreatedTimestamp },
                { "last_modified_timestamp", LastModifiedTimestamp },
            };
            if (this.Owner == SocialUserId) {
                ret.Add("id", this.Id);
            }
            return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(ret));
        }

        public override JObject GetPublicJsonObject(List<string> publicFields = null)
        {
            if (publicFields == default) {
                publicFields = new List<string>() {
                    "owner",
                    "title",
                    "slug",
                    "thumbnail",
                    "time_read",
                    "views",
                    "likes",
                    "dislikes",
                    "comments",
                    "tags",
                    "categories",
                    "content",
                    "thumbnail",
                    "content",
                    "content_type",
                    "short_content",
                    "status",
                    "created_timestamp",
                    "last_modified_timestamp",
                };
            }
            var ret = GetJsonObject();
            foreach (var x in __ObjectJson) {
                if (!publicFields.Contains(x.Key)) {
                    ret.Remove(x.Key);
                }
            }
            return ret;
        }
        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>
            {
                { "id", Id },
                {
                    "owner",
                    new JObject(){
                        { "user_name", this.OwnerNavigation.UserName },
                        { "display_name", this.OwnerNavigation.DisplayName },
                        { "avatar", this.OwnerNavigation.Avatar },
                        { "status", this.OwnerNavigation.StatusStr },
                    }
                },
                { "title", Title },
                { "slug", Slug },
                { "thumbnail", Thumbnail },
                { "time_read", TimeRead },
                { "views", Views },
                { "likes", CountLikes() },
                { "dislikes", CountDisLikes() },
                { "comments", Comments },
                { "tags", Tags },
                { "categories", Categories },
                { "visited_count", CountVisited() },
                { "content", Content },
                { "content_type", ContenTypeStr },
                { "short_content", ShortContent },
                { "status", StatusStr },
                { "created_timestamp", CreatedTimestamp },
                { "last_modified_timestamp", LastModifiedTimestamp },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }
        public override JObject GetJsonObjectForLog() {
            var ret = base.GetJsonObjectForLog();
            var removeFields = new List<string>(){"views", "likes", "dislikes", "comments", "owner"};
            removeFields.ForEach(r => ret.Remove(r));
            return ret;
        }

        public string[] GetActionWithUser(Guid socialUserId) {
            var action = this.SocialUserActionWithPosts
                .Where(e => e.UserId == socialUserId)
                .FirstOrDefault();
            return action != default ? action.Actions.Select(e => e.action).ToArray() : new string[]{};
        }

        #region static func/params
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
                    return "MARKDOWN";
                default:
                    return "Invalid content type.";
            }
        }
        #endregion
    }
}
