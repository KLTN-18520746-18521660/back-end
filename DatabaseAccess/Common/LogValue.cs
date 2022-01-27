using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DatabaseAccess.Common
{
    public class LogValue
    {
        public EntityValue[] Data { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(Data);
        }
    }
    public class EntityValue
    {
        public string Property { get; set; }
        public string Value { get; set; }
    }
}
