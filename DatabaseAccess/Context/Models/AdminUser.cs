using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DatabaseAccess.Common;

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
        [NotMapped]
        private string StorePassword;
        [Required]
        [Column("password")]
        [StringLength(32)]
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
            get => AdminUserStatus.StatusToString(Status);
            set => Status = AdminUserStatus.StatusFromString(value);
        }
        [NotMapped]
        public List<string> Roles { get; set; }
        [Required]
        [Column("roles", TypeName = "json")]
        public string RolesStr {
            get { return JsonConvert.SerializeObject(Roles).ToString(); }
            set { Roles = JsonConvert.DeserializeObject<List<string>>(value); }
        }
        [NotMapped]
        public Dictionary<string, List<string>> Rights { get; set; }
        [NotMapped]
        public JObject Settings { get; set; }
        [Required]
        [Column("settings", TypeName = "json")]
        public string SettingsStr {
            get { return Settings.ToString(); }
            set { Settings = JsonConvert.DeserializeObject<JObject>(value); }
        }
        [Column("last_access_timestamp", TypeName = "timestamp with time zone")]
        public DateTime? LastAccessTimestamp { get; set; }
        [Column("created_timestamp", TypeName = "timestamp with time zone")]
        public DateTime CreatedTimestamp { get; private set; }

        [InverseProperty(nameof(SessionAdminUser.User))]
        public virtual ICollection<SessionAdminUser> SessionAdminUsers { get; set; }

        public AdminUser() : base()
        {
            SessionAdminUsers = new HashSet<SessionAdminUser>();
            __ModelName = "AdminUser";
            Id = Guid.NewGuid();
            CreatedTimestamp = DateTime.UtcNow;
            Status = AdminUserStatus.Activated;
            Salt = PasswordEncryptor.GenerateSalt();
            RolesStr = "[]";
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

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>()
            {
                { "id", Id },
                { "user_name", UserName},
                { "display_name", DisplayName},
                { "email", Email },
                { "status", StatusStr},
                { "roles", Roles},
                { "rights", Rights},
                { "settings", Settings},
                { "last_access_timestamp", LastAccessTimestamp},
                { "created_timestamp", CreatedTimestamp},
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
                    Id = Guid.NewGuid(),
                    UserName = "admin",
                    DisplayName = "Administrator",
                    Salt = PasswordEncryptor.GenerateSalt(),
                    Password = "admin",
                    Email = "admin@admin",
                    Status = AdminUserStatus.Readonly,
                    RolesStr = "[]",
                    SettingsStr = "{}",
                    CreatedTimestamp = DateTime.UtcNow
                }
            };
            return ListData;
        }
    }
}
