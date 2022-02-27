
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
namespace DatabaseAccess.Contexts.ConfigDB.Models
{
    [Table("admin_user_role")]
    public class AdminUserRole : BaseModel
    {
        public AdminUserRole()
        {
            __ModelName = "AdminUserRole";
            //Status = EntityStatus.Enabled;
        }

        public static List<AdminUserRole> GetDefaultData()
        {
            List<AdminUserRole> ListData = new List<AdminUserRole>()
            {
                //new AdminUserRole()
                //{
                //    Id = 1,
                //    RoleName = "admin",
                //    DisplayName = "Administrator",
                //    Describe = "Administrator",
                //    Status = EntityStatus.Readonly,
                //    Rights = AdminUserRight.GenerateAdminRights()
                //}
            };
            return ListData;
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
            get => "";
            set => Status = 1;
        }
    }
}