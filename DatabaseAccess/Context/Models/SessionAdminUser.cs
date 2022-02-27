
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DatabaseAccess.Common.Interface;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("session_admin_user")]
    public class SessionAdminUser : BaseModel
    {
        [Key]
        [Column("session_token")]
        [StringLength(30)]
        public string SessionToken { get; private set; }
        [Column("user_id")]
        public Guid UserId { get; set; }
        [Column("saved")]
        public bool Saved { get; set; }
        [NotMapped]
        public JObject Data { get; set; }
        [Required]
        [Column("data", TypeName = "json")]
        public string DataStr {
            get { return Data.ToString(); }
            set { Data = JsonConvert.DeserializeObject<JObject>(value); }
        }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; private set; }
        [Column("last_interaction_time", TypeName = "timestamp with time zone")]
        public DateTime? LastInteractionTime { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(AdminUser.SessionAdminUsers))]
        public virtual AdminUser User { get; set; }

        public SessionAdminUser()
        {
            __ModelName = "SeesionAdminUser";
            SessionToken = Utils.GenerateSessionToken();
            CreatedTimestamp = DateTime.UtcNow;
            LastInteractionTime = CreatedTimestamp;
            Saved = false;
            DataStr = "{}";
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
                { "session_token", SessionToken },
                { "created_timestamp", CreatedTimestamp },
                { "last_interaction_time", LastInteractionTime },
                { "saved", Saved },
                { "data", Data },
#if DEBUG
                { "user_id", UserId },
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }
    }
}
