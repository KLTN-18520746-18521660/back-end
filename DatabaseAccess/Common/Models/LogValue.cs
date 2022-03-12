using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// using System.Text.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DatabaseAccess.Common.Models
{
    public static class LOG_ACTIONS
    {
        public static readonly string CREATE = "create";
        public static readonly string MODIFY = "modify";
        public static readonly string DELETE = "delete";
    }
    public class LogValue
    {
        public LogValue() {
            Data = new List<EntityValue>();
        }
        public LogValue(JObject obj) {
            Data = new List<EntityValue>();
            List<string> keys = obj.Properties().Select(p => p.Name).ToList();
            foreach (var key in keys) {
                Data.Add(new EntityValue(){
                    Property = key,
                    Value = obj[key]
                });
            }
        }
        public LogValue(string value) {
            Data = JsonConvert.DeserializeObject<List<EntityValue>>(value);
        }
        public List<EntityValue> Data { get; set; }

        public override string ToString() {
            return JsonConvert.SerializeObject(Data);
        }
    }
    public class EntityValue
    {
        public string Property { get; set; }
        public object Value { get; set; }
    }
}
