
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
        public int Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => BaseStatus.StatusToString(Status, EntityStatus.AdminUserRoleStatus);
            set => Status = BaseStatus.StatusFromString(value,  EntityStatus.AdminUserRoleStatus);
        }

        [InverseProperty(nameof(AdminUserRoleDetail.Role))]
        public virtual ICollection<AdminUserRoleDetail> AdminUserRoleDetails { get; set; }
        [InverseProperty(nameof(AdminUserRoleOfUser.Role))]
        public virtual ICollection<AdminUserRoleOfUser> AdminUserRoleOfUsers { get; set; }

        public AdminUserRole()
        {
            AdminUserRoleDetails = new HashSet<AdminUserRoleDetail>();
            AdminUserRoleOfUsers = new HashSet<AdminUserRoleOfUser>();
            __ModelName = "AdminUserRole";
            Status = AdminUserRoleStatus.Enabled;
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = string.Empty;
            try {
                var parser = (ParserModels.ParserAdminUserRole)Parser;
                RoleName = parser.role_name;
                DisplayName = parser.display_name;
                Describe = parser.describe;
                // Rights = parser.rights;
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
                // { "rights", Rights },
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
                    Status = AdminUserRoleStatus.Readonly
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
