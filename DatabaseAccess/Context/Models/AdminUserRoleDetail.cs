﻿
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
    [Table("admin_user_role_detail")]
    public class AdminUserRoleDetail : BaseModel
    {
        [Key]
        [Column("role_id")]
        public int RoleId { get; set; }
        [Key]
        [Column("right_id")]
        public int RightId { get; set; }
        [NotMapped]
        public JObject Actions { get; set; }
        [Required]
        [Column("actions", TypeName = "jsonb")]
        public string ActionsStr {
            get { return Actions.ToString(Formatting.None); }
            set { Actions = JsonConvert.DeserializeObject<JObject>(value); }
        }

        [ForeignKey(nameof(RightId))]
        [InverseProperty(nameof(AdminUserRight.AdminUserRoleDetails))]
        public virtual AdminUserRight Right { get; set; }
        [ForeignKey(nameof(RoleId))]
        [InverseProperty(nameof(AdminUserRole.AdminUserRoleDetails))]
        public virtual AdminUserRole Role { get; set; }

        public AdminUserRoleDetail() : base()
        {
            __ModelName = "AdminUserRoleDetail";
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = string.Empty;
            try {
                var parser = (ParserModels.ParserAdminUserRoleDetail)Parser;
                RoleId = parser.role_id;
                RightId = parser.right_id;
                Actions = parser.actions;

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
                { "role_id", RoleId },
                { "right_id", RightId },
                { "actions", Actions },
#if DEBUG
                { "__ModelName", __ModelName }
#endif
            };
            return true;
        }

        public static List<AdminUserRoleDetail> GetDefaultData()
        {
            List<AdminUserRoleDetail> ListData = new ();
            List<AdminUserRight> ListDefaultRight = AdminUserRight.GetDefaultData();
            int AdminRoleId = AdminUserRole.GetAdminRoleId();
            JObject DefaultActions = new JObject{
                { "read",  true },
                { "write", true }
            };

            ListDefaultRight.ForEach(e => {
                ListData.Add(new AdminUserRoleDetail() {
                    RoleId      = AdminRoleId,
                    RightId     = e.Id,
                    ActionsStr  = DefaultActions.ToString(Formatting.None)
                });
            });
            return ListData;
        }
    }
}
