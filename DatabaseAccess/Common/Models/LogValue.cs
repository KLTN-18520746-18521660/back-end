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
            Data = new JObject();
        }
        public LogValue(JObject obj) {
            Data = obj;
        }
        public LogValue(string value) {
            Data = JsonConvert.DeserializeObject<JObject>(value);
        }
        public JObject Data { get; set; }

        public override string ToString() {
            return JsonConvert.SerializeObject(Data);
        }
    }
}
