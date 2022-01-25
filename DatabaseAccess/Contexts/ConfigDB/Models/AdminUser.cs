using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Common.Password;

using DatabaseAccess.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DatabaseAccess.Contexts.ConfigDB.Models
{
    [Table("admin_user")]
    public class AdminUser
    {   
        public AdminUser()
        {
            Id = Guid.NewGuid();
            CreatedTimestamp = DateTime.UtcNow;
            Status = UserStatus.NotActivated;
            Salt = PasswordEncryptor.GenerateSalt();
            RightsStr = "[]";
            SettingsStr = "{}";
        }

        public static AdminUser GetUserDefault()
        {
            AdminUser UserDefault = new AdminUser()
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                DisplayName = "Administrator",
                Salt = PasswordEncryptor.GenerateSalt(),
                Password = "admin",
                Email = "admin@admin",
                Status = UserStatus.Readonly,
                RightsStr = "[]",
                SettingsStr = "{}",
                CreatedTimestamp = DateTime.UtcNow
            };
            return UserDefault;
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

        [Column("status", TypeName = "SMALLINT")]
        public int Status { get; set; }

        [NotMapped]
        public JContainer Rights { get; set; }
        [Column("rights", TypeName = "JSON")]
        public string RightsStr {
            get { return Rights.ToString(); }
            set { Rights = JsonConvert.DeserializeObject<JContainer>(value); }
        }

        [NotMapped]
        public JContainer Settings { get; set; }
        [Column("settings", TypeName = "JSON")]
        public string SettingsStr {
            get { return Settings.ToString(); }
            set { Settings = JsonConvert.DeserializeObject<JContainer>(value); }
        }

        [Column("last_access_timestamp", TypeName = "TIMESTAMPTZ")]
        public DateTime LastAccessTimestamp { get; set; }

        [Column("created_timestamp", TypeName = "TIMESTAMPTZ")]
        public DateTime CreatedTimestamp { get; private set; }
    }
}