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
        public int Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => BaseStatus.StatusToString(Status, EntityStatus.AdminUserStatus);
            set => Status = BaseStatus.StatusFromString(value, EntityStatus.AdminUserStatus);
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
            get { return Settings.ToString(); }
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

        public AdminUser() : base()
        {
            AdminUserRoleOfUsers = new HashSet<AdminUserRoleOfUser>();
            SessionAdminUsers = new HashSet<SessionAdminUser>();
            AdminAuditLogs = new HashSet<AdminAuditLog>();
            SocialAuditLogs = new HashSet<SocialAuditLog>();
            __ModelName = "AdminUser";
            Id = Guid.NewGuid();
            CreatedTimestamp = DateTime.UtcNow;
            Status = AdminUserStatus.Activated;
            Salt = PasswordEncryptor.GenerateSalt();
            SettingsStr = "{}";
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "";
            try {
                var parser = (ParserModels.ParserAdminUser)Parser;
                UserName = parser.user_name;
                DisplayName = parser.display_name;
                Password = parser.password;
                Email = parser.email;
                Settings = parser.settings;

                return true;
            } catch (Exception ex) {
                Error = ex.ToString();
                return false;
            }
        }

        #region Handle default data
        public List<string> GetRoles()
        {
            return AdminUserRoleOfUsers.Select(e => e.Role.RoleName).ToList();
        }

        public Dictionary<string, JObject> GetRights()
        {
            Dictionary<string, JObject> rights = new();
            var allRoleDetails = AdminUserRoleOfUsers
                .Select(e => e.Role.AdminUserRoleDetails)
                .ToList();

            foreach(var roleDetails in allRoleDetails) {
                foreach(var detail in roleDetails) {
                    var _obj = rights.GetValueOrDefault(detail.Right.RightName, new JObject());
                    var obj = detail.Actions;
                    JObject action;
                    if (_obj.Count != 0) {
                        try {
                            var _read = _obj.Value<bool>("read");
                            var _write = _obj.Value<bool>("write");
                            var read = obj.Value<bool>("read") ? true : _read;
                            var write = obj.Value<bool>("write") ? true : _write;
                            rights.Remove(detail.Right.RightName);
                            action = new JObject {
                                { "read", read },
                                { "write", write }
                            };
                        } catch (Exception) {
                            action = _obj;
                        }
                        rights.Add(detail.Right.RightName, action);
                    } else {
                        rights.Add(detail.Right.RightName, obj);
                    }
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

        private static Guid AdminUserId = Guid.NewGuid();
        private static string AdminUserName = "admin";
        public static List<AdminUser> GetDefaultData()
        {
            List<AdminUser> ListData = new ()
            {
                new AdminUser() {
                    Id = AdminUserId,
                    UserName = "admin",
                    DisplayName = "Administrator",
                    Salt = PasswordEncryptor.GenerateSalt(),
                    Password = AdminUserName,
                    Email = "admin@admin",
                    Status = AdminUserStatus.Readonly,
                    SettingsStr = "{}",
                    CreatedTimestamp = DateTime.UtcNow
                }
            };
            return ListData;
        }

        public static Guid GetAdminUserId()
        {
            return AdminUserId;
        }

        public static string GetAdminUserName()
        {
            return AdminUserName;
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
