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
        [Column("user_id")]
        public Guid UserId { get; set; }
        [NotMapped]
        public int Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => BaseStatus.StatusToString(Status, EntityStatus.SocialNotificationStatus);
            set => Status = BaseStatus.StatusFromString(value, EntityStatus.SocialNotificationStatus);
        }
        [NotMapped]
        public JObject Content { get; set; }
        [Required]
        [Column("content", TypeName = "json")]
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

        public SocialNotification()
        {
            __ModelName = "SocialNotification";
            CreatedTimestamp = DateTime.UtcNow;
            ContentStr = "{}";
            Status = SocialNotificationStatus.Sent;
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
