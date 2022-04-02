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
    [Table("social_user_action_with_tag")]
    public class SocialUserActionWithTag : BaseModel
    {
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Key]
        [Column("tag_id")]
        public long TagId { get; set; }
        [NotMapped]
        public List<string> Actions { get; set; }
        [Required]
        [Column("actions", TypeName = "jsonb")]
        public string ActionsStr {
            get { return JArray.FromObject(Actions).ToString(); }
            set { Actions = JsonConvert.DeserializeObject<List<string>>(value); }
        }

        [ForeignKey(nameof(TagId))]
        [InverseProperty(nameof(SocialTag.SocialUserActionWithTags))]
        public virtual SocialTag Tag { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialUserActionWithTags))]
        public virtual SocialUser User { get; set; }

        public SocialUserActionWithTag()
        {
            __ModelName = "SocialUserActionWithTag";
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
                { "tag_id", TagId },
                { "actions", Actions },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }
    }
}
