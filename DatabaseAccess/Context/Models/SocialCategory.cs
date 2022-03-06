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
                Slug = Common.Utils.GenerateSlug(value);
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
        public int Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => BaseStatus.StatusToString(Status, EntityStatus.SocialCategoryStatus);
            set => Status = BaseStatus.StatusFromString(value,  EntityStatus.SocialCategoryStatus);
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
        public virtual List<SocialCategory> InverseParent { get; set; }
        [InverseProperty(nameof(SocialPostCategory.Category))]
        public virtual List<SocialPostCategory> SocialPostCategories { get; set; }
        [InverseProperty(nameof(SocialUserActionWithCategory.Category))]
        public virtual List<SocialUserActionWithCategory> SocialUserActionWithCategories { get; set; }

        public SocialCategory()
        {
            InverseParent = new List<SocialCategory>();
            SocialPostCategories = new List<SocialPostCategory>();
            SocialUserActionWithCategories = new List<SocialUserActionWithCategory>();

            __ModelName = "SocialCategory";
            Status = SocialCategoryStatus.Enabled;
            CreatedTimestamp = DateTime.UtcNow;
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "";
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

        public static List<SocialCategory> GetDefaultData()
        {
            List<SocialCategory> ListData = new()
            {
                new SocialCategory
                {
                    Id = 1,
                    ParentId = null,
                    Name = "technology",
                    DisplayName = "Technology",
                    Describe = "This not a bug this a feature",
                    Slug = "technology",
                    Thumbnail = null,
                    CreatedTimestamp = DateTime.UtcNow,
                    Status = SocialCategoryStatus.Readonly
                },
                new SocialCategory
                {
                    Id = 2,
                    ParentId = null,
                    Name = "developer",
                    DisplayName = "Developer",
                    Describe = "Do not click to this",
                    Slug = "developer",
                    Thumbnail = null,
                    CreatedTimestamp = DateTime.UtcNow,
                    Status = SocialCategoryStatus.Readonly
                },
                new SocialCategory
                {
                    Id = 3,
                    ParentId = null,
                    Name = "dicussion",
                    DisplayName = "Dicussion",
                    Describe = "Search google to have better solution",
                    Slug = "dicussion",
                    Thumbnail = null,
                    CreatedTimestamp = DateTime.UtcNow,
                    Status = SocialCategoryStatus.Readonly
                },
                new SocialCategory
                {
                    Id = 4,
                    ParentId = null,
                    Name = "blog",
                    DisplayName = "Blog",
                    Describe = "Nothing in here",
                    Slug = "blog",
                    Thumbnail = null,
                    CreatedTimestamp = DateTime.UtcNow,
                    Status = SocialCategoryStatus.Readonly
                },
                new SocialCategory
                {
                    Id = 5,
                    ParentId = null,
                    Name = "left",
                    DisplayName = "Left",
                    Describe = "Life die have number",
                    Slug = "left",
                    Thumbnail = null,
                    CreatedTimestamp = DateTime.UtcNow,
                    Status = SocialCategoryStatus.Readonly
                }
            };
            return ListData;
        }
    }
}
