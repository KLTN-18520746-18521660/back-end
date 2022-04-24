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
using DatabaseAccess.Common.Actions;


#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_user_action_with_user")]
    public class SocialUserActionWithUser : BaseModel
    {
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Key]
        [Column("user_id_des")]
        public Guid UserIdDes { get; set; }
        [NotMapped]
        public List<EntityAction> Actions { get; set; }
        [Required]
        [Column("actions", TypeName = "jsonb")]
        public string ActionsStr {
            get { return JArray.FromObject(Actions).ToString(); }
            set {
                Actions = new List<EntityAction>();
                foreach (var v in JsonConvert.DeserializeObject<JArray>(value)) {
                    Actions.Add(new(EntityActionType.UserActionWithUser,
                                    (v as JObject).Value<string>("action"))
                    {
                        date = (v as JObject).Value<DateTime>("date")
                    });
                }
            }
        }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialUserActionWithUserUsers))]
        public virtual SocialUser User { get; set; }
        [ForeignKey(nameof(UserIdDes))]
        [InverseProperty(nameof(SocialUser.SocialUserActionWithUserUserIdDesNavigations))]
        public virtual SocialUser UserIdDesNavigation { get; set; }

        public SocialUserActionWithUser()
        {
            __ModelName = "SocialUserActionWithUser";
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
                { "user_id_des", UserIdDes },
                { "actions", Actions },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }
    }
}
