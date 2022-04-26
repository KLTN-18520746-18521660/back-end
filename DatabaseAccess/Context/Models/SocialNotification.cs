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
    [Table("social_notification")]
    public class SocialNotification : BaseModel
    {
        [Key]
        [Column("id")]
        public long Id { get; private set; }
        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Column("post_id")]
        public long? PostId { get; private set; }
        [Column("comment_id")]
        public long? CommentId { get; private set; }
        [Column("user_id_des")]
        public Guid? UserIdDes { get; set; }
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
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; private set; }
        [Column("last_modified_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastModifiedTimestamp { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(SocialUser.SocialNotifications))]
        public virtual SocialUser User { get; set; }
        [ForeignKey(nameof(UserIdDes))]
        [InverseProperty(nameof(SocialUser.SocialNotificationUserIdDesNavigations))]
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
                { "content", Content },
                { "status", StatusStr },
                { "created_timestamp", CreatedTimestamp },
                { "last_modified_timestamp", LastModifiedTimestamp },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }
    }
}
