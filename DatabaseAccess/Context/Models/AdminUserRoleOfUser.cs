
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
    [Table("admin_user_role_of_user")]
    public class AdminUserRoleOfUser :  BaseModel
    {
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Key]
        [Column("role_id")]
        public int RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        [InverseProperty(nameof(AdminUserRole.AdminUserRoleOfUsers))]
        public virtual AdminUserRole Role { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(AdminUser.AdminUserRoleOfUsers))]
        public virtual AdminUser User { get; set; }

        public AdminUserRoleOfUser() : base()
        {
            __ModelName = "AdminUserRoleOfUser";
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "";
            try {
                var parser = (ParserModels.ParserAdminUserRoleOfUser)Parser;
                UserId = parser.user_id;
                RoleId = parser.role_id;

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
                { "user_id", UserId },
                { "role_id", RoleId },
#if DEBUG
                { "__ModelName", __ModelName }
#endif
            };
            return true;
        }

        public static List<AdminUserRoleOfUser> GetDefaultData()
        {
            List<AdminUserRoleOfUser> ListData = new ()
            {
                new AdminUserRoleOfUser(){
                    UserId = AdminUser.GetAdminUserId(),
                    RoleId = AdminUserRole.GetAdminRoleId()
                }
            };
            return ListData;
        }
    }
}
