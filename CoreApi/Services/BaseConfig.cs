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

namespace CoreApi.Services
{
    public class BaseConfig : BaseService
    {
        List<AdminBaseConfig> Configs;
        public BaseConfig(DBContext _DBContext,
                          IServiceProvider _IServiceProvider)
            : base(_DBContext, _IServiceProvider)
        {
            __ServiceName = "BaseConfig";
            Configs = __DBContext.AdminBaseConfigs.ToList();
            LogInformation("Init load all config successfully.");
        }

        public async Task ReLoadConfig()
        {
            Configs = await __DBContext.AdminBaseConfigs.ToListAsync();
            LogInformation("Reload all config successfully.");
        }

        public (JObject Value, string Error) GetConfigValue(CONFIG_KEY ConfigKey)
        {
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