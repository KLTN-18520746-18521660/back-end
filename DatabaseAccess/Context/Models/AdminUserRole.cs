
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Common.Actions;
using DatabaseAccess.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DatabaseAccess.Common.Interface;
using System.Linq;

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
        public EntityStatus Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => Status.ToString();
            set => Status = new EntityStatus(EntityStatusType.AdminUserRole, value);
        }
        [Required]
        [Column("priority")]
        public bool Priority { get; set; }

        [InverseProperty(nameof(AdminUserRoleDetail.Role))]
        public virtual ICollection<AdminUserRoleDetail> AdminUserRoleDetails { get; set; }
        [InverseProperty(nameof(AdminUserRoleOfUser.Role))]
        public virtual ICollection<AdminUserRoleOfUser> AdminUserRoleOfUsers { get; set; }

        public AdminUserRole()
        {
            AdminUserRoleDetails = new HashSet<AdminUserRoleDetail>();
            AdminUserRoleOfUsers = new HashSet<AdminUserRoleOfUser>();
            __ModelName = "AdminUserRole";
            Status = new EntityStatus(EntityStatusType.AdminUserRole, StatusType.Enabled);
            Priority = false;
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = string.Empty;
            try {
                var parser = (ParserModels.ParserAdminUserRole)Parser;
                if (!parser.IsValidRights()) {
                    Error = "Invalid role details.";
                    return false;
                }
                RoleName    = parser.role_name;
                DisplayName = parser.display_name;
                Describe    = parser.describe;
                Priority    = parser.priority;
                return true;
            } catch (Exception ex) {
                Error = ex.ToString();
                return false;
            }
        }

        public JObject GetRights()
        {
            JObject ret = new JObject();
            var rights = AdminUserRoleDetails
                .Select(e => (e.Right.RightName, e.Actions)).ToList();
            foreach (var r in rights) {
                ret.Add(r.RightName, r.Actions);
            }
            return ret;
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>
            {
                { "id", Id },
                { "role_name", RoleName },
                { "display_name", DisplayName },
                { "describe", Describe },
                { "rights", GetRights() },
                { "priority", Priority },
                { "status", StatusStr },
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
                    Status = new EntityStatus(EntityStatusType.AdminUserRole, StatusType.Readonly)
                }
            };
            return ListData;
        }
        public static int GetAdminRoleId()
        {
            return 1;
        }
    }
}
