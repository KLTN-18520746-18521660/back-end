using Common;
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
        INVALID                                     = 0,
        ADMIN_USER_LOGIN_CONFIG                     = 1,
        SOCIAL_USER_LOGIN_CONFIG                    = 2,
        SESSION_ADMIN_USER_CONFIG                   = 3,
        SESSION_SOCIAL_USER_CONFIG                  = 4,
        EMAIL_CLIENT_CONFIG                         = 5,
        SOCIAL_USER_CONFIRM_CONFIG                  = 6,
        UI_CONFIG                                   = 7,
        PUBLIC_CONFIG                               = 8,
        UPLOAD_FILE_CONFIG                          = 9,
        NOTIFICATION                                = 10,
        SOCIAL_USER_IDLE                            = 11,
        ADMIN_USER_IDLE                             = 12,
        SOCIAL_PASSWORD_POLICY                      = 13,
        ADMIN_PASSWORD_POLICY                       = 14,
        API_GET_COMMENT_CONFIG                      = 15,
        SOCIAL_FORGOT_PASSWORD_CONFIG               = 16,
        ADMIN_FORGOT_PASSWORD_CONFIG                = 17,
        API_GET_RECOMMEND_POSTS_FOR_POST_CONFIG     = 18,
        API_GET_RECOMMEND_POSTS_FOR_USER_CONFIG     = 19,
    }

    public enum SUB_CONFIG_KEY
    {
        ALL                                         = -1,
        INVALID                                     = 0,
        NUMBER_OF_TIMES_ALLOW_FAILURE               = 1,
        LOCK_TIME                                   = 2,
        EXPIRY_TIME                                 = 3,
        EXTENSION_TIME                              = 4,
        LIMIT_SENDER                                = 5,
        TEMPLATE_USER_SIGNUP                        = 6,
        PREFIX_URL                                  = 7,
        HOST_NAME                                   = 8,
        MAX_LENGTH_OF_SINGLE_FILE                   = 9,
        INTERVAL_TIME                               = 10,
        IDLE                                        = 11,
        TIMEOUT                                     = 12,
        PING                                        = 13,
        MIN_LEN                                     = 14,
        MAX_LEN                                     = 15,
        MIN_UPPER_CHAR                              = 16,
        MIN_LOWER_CHAR                              = 17,
        MIN_NUMBER_CHAR                             = 18,
        MIN_SPECIAL_CHAR                            = 19,
        REQUIRED_CHANGE_EXPIRED_PASSWORD            = 20,
        LIMIT_SIZE_GET_REPLY_COMMENT                = 21,
        TEMPLATE_FORGOT_PASSWORD                    = 22,
        SUBJECT                                     = 23,
        VISTED_FACTOR                               = 24,
        VIEWS_FACTOR                                = 25,
        LIKES_FACTOR                                = 26,
        COMMENTS_FACTOR                             = 27,
        TAGS_FACTOR                                 = 28,
        CATEGORIES_FACTOR                           = 29,
        COMMON_WORDS_SIZE                           = 30,
        COMMON_WORDS_FACTOR                         = 31,
        MAX_SIZE                                    = 32,
        TS_RANK_FACTOR                              = 33,
    }

    public static class DEFAULT_BASE_CONFIG
    {
        #region Default Config
        public static readonly Dictionary<string, int> AdminUserLoginConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_FAILURE),           5 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.LOCK_TIME),                               360 },
        };
        public static readonly Dictionary<string, int> SocialUserLoginConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_FAILURE),           5 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.LOCK_TIME),                               360 },
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
                @"<p>Dear @Model.DisplayName,</p>"
                + @"<p>Confirm link <a href='@Model.ConfirmLink'>here</a><br>"
                + @"Send datetime: @Model.DateTimeSend</p>"
                + @"<p>Thanks for your register.</p>"
            },
            {
                SubConfigKeyToString(SUB_CONFIG_KEY.TEMPLATE_FORGOT_PASSWORD),
                @"<p>Dear @Model.DisplayName,</p>"
                + @"<p>Click <a href='@Model.ResetPasswordLink'>here</a><br> to reset password"
                + @"Send datetime: @Model.DateTimeSend.</p>"
            },
        };
        public static readonly Dictionary<string, object> SocialUserConfirmConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.EXPIRY_TIME),                             2880 }, // 2 days
            { SubConfigKeyToString(SUB_CONFIG_KEY.TIMEOUT),                                 720 }, // 12 hour
            { SubConfigKeyToString(SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_FAILURE),           3 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.PREFIX_URL),                              "/auth/confirm-account" },
            { SubConfigKeyToString(SUB_CONFIG_KEY.HOST_NAME),                               "http://localhost:7005" },
            { SubConfigKeyToString(SUB_CONFIG_KEY.SUBJECT),                                 $"[{ BaseConfigurationDB.APP_NAME }] Confirm signup." },
        };
        public static readonly Dictionary<string, object> SocialForgotPasswordConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.EXPIRY_TIME),                             2880 }, // 2 days
            { SubConfigKeyToString(SUB_CONFIG_KEY.TIMEOUT),                                 720 }, // 12 hour
            { SubConfigKeyToString(SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_FAILURE),           1 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.PREFIX_URL),                              "/auth/new-password" },
            { SubConfigKeyToString(SUB_CONFIG_KEY.HOST_NAME),                               "http://localhost:7005" },
            { SubConfigKeyToString(SUB_CONFIG_KEY.SUBJECT),                                 $"[{ BaseConfigurationDB.APP_NAME }] Forgot password." },
        };
        public static readonly Dictionary<string, object> AdminForgotPasswordConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.EXPIRY_TIME),                             2880 }, // 2 days
            { SubConfigKeyToString(SUB_CONFIG_KEY.TIMEOUT),                                 720 }, // 12 hour
            { SubConfigKeyToString(SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_FAILURE),           1 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.PREFIX_URL),                              "/admin/new-password" },
            { SubConfigKeyToString(SUB_CONFIG_KEY.HOST_NAME),                               "http://localhost:7005" },
            { SubConfigKeyToString(SUB_CONFIG_KEY.SUBJECT),                                 $"[{ BaseConfigurationDB.APP_NAME }] Forgot password." },
        };
        public static readonly Dictionary<string, object> UploadFileConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.MAX_LENGTH_OF_SINGLE_FILE),   5242880 }, // byte ~ 5MB
        };
        public static readonly Dictionary<string, object> Notification = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.INTERVAL_TIME),   120 }, // minute
        };
        public static readonly Dictionary<string, object> SocialUserIdle = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.IDLE),       300 }, // sec
            { SubConfigKeyToString(SUB_CONFIG_KEY.TIMEOUT),    10 }, // sec
            { SubConfigKeyToString(SUB_CONFIG_KEY.PING),       10 }, // sec
        };
        public static readonly Dictionary<string, object> AdminUserIdle = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.IDLE),       300 }, // sec
            { SubConfigKeyToString(SUB_CONFIG_KEY.TIMEOUT),    10 }, // sec
            { SubConfigKeyToString(SUB_CONFIG_KEY.PING),       10 }, // sec
        };
        public static readonly Dictionary<string, object> SocialPasswordPolicy = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_LEN),                                 5 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MAX_LEN),                                 25 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_UPPER_CHAR),                          0 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_LOWER_CHAR),                          0 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_NUMBER_CHAR),                         0 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_SPECIAL_CHAR),                        0 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.EXPIRY_TIME),                             30 }, // days
            { SubConfigKeyToString(SUB_CONFIG_KEY.REQUIRED_CHANGE_EXPIRED_PASSWORD),        true },
        };
        public static readonly Dictionary<string, object> AdminPasswordPolicy = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_LEN),                                 5 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MAX_LEN),                                 25 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_UPPER_CHAR),                          0 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_LOWER_CHAR),                          0 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_NUMBER_CHAR),                         0 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MIN_SPECIAL_CHAR),                        0 },
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
            { ConfigKeyToString(CONFIG_KEY.SOCIAL_USER_IDLE),               SubConfigKeyToString(SUB_CONFIG_KEY.ALL) },
            { ConfigKeyToString(CONFIG_KEY.ADMIN_USER_IDLE),                SubConfigKeyToString(SUB_CONFIG_KEY.ALL) },
            { ConfigKeyToString(CONFIG_KEY.SOCIAL_PASSWORD_POLICY),         SubConfigKeyToString(SUB_CONFIG_KEY.ALL) },
            { ConfigKeyToString(CONFIG_KEY.ADMIN_PASSWORD_POLICY),          SubConfigKeyToString(SUB_CONFIG_KEY.ALL) },
        };
        public static readonly Dictionary<string, object> APIGetCommentConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.LIMIT_SIZE_GET_REPLY_COMMENT),                                    2 },
        };
        public static readonly Dictionary<string, object> APIGetRecommendPostsForPostConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.VISTED_FACTOR),                                       5 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.VIEWS_FACTOR),                                        1 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.LIKES_FACTOR),                                        2 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.COMMENTS_FACTOR),                                     1 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.TAGS_FACTOR),                                         100 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.CATEGORIES_FACTOR),                                   100 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.COMMON_WORDS_FACTOR),                                 500 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.TS_RANK_FACTOR),                                      10 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.COMMON_WORDS_SIZE),                                   10 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MAX_SIZE),                                            50 },
        };
        public static readonly Dictionary<string, object> APIGetRecommendPostsForUserConfig = new() {
            { SubConfigKeyToString(SUB_CONFIG_KEY.VISTED_FACTOR),                                       5 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.VIEWS_FACTOR),                                        1 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.LIKES_FACTOR),                                        2 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.COMMENTS_FACTOR),                                     1 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.TAGS_FACTOR),                                         100 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.CATEGORIES_FACTOR),                                   100 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.COMMON_WORDS_FACTOR),                                 500 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.COMMON_WORDS_SIZE),                                   10 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.TS_RANK_FACTOR),                                      10 },
            { SubConfigKeyToString(SUB_CONFIG_KEY.MAX_SIZE),                                            50 },
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
            ConfigKeyToString(CONFIG_KEY.SOCIAL_USER_IDLE),
            ConfigKeyToString(CONFIG_KEY.ADMIN_USER_IDLE),
            ConfigKeyToString(CONFIG_KEY.SOCIAL_PASSWORD_POLICY),
            ConfigKeyToString(CONFIG_KEY.ADMIN_PASSWORD_POLICY),
            ConfigKeyToString(CONFIG_KEY.API_GET_COMMENT_CONFIG),
            ConfigKeyToString(CONFIG_KEY.SOCIAL_FORGOT_PASSWORD_CONFIG),
            ConfigKeyToString(CONFIG_KEY.ADMIN_FORGOT_PASSWORD_CONFIG),
            ConfigKeyToString(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_POST_CONFIG),
            ConfigKeyToString(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_USER_CONFIG),
        };
        #endregion
        public static JObject GetConfig(CONFIG_KEY ConfigKey, string Error = default)
        {
            Error ??= string.Empty;
            if (ConfigKey == CONFIG_KEY.INVALID) {
                Error ??= "Invalid config key.";
                return new JObject();
            }
            var Config = typeof(DEFAULT_BASE_CONFIG)
                .GetField(ConfigKeyToString(ConfigKey))
                .GetValue(null);
            return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(Config));
        }
        public static T GetConfigValue<T>(CONFIG_KEY ConfigKey, SUB_CONFIG_KEY SubConfigKey, string Error = default)
        {
            if (typeof(T) != typeof(string) && typeof(T) != typeof(int) && typeof(T) != typeof(bool)) {
                Error = $"GetConfigValue. Unsupport convert type: { typeof(T) }";
                return default;
            }

            Error ??= string.Empty;
            var ConfigKeyStr    = ConfigKeyToString(ConfigKey);
            var SubConfigKeyStr = SubConfigKeyToString(SubConfigKey);
            var Config          = GetConfig(ConfigKey, Error);

            if (Error != default && Error != string.Empty) {
                return default;
            }
            if (Config[SubConfigKeyStr] == default) {
                Error = $"Invalid pair, config_key: { ConfigKeyStr }, sub_config_key: { SubConfigKeyStr }.";
                return default;
            }
            return (T) System.Convert.ChangeType(Config[SubConfigKeyStr], typeof(T));
        }
        #region Convert Key
        public static CONFIG_KEY StringToConfigKey(string ConfigKey, string Error = default)
        {
            Error ??= string.Empty;
            
            foreach (CONFIG_KEY key in Enum.GetValues(typeof(CONFIG_KEY))) {
                if (key == CONFIG_KEY.INVALID) {
                    continue;
                }
                var keyName = string.Empty;
                switch (key) {
                    case CONFIG_KEY.UI_CONFIG:
                    case CONFIG_KEY.API_GET_COMMENT_CONFIG:
                    case CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_POST_CONFIG:
                    case CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_USER_CONFIG:
                        // UI_CONFIG --> UIConfig
                        keyName = string.Join(
                            string.Empty,
                            key.ToString().ToLower().Split('_')
                                .Select((str, index) => index != 0 ? char.ToUpper(str[0]) + str.Substring(1) : str.ToUpper())
                                .ToArray()
                        );
                        break;
                    default:
                        // EMAIL_CONFIG --> EmailConfig
                        keyName = string.Join(
                            string.Empty,
                            key.ToString().ToLower().Split('_')
                                .Select(str => char.ToUpper(str[0]) + str.Substring(1))
                                .ToArray()
                        );
                        break;
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
                switch (key) {
                    case CONFIG_KEY.UI_CONFIG:
                    case CONFIG_KEY.API_GET_COMMENT_CONFIG:
                    case CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_POST_CONFIG:
                    case CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_USER_CONFIG:
                        // UI_CONFIG --> UIConfig
                        keyName = string.Join(
                            string.Empty,
                            key.ToString().ToLower().Split('_')
                                .Select((str, index) => index != 0 ? char.ToUpper(str[0]) + str.Substring(1) : str.ToUpper())
                                .ToArray()
                        );
                        break;
                    default:
                        // EMAIL_CONFIG --> EmailConfig
                        keyName = string.Join(
                            string.Empty,
                            key.ToString().ToLower().Split('_')
                                .Select(str => char.ToUpper(str[0]) + str.Substring(1))
                                .ToArray()
                        );
                        break;
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
        public static JObject GetValueFormatOfConfigKey(CONFIG_KEY ConfigKey, string Error = default)
        {
            Error ??= string.Empty;
            var ConfigKeyStr    = ConfigKeyToString(ConfigKey);
            var Config          = GetConfig(ConfigKey, Error);

            if (Error != default && Error != string.Empty) {
                return default;
            }
            switch (ConfigKey) {
                case CONFIG_KEY.UI_CONFIG:
                    return new JObject(){
                        { "any", "any" }
                    };
                default:
                    break;
            }

            var Ret = new JObject();
            foreach (var It in Config) {
                Ret[It.Key]         = new JObject();
                Ret[It.Key]["type"] = It.Value.Type.ToString().ToLower();

                var SubKey = StringToSubConfigKey(It.Key);
                switch (SubKey) {
                    case SUB_CONFIG_KEY.TEMPLATE_FORGOT_PASSWORD:
                    case SUB_CONFIG_KEY.TEMPLATE_USER_SIGNUP:
                        Ret[It.Key]["type"] = "text";
                        Ret[It.Key]["contains"] = Utils.ObjectToJsonToken(Utils.GetModelProperties((string)It.Value));
                        break;
                    default:
                        break;
                }

                if ((string)Ret[It.Key]["type"] == JTokenType.Integer.ToString().ToLower()) {
                    Ret[It.Key]["min"] = 0;
                    Ret[It.Key]["regex"] = "[0-9]{1,8}";
                }
            }
            return Ret;
        }
        #endregion
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
            get { return Value.ToString(Formatting.None); }
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
            var ListData    = new List<AdminBaseConfig>();
            var InitId      = 1;
            foreach (var Key in DEFAULT_BASE_CONFIG.DEFAULT_CONFIG_KEYS) {
                ListData.Add(
                    new AdminBaseConfig() {
                        Id          = InitId++,
                        ConfigKey   = Key,
                        Value       = DEFAULT_BASE_CONFIG.GetConfig(DEFAULT_BASE_CONFIG.StringToConfigKey(Key)),
                        Status      = new EntityStatus(EntityStatusType.AdminBaseConfig, StatusType.Enabled)
                    }
                );
            }
            return ListData;
        }
    }
}
