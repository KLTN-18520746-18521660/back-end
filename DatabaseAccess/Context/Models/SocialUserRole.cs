
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
    [Table("social_user_role")]
    public class SocialUserRole : BaseModel
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
            get => BaseStatus.StatusToString(Status, EntityStatus.SocialUserRoleStatus);
            set => Status = BaseStatus.StatusFromString(value,  EntityStatus.SocialUserRoleStatus);
        }
        [InverseProperty(nameof(SocialUserRoleDetail.Role))]
        public virtual ICollection<SocialUserRoleDetail> SocialUserRoleDetails { get; set; }
        [InverseProperty(nameof(SocialUserRoleOfUser.Role))]
        public virtual ICollection<SocialUserRoleOfUser> SocialUserRoleOfUsers { get; set; }
        
        public SocialUserRole()
        {
            SocialUserRoleDetails = new HashSet<SocialUserRoleDetail>();
            SocialUserRoleOfUsers = new HashSet<SocialUserRoleOfUser>();
            __ModelName = "SocialUserRole";
            Status = SocialUserRoleStatus.Enabled;
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "";
            try {
                var parser = (ParserModels.ParserSocialUserRole)Parser;
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
        public static List<SocialUserRole> GetDefaultData()
        {
            List<SocialUserRole> ListData = new ()
            {
                new SocialUserRole()
                {
                    Id = 1,
                    RoleName = "user",
                    DisplayName = "User",
                    Describe = "Normal user",
                    Status = SocialUserRoleStatus.Readonly,
                    // Rights = SocialUserRight.GenerateSocialUserRights()
                }
            };
            return ListData;
        }

        public static int GetDefaultRoleId()
        {
            return 1;
        }
    }
}
