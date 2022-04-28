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
        [StringLength(100)]
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

        public int CountViews()
        {
            return SocialPostCategories.Sum(e => e.Post.Views);
        }

        public int CountFollow()
        {
            return SocialUserActionWithCategories.Count(e =>
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
                { "id", Id },
                { "parent_id", ParentId},
                { "name", Name},
                { "display_name", DisplayName},
                { "describe", Describe},
                { "slug", Slug },
                { "thumbnail", Thumbnail},
                { "status", StatusStr},
                { "last_modified_timestamp", LastModifiedTimestamp},
                { "created_timestamp", CreatedTimestamp},
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

        public string[] GetActionWithUser(Guid socialUserId) {
            var action = this.SocialUserActionWithCategories
                .Where(e => e.UserId == socialUserId)
                .FirstOrDefault();
            return action != default ? action.Actions.Select(e => e.action).ToArray() : new string[]{};
        }

        public static List<SocialCategory> GetDefaultData()
        {
            List<SocialCategory> ListData = new()
            {
                new SocialCategory
                {
                    Id = 1,
                    ParentId = default,
                    Name = "technology",
                    DisplayName = "Technology",
                    Describe = "This not a bug this a feature",
                    Slug = "technology",
                    Thumbnail = default,
                    CreatedTimestamp = DBCommon.DEFAULT_DATETIME_FOR_DATA_SEED,
                    Status = new EntityStatus(EntityStatusType.SocialCategory, StatusType.Readonly)
                },
                new SocialCategory
                {
                    Id = 2,
                    ParentId = default,
                    Name = "developer",
                    DisplayName = "Developer",
                    Describe = "Do not click to this",
                    Slug = "developer",
                    Thumbnail = default,
                    CreatedTimestamp = DBCommon.DEFAULT_DATETIME_FOR_DATA_SEED,
                    Status = new EntityStatus(EntityStatusType.SocialCategory, StatusType.Readonly)
                },
                new SocialCategory
                {
                    Id = 3,
                    ParentId = default,
                    Name = "dicussion",
                    DisplayName = "Dicussion",
                    Describe = "Search google to have better solution",
                    Slug = "dicussion",
                    Thumbnail = default,
                    CreatedTimestamp = DBCommon.DEFAULT_DATETIME_FOR_DATA_SEED,
                    Status = new EntityStatus(EntityStatusType.SocialCategory, StatusType.Readonly)
                },
                new SocialCategory
                {
                    Id = 4,
                    ParentId = default,
                    Name = "blog",
                    DisplayName = "Blog",
                    Describe = "Nothing in here",
                    Slug = "blog",
                    Thumbnail = default,
                    CreatedTimestamp = DBCommon.DEFAULT_DATETIME_FOR_DATA_SEED,
                    Status = new EntityStatus(EntityStatusType.SocialCategory, StatusType.Readonly)
                },
                new SocialCategory
                {
                    Id = 5,
                    ParentId = default,
                    Name = "left",
                    DisplayName = "Left",
                    Describe = "Life die have number",
                    Slug = "left",
                    Thumbnail = default,
                    CreatedTimestamp = DBCommon.DEFAULT_DATETIME_FOR_DATA_SEED,
                    Status = new EntityStatus(EntityStatusType.SocialCategory, StatusType.Readonly)
                }
            };
            return ListData;
        }
    }
}
