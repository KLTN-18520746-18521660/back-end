
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
    [Table("social_user_role_of_user")]
    public class SocialUserRoleOfUser : BaseModel
    {
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Key]
        [Column("role_id")]
        public int RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        [InverseProperty(nameof(SocialUserRole.SocialUserRoleOfUsers))]
        public virtual SocialUserRole Role { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialUserRoleOfUsers))]
        public virtual SocialUser User { get; set; }

        public SocialUserRoleOfUser() : base()
        {
            __ModelName = "SocialUserRoleOfUser";
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = string.Empty;
            try {
                var parser = (ParserModels.ParserSocialUserRoleOfUser)Parser;
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
    }
}
