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

namespace CoreApi.Services
{
    public class BaseConfig : BaseService
    {
        List<AdminBaseConfig> Configs;
        SemaphoreSlim Gate;
        bool isReloadConfig;
        public BaseConfig(DBContext _DBContext,
                          IServiceProvider _IServiceProvider)
            : base(_DBContext, _IServiceProvider)
        {
            __ServiceName = "BaseConfig";
            Configs = __DBContext.AdminBaseConfigs.ToList();
            Gate = new SemaphoreSlim(1);
            isReloadConfig = false;
            LogInformation("Init load all config successfully.");
        }

        public async Task<ErrorCodes> ReLoadConfig()
        {
            await Gate.WaitAsync();
            isReloadConfig = true;
            Configs = await __DBContext.AdminBaseConfigs.ToListAsync();
            isReloadConfig = false;
            Gate.Release();
            LogInformation("Reload all config successfully.");
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
            return (JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(ret)), "");
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
            return (JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(ret)), "");
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
                    return (null, $"Not found config. But key exist on public configs, key: { ConfigKey }");
                }
                if (it.Key == DefaultBaseConfig.ConfigKeyToString(ConfigKey)) {
                    if (DefaultBaseConfig.StringToSubConfigKey(it.Value.ToString()) == SUB_CONFIG_KEY.ALL) {
                        return (found.Value, "");
                    }
                } else {
                    var valStr = found.Value.Value<string>(it.Value.ToString());
                    var isInt = int.TryParse(valStr, out var valInt);
                    return (new JObject(){
                        { it.Value.ToString(), isInt ? valInt : valStr },
                    }, "");
                }
            }
            return (null, $"Not found config. key: { ConfigKey }");
        }

        public (JObject Value, string Error) GetConfigValue(CONFIG_KEY ConfigKey)
        {
            while(isReloadConfig);
            string configKeyStr = DefaultBaseConfig.ConfigKeyToString(ConfigKey);
            var config = Configs
                            .Where<AdminBaseConfig>(e => e.ConfigKey == configKeyStr)
                            .Select(e => e.Value)
                            .DefaultIfEmpty(null)
                            .FirstOrDefault();
            if (config != null) {
                return (config, "");
            }

            string Error = "";
            Error = $"Invalid config data. Default vaue will be use. config_key: { configKeyStr }.";
            LogWarning(Error);
            return (DefaultBaseConfig.GetConfig(ConfigKey), Error);
        }

        public (T Value, string Error) GetConfigValue<T>(CONFIG_KEY ConfigKey, SUB_CONFIG_KEY SubConfigKey)
        {
            string Error = "";
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
                            .DefaultIfEmpty(null)
                            .FirstOrDefault();

            if (config != null && config[subConfigKeyStr] != null) {
                return ((T) System.Convert.ChangeType(config[subConfigKeyStr], typeof(T)), Error);
            } else {
                var defaultConfig = DefaultBaseConfig.GetConfig(ConfigKey, Error);
                if (Error != null && Error != "") {
                    throw new Exception(Error);
                }
                if (defaultConfig[subConfigKeyStr] == null) {
                    throw new Exception($"Invalid pair, config_key: { configKeyStr }, sub_config_key: { subConfigKeyStr }.");
                }
                Error = $"Invalid config data. Default vaue will be use. config_key: { configKeyStr }, sub_config_key: { subConfigKeyStr }.";
                LogWarning(Error);
                return ((T) System.Convert.ChangeType(defaultConfig[subConfigKeyStr], typeof(T)), Error);
            }
        }

        public async Task<(JObject Value, string Error)> GetConfigValueFromDB(CONFIG_KEY ConfigKey)
        {
            string Error = "";
            string configKeyStr = DefaultBaseConfig.ConfigKeyToString(ConfigKey);
            var config = (await __DBContext.AdminBaseConfigs
                            .Where<AdminBaseConfig>(e => e.ConfigKey == configKeyStr)
                            .Select(e => e.Value)
                            .ToListAsync())
                            .DefaultIfEmpty(null)
                            .FirstOrDefault();
            if (config != null) {
                return (config, Error);
            }

            Error = $"Invalid config data. Default vaue will be use. config_key: { configKeyStr }.";
            LogWarning(Error);
            return (DefaultBaseConfig.GetConfig(ConfigKey), Error);
        }

        public async Task<(T Value, string Error)> GetConfigValueFromDB<T>(CONFIG_KEY ConfigKey, SUB_CONFIG_KEY SubConfigKey)
        {
            string Error = "";
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
            var config = (await __DBContext.AdminBaseConfigs
                            .Where<AdminBaseConfig>(e => e.ConfigKey == configKeyStr)
                            .Select(e => e.Value)
                            .ToListAsync())
                            .DefaultIfEmpty(null)
                            .FirstOrDefault();

            if (config != null && config[subConfigKeyStr] != null) {
                return ((T) System.Convert.ChangeType(config[subConfigKeyStr], typeof(T)), Error);
            } else {
                var defaultConfig = DefaultBaseConfig.GetConfig(ConfigKey, Error);
                if (Error != null && Error != "") {
                    throw new Exception(Error);
                }
                if (defaultConfig[subConfigKeyStr] == null) {
                    throw new Exception($"Invalid pair, config_key: { configKeyStr }, sub_config_key: { subConfigKeyStr }.");
                }
                Error = $"Invalid config data. Default vaue will be use. config_key: { configKeyStr }, sub_config_key: { subConfigKeyStr }.";
                LogWarning(Error);
                return ((T) System.Convert.ChangeType(defaultConfig[subConfigKeyStr], typeof(T)), Error);
            }
        }
    }
}