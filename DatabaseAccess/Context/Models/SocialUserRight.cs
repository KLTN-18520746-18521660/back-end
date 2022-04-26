
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
    [Table("social_user_right")]
    public class SocialUserRight : BaseModel
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
        public EntityStatus Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => Status.ToString();
            set => Status = new EntityStatus(EntityStatusType.SocialUserRight, value);
        }
        [InverseProperty(nameof(SocialUserRoleDetail.Right))]
        public virtual ICollection<SocialUserRoleDetail> SocialUserRoleDetails { get; set; }

        public SocialUserRight()
        {
            SocialUserRoleDetails = new HashSet<SocialUserRoleDetail>();
            __ModelName = "SocialUserRight";
            Status = new EntityStatus(EntityStatusType.SocialUserRight, StatusType.Enabled);
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = string.Empty;
            try {
                var parser = (ParserModels.ParserSocialUserRight)Parser;
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

        public static List<SocialUserRight> GetDefaultData()
        {
            List<SocialUserRight> ListData = new List<SocialUserRight>()
            {
                new SocialUserRight()
                {
                    Id = 1,
                    RightName = "post",
                    DisplayName = "Post",
                    Describe = "Can create, interactive posts.",
                    Status = new EntityStatus(EntityStatusType.SocialUserRight, StatusType.Readonly)
                },
                new SocialUserRight()
                {
                    Id = 2,
                    RightName = "comment",
                    DisplayName = "Comment",
                    Describe = "Can create, interactive comment.",
                    Status = new EntityStatus(EntityStatusType.SocialUserRight, StatusType.Readonly)
                },
                new SocialUserRight()
                {
                    Id = 3,
                    RightName = "report",
                    DisplayName = "Report",
                    Describe = "Can create, interactive report.",
                    Status = new EntityStatus(EntityStatusType.SocialUserRight, StatusType.Readonly)
                },
                new SocialUserRight()
                {
                    Id = 4,
                    RightName = "upload",
                    DisplayName = "Upload",
                    Describe = "Can create, interactive report.",
                    Status = new EntityStatus(EntityStatusType.SocialUserRight, StatusType.Readonly)
                }
            };
            return ListData;
        }

        public static Dictionary<string, List<string>> GenerateSocialUserRights()
        {
            Dictionary<string, List<string>> Rights = new();
            var AllRights = SocialUserRight.GetDefaultData();
            foreach (var Right in AllRights)
            {
                Rights.Add(Right.RightName, new List<string>() { "write", "read" });
            }
            return Rights;
        }
    }
    public static class SOCIAL_RIGHTS
    {
        public static readonly string POST = "post";
        public static readonly string COMMENT = "comment";
        public static readonly string REPORT = "report";
        public static readonly string UPLOAD = "upload";
    }
}
