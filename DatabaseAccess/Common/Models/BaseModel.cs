using System;
// using System.Text.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using DatabaseAccess.Common.Interface;

namespace DatabaseAccess.Common.Models
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
                return JsonConvert.SerializeObject(__ObjectJson);
            }
            return $"{{\"err\": \"Can't convert Object[{__ModelName}] to Json\"}}";
        }
        public JObject GetJsonObject() {
            return JsonConvert.DeserializeObject<JObject>(ToJsonString());
        }
        public virtual JObject GetPublicJsonObject(List<string> publicFields = default) {
            return GetJsonObject();
        }
        public virtual JObject GetJsonObjectForLog() {
            return GetJsonObject();
        }

        public abstract bool PrepareExportObjectJson();
        public abstract bool Parse(IBaseParserModel Parser, out string Error);
    }
}