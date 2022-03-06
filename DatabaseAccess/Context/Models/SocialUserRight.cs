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
        public int Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => BaseStatus.StatusToString(Status, EntityStatus.SocialUserRightStatus);
            set => Status = BaseStatus.StatusFromString(value, EntityStatus.SocialUserRightStatus);
        }
        [InverseProperty(nameof(SocialUserRoleDetail.Right))]
        public virtual List<SocialUserRoleDetail> SocialUserRoleDetails { get; set; }

        public SocialUserRight()
        {
            SocialUserRoleDetails = new List<SocialUserRoleDetail>();
            __ModelName = "SocialUserRight";
            Status = SocialUserRightStatus.Enabled;
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "";
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
                    Status = SocialUserRightStatus.Readonly
                },
                new SocialUserRight()
                {
                    Id = 2,
                    RightName = "comment",
                    DisplayName = "Comment",
                    Describe = "Can create, interactive comment.",
                    Status = SocialUserRightStatus.Readonly
                },
                new SocialUserRight()
                {
                    Id = 3,
                    RightName = "report",
                    DisplayName = "Report",
                    Describe = "Can create, interactive report.",
                    Status = SocialUserRightStatus.Readonly
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
}
