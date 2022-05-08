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
using System.Text;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("social_notification")]
    public class SocialNotification : BaseModel
    {
        [Key]
        [Column("id")]
        public long Id { get; private set; }
        [Required]
        [Column("owner")]
        public Guid Owner { get; set; }
        [Column("action_of_user_id")]
        public Guid? ActionOfUserId { get; set; }
        [Column("action_of_admin_user_id")]
        public Guid? ActionOfAdminUserId { get; set; }
        [Column("post_id")]
        public long? PostId { get; set; }
        [Column("comment_id")]
        public long? CommentId { get; set; }
        [Column("user_id")]
        public Guid? UserId { get; set; }
        [NotMapped]
        public EntityStatus Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => Status.ToString();
            set => Status = new EntityStatus(EntityStatusType.SocialNotification, value);
        }
        [NotMapped]
        public JObject Content { get; set; }
        [Required]
        [Column("type")]
        [StringLength(25)]
        public string Type { get; set; }
        [Required]
        [Column("content", TypeName = "jsonb")]
        public string ContentStr {
            get { return Content.ToString(); }
            set { Content = JsonConvert.DeserializeObject<JObject>(value); }
        }
        [Column("last_update_content", TypeName = "timestamp with time zone")]
        public DateTime? LastUpdateContent { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; private set; }
        [Column("last_modified_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastModifiedTimestamp { get; set; }

        [ForeignKey(nameof(Owner))]
        [InverseProperty(nameof(SocialUser.SocialNotifications))]
        public virtual SocialUser OwnerNavigation { get; set; }
        [ForeignKey(nameof(ActionOfUserId))]
        [InverseProperty(nameof(SocialUser.SocialNotificationActionOfUserIdNavigations))]
        public virtual SocialUser ActionOfUserIdNavigation { get; set; }
        [ForeignKey(nameof(ActionOfAdminUserId))]
        [InverseProperty(nameof(AdminUser.SocialNotificationActionOfAdminUserIdNavigations))]
        public virtual AdminUser ActionOfAdminUserIdNavigation { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialNotificationUserIdNavigations))]
        public virtual SocialUser UserIdDesNavigation { get; set; }
        [ForeignKey(nameof(PostId))]
        [InverseProperty(nameof(SocialPost.SocialNotifications))]
        public virtual SocialPost Post { get; set; }
        [ForeignKey(nameof(CommentId))]
        [InverseProperty(nameof(SocialComment.SocialNotifications))]
        public virtual SocialComment Comment { get; set; }

        public SocialNotification()
        {
            __ModelName = "SocialNotification";
            CreatedTimestamp = DateTime.UtcNow;
            ContentStr = "{}";
            Status = new EntityStatus(EntityStatusType.SocialNotification, StatusType.Sent);
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "Not Implemented Error";
            return false;
        }


        public override JObject GetPublicJsonObject(List<string> publicFields = null)
        {
            if (publicFields == default) {
                publicFields = new List<string>(){
                    "id",
                    "type",
                    "user_action",
                    "content",
                    "status",
                    "created_timestamp",
                    "last_modified_timestamp"
                };
            }
            var ret = GetJsonObject();
            foreach (var x in __ObjectJson) {
                if (!publicFields.Contains(x.Key)) {
                    ret.Remove(x.Key);
                }
            }
            return ret;
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>
            {
                { "id", Id },
                { "user_id", UserId },
                { "type", Type },
                { "content", Content },
                { "status", StatusStr },
                { "created_timestamp", CreatedTimestamp },
                { "last_modified_timestamp", LastModifiedTimestamp },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };

            if (ActionOfUserId != default) {
                __ObjectJson.Add(
                    "user_action",
                    new JObject(){
                        { "user_name", this.ActionOfUserIdNavigation.UserName },
                        { "display_name", this.ActionOfUserIdNavigation.DisplayName },
                        { "avatar", this.ActionOfUserIdNavigation.Avatar },
                        { "status", this.ActionOfUserIdNavigation.StatusStr },
                        { "admin", false },
                    }
                );
            } else if (ActionOfAdminUserId != default) {
                __ObjectJson.Add(
                    "user_action",
                    new JObject(){
                        { "user_name", default },
                        { "display_name", this.ActionOfAdminUserIdNavigation.DisplayName },
                        { "avatar", default },
                        { "status", this.ActionOfAdminUserIdNavigation.StatusStr },
                        { "admin", true },
                    }
                );
            } else {
                __ObjectJson.Add(
                    "user_action",
                    default
                );
            }
            return true;
        }
    }
}
