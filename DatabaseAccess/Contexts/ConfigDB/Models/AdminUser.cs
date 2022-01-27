using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DatabaseAccess.Contexts.ConfigDB.Models
{
    [Table("admin_user")]
    public class AdminUser : BaseModel
    {   
        public AdminUser() : base()
        {
            __ModelName = "AdminUser";
            Id = Guid.NewGuid();
            CreatedTimestamp = DateTime.UtcNow;
            Status = UserStatus.NotActivated;
            Salt = PasswordEncryptor.GenerateSalt();
            RolesStr = "[]";
            SettingsStr = "{}";
        }

        public static List<AdminUser> GetDefaultData()
        {
            List<AdminUser> ListData = new List<AdminUser>()
            {
                new AdminUser() {
                    Id = Guid.NewGuid(),
                    UserName = "admin",
                    DisplayName = "Administrator",
                    Salt = PasswordEncryptor.GenerateSalt(),
                    Password = "admin",
                    Email = "admin@admin",
                    Status = UserStatus.Readonly,
                    RolesStr = "[]",
                    SettingsStr = "{}",
                    CreatedTimestamp = DateTime.UtcNow
                }
            };
            return ListData;
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>()
            {
                { "id", Id },
                {"user_name", UserName},
                {"display_name", DisplayName},
                {"email", Email },
                {"status", Status},
                {"roles", Roles},
                {"rights", Rights},
                {"settings", Settings},
                {"last_access_timestamp", LastAccessTimestamp},
                {"created_timestamp", CreatedTimestamp},
#if DEBUG
                { "password", Password },
                { "salt", Salt },
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }

        public override bool Parse(IBaseParserModel Parser, string Error = null)
        {
            Error ??= "";
            try {
                var parser = (ParserModels.ParserAdminUser)Parser;
                UserName = parser.user_name;
                DisplayName = parser.display_name;
                Password = parser.password;
                Email = parser.email;
                Settings = parser.settings;

                return true;
            } catch (Exception ex) {
                Error ??= ex.ToString();
                return false;
            }
        }

        [Column("id", TypeName = "UUID")]
        public Guid Id { get; private set; }

        [Column("user_name", TypeName = "VARCHAR(50)")]
        public string UserName { get; set; }

        [Column("display_name", TypeName = "VARCHAR(50)")]
        public string DisplayName { get; set; }

        [NotMapped]
        private string StorePassword;
        [Column("password", TypeName = "VARCHAR(32)")]
        public string Password {
            get => StorePassword;
            set => StorePassword = PasswordEncryptor.EncryptPassword(value, Salt); 
        }

        [Column("salt", TypeName = "VARCHAR(8)")]
        public string Salt { get; private set; }

        [Column("email", TypeName = "VARCHAR(320)")]
        public string Email { get; set; }

        [NotMapped]
        public int Status { get; set; }
        [Column("status", TypeName = "VARCHAR(20)")]
        public string StatusStr {
            get => UserStatus.StatusToString(Status);
            set => Status = UserStatus.StatusFromString(value);
        }

        [NotMapped]
        public List<string> Roles { get; set; }
        [Column("roles", TypeName = "JSON")]
        public string RolesStr
        {
            get { return JsonConvert.SerializeObject(Roles).ToString(); }
            set { Roles = JsonConvert.DeserializeObject<List<string>>(value); }
        }

        [NotMapped]
        public Dictionary<string, List<string>> Rights { get; set; }

        [NotMapped]
        public JObject Settings { get; set; }
        [Column("settings", TypeName = "JSON")]
        public string SettingsStr
        {
            get { return Settings.ToString(); }
            set { Settings = JsonConvert.DeserializeObject<JObject>(value); }
        }

        [Column("last_access_timestamp", TypeName = "TIMESTAMPTZ")]
        public DateTime? LastAccessTimestamp { get; set; }

        [Column("created_timestamp", TypeName = "TIMESTAMPTZ")]
        public DateTime CreatedTimestamp { get; private set; }
    }
}