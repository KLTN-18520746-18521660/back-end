using Serilog;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using CoreApi.Common;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using Common;
using Microsoft.Extensions.DependencyInjection;
using DatabaseAccess.Common.Status;

namespace CoreApi.Services
{
    public class BaseConfig : BaseSingletonService
    {
        List<AdminBaseConfig> Configs;
        SemaphoreSlim Gate;
        bool isReloadConfig;
        public BaseConfig(IServiceProvider _IServiceProvider)
            : base(_IServiceProvider)
        {
            __ServiceName = "BaseConfig";
            InitConfig();
            Gate = new SemaphoreSlim(1);
            isReloadConfig = false;
            LogInformation("Init load all config successfully.");
        }

        protected void InitConfig()
        {
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __DBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                foreach (var key in DefaultBaseConfig.DEFAULT_CONFIG_KEYS) {
                    if (__DBContext.AdminBaseConfigs.Count(e => e.ConfigKey == key) == 0) {
                        __DBContext.AdminBaseConfigs.Add(
                            new AdminBaseConfig() {
                                ConfigKey = key,
                                Value = DefaultBaseConfig.GetConfig(DefaultBaseConfig.StringToConfigKey(key)),
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
                foreach (var key in DefaultBaseConfig.DEFAULT_CONFIG_KEYS) {
                    if (await __DBContext.AdminBaseConfigs.CountAsync(e => e.ConfigKey == key) == 0) {
                        await __DBContext.AdminBaseConfigs.AddAsync(
                            new AdminBaseConfig() {
                                ConfigKey = key,
                                Value = DefaultBaseConfig.GetConfig(DefaultBaseConfig.StringToConfigKey(key)),
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

        public async Task<ErrorCodes> ReLoadConfig()
        {
            await Gate.WaitAsync();
            isReloadConfig = true;
            await InitConfigAsync();
            isReloadConfig = false;
            Gate.Release();
            LogInformation("Reload all base config successfully.");

            #region Read other service
            var __EmailSender = (EmailSender)__ServiceProvider.GetService(typeof(EmailSender));
            if (!__EmailSender.ReloadEmailConfig()) {
                LogInformation("Reload EmailSender config Failed.");
            } else {
                LogInformation("Reload EmailSender config successfully.");
            }
            #endregion
            return ErrorCodes.NO_ERROR;
        }

        public (JObject Value, string Error) GetAllConfig()
        {
            while(isReloadConfig);
            Dictionary<string, JObject> ret = new Dictionary<string, JObject>();
            Configs.ForEach(e => {
                if (!ret.ContainsKey(e.ConfigKey)) {
                    ret.Add(e.ConfigKey, e.GetJsonObject());
                }
            });
            return (JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(ret)), string.Empty);
        }

        public (JObject Value, string Error) GetAllPublicConfig()
        {
            while(isReloadConfig);
            List<AdminBaseConfig> configs = Utils.DeepClone<List<AdminBaseConfig>>(Configs);
            var (publicConfig, error) = GetConfigValue(CONFIG_KEY.PUBLIC_CONFIG);

            Dictionary<string, JObject> ret = new Dictionary<string, JObject>();
            foreach (var it in publicConfig) {
                if (DefaultBaseConfig.StringToConfigKey(it.Key) == CONFIG_KEY.INVALID
                    || DefaultBaseConfig.StringToSubConfigKey(it.Value.ToString()) == SUB_CONFIG_KEY.INVALID
                    || ret.ContainsKey(it.Key)) {
                    continue;
                }

                var found = configs.Where(e => e.ConfigKey == it.Key).FirstOrDefault();
                if (found == default) {
                    continue;
                }

                if (DefaultBaseConfig.StringToSubConfigKey(it.Value.ToString()) == SUB_CONFIG_KEY.ALL) {
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
                if (it.Key == DefaultBaseConfig.ConfigKeyToString(ConfigKey)) {
                    return true;
                }
            }
            return false;
        }

        public (JObject Value, string Error) GetPublicConfig(CONFIG_KEY ConfigKey)
        {
            while(isReloadConfig);
            List<AdminBaseConfig> configs = Utils.DeepClone<List<AdminBaseConfig>>(Configs);
            var (publicConfig, error) = GetConfigValue(CONFIG_KEY.PUBLIC_CONFIG);

            foreach (var it in publicConfig) {
                var found = configs.Where(e => e.ConfigKey == it.Key).FirstOrDefault();
                if (found == default) {
                    return (default, $"Not found config. But key exist on public configs, key: { ConfigKey }");
                }
                if (it.Key == DefaultBaseConfig.ConfigKeyToString(ConfigKey)) {
                    if (DefaultBaseConfig.StringToSubConfigKey(it.Value.ToString()) == SUB_CONFIG_KEY.ALL) {
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
            while(isReloadConfig);
            string configKeyStr = DefaultBaseConfig.ConfigKeyToString(ConfigKey);
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
            LogWarning(Error);
            return (DefaultBaseConfig.GetConfig(ConfigKey), Error);
        }

        public (T Value, string Error) GetConfigValue<T>(CONFIG_KEY ConfigKey, SUB_CONFIG_KEY SubConfigKey)
        {
            string Error = string.Empty;
            if (SubConfigKey == SUB_CONFIG_KEY.ALL) {
                Error = $"GetConfigValue. Unsupport get sub config type: { SubConfigKey }";
                throw new Exception(Error);
            }
            while(isReloadConfig);
            if (typeof(T) != typeof(string) && typeof(T) != typeof(int)) {
                Error = $"GetConfigValue. Unsupport convert type: { typeof(T) }";
                throw new Exception(Error);
            }
            string configKeyStr = DefaultBaseConfig.ConfigKeyToString(ConfigKey);
            string subConfigKeyStr = DefaultBaseConfig.SubConfigKeyToString(SubConfigKey);
            var config = Configs
                            .Where<AdminBaseConfig>(e => e.ConfigKey == configKeyStr)
                            .Select(e => e.Value)
                            .DefaultIfEmpty(default)
                            .FirstOrDefault();

            if (config != default && config[subConfigKeyStr] != default) {
                return ((T) System.Convert.ChangeType(config[subConfigKeyStr], typeof(T)), Error);
            } else {
                var defaultConfig = DefaultBaseConfig.GetConfig(ConfigKey, Error);
                if (Error != default && Error != string.Empty) {
                    throw new Exception(Error);
                }
                if (defaultConfig[subConfigKeyStr] == default) {
                    throw new Exception($"Invalid pair, config_key: { configKeyStr }, sub_config_key: { subConfigKeyStr }.");
                }
                Error = $"Invalid config data. Default vaue will be use. config_key: { configKeyStr }, sub_config_key: { subConfigKeyStr }.";
                LogWarning(Error);
                return ((T) System.Convert.ChangeType(defaultConfig[subConfigKeyStr], typeof(T)), Error);
            }
        }

        public async Task<(JObject Value, string Error)> GetConfigValueFromDB(CONFIG_KEY ConfigKey)
        {
            string Error = string.Empty;
            string configKeyStr = DefaultBaseConfig.ConfigKeyToString(ConfigKey);
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
            LogWarning(Error);
            return (DefaultBaseConfig.GetConfig(ConfigKey), Error);
        }

        public async Task<(T Value, string Error)> GetConfigValueFromDB<T>(CONFIG_KEY ConfigKey, SUB_CONFIG_KEY SubConfigKey)
        {
            string Error = string.Empty;
            if (SubConfigKey == SUB_CONFIG_KEY.ALL) {
                Error = $"GetConfigValue. Unsupport get sub config type: { SubConfigKey }";
                throw new Exception(Error);
            }
            if (typeof(T) != typeof(string) && typeof(T) != typeof(int)) {
                Error = $"GetConfigValue. Unsupport convert type: { typeof(T) }";
                throw new Exception(Error);
            }
            string configKeyStr = DefaultBaseConfig.ConfigKeyToString(ConfigKey);
            string subConfigKeyStr = DefaultBaseConfig.SubConfigKeyToString(SubConfigKey);
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
                var defaultConfig = DefaultBaseConfig.GetConfig(ConfigKey, Error);
                if (Error != default && Error != string.Empty) {
                    throw new Exception(Error);
                }
                if (defaultConfig[subConfigKeyStr] == default) {
                    throw new Exception($"Invalid pair, config_key: { configKeyStr }, sub_config_key: { subConfigKeyStr }.");
                }
                Error = $"Invalid config data. Default vaue will be use. config_key: { configKeyStr }, sub_config_key: { subConfigKeyStr }.";
                LogWarning(Error);
                return ((T) System.Convert.ChangeType(defaultConfig[subConfigKeyStr], typeof(T)), Error);
            }
        }
    }
}