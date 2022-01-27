
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace DatabaseAccess.Contexts.ConfigDB.Models
{
    [Table("social_user_right")]
    public class SocialUserRight : BaseModel
    {
        public SocialUserRight()
        {
            __ModelName = "SocialUserRight";
            Status = EntityStatus.Enabled;
        }

        public static List<SocialUserRight> GetDefaultData()
        {
            List<SocialUserRight> ListData = new List<SocialUserRight>()
            {
                new SocialUserRight()
                {
                    Id = 1,
                    RightName = "post",
                    DisplayName = "Post",
                    Describe = "Can read, write post.",
                    Status = EntityStatus.Readonly
                },
                new SocialUserRight()
                {
                    Id = 2,
                    RightName = "comment",
                    DisplayName = "Comment",
                    Describe = "Can read, write comment.",
                    Status = EntityStatus.Readonly
                }
            };
            return ListData;
        }

        public override bool Parse(IBaseParserModel Parser, string Error = null)
        {
            Error ??= "";
            try {
                var parser = (ParserModels.ParserSocialUserRight)Parser;
                RightName = parser.display_name;
                DisplayName = parser.display_name;
                Describe = parser.describe;
                return true;
            } catch (Exception ex) {
                Error ??= ex.ToString();
                return false;
            }
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>
            {
                { "id", Id },
                { "right_name", RightName },
                { "display_name", DisplayName },
                { "describe", Describe },
                { "status", Status },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }

        [Column("id", TypeName = "INTEGER")]
        public int Id { get; private set; }

        [Column("right_name", TypeName = "VARCHAR(50)")]
        public string RightName { get; set; }
        
        [Column("display_name", TypeName = "VARCHAR(50)")]
        public string DisplayName { get; set; }

        [Column("describe", TypeName = "VARCHAR(150)")]
        public string Describe { get; set; }

        [NotMapped]
        public int Status { get; set; }
        [Column("status", TypeName = "VARCHAR(20)")]
        public string StatusStr {
            get => EntityStatus.StatusToString(Status);
            set => Status = EntityStatus.StatusFromString(value);
        }
    }
}