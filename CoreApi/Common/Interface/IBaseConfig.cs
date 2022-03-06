using Microsoft.AspNetCore.Mvc;
using Serilog;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;

namespace CoreApi.Common.Interface
{
    
    
    public interface IBaseConfig
    {
        public JObject GetConfigValue(CONFIG_KEY ConfigKey, string Error = null);
        // only support int || string value
        public T GetConfigValue<T>(CONFIG_KEY ConfigKey, string SubKey, string Error = null);
        public List<JObject> GetAllDefaultConfig();
    }
}