﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Newtonsoft.Json;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Interface;
using DatabaseAccess.Common.Status;
using Newtonsoft.Json.Linq;
using System.Linq;
using Common;

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
        [StringLength(25)]
        public string Tag { get; set; }
        [Required]
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [Column("describe")]
        [StringLength(300)]
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
            Status = SocialTagStatus.Disabled;
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = string.Empty;
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

        public override JObject GetPublicJsonObject(List<string> publicFields = null)
        {
            if (publicFields == default) {
                publicFields = new List<string>(){
                    "tag",
                    "name",
                    "describe",
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
            __ObjectJson = new Dictionary<string, object>()
            {
                { "id", Id },
                { "tag", Tag },
                { "name", Name },
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

        public string[] GetActionWithUser(Guid socialUserId) {
            var action = this.SocialUserActionWithTags
                .Where(e => e.UserId == socialUserId)
                .FirstOrDefault();
            return action != default ? action.Actions.ToArray() : new string[]{};
        }

        public class SocialTagSeed
        {
            public long id { get; set; }
            public string tag { get; set; }
            public string name { get; set; }
            public string describe { get; set; }
        }
        public static List<SocialTag> GetDefaultData()
        {
            List<SocialTag> ListData = new();
            var (listDataSeed, errMsg) = Utils.LoadListJsonFromFile<SocialTagSeed>(@"..\DatabaseAccess\Context\DataSeed\SocialTag.json");
            if (listDataSeed == default) {
                throw new Exception($"GetDefaultData for SocialTag failed, error: { errMsg }");
            }
            listDataSeed.ForEach(e => {
                ListData.Add(new SocialTag
                {
                    Id = e.id,
                    Tag = e.tag,
                    Name = e.name,
                    Describe = e.describe,
                    CreatedTimestamp = DateTime.UtcNow,
                    Status = SocialCategoryStatus.Readonly
                });
            });
            return ListData;
        }
    }
}
