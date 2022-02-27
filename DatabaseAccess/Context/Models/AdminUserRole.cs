
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DatabaseAccess.Common.Interface;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("admin_user_role")]
    public class AdminUserRole : BaseModel
    {
        [Key]
        [Column("id")]
        public int Id { get; private set; }
        [Required]
        [Column("role_name")]
        [StringLength(50)]
        public string RoleName { get; set; }
        [Required]
        [Column("display_name")]
        [StringLength(50)]
        public string DisplayName { get; set; }
        [Required]
        [Column("describe")]
        [StringLength(150)]
        public string Describe { get; set; }
        [NotMapped]
        public Dictionary<string, List<string>> Rights { get; set; }
        [Required]
        [Column("rights", TypeName = "json")]
        public string RightsStr {
            get { return JsonConvert.SerializeObject(Rights).ToString(); }
            set { Rights = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(value); }
        }
        [NotMapped]
        public int Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => AdminUserRoleStatus.StatusToString(Status);
            set => Status = AdminUserRoleStatus.StatusFromString(value);
        }
        
        public AdminUserRole()
        {
            __ModelName = "AdminUserRole";
            Status = AdminUserRoleStatus.Enabled;
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "";
            try {
                var parser = (ParserModels.ParserAdminUserRole)Parser;
                RoleName = parser.role_name;
                DisplayName = parser.display_name;
                Describe = parser.describe;
                Rights = parser.rights;
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
        public static List<AdminUserRole> GetDefaultData()
        {
            List<AdminUserRole> ListData = new ()
            {
                new AdminUserRole()
                {
                    Id = 1,
                    RoleName = "admin",
                    DisplayName = "Administrator",
                    Describe = "Administrator",
                    Status = AdminUserRoleStatus.Readonly,
                    Rights = AdminUserRight.GenerateAdminRights()
                }
            };
            return ListData;
        }
    }
}
