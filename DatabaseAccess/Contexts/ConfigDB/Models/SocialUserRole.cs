
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Interface;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace DatabaseAccess.Contexts.ConfigDB.Models
{
    [Table("social_user_role")]
    public class SocialUserRole : BaseModel
    {
        public SocialUserRole()
        {
            __ModelName = "SocialUserRole";
            Status = EntityStatus.Enabled;
        }
    
        public static List<SocialUserRole> GetDefaultData()
        {
            List<SocialUserRole> ListData = new List<SocialUserRole>();
            var DefaultRights = SocialUserRight.GetDefaultData();
            int IdIdentity = 1;
            foreach(var Right in DefaultRights)
            {
                // Read Role
                ListData.Add(new SocialUserRole()
                {
                    Id = IdIdentity++,
                    RoleName = $"{Right.RightName}_read",
                    DisplayName = $"{Right.DisplayName} - Read",
                    Describe = $"{Right.DisplayName} - Read",
                    Status = EntityStatus.Readonly,
                    Rights = new Dictionary<string, List<string>>()
                    {
                        { Right.RightName, new List<string>(){"read"} }
                    }
                });
                // Write Role
                ListData.Add(new SocialUserRole()
                {
                    Id = IdIdentity++,
                    RoleName = $"{Right.RightName}_write",
                    DisplayName = $"{Right.DisplayName} - Write",
                    Describe = $"{Right.DisplayName} - Write",
                    Status = EntityStatus.Readonly,
                    Rights = new Dictionary<string, List<string>>()
                    {
                        { Right.RightName, new List<string>(){"write"} }
                    }
                });
            }
            return ListData;
        }

        public override bool Parse(IBaseParserModel Parser, string Error = null)
        {
            Error ??= "";
            try {
                var parser = (ParserModels.ParserSocialUserRole)Parser;
                RoleName = parser.role_name;
                DisplayName = parser.display_name;
                Describe = parser.describe;
                Rights = parser.rights;
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
                { "role_name", RoleName },
                { "display_name", DisplayName },
                { "describe", Describe },
                { "rights", Rights },
                { "status", Status },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }

        [Column("id", TypeName = "INTEGER")]
        public int Id { get; private set; }

        [Column("role_name", TypeName = "VARCHAR(50)")]
        public string RoleName { get; set; }

        [Column("display_name", TypeName = "VARCHAR(50)")]
        public string DisplayName { get; set; }

        [Column("describe", TypeName = "VARCHAR(150)")]
        public string Describe { get; set; }

        [NotMapped]
        public Dictionary<string, List<string>> Rights { get; set; }
        [Column("rights", TypeName = "JSON")]
        public string RightsStr
        {
            get { return JsonConvert.SerializeObject(Rights).ToString(); }
            set { Rights = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(value); }
        }

        [NotMapped]
        public int Status { get; set; }
        [Column("status", TypeName = "VARCHAR(20)")]
        public string StatusStr
        {
            get => EntityStatus.StatusToString(Status);
            set => Status = EntityStatus.StatusFromString(value);
        }
    }
}