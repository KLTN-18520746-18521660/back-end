using Serilog;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;

using CoreApi.Common;

namespace CoreApi.Services
{
    public class BaseConfig : BaseService
    {
        protected DBContext __DBContext;
        public BaseConfig() : base()
        {
            __DBContext = new DBContext();
            __ServiceName = "BaseConfig";
        }

        public JObject GetConfigValue(CONFIG_KEY ConfigKey, out string Error)
        {
            Error = "";
            string configKeyStr = DefaultBaseConfig.ConfigKeyToString(ConfigKey);
            var configs = __DBContext.AdminBaseConfigs
                            .Where<AdminBaseConfig>(e => e.ConfigKey == configKeyStr)
                            .Select(e => e.Value)
                            .ToList();
            if (configs.Count > 0) {
                return configs.First();
            }

            Error = $"Invalid config data. Default vaue will be use. config_key: { configKeyStr }.";
            LogWarning(Error);
            return DefaultBaseConfig.GetConfig(ConfigKey);
        }

        public T GetConfigValue<T>(CONFIG_KEY ConfigKey, string SubKey, out string Error)
        {
            Error = "";
            if (typeof(T) != typeof(string) && typeof(T) != typeof(int)) {
                Error = $"GetConfigValue. Unsupport convert type: { typeof(T) }";
                throw new Exception(Error);
            }
            string configKeyStr = DefaultBaseConfig.ConfigKeyToString(ConfigKey);
            var configs = __DBContext.AdminBaseConfigs
                            .Where<AdminBaseConfig>(e => e.ConfigKey == configKeyStr)
                            .Select(e => e.Value)
                            .ToList();

            if (configs.Count > 0 && configs.First()[SubKey] != null) {
                return (T) System.Convert.ChangeType(configs.First()[SubKey], typeof(T));
            } else {
                var defaultConfig = DefaultBaseConfig.GetConfig(ConfigKey, Error);
                if (Error != null && Error != "") {
                    throw new Exception(Error);
                }
                if (defaultConfig[SubKey] == null) {
                    throw new Exception("Invalid subkey.");
                }
                Error = $"Invalid config data. Default vaue will be use. config_key: { configKeyStr }, subkey: { SubKey }.";
                LogWarning(Error);
                return (T) System.Convert.ChangeType(defaultConfig[SubKey], typeof(T));
            }
        }
    }
}