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
    public class LogValue
    {
        public EntityValue[] Data { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(Data);
        }
    }
    public class EntityValue
    {
        public string Property { get; set; }
        public string Value { get; set; }
    }
}
