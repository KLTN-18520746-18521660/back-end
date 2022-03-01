
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
    [Table("admin_user_right")]
    public class AdminUserRight : BaseModel
    {
        [Key]
        [Column("id")]
        public int Id { get; private set; }
        [Required]
        [Column("right_name")]
        [StringLength(50)]
        public string RightName { get; set; }
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
            get => BaseStatus.StatusToString(Status, EntityStatus.AdminUserRightStatus);
            set => Status = BaseStatus.StatusFromString(value, EntityStatus.AdminUserRightStatus);
        }
        
        public AdminUserRight()
        {
            __ModelName = "SocialUserRight";
            Status = AdminUserRightStatus.Enabled;
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "";
            try {
                var parser = (ParserModels.ParserAdminUserRight)Parser;
                RightName = parser.right_name;
                DisplayName = parser.display_name;
                Describe = parser.describe;
                return true;
            }
            catch (Exception ex) {
                Error = ex.ToString();
                return false;
            }
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>
            {
                { "id", Id },
                { "right_name", RightName },
                { "display_name", DisplayName },
                { "describe", Describe },
                { "status", Status },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }

        public static List<AdminUserRight> GetDefaultData()
        {
            List<AdminUserRight> ListData = new List<AdminUserRight>()
            {
                new AdminUserRight()
                {
                    Id = 1,
                    RightName = "dashboard",
                    DisplayName = "Dashboard",
                    Describe = "Can access Homepage and see statistic",
                    Status = AdminUserRightStatus.Readonly
                },
                new AdminUserRight()
                {
                    Id = 2,
                    RightName = "category",
                    DisplayName = "Category",
                    Describe = "Add, create, disable category",
                    Status = AdminUserRightStatus.Readonly
                },
                new AdminUserRight()
                {
                    Id = 3,
                    RightName = "topic",
                    DisplayName = "Topic",
                    Describe = "Add, create, disable topics",
                    Status = AdminUserRightStatus.Readonly
                },
                new AdminUserRight()
                {
                    Id = 4,
                    RightName = "type_of_post",
                    DisplayName = "Type of post",
                    Describe = "Add, create, disable type of post.",
                    Status = AdminUserRightStatus.Readonly
                },
                new AdminUserRight()
                {
                    Id = 5,
                    RightName = "post",
                    DisplayName = "Post",
                    Describe = "Review, accept, deny post. See report about post.",
                    Status = AdminUserRightStatus.Readonly
                },
                new AdminUserRight()
                {
                    Id = 6,
                    RightName = "comment",
                    DisplayName = "Comment",
                    Describe = "Delete comment. See report about comment.",
                    Status = AdminUserRightStatus.Readonly
                },
                new AdminUserRight()
                {
                    Id = 7,
                    RightName = "security",
                    DisplayName = "Security",
                    Describe = "Configure security of Server.",
                    Status = AdminUserRightStatus.Readonly
                },
                new AdminUserRight()
                {
                    Id = 8,
                    RightName = "social_user",
                    DisplayName = "Social User",
                    Describe = "Deactivate, activate SocialUser",
                    Status = AdminUserRightStatus.Readonly
                },
                new AdminUserRight()
                {
                    Id = 9,
                    RightName = "admin_user",
                    DisplayName = "Admin User",
                    Describe = "Add, deactivate, activate, delete AdminUser.",
                    Status = AdminUserRightStatus.Readonly
                },
                new AdminUserRight()
                {
                    Id = 10,
                    RightName = "log",
                    DisplayName = "Log",
                    Describe = "See and tracking log file.",
                    Status = AdminUserRightStatus.Readonly
                }
            };
            return ListData;
        }

        public static Dictionary<string, List<string>> GenerateAdminRights()
        {
            Dictionary<string, List<string>> Rights = new();
            var AllRights = AdminUserRight.GetDefaultData();
            foreach (var Right in AllRights)
            {
                Rights.Add(Right.RightName, new List<string>() { "write", "read" });
            }
            return Rights;
        }
    }
}
