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
using System.Linq;

#nullable disable

namespace DatabaseAccess.Context.Models
{
    public enum CONFIG_KEY
    {
        INVALID                             = 0,
        ADMIN_USER_LOGIN_CONFIG             = 1,
        SOCIAL_USER_LOGIN_CONFIG            = 2,
        SESSION_ADMIN_USER_CONFIG           = 3,
        SESSION_SOCIAL_USER_CONFIG          = 4,
        EMAIL_CLIENT_CONFIG                 = 5,
        SOCIAL_USER_CONFIRM_CONFIG          = 6,
        UI_CONFIG                           = 7,
        PUBLIC_CONFIG                       = 8,
        UPLOAD_FILE_CONFIG                  = 9,
        NOTIFICATION                        = 10,
        USER_IDLE                           = 11,
        ADMIN_USER_IDLE                     = 12,
        PASSWORD_POLICY                     = 13,
        ADMIN_PASSWORD_POLICY               = 14,
    }

    public enum SUB_CONFIG_KEY
    {
        ALL                                         = -1,
        INVALID                                     = 0,
        NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE         = 1,
        LOCK_TIME                                   = 2,
        EXPIRY_TIME                                 = 3,
        EXTENSION_TIME                              = 4,
        LIMIT_SENDER                                = 5,
        TEMPLATE_USER_SIGNUP                        = 6,
        NUMBER_OF_TIMES_ALLOW_CONFIRM_FAILURE       = 7,
        PREFIX_URL                                  = 8,
        HOST_NAME                                   = 9,
        MAX_LENGTH_OF_SINGLE_FILE                   = 10,
        INTERVAL_TIME                               = 11,
        IDLE                                        = 12,
        TIMEOUT                                     = 13,
        PING                                        = 14,
        MIN_LEN                                     = 15,
        MAX_LEN                                     = 16,
        MIN_UPPER_CHAR                              = 17,
        MIN_LOWER_CHAR                              = 18,
        MIN_NUMBER_CHAR                             = 19,
        MIN_SPECIAL_CHAR                            = 20,
        REQUIRED_CHANGE_EXPIRED_PASSWORD            = 21,
    }

    public static class DefaultBaseConfig
    {
        #region Default Config
        public static readonly Dictionary<string, int> AdminUserLoginConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE),    5 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.LOCK_TIME),                              360 },
        };
        public static readonly Dictionary<string, int> SocialUserLoginConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE),    5 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.LOCK_TIME),                              360 },
        };
        public static readonly Dictionary<string, int> SessionAdminUserConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.EXPIRY_TIME),        5 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.EXTENSION_TIME),     5 },
        };
        public static readonly Dictionary<string, int> SessionSocialUserConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.EXPIRY_TIME),        5 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.EXTENSION_TIME),     5 },
        };
        public static readonly Dictionary<string, object> EmailClientConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.LIMIT_SENDER),           5 },
            {
                SubConfigKeyToString(SUB_CONFIG_KEY.TEMPLATE_USER_SIGNUP),
                @"<p>Dear @Model.UserName,</p>"
                    + @"<p>Confirm link here: <a href='@Model.ConfirmLink'>@Model.ConfirmLink</a><br>"
                    + @"Send datetime: @Model.DateTimeSend</p>"
                    + @"<p>Thanks for your register.</p>"
            },
        };
        public static readonly Dictionary<string, object> SocialUserConfirmConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.EXPIRY_TIME),                                2880 }, // 2 days
            { SubConfigKeyToString(SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_CONFIRM_FAILURE),      3 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.PREFIX_URL),                                 "/auth/confirm-account" },
            { SubConfigKeyToString(SUB_CONFIG_KEY.HOST_NAME),                                  "http://localhost:4200" },
        };
        public static readonly Dictionary<string, object> UploadFileConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.MAX_LENGTH_OF_SINGLE_FILE),   5242880 }, // byte ~ 5MB
        };
        public static readonly Dictionary<string, object> Notification = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.INTERVAL_TIME),   120 }, // minute
        };
        public static readonly Dictionary<string, object> UserIdle = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.IDLE),       300 }, // sec
            { SubConfigKeyToString(SUB_CONFIG_KEY.TIMEOUT),    10 }, // sec
            { SubConfigKeyToString(SUB_CONFIG_KEY.PING),       10 }, // sec
        };
        public static readonly Dictionary<string, object> AdminUserIdle = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.IDLE),       300 }, // sec
            { SubConfigKeyToString(SUB_CONFIG_KEY.TIMEOUT),    10 }, // sec
            { SubConfigKeyToString(SUB_CONFIG_KEY.PING),       10 }, // sec
        };
        public static readonly Dictionary<string, object> PasswordPolicy = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_LEN),                                 5 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MAX_LEN),                                 25 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_UPPER_CHAR),                          1 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_LOWER_CHAR),                          1 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_NUMBER_CHAR),                         1 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_SPECIAL_CHAR),                        1 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.EXPIRY_TIME),                             30 }, // days
            { SubConfigKeyToString(SUB_CONFIG_KEY.REQUIRED_CHANGE_EXPIRED_PASSWORD),        false },
        };
        public static readonly Dictionary<string, object> AdminPasswordPolicy = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_LEN),                                 5 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MAX_LEN),                                 25 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_UPPER_CHAR),                          1 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_LOWER_CHAR),                          1 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_NUMBER_CHAR),                         1 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_SPECIAL_CHAR),                        1 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.EXPIRY_TIME),                             30 }, // days
            { SubConfigKeyToString(SUB_CONFIG_KEY.REQUIRED_CHANGE_EXPIRED_PASSWORD),        true },
        };
        public static readonly Dictionary<string, object> UIConfig = new() {};
        public static readonly Dictionary<string, object> PublicConfig = new() {
            // { "UIConfig", "all" } --> mean all config in 'UIConfig' is public
            // { "EmailClientConfig", "limit_sender" } --> mean 'limit_sender' in 'EmailClientConfig' is public config
            { ConfigKeyToString(CONFIG_KEY.UI_CONFIG),                      SubConfigKeyToString(SUB_CONFIG_KEY.ALL) },
            { ConfigKeyToString(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG),      SubConfigKeyToString(SUB_CONFIG_KEY.ALL) },
            { ConfigKeyToString(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG),     SubConfigKeyToString(SUB_CONFIG_KEY.ALL) },
            { ConfigKeyToString(CONFIG_KEY.UPLOAD_FILE_CONFIG),             SubConfigKeyToString(SUB_CONFIG_KEY.ALL) },
            { ConfigKeyToString(CONFIG_KEY.USER_IDLE),                      SubConfigKeyToString(SUB_CONFIG_KEY.ALL) },
            { ConfigKeyToString(CONFIG_KEY.ADMIN_USER_IDLE),                SubConfigKeyToString(SUB_CONFIG_KEY.ALL) },
        };
        public static readonly string[] DEFAULT_CONFIG_KEYS = new string[]{
            ConfigKeyToString(CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG),
            ConfigKeyToString(CONFIG_KEY.SOCIAL_USER_LOGIN_CONFIG),
            ConfigKeyToString(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG),
            ConfigKeyToString(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG),
            ConfigKeyToString(CONFIG_KEY.EMAIL_CLIENT_CONFIG),
            ConfigKeyToString(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG),
            ConfigKeyToString(CONFIG_KEY.UI_CONFIG),
            ConfigKeyToString(CONFIG_KEY.PUBLIC_CONFIG),
            ConfigKeyToString(CONFIG_KEY.UPLOAD_FILE_CONFIG),
            ConfigKeyToString(CONFIG_KEY.NOTIFICATION),
            ConfigKeyToString(CONFIG_KEY.USER_IDLE),
            ConfigKeyToString(CONFIG_KEY.ADMIN_USER_IDLE),
            ConfigKeyToString(CONFIG_KEY.PASSWORD_POLICY),
            ConfigKeyToString(CONFIG_KEY.ADMIN_PASSWORD_POLICY),
        };
        #endregion
        public static JObject GetConfig(CONFIG_KEY ConfigKey, string Error = default)
        {
            Error ??= string.Empty;
            if (ConfigKey == CONFIG_KEY.INVALID) {
                Error ??= "Invalid config key.";
                return new JObject();
            }
            var config = typeof(DefaultBaseConfig)
                .GetField(DefaultBaseConfig.ConfigKeyToString(ConfigKey))
                .GetValue(null);
            return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(config));
        }
        public static CONFIG_KEY StringToConfigKey(string ConfigKey, string Error = default)
        {
            Error ??= string.Empty;
            
            foreach (CONFIG_KEY key in Enum.GetValues(typeof(CONFIG_KEY))) {
                if (key == CONFIG_KEY.INVALID) {
                    continue;
                }
                var keyName = string.Empty;
                if (key == CONFIG_KEY.UI_CONFIG) {
                    keyName = "UIConfig";
                } else {
                    keyName = string.Join(
                        string.Empty,
                        key.ToString().ToLower().Split('_')
                            .Select(str => char.ToUpper(str[0]) + str.Substring(1))
                            .ToArray()
                    );
                }

                if (ConfigKey == keyName) {
                    return key;
                }
            }
            Error ??= "Invalid config key.";
            return CONFIG_KEY.INVALID;
        }
        public static string ConfigKeyToString(CONFIG_KEY ConfigKey, string Error = default)
        {
            Error ??= string.Empty;
            foreach (CONFIG_KEY key in Enum.GetValues(typeof(CONFIG_KEY))) {
                if (key == CONFIG_KEY.INVALID) {
                    continue;
                }
                var keyName = string.Empty;
                if (key == CONFIG_KEY.UI_CONFIG) {
                    keyName = "UIConfig";
                } else {
                    keyName = string.Join(
                        string.Empty,
                        key.ToString().ToLower().Split('_')
                            .Select(str => char.ToUpper(str[0]) + str.Substring(1))
                            .ToArray()
                    );
                }

                if (ConfigKey == key) {
                    return keyName;
                }
            }
            Error ??= "Invalid config key.";
            return Error;
        }
        public static SUB_CONFIG_KEY StringToSubConfigKey(string SubConfigKey, string Error = default)
        {
            Error ??= string.Empty;
            foreach (SUB_CONFIG_KEY subKey in Enum.GetValues(typeof(SUB_CONFIG_KEY))) {
                if (subKey == SUB_CONFIG_KEY.INVALID) {
                    continue;
                }
                var keyName = subKey.ToString().ToLower();
                if (keyName == SubConfigKey) {
                    return subKey;
                }
            }

            Error ??= "Invalid sub config key.";
            return SUB_CONFIG_KEY.INVALID;
        }
        public static string SubConfigKeyToString(SUB_CONFIG_KEY SubConfigKey, string Error = default)
        {
            Error ??= string.Empty;
            foreach (SUB_CONFIG_KEY subKey in Enum.GetValues(typeof(SUB_CONFIG_KEY))) {
                if (subKey == SUB_CONFIG_KEY.INVALID) {
                    continue;
                }
                var keyName = subKey.ToString().ToLower();
                if (subKey == SubConfigKey) {
                    return keyName;
                }
            }

            Error ??= "Invalid sub config key.";
            return Error;
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
        public string ValueStr {
            get { return Value.ToString(); }
            set { Value = JsonConvert.DeserializeObject<JObject>(value); }
        }
        [NotMapped]
        public EntityStatus Status { get; set; }
        [Required]
        [Column("status")]
        [StringLength(15)]
        public string StatusStr {
            get => Status.ToString();
            set => Status = new EntityStatus(EntityStatusType.AdminBaseConfig, value);
        }
        public AdminBaseConfig()
        {
            __ModelName = "BaseConfig";
            ConfigKey = string.Empty;
            ValueStr = "{}";
            Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled);
        }

        public override bool Parse(IBaseParserModel Parser, out string Error)
        {
            Error = string.Empty;
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
        public override JObject GetPublicJsonObject(List<string> publicFields = default) {
            if (publicFields == default) {
                publicFields = new List<string>(){
                    "config_key",
                    "value",
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
        public static List<AdminBaseConfig> GetDefaultData()
        {
            List<AdminBaseConfig> ListData = new()
            {
                new AdminBaseConfig() {
                    Id = 1,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG),
                    Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                },
                new AdminBaseConfig() {
                    Id = 2,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.SOCIAL_USER_LOGIN_CONFIG),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.SOCIAL_USER_LOGIN_CONFIG),
                    Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                },
                new AdminBaseConfig() {
                    Id = 3,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG),
                    Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                },
                new AdminBaseConfig() {
                    Id = 4,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG),
                    Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                },
                new AdminBaseConfig() {
                    Id = 5,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.EMAIL_CLIENT_CONFIG),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.EMAIL_CLIENT_CONFIG),
                    Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                },
                new AdminBaseConfig() {
                    Id = 6,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG),
                    Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                },
                new AdminBaseConfig() {
                    Id = 7,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.UI_CONFIG),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.UI_CONFIG),
                    Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                },
                new AdminBaseConfig() {
                    Id = 8,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.PUBLIC_CONFIG),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.PUBLIC_CONFIG),
                    Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                },
                new AdminBaseConfig() {
                    Id = 9,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.UPLOAD_FILE_CONFIG),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.UPLOAD_FILE_CONFIG),
                    Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                },
                new AdminBaseConfig() {
                    Id = 10,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.NOTIFICATION),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.NOTIFICATION),
                    Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                },
                new AdminBaseConfig() {
                    Id = 11,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.USER_IDLE),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.USER_IDLE),
                    Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                },
                new AdminBaseConfig() {
                    Id = 12,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.ADMIN_USER_IDLE),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.ADMIN_USER_IDLE),
                    Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                },
                new AdminBaseConfig() {
                    Id = 13,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.PASSWORD_POLICY),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.PASSWORD_POLICY),
                    Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                },
                new AdminBaseConfig() {
                    Id = 14,
                    ConfigKey = DefaultBaseConfig.ConfigKeyToString(CONFIG_KEY.ADMIN_PASSWORD_POLICY),
                    Value = DefaultBaseConfig.GetConfig(CONFIG_KEY.ADMIN_PASSWORD_POLICY),
                    Status = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                },
            };
            return ListData;
        }
    }
}
