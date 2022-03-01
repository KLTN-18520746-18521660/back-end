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
    [Table("social_tag")]
    public class SocialTag : BaseModel
    {
        [Key]
        [Column("id")]
        public long Id { get; private set; }
        [Required]
        [Column("tag")]
        [StringLength(20)]
        public string Tag { get; private set; }
        [Required]
        [Column("describe")]
        [StringLength(100)]
        public string Describe { get; set; }
        [NotMapped]
        public int Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => BaseStatus.StatusToString(Status, EntityStatus.SocialTagStatus);
            set => Status = BaseStatus.StatusFromString(value, EntityStatus.SocialTagStatus);
        }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; private set; }
        [Column("last_modified_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastModifiedTimestamp { get; set; }
        [InverseProperty(nameof(SocialPostTag.Tag))]
        public virtual ICollection<SocialPostTag> SocialPostTags { get; set; }

        [InverseProperty(nameof(SocialUserActionWithTag.Tag))]
        public virtual ICollection<SocialUserActionWithTag> SocialUserActionWithTags { get; set; }

        
        public SocialTag()
        {
            SocialPostTags = new HashSet<SocialPostTag>();
            SocialUserActionWithTags = new HashSet<SocialUserActionWithTag>();

            __ModelName = "SocialTag";
            CreatedTimestamp = DateTime.UtcNow;
            Status = SocialTagStatus.Enabled;
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "";
            try {
                var parser = (ParserModels.ParserSocialTag)Parser;
                Tag = parser.tag;
                Describe = parser.describe;

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
                { "tag", Tag },
                { "describe", Describe },
                { "status", StatusStr },
                { "created_timestamp", CreatedTimestamp},
                { "last_modified_timestamp", LastModifiedTimestamp},
#if DEBUG
                { "__ModelName", __ModelName }
#endif
            };
            return true;
        }
        
        public static List<SocialTag> GetDefaultData()
        {
            List<SocialTag> ListData = new()
            {
                new SocialTag
                {
                    Id = 1,
                    Tag = "#angular",
                    Describe = "Angular",
                    CreatedTimestamp = DateTime.UtcNow,
                    Status = SocialCategoryStatus.Readonly
                },
                new SocialTag
                {
                    Id = 2,
                    Tag = "#life-die-have-number",
                    Describe = "Something is not thing",
                    CreatedTimestamp = DateTime.UtcNow,
                    Status = SocialCategoryStatus.Readonly
                },
                new SocialTag
                {
                    Id = 3,
                    Tag = "#develop",
                    Describe = "Dot not choose this tag",
                    CreatedTimestamp = DateTime.UtcNow,
                    Status = SocialCategoryStatus.Readonly
                },
                new SocialTag
                {
                    Id = 4,
                    Tag = "#nothing",
                    Describe = "Nothing in here",
                    CreatedTimestamp = DateTime.UtcNow,
                    Status = SocialCategoryStatus.Readonly
                },
                new SocialTag
                {
                    Id = 5,
                    Tag = "#hihi",
                    Describe = "hi hi",
                    CreatedTimestamp = DateTime.UtcNow,
                    Status = SocialCategoryStatus.Readonly
                }
            };
            return ListData;
        }
    }
}
