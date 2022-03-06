using Microsoft.AspNetCore.Mvc;
using Serilog;
using DatabaseAccess.Context;
// using DatabaseAccess.Context.Models;
using DatabaseAccess.Context.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using CoreApi.Common.Interface;

namespace CoreApi.Common
{
    public class BaseConfig : IBaseConfig
    {
        protected ILogger __Logger;
        protected DBContext __DBContext;
        public BaseConfig()
        {
            __Logger = Log.Logger;
            __DBContext = new DBContext();
        }

        public List<JObject> GetAllDefaultConfig()
        {
            throw new NotImplementedException();
        }

        public JObject GetConfigValue(CONFIG_KEY ConfigKey, string Error = null)
        {
            Error ??= "";
            string configKeyStr = DefaultBaseConfig.ConfigKeyToString(ConfigKey);
            var configs = __DBContext.AdminBaseConfigs
                            .Where<AdminBaseConfig>(e => e.ConfigKey == configKeyStr)
                            .Select(e => e.Value)
                            .ToList();
            if (configs.Count > 0) {
                return configs.First();
            }

            Error ??= "Invalid config data. Default vaue will be use.";
            return DefaultBaseConfig.GetConfig(ConfigKey);
        }

        public T GetConfigValue<T>(CONFIG_KEY ConfigKey, string SubKey, string Error = null)
        {
            Error ??= "";
            if (typeof(T) != typeof(string) && typeof(T) != typeof(int)) {
                Error ??= "GetConfigValue. Unsupport convert type.";
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
                Error = "Invalid config data. Default vaue will be use.";
                return (T) System.Convert.ChangeType(defaultConfig[SubKey], typeof(T));
            }
        }
    }
}