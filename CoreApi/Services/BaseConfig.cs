
using Common;
using CoreApi.Common;
using CoreApi.Common.Base;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreApi.Services
{
    public class BaseConfig : BaseSingletonService
    {
        List<AdminBaseConfig> Configs;
        SemaphoreSlim Gate;
        bool IsReloadConfig;
        public BaseConfig(IServiceProvider _IServiceProvider)
            : base(_IServiceProvider)
        {
            __ServiceName = "BaseConfig";
            InitConfig();
            Gate = new SemaphoreSlim(1);
            IsReloadConfig = false;
            WriteLog(LOG_LEVEL.INFO, string.Empty, "Init load all config successfully.");
        }

        protected void InitConfig()
        {
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __DBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                foreach (var key in DEFAULT_BASE_CONFIG.DEFAULT_CONFIG_KEYS) {
                    if (__DBContext.AdminBaseConfigs.Count(e => e.ConfigKey == key) == 0) {
                        __DBContext.AdminBaseConfigs.Add(
                            new AdminBaseConfig() {
                                ConfigKey = key,
                                Value = DEFAULT_BASE_CONFIG.GetConfig(DEFAULT_BASE_CONFIG.StringToConfigKey(key)),
                            }
                        );
                        if (__DBContext.SaveChanges() <= 0) {
                            throw new Exception("InitConfig failed.");
                        }
                    }
                }
                Configs = __DBContext.AdminBaseConfigs.ToList();
            }
        }

        protected async Task InitConfigAsync()
        {
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __DBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                foreach (var key in DEFAULT_BASE_CONFIG.DEFAULT_CONFIG_KEYS) {
                    if (await __DBContext.AdminBaseConfigs.CountAsync(e => e.ConfigKey == key) == 0) {
                        await __DBContext.AdminBaseConfigs.AddAsync(
                            new AdminBaseConfig() {
                                ConfigKey = key,
                                Value = DEFAULT_BASE_CONFIG.GetConfig(DEFAULT_BASE_CONFIG.StringToConfigKey(key)),
                            }
                        );
                        if (await __DBContext.SaveChangesAsync() <= 0) {
                            throw new Exception("InitConfigAsync failed.");
                        }
                    }
                }
                Configs = await __DBContext.AdminBaseConfigs.ToListAsync();
            }
        }

        public async Task<(ErrorCodes ErrorCode, string[] Errors)> ReLoadConfig()
        {
            await Gate.WaitAsync();
            IsReloadConfig = true;
            await InitConfigAsync();
            IsReloadConfig = false;
            Gate.Release();
            WriteLog(LOG_LEVEL.INFO, string.Empty, "Reload all base config successfully.");

            #region Read other service
            var Errors          = new List<string>();
            var __EmailSender   = (EmailSender)__ServiceProvider.GetService(typeof(EmailSender));

            Errors.AddRange(__EmailSender.ReloadEmailConfig());
            Errors = Errors.Where(e => e != string.Empty).ToList();
            #endregion
            if (Errors.Count != 0) {
                WriteLog(LOG_LEVEL.ERROR, string.Empty, "Reload other services failed.");
                return (ErrorCodes.INTERNAL_SERVER_ERROR, Errors.ToArray());
            } else {
                WriteLog(LOG_LEVEL.INFO, string.Empty, "Reload other services successfully.");
                return (ErrorCodes.NO_ERROR, default);
            }
        }

        public (JObject Value, string Error) GetAllConfig()
        {
            while(IsReloadConfig);
            Dictionary<string, JObject> ret = new Dictionary<string, JObject>();
            Configs.ForEach(e => {
                if (!ret.ContainsKey(e.ConfigKey)) {
                    ret.Add(e.ConfigKey, e.Value);
                }
            });
            return (JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(ret)), string.Empty);
        }

        public (JObject Value, string Error) GetAllPublicConfig()
        {
            while(IsReloadConfig);
            List<AdminBaseConfig> configs = Utils.DeepClone<List<AdminBaseConfig>>(Configs);
            var (publicConfig, error) = GetConfigValue(CONFIG_KEY.PUBLIC_CONFIG);

            Dictionary<string, JObject> ret = new Dictionary<string, JObject>();
            foreach (var it in publicConfig) {
                if (DEFAULT_BASE_CONFIG.StringToConfigKey(it.Key) == CONFIG_KEY.INVALID
                    || DEFAULT_BASE_CONFIG.StringToSubConfigKey(it.Value.ToString()) == SUB_CONFIG_KEY.INVALID
                    || ret.ContainsKey(it.Key)) {
                    continue;
                }

                var found = configs.Where(e => e.ConfigKey == it.Key).FirstOrDefault();
                if (found == default) {
                    continue;
                }

                if (DEFAULT_BASE_CONFIG.StringToSubConfigKey(it.Value.ToString()) == SUB_CONFIG_KEY.ALL) {
                    ret.Remove(it.Key);
                    ret.Add(it.Key, found.Value);
                } else {
                    var valStr = found.Value.Value<string>(it.Value.ToString());
                    var isInt = int.TryParse(valStr, out var valInt);
                    ret.Add(it.Key, new JObject(){
                        { it.Value.ToString(), isInt ? valInt : valStr },
                    });
                }
            }
            return (JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(ret)), string.Empty);
        }

        public bool IsPublicConfig(CONFIG_KEY ConfigKey)
        {
            var (publicConfig, error) = GetConfigValue(CONFIG_KEY.PUBLIC_CONFIG);
            foreach (var it in publicConfig) {
                if (it.Key == DEFAULT_BASE_CONFIG.ConfigKeyToString(ConfigKey)) {
                    return true;
                }
            }
            return false;
        }

        public (JObject Value, string Error) GetPublicConfig(CONFIG_KEY ConfigKey)
        {
            while(IsReloadConfig);
            List<AdminBaseConfig> configs = Utils.DeepClone<List<AdminBaseConfig>>(Configs);
            var (publicConfig, error) = GetConfigValue(CONFIG_KEY.PUBLIC_CONFIG);

            foreach (var it in publicConfig) {
                var found = configs.Where(e => e.ConfigKey == it.Key).FirstOrDefault();
                if (found == default) {
                    return (default, $"Not found config. But key exist on public configs, key: { ConfigKey }");
                }
                if (it.Key == DEFAULT_BASE_CONFIG.ConfigKeyToString(ConfigKey)) {
                    if (DEFAULT_BASE_CONFIG.StringToSubConfigKey(it.Value.ToString()) == SUB_CONFIG_KEY.ALL) {
                        return (found.Value, string.Empty);
                    }
                } else {
                    var valStr = found.Value.Value<string>(it.Value.ToString());
                    var isInt = int.TryParse(valStr, out var valInt);
                    return (new JObject(){
                        { it.Value.ToString(), isInt ? valInt : valStr },
                    }, string.Empty);
                }
            }
            return (default, $"Not found config. key: { ConfigKey }");
        }

        public (JObject Value, string Error) GetConfigValue(CONFIG_KEY ConfigKey)
        {
            while(IsReloadConfig);
            string configKeyStr = DEFAULT_BASE_CONFIG.ConfigKeyToString(ConfigKey);
            var config = Configs
                            .Where<AdminBaseConfig>(e => e.ConfigKey == configKeyStr)
                            .Select(e => e.Value)
                            .DefaultIfEmpty(default)
                            .FirstOrDefault();
            if (config != default) {
                return (config, string.Empty);
            }

            string Error = string.Empty;
            Error = $"Invalid config data. Default vaue will be use. config_key: { configKeyStr }.";
            WriteLog(LOG_LEVEL.WARNING, string.Empty, Error);
            return (DEFAULT_BASE_CONFIG.GetConfig(ConfigKey), Error);
        }

        public (T Value, string Error) GetConfigValue<T>(CONFIG_KEY ConfigKey, SUB_CONFIG_KEY SubConfigKey)
        {
            string Error = string.Empty;
            if (SubConfigKey == SUB_CONFIG_KEY.ALL) {
                Error = $"GetConfigValue. Unsupport get sub config type: { SubConfigKey }";
                throw new Exception(Error);
            }
            while(IsReloadConfig);
            if (typeof(T) != typeof(string) && typeof(T) != typeof(int) && typeof(T) != typeof(float) && typeof(T) != typeof(bool)) {
                Error = $"GetConfigValue. Unsupport convert type: { typeof(T) }";
                throw new Exception(Error);
            }
            string configKeyStr = DEFAULT_BASE_CONFIG.ConfigKeyToString(ConfigKey);
            string subConfigKeyStr = DEFAULT_BASE_CONFIG.SubConfigKeyToString(SubConfigKey);
            var config = Configs
                            .Where<AdminBaseConfig>(e => e.ConfigKey == configKeyStr)
                            .Select(e => e.Value)
                            .DefaultIfEmpty(default)
                            .FirstOrDefault();

            if (config != default && config[subConfigKeyStr] != default) {
                return ((T) System.Convert.ChangeType(config[subConfigKeyStr], typeof(T)), Error);
            } else {
                var defaultConfig = DEFAULT_BASE_CONFIG.GetConfig(ConfigKey, Error);
                if (Error != default && Error != string.Empty) {
                    throw new Exception(Error);
                }
                if (defaultConfig[subConfigKeyStr] == default) {
                    throw new Exception($"Invalid pair, config_key: { configKeyStr }, sub_config_key: { subConfigKeyStr }.");
                }
                Error = $"Invalid config data. Default vaue will be use. config_key: { configKeyStr }, sub_config_key: { subConfigKeyStr }.";
                WriteLog(LOG_LEVEL.WARNING, string.Empty, Error);
                return ((T) System.Convert.ChangeType(defaultConfig[subConfigKeyStr], typeof(T)), Error);
            }
        }

        public async Task<ErrorCodes> UpdateConfig(CONFIG_KEY ConfigKey, JObject ModifyData, Guid UserId)
        {
            var ConfigKeyStr    = DEFAULT_BASE_CONFIG.ConfigKeyToString(ConfigKey);
            using (var Scope = __ServiceProvider.CreateScope())
            {
                var __DBContext                 = Scope.ServiceProvider.GetRequiredService<DBContext>();
                var __AdminAuditLogManagement   = Scope.ServiceProvider.GetRequiredService<AdminAuditLogManagement>();
                var Config = await __DBContext.AdminBaseConfigs
                            .Where<AdminBaseConfig>(e => e.ConfigKey == ConfigKeyStr)
                            .FirstOrDefaultAsync();
                if (Config == default) {
                    WriteLog(LOG_LEVEL.ERROR, string.Empty, $"Missing config key in database: { ConfigKeyStr }");
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
                if (Config.Value.ToString(Formatting.None) == ModifyData.ToString(Formatting.None)) {
                    return ErrorCodes.NO_CHANGE_DETECTED;
                }
                var OldValue   = Utils.DeepClone(Config.GetJsonObjectForLog());
                Config.Value    = ModifyData;
                if (await __DBContext.SaveChangesAsync() <= 0) {
                    WriteLog(LOG_LEVEL.ERROR, string.Empty, $"Cannot save changes: { ConfigKeyStr }");
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
                var (OldVal, NewVal) = Utils.GetDataChanges(OldValue, Config.GetJsonObjectForLog());
                await __AdminAuditLogManagement.AddNewAuditLog(
                    Config.GetModelName(),
                    ConfigKeyStr,
                    LOG_ACTIONS.MODIFY,
                    UserId,
                    OldVal,
                    NewVal
                );
            }
            return ErrorCodes.NO_ERROR;
        }

        public async Task<(JObject Value, string Error)> GetConfigValueFromDB(CONFIG_KEY ConfigKey)
        {
            string Error = string.Empty;
            string configKeyStr = DEFAULT_BASE_CONFIG.ConfigKeyToString(ConfigKey);
            JObject config = default;
            using (var scope = __ServiceProvider.CreateScope())
            {
                config = await scope.ServiceProvider.GetRequiredService<DBContext>().AdminBaseConfigs
                            .Where<AdminBaseConfig>(e => e.ConfigKey == configKeyStr)
                            .Select(e => e.Value)
                            .FirstOrDefaultAsync();
            }
            if (config != default) {
                return (config, Error);
            }

            Error = $"Invalid config data. Default vaue will be use. config_key: { configKeyStr }.";
            WriteLog(LOG_LEVEL.WARNING, string.Empty, Error);
            return (DEFAULT_BASE_CONFIG.GetConfig(ConfigKey), Error);
        }

        public async Task<(T Value, string Error)> GetConfigValueFromDB<T>(CONFIG_KEY ConfigKey, SUB_CONFIG_KEY SubConfigKey)
        {
            string Error = string.Empty;
            if (SubConfigKey == SUB_CONFIG_KEY.ALL) {
                Error = $"GetConfigValue. Unsupport get sub config type: { SubConfigKey }";
                throw new Exception(Error);
            }
            if (typeof(T) != typeof(string) && typeof(T) != typeof(int) && typeof(T) != typeof(float) && typeof(T) != typeof(bool)) {
                Error = $"GetConfigValue. Unsupport convert type: { typeof(T) }";
                throw new Exception(Error);
            }
            string configKeyStr = DEFAULT_BASE_CONFIG.ConfigKeyToString(ConfigKey);
            string subConfigKeyStr = DEFAULT_BASE_CONFIG.SubConfigKeyToString(SubConfigKey);
            JObject config = default;
            using (var scope = __ServiceProvider.CreateScope())
            {
                config = await scope.ServiceProvider.GetRequiredService<DBContext>().AdminBaseConfigs
                            .Where<AdminBaseConfig>(e => e.ConfigKey == configKeyStr)
                            .Select(e => e.Value)
                            .FirstOrDefaultAsync();
            }

            if (config != default && config[subConfigKeyStr] != default) {
                return ((T) System.Convert.ChangeType(config[subConfigKeyStr], typeof(T)), Error);
            } else {
                var defaultConfig = DEFAULT_BASE_CONFIG.GetConfig(ConfigKey, Error);
                if (Error != default && Error != string.Empty) {
                    throw new Exception(Error);
                }
                if (defaultConfig[subConfigKeyStr] == default) {
                    throw new Exception($"Invalid pair, config_key: { configKeyStr }, sub_config_key: { subConfigKeyStr }.");
                }
                Error = $"Invalid config data. Default vaue will be use. config_key: { configKeyStr }, sub_config_key: { subConfigKeyStr }.";
                WriteLog(LOG_LEVEL.WARNING, string.Empty, Error);
                return ((T) System.Convert.ChangeType(defaultConfig[subConfigKeyStr], typeof(T)), Error);
            }
        }
    }
}