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
    }

    public enum SUB_CONFIG_KEY
    {
        ALL                                         = -1,
        INVALID                                     = 0,
        NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE         = 1,
        LOCK_TIME                                   = 2,
        EXPIRY_TIME                                 = 3,
        EXTENSION_TIME                              = 4,
        EMAIL_LIMIT_SENDER                          = 5,
        EMAIL_TEMPLATE_USER_SIGNUP                  = 6,
        NUMBER_OF_TIMES_ALLOW_CONFIRM_FAILURE       = 7,
        PREFIX_URL                                  = 8,
        HOST_NAME                                   = 9,
        MAX_LENGTH_OF_SINGLE_FILE                   = 10,
        INTERVAL_TIME                               = 11,
    }

    public static class DefaultBaseConfig
    {
        #region Default Config
        public static readonly Dictionary<string, int> AdminUserLoginConfig = new() {
            { "number_of_times_allow_login_failure", 5 },
            { "lock_time", 360 },
        };
        public static readonly Dictionary<string, int> SocialUserLoginConfig = new() {
            { "number_of_times_allow_login_failure", 5 },
            { "lock_time", 360 },
        };
        public static readonly Dictionary<string, int> SessionAdminUserConfig = new() {
            { "expiry_time", 5 },
            { "extension_time", 5 },
        };
        public static readonly Dictionary<string, int> SessionSocialUserConfig = new() {
            { "expiry_time", 5 },
            { "extension_time", 5 },
        };
        public static readonly Dictionary<string, object> EmailClientConfig = new() {
            { "limit_sender", 5 },
            { "template_user_signup", @"<p>Dear @Model.UserName,</p>
                                        <p>Confirm link here: <a href='@Model.ConfirmLink'>@Model.ConfirmLink</a><br>
                                        Send datetime: @Model.DateTimeSend</p>
                                        <p>Thanks for your register.</p>" },
        };
        public static readonly Dictionary<string, object> SocialUserConfirmConfig = new() {
            { "expiry_time", 2880 }, // 2 days
            { "number_of_times_allow_confirm_failure", 3 },
            { "prefix_url", "/auth/confirm-account"},
            { "host_name", "http://localhost:4200" },
        };
        public static readonly Dictionary<string, object> UploadFileConfig = new() {
            { "max_len_of_single_file", 5242880 }, // byte ~ 5MB
        };
        public static readonly Dictionary<string, object> Notification = new() {
            { "interval_time", 120 }, // minute
        };
        public static readonly Dictionary<string, object> UIConfig = new() {};
        public static readonly Dictionary<string, object> PublicConfig = new() {
            // { "UIConfig", "all" } --> mean all config in 'UIConfig' is public
            // { "EmailClientConfig", "limit_sender" } --> mean 'limit_sender' in 'EmailClientConfig' is public config
            { ConfigKeyToString(CONFIG_KEY.UI_CONFIG), SubConfigKeyToString(SUB_CONFIG_KEY.ALL) },
            { ConfigKeyToString(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG), SubConfigKeyToString(SUB_CONFIG_KEY.ALL) },
            { ConfigKeyToString(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG), SubConfigKeyToString(SUB_CONFIG_KEY.ALL) },
            { ConfigKeyToString(CONFIG_KEY.UPLOAD_FILE_CONFIG), SubConfigKeyToString(SUB_CONFIG_KEY.ALL) },
        };
        public static readonly string[] DEFAULT_CONFIG_KEYS = new string[]{
            "AdminUserLoginConfig",
            "SocialUserLoginConfig",
            "SessionAdminUserConfig",
            "SessionSocialUserConfig",
            "EmailClientConfig",
            "SocialUserConfirmConfig",
            "UIConfig",
            "PublicConfig",
            "UploadFileConfig",
            "Notification",
        };
        #endregion
        public static JObject GetConfig(CONFIG_KEY ConfigKey, string Error = default)
        {
            Error ??= string.Empty;
            switch(ConfigKey) {
                case CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG:
                    return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(AdminUserLoginConfig));
                case CONFIG_KEY.SOCIAL_USER_LOGIN_CONFIG:
                    return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(SocialUserLoginConfig));
                case CONFIG_KEY.SESSION_ADMIN_USER_CONFIG:
                    return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(SessionAdminUserConfig));
                case CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG:
                    return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(SessionSocialUserConfig));
                case CONFIG_KEY.EMAIL_CLIENT_CONFIG:
                    return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(EmailClientConfig));
                case CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG:
                    return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(SocialUserConfirmConfig));
                case CONFIG_KEY.UI_CONFIG:
                    return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(UIConfig));
                case CONFIG_KEY.PUBLIC_CONFIG:
                    return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(PublicConfig));
                case CONFIG_KEY.UPLOAD_FILE_CONFIG:
                    return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(UploadFileConfig));
                case CONFIG_KEY.NOTIFICATION:
                    return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(Notification));
                default:
                    Error ??= "Invalid config key.";
                    return new JObject();
            }
        }
        public static CONFIG_KEY StringToConfigKey(string ConfigKey, string Error = default)
        {
            Error ??= string.Empty;
            switch(ConfigKey) {
                case "AdminUserLoginConfig":
                    return CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG;
                case "SocialUserLoginConfig":
                    return CONFIG_KEY.SOCIAL_USER_LOGIN_CONFIG;
                case "SessionAdminUserConfig":
                    return CONFIG_KEY.SESSION_ADMIN_USER_CONFIG;
                case "SessionSocialUserConfig":
                    return CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG;
                case "EmailClientConfig":
                    return CONFIG_KEY.EMAIL_CLIENT_CONFIG;
                case "SocialUserConfirmConfig":
                    return CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG;
                case "UIConfig":
                    return CONFIG_KEY.UI_CONFIG;
                case "PublicConfig":
                    return CONFIG_KEY.PUBLIC_CONFIG;
                case "UploadFileConfig":
                    return CONFIG_KEY.UPLOAD_FILE_CONFIG;
                case "Notification":
                    return CONFIG_KEY.NOTIFICATION;
                default:
                    Error ??= "Invalid config key.";
                    return CONFIG_KEY.INVALID;
            }
        }
        public static string ConfigKeyToString(CONFIG_KEY ConfigKey, string Error = default)
        {
            Error ??= string.Empty;
            switch(ConfigKey) {
                case CONFIG_KEY.ADMIN_USER_LOGIN_CONFIG:
                    return "AdminUserLoginConfig";
                case CONFIG_KEY.SOCIAL_USER_LOGIN_CONFIG:
                    return "SocialUserLoginConfig";
                case CONFIG_KEY.SESSION_ADMIN_USER_CONFIG:
                    return "SessionAdminUserConfig";
                case CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG:
                    return "SessionSocialUserConfig";
                case CONFIG_KEY.EMAIL_CLIENT_CONFIG:
                    return "EmailClientConfig";
                case CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG:
                    return "SocialUserConfirmConfig";
                case CONFIG_KEY.UI_CONFIG:
                    return "UIConfig";
                case CONFIG_KEY.PUBLIC_CONFIG:
                    return "PublicConfig";
                case CONFIG_KEY.UPLOAD_FILE_CONFIG:
                    return "UploadFileConfig";
                case CONFIG_KEY.NOTIFICATION:
                    return "Notification";
                default:
                    Error ??= "Invalid config key.";
                    return "Invalid config key.";
            }
        }
        public static SUB_CONFIG_KEY StringToSubConfigKey(string SubConfigKey, string Error = default)
        {
            Error ??= string.Empty;
            switch(SubConfigKey) {
                case "all":
                    return SUB_CONFIG_KEY.ALL;
                case "number_of_times_allow_login_failure":
                    return SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE;
                case "lock_time":
                    return SUB_CONFIG_KEY.LOCK_TIME;
                case "expiry_time":
                    return SUB_CONFIG_KEY.EXPIRY_TIME;
                case "extension_time":
                    return SUB_CONFIG_KEY.EXTENSION_TIME;
                case "limit_sender":
                    return SUB_CONFIG_KEY.EMAIL_LIMIT_SENDER;
                case "template_user_signup":
                    return SUB_CONFIG_KEY.EMAIL_TEMPLATE_USER_SIGNUP;
                case "number_of_times_allow_confirm_failure":
                    return SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_CONFIRM_FAILURE;
                case "prefix_url":
                    return SUB_CONFIG_KEY.PREFIX_URL;
                case "host_name":
                    return SUB_CONFIG_KEY.HOST_NAME;
                case "max_len_of_single_file":
                    return SUB_CONFIG_KEY.MAX_LENGTH_OF_SINGLE_FILE;
                case "interval_time":
                    return SUB_CONFIG_KEY.INTERVAL_TIME;
                default:
                    Error ??= "Invalid sub config key.";
                    return SUB_CONFIG_KEY.INVALID;
            }
        }
        public static string SubConfigKeyToString(SUB_CONFIG_KEY SubConfigKey, string Error = default)
        {
            Error ??= string.Empty;
            switch(SubConfigKey) {
                case SUB_CONFIG_KEY.ALL:
                    return "all";
                case SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_LOGIN_FAILURE:
                    return "number_of_times_allow_login_failure";
                case SUB_CONFIG_KEY.LOCK_TIME:
                    return "lock_time";
                case SUB_CONFIG_KEY.EXPIRY_TIME:
                    return "expiry_time";
                case SUB_CONFIG_KEY.EXTENSION_TIME:
                    return "extension_time";
                case SUB_CONFIG_KEY.EMAIL_LIMIT_SENDER:
                    return "limit_sender";
                case SUB_CONFIG_KEY.EMAIL_TEMPLATE_USER_SIGNUP:
                    return "template_user_signup";
                case SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_CONFIRM_FAILURE:
                    return "number_of_times_allow_confirm_failure";
                case SUB_CONFIG_KEY.PREFIX_URL:
                    return "prefix_url";
                case SUB_CONFIG_KEY.HOST_NAME:
                    return "host_name";
                case SUB_CONFIG_KEY.MAX_LENGTH_OF_SINGLE_FILE:
                    return "max_len_of_single_file";
                case SUB_CONFIG_KEY.INTERVAL_TIME:
                    return "interval_time";
                default:
                    Error ??= "Invalid sub config key.";
                    return "Invalid sub config key.";
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
            };
            return ListData;
        }
    }
}
