using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json.Linq;
using DatabaseAccess.Common.Interface;

namespace DatabaseAccess.Common
{
    public abstract class BaseModel : ICloneable, IBaseModel
    {
        [NotMapped]
        protected string __ModelName;
        [NotMapped]
        protected Dictionary<string, object> __ObjectJson;
        public BaseModel() {
            __ModelName = "BaseModel";
            __ObjectJson = new Dictionary<string, object>();
        }
        public string GetModelName() {
            return __ModelName;
        }
        public object Clone() {
            return MemberwiseClone();
        }
        public string ToJsonString() {
            if (PrepareExportObjectJson()) {
                return JsonSerializer.Serialize(__ObjectJson);
            }
            return $"{{\"err\": \"Can't convert Object[{__ModelName}] to Json\"}}";
        }
        public JObject GetJsonObject() {
            if (PrepareExportObjectJson()) {
                return JsonSerializer.Deserialize<JObject>(JsonSerializer.Serialize(__ObjectJson));
            }
            return JsonSerializer.Deserialize<JObject>($"{{\"err\": \"Can't convert Object[{__ModelName}] to Json\"}}");
        }

        public abstract bool PrepareExportObjectJson();
        public abstract bool Parse(IBaseParserModel Parser, string Error = null);
    }
}