
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Common.Actions;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_user_role_detail")]
    public class SocialUserRoleDetail : BaseModel
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
            get { return Actions.ToString(); }
            set { Actions = JsonConvert.DeserializeObject<JObject>(value); }
        }

        [ForeignKey(nameof(RightId))]
        [InverseProperty(nameof(SocialUserRight.SocialUserRoleDetails))]
        public virtual SocialUserRight Right { get; set; }
        [ForeignKey(nameof(RoleId))]
        [InverseProperty(nameof(SocialUserRole.SocialUserRoleDetails))]
        public virtual SocialUserRole Role { get; set; }

        public SocialUserRoleDetail() : base()
        {
            __ModelName = "SocialUserRoleDetail";
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "";
            try {
                var parser = (ParserModels.ParserSocialUserRoleDetail)Parser;
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
        public static List<SocialUserRoleDetail> GetDefaultData()
        {
            List<SocialUserRoleDetail> ListData = new ();
            List<SocialUserRight> ListDefaultRight = SocialUserRight.GetDefaultData();
            int DefaultRoleId = SocialUserRole.GetDefaultRoleId();
            JObject DefaultActions = new JObject{
                { "read",  true },
                { "write", true }
            };

            ListDefaultRight.ForEach(e => {
                ListData.Add(new SocialUserRoleDetail() {
                    RoleId = DefaultRoleId,
                    RightId = e.Id,
                    ActionsStr = DefaultActions.ToString()
                });
            });
            return ListData;
        }
    }
}
