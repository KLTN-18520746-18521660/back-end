using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Interface;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Common.Actions;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;
using DatabaseAccess.Common;
using Microsoft.EntityFrameworkCore;
using Common;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    [Table("admin_user")]
    public class AdminUser : BaseModel
    {
        [Key]
        [Column("id")]
        public Guid Id { get; private set; }
        [Required]
        [Column("user_name")]
        [StringLength(50)]
        public string UserName { get; set; }
        [Required]
        [Column("display_name")]
        [StringLength(50)]
        public string DisplayName { get; set; }
        [Column("password")]
        [Required]
        [StringLength(32)]
        public string StorePassword { get; private set; }
        [NotMapped]
        public string Password {
            get => StorePassword;
            set => StorePassword = PasswordEncryptor.EncryptPassword(value, Salt); 
        }
        [Required]
        [Column("salt")]
        [StringLength(8)]
        public string Salt { get; private set; }
        [Required]
        [Column("email")]
        [StringLength(320)]
        public string Email { get; set; }
        [NotMapped]
        public EntityStatus Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => Status.ToString();
            set => Status = new EntityStatus(EntityStatusType.AdminUser, value);
        }
        [NotMapped]
        public Dictionary<string, JObject> Rights { get => GetRights(); }
        [NotMapped]
        public List<string> Roles { get => GetRoles(); }
        [NotMapped]
        public JObject Settings { get; set; }
        [Required]
        [Column("settings", TypeName = "jsonb")]
        public string SettingsStr {
            get { return Settings.ToString(Formatting.None); }
            set { Settings = JsonConvert.DeserializeObject<JObject>(value); }
        }
        [Column("last_access_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastAccessTimestamp { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; private set; }
        [InverseProperty(nameof(AdminUserRoleOfUser.User))]
        public virtual ICollection<AdminUserRoleOfUser> AdminUserRoleOfUsers { get; set; }
        [InverseProperty(nameof(SessionAdminUser.User))]
        public virtual ICollection<SessionAdminUser> SessionAdminUsers { get; set; }
        [InverseProperty(nameof(AdminAuditLog.User))]
        public virtual ICollection<AdminAuditLog> AdminAuditLogs { get; set; }
        [InverseProperty(nameof(SocialAuditLog.User))]
        public virtual ICollection<SocialAuditLog> SocialAuditLogs { get; set; }
        [InverseProperty(nameof(SocialUserAuditLog.UserAdmin))]
        public virtual ICollection<SocialUserAuditLog> SocialUserAuditLogs { get; set; }
        [InverseProperty(nameof(SocialNotification.ActionOfAdminUserIdNavigation))]
        public virtual ICollection<SocialNotification> SocialNotificationActionOfAdminUserIdNavigations { get; set; }

        public AdminUser() : base()
        {
            AdminAuditLogs                                          = new HashSet<AdminAuditLog>();
            SocialAuditLogs                                         = new HashSet<SocialAuditLog>();
            SessionAdminUsers                                       = new HashSet<SessionAdminUser>();
            SocialUserAuditLogs                                     = new HashSet<SocialUserAuditLog>();
            AdminUserRoleOfUsers                                    = new HashSet<AdminUserRoleOfUser>();
            SocialNotificationActionOfAdminUserIdNavigations        = new HashSet<SocialNotification>();
            __ModelName = "AdminUser";
            Id = Guid.NewGuid();
            CreatedTimestamp = DateTime.UtcNow;
            Status = new EntityStatus(EntityStatusType.AdminUser, StatusType.Activated);
            Salt = PasswordEncryptor.GenerateSalt();
            SettingsStr = "{}";
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = string.Empty;
            try {
                var parser  = (ParserModels.ParserAdminUser)Parser;
                Email       = parser.email.ToLower();
                UserName    = parser.user_name;
                Password    = parser.password;
                Settings    = parser.settings;
                DisplayName = parser.display_name;

                return true;
            } catch (Exception ex) {
                Error = ex.ToString();
                return false;
            }
        }

        #region Handle default data
        public List<string> GetRoles()
        {
            return AdminUserRoleOfUsers
                .Where(e => e.Role.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                .Select(e => e.Role.RoleName).ToList();
        }

        public Dictionary<string, JObject> GetRights()
        {
            Dictionary<string, JObject> rights = new();
            Dictionary<string, JObject> rightsPriority = new();
            var notPriorityRoleDetails = AdminUserRoleOfUsers
                .Where(e => e.Role.Priority == false && e.Role.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                .Select(e => e.Role.AdminUserRoleDetails)
                .ToList();

            var priorityRoleDetails = AdminUserRoleOfUsers
                .Where(e => e.Role.Priority && e.Role.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                .Select(e => e.Role.AdminUserRoleDetails)
                .ToList();

            foreach (var roleDetails in notPriorityRoleDetails) {
                foreach (var detail in roleDetails) {
                    if (detail.Right.StatusStr == EntityStatus.StatusTypeToString(StatusType.Disabled)) {
                        continue;
                    }
                    var _obj = rights.GetValueOrDefault(detail.Right.RightName, new JObject());
                    var obj = detail.Actions;
                    JObject action;
                    if (_obj.Count != 0) {
                        try {
                            var _read = _obj.Value<bool>("read");
                            var _write = _obj.Value<bool>("write");
                            var read = obj.Value<bool>("read") ? true : _read;
                            var write = obj.Value<bool>("write") ? true : _write;
                            action = new JObject {
                                { "read", read },
                                { "write", write }
                            };
                        } catch (Exception) {
                            action = _obj;
                        }
                        rights[detail.Right.RightName] = action;
                    } else {
                        rights.Add(detail.Right.RightName, obj);
                    }
                }
            }

            foreach (var roleDetails in priorityRoleDetails) {
                foreach (var detail in roleDetails) {
                    if (detail.Right.StatusStr == EntityStatus.StatusTypeToString(StatusType.Disabled)) {
                        continue;
                    }
                    var _obj = rightsPriority.GetValueOrDefault(detail.Right.RightName, new JObject());
                    var obj = detail.Actions;
                    JObject action;
                    if (_obj.Count != 0) {
                        try {
                            var _read = _obj.Value<bool>("read");
                            var _write = _obj.Value<bool>("write");
                            var read = obj.Value<bool>("read") ? true : _read;
                            var write = obj.Value<bool>("write") ? true : _write;
                            action = new JObject {
                                { "read", read },
                                { "write", write }
                            };
                        } catch (Exception) {
                            action = _obj;
                        }
                        rightsPriority[detail.Right.RightName] = action;
                    } else {
                        rightsPriority.Add(detail.Right.RightName, obj);
                    }
                }
            }

            foreach (var rp in rightsPriority) {
                if (rights.ContainsKey(rp.Key)) {
                    rights[rp.Key] = rp.Value;
                } else {
                    rights.Add(rp.Key, rp.Value);
                }
            }

            return rights;
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>()
            {
                { "id", Id },
                { "user_name", UserName },
                { "display_name", DisplayName },
                { "email", Email },
                { "status", StatusStr },
                { "roles", Roles },
                { "rights", Rights },
                { "settings", Settings },
                { "last_access_timestamp", LastAccessTimestamp },
                { "created_timestamp", CreatedTimestamp },
#if DEBUG
                { "password", Password },
                { "salt", Salt },
                { "__ModelName", __ModelName }
#endif
            };
            return true;
        }

        public static List<AdminUser> GetDefaultData()
        {
            List<AdminUser> ListData = new ()
            {
                new AdminUser() {
                    Id                  = DBCommon.FIRST_ADMIN_USER_ID,
                    UserName            = DBCommon.FIRST_ADMIN_USER_NAME,
                    DisplayName         = "Administrator",
                    Salt                = DBCommon.FIRST_ADMIN_USER_SALT,
                    Password            = DBCommon.FIRST_ADMIN_USER_NAME,
                    Email               = "admin@admin",
                    Status              = new EntityStatus(EntityStatusType.AdminUser, StatusType.Readonly),
                    SettingsStr         = "{}",
                    CreatedTimestamp    = DBCommon.DEFAULT_DATETIME_FOR_DATA_SEED,
                }
            };
            return ListData;
        }

        public static Guid GetAdminUserId()
        {
            return DBCommon.FIRST_ADMIN_USER_ID;
        }

        public static string GetAdminUserName()
        {
            return DBCommon.FIRST_ADMIN_USER_NAME;
        }
        #endregion
        #region Handle session user
        public List<string> GetExpiredSessions(int ExpiryTime) // minute
        {
            var now = DateTime.UtcNow;
            return SessionAdminUsers
                    .Where(e => (now - e.LastInteractionTime.ToUniversalTime()).TotalMinutes >= ExpiryTime && e.Saved == false)
                    .Select(e => e.SessionToken)
                    .ToList();
        }
        #endregion
    }
}
