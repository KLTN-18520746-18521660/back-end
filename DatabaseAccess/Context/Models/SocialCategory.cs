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
using Common;
using Newtonsoft.Json.Linq;
using System.Linq;
using DatabaseAccess.Common.Actions;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_category")]
    public class SocialCategory : BaseModel
    {
        [Key]
        [Column("id")]
        public long Id { get; private set; }
        [Column("parent_id")]
        public long? ParentId { get; set; }
        [NotMapped]
        private string _name;
        [Required]
        [Column("name")]
        [StringLength(20)]
        public string Name {
            get => _name;
            set {
                _name = value;
                Slug = Utils.GenerateSlug(value);
            }
        }
        [Required]
        [Column("display_name")]
        [StringLength(50)]
        public string DisplayName { get; set; }
        [Required]
        [Column("describe")]
        [StringLength(300)]
        public string Describe { get; set; }
        [Required]
        [Column("slug")]
        public string Slug { get; set; }
        [Column("thumbnail")]
        public string Thumbnail { get; set; }
        [NotMapped]
        public EntityStatus Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => Status.ToString();
            set => Status = new EntityStatus(EntityStatusType.SocialCategory, value);
        }
        [Column("search_vector")]
        public NpgsqlTsVector SearchVector { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; private set; }
        [Column("last_modified_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastModifiedTimestamp { get; set; }

        [ForeignKey(nameof(ParentId))]
        [InverseProperty(nameof(SocialCategory.InverseParent))]
        public virtual SocialCategory Parent { get; set; }
        [InverseProperty(nameof(SocialCategory.Parent))]
        public virtual ICollection<SocialCategory> InverseParent { get; set; }
        [InverseProperty(nameof(SocialPostCategory.Category))]
        public virtual ICollection<SocialPostCategory> SocialPostCategories { get; set; }
        [InverseProperty(nameof(SocialUserActionWithCategory.Category))]
        public virtual ICollection<SocialUserActionWithCategory> SocialUserActionWithCategories { get; set; }

        public SocialCategory()
        {
            InverseParent = new HashSet<SocialCategory>();
            SocialPostCategories = new HashSet<SocialPostCategory>();
            SocialUserActionWithCategories = new HashSet<SocialUserActionWithCategory>();

            __ModelName = "SocialCategory";
            Status = new EntityStatus(EntityStatusType.SocialCategory, StatusType.Enabled);
            CreatedTimestamp = DateTime.UtcNow;
        }

        public int CountPosts(bool isOwner = true)
        {
            return SocialPostCategories.Count(e => e.Post.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted));
        }

        public int CountViews()
        {
            return SocialPostCategories
                .Where(e => e.Post.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted))
                .Sum(e => e.Post.Views);
        }

        public int CountFollow()
        {
            return SocialUserActionWithCategories.Count(e =>
                // e.User.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted) &&
                e.Actions.Count(a => a.action == EntityAction.ActionTypeToString(ActionType.Follow)) > 0
            );
        }

        public int CountVisited()
        {
            return SocialUserActionWithCategories.Count(e => 
                e.Actions.Count(a => a.action == EntityAction.ActionTypeToString(ActionType.Visited)) > 0
            );
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = string.Empty;
            try {
                var parser = (ParserModels.ParserSocialCategory)Parser;
                ParentId = parser.parent_id;
                Name = parser.name;
                DisplayName = parser.display_name;
                Describe = parser.describe;
                Thumbnail = parser.thumbnail;

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
                { "id",                         Id },
                { "parent_id",                  ParentId },
                { "name",                       Name },
                { "display_name",               DisplayName },
                { "describe",                   Describe },
                { "slug",                       Slug },
                { "thumbnail",                  Thumbnail },
                { "status",                     StatusStr },
                { "posts",                      CountPosts() },
                { "views",                      CountViews() },
                { "follows",                    CountFollow() },
                { "visited",                    CountVisited() },
                { "last_modified_timestamp",    LastModifiedTimestamp },
                { "created_timestamp",          CreatedTimestamp },
#if DEBUG
                { "__ModelName", __ModelName }
#endif
            };
            return true;
        }

        public override JObject GetPublicJsonObject(List<string> publicFields = default) {
            if (publicFields == default) {
                publicFields = new List<string>(){
                    "id",
                    "parent_id",
                    "name",
                    "display_name",
                    "describe",
                    "slug",
                    "thumbnail",
                    "posts",
                    "views",
                    "follows",
                    "visited",
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

        public string[] GetActionByUser(Guid socialUserId) {
            var action = this.SocialUserActionWithCategories
                .Where(e => e.UserId == socialUserId)
                .FirstOrDefault();
            return action != default ? action.Actions.Select(e => e.action).ToArray() : new string[]{};
        }

        class SocialCategorySeed
        {
            public long id              { get; set; }
            public long? parent_id      { get; set; }
            public string name          { get; set; }
            public string display_name  { get; set; }
            public string slug          { get; set; }
            public string thumbnail     { get; set; }
            public string describe      { get; set; }
        }
        public static List<SocialCategory> GetDefaultData()
        {
            var ListData = new List<SocialCategory>();
            var (ListDataSeed, ErrMsg) = Utils.LoadListJsonFromFile<SocialCategorySeed>(DataSeed.DATA_PATH.SOCIAL_CATEGORY);
            if (ListDataSeed == default) {
#if DEBUG
                return ListData;
#else
                ListDataSeed = new();
#endif
            }
            ListDataSeed.ForEach(e => {
                ListData.Add(new SocialCategory
                {
                    Id                  = e.id,
                    ParentId            = e.parent_id,
                    Name                = e.name,
                    DisplayName         = e.display_name,
                    Slug                = e.slug,
                    Thumbnail           = e.thumbnail,
                    Describe            = e.describe,
                    CreatedTimestamp    = DBCommon.DEFAULT_DATETIME_FOR_DATA_SEED,
                    Status              = new EntityStatus(EntityStatusType.SocialTag, StatusType.Readonly),
                });
            });
            return ListData;
        }
    }
}
