using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Interface;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Common.Actions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    public enum CONFIG_KEY
    {
        ADMIN_USER_LOGIN_CONFIG = 1,
        SOCIAL_USER_LOGIN_CONFIG = 2,
        SESSION_ADMIN_USER_CONFIG = 3,
        SESSION_SOCIAL_USER_CONFIG = 4,
    }

    public static class DefaultBaseConfig
    {
        #region Default Config
        public static readonly Dictionary<string, int> AdminUserLoginConfig = new() {
            { "number", 5 },
            { "time", 5 },
            { "lock", 360 },
        };
        public static readonly Dictionary<string, int> SocialUserLoginConfig = new() {
            { "number", 5 },
            { "time", 5 },
            { "lock", 360 },
        };
        public static readonly Dictionary<string, int> SessionAdminUserConfig = new() {
            { "expiry_time", 5 },
            { "extension_time", 5 },
        };
        public static readonly Dictionary<string, int> SessionSocialUserConfig = new() {
            { "expiry_time", 5 },
            { "extension_time", 5 },
        };
        #endregion
        public static JObject GetConfig(CONFIG_KEY ConfigKey, string Error = null)
        {
            Error ??= "";
            switch(ConfigKey) {
                case CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG:
                    return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(AdminUserLoginConfig));
                case CONFIG_KEY.SOCIAL_USER_LOGIN_CONFIG:
                    return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(SocialUserLoginConfig));
                case CONFIG_KEY.SESSION_ADMIN_USER_CONFIG:
                    return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(SessionAdminUserConfig));
                case CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG:
                    return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(SessionSocialUserConfig));
                default:
                    Error ??= "Invalid config key.";
                    return new JObject();
            }
        }
        public static string ConfigKeyToString(CONFIG_KEY ConfigKey, string Error = null)
        {
            Error ??= "";
            switch(ConfigKey) {
                case CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG:
                    return "AdminUserLoginConfig";
                case CONFIG_KEY.SOCIAL_USER_LOGIN_CONFIG:
                    return "SocialUserLoginConfig";
                case CONFIG_KEY.SESSION_ADMIN_USER_CONFIG:
                    return "SessionAdminUserConfig";
                case CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG:
                    return "SessionSocialUserConfig";
                default:
                    Error ??= "Invalid config key.";
                    return "Invalid config key.";
            }
        }
    }
    [Table("admin_base_config")]
    public class AdminBaseConfig : BaseModel
    {
        [Key]
        [Column("id")]
        public int Id { get; private set; }
        [Required]
        [Column("config_key")]
        [StringLength(50)]
        public string ConfigKey { get; set; }
        [NotMapped]
        public JObject Value { get; set; }
        [Column("value", TypeName = "JSON")]
        public string ValueStr  {
            get { return Value.ToString(); }
            set { Value = JsonConvert.DeserializeObject<JObject>(value); }
        }
        [NotMapped]
        public int Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr
        {
            get => BaseStatus.StatusToString(Status, EntityStatus.AdminBaseConfigStatus);
            set => Status = BaseStatus.StatusFromString(value, EntityStatus.AdminBaseConfigStatus);
        }
        public AdminBaseConfig()
        {
            __ModelName = "BaseConfig";
            ConfigKey = "";
            ValueStr = "{}";
            Status = AdminBaseConfigStatus.Enabled;
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = "";
            try {
                var parser = (ParserModels.ParserAdminBaseConfig)Parser;
                ConfigKey = parser.config_key;
                Value = parser.value;
                return true;
            } catch (Exception ex) {
                Error = ex.ToString();
                return false;
            }
        }

        public override bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, object>
            {
                { "id", Id },
                { "config_key", ConfigKey },
                { "value", Value },
                { "status", StatusStr },
#if DEBUG
                {"__ModelName", __ModelName }
#endif
            };
            return true;
        }

        public static List<AdminBaseConfig> GetDefaultData()
        {
            List<AdminBaseConfig> ListData = new()
            {
                new AdminBaseConfig() {
                    Id = 1,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG),
                    Status = AdminBaseConfigStatus.Enabled
                },
                new AdminBaseConfig() {
                    Id = 2,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.SOCIAL_USER_LOGIN_CONFIG),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.SOCIAL_USER_LOGIN_CONFIG),
                    Status = AdminBaseConfigStatus.Enabled
                },
                new AdminBaseConfig() {
                    Id = 3,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG),
                    Status = AdminBaseConfigStatus.Enabled
                },
                new AdminBaseConfig() {
                    Id = 4,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG),
                    Status = AdminBaseConfigStatus.Enabled
                },
            };
            return ListData;
        }
    }
}
