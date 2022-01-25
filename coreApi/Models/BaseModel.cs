using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Serilog;

namespace CoreApi.Models
{
    public class BaseModel : ICloneable
    {
        protected string __ModelName;
        protected Dictionary<string, string> __ObjectJson;
        protected ILogger __Logger;
        public string ModelName { get => __ModelName; }
        public BaseModel()
        {
            __ModelName = "BaseModel";
            __ObjectJson = new Dictionary<string, string>();
            __Logger = Log.Logger;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public string ToJsonString()
        {
            if (PrepareExportObjectJson()) {
                return JsonSerializer.Serialize(__ObjectJson);
            }
            return $"{{\"err\": \"Can't convert Object[{__ModelName}] to Json\"}}";
        }
        public bool FromJsonString(string jsonString)
        {
            __ObjectJson = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
            if (!InitFromObjecJson()) {
                __ObjectJson = new Dictionary<string, string>();
                return false;
            }
            return true;
        }

        /* [IMPORTANT] Functions need implement */
        // Function need to run before use function ToJsonString()
        public virtual bool PrepareExportObjectJson()
        {
            __ObjectJson = new Dictionary<string, string>();
            return true;
        }
        // Function need to run after use function FromJsonString(string)
        public virtual bool InitFromObjecJson()
        {
            return ValidateObjectJson();
        }
        public virtual bool ValidateObjectJson()
        {
            BaseModel baseModel = new BaseModel();
            var dic = baseModel
                        .GetType()
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .ToDictionary(prop => prop.Name, prop => (string)prop.GetValue(baseModel, null));
            foreach(var key in __ObjectJson.Keys) {
                if (!dic.ContainsKey(key)) {
                    return false;
                }
            }
            return __ObjectJson.Keys.Count == dic.Keys.Count;
        }
    }
}