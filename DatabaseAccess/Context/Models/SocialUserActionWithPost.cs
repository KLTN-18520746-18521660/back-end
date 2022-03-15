using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Interface;
using DatabaseAccess.Common.Status;


#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_user_action_with_post")]
    public class SocialUserActionWithPost : BaseModel
    {
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Key]
        [Column("post_id")]
        public long PostId { get; set; }
        [NotMapped]
        public List<string> Actions { get; set; }
        [Required]
        [Column("actions", TypeName = "json")]
        public string ActionsStr {
            get { return JArray.FromObject(Actions).ToString(); }
            set { Actions = JsonConvert.DeserializeObject<List<string>>(value); }
        }

        [ForeignKey(nameof(PostId))]
        [InverseProperty(nameof(SocialPost.SocialUserActionWithPosts))]
        public virtual SocialPost Post { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialUserActionWithPosts))]
        public virtual SocialUser User { get; set; }

        public SocialUserActionWithPost()
        {
            __ModelName = "SocialUserActionWithPost";
            ActionsStr = "[]";
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "Not Implemented Error";
            return false;
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>
            {
                { "user_id", UserId },
                { "post_id", PostId },
                { "actions", Actions },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }
    }
}
