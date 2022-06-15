using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using DatabaseAccess.Common.Interface;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace DatabaseAccess.Context.ParserModels
{
    public class ParserSocialUserRole : IBaseParserModel
    {
        public string role_name { get; set; }
        public string display_name { get; set; }
        public string describe { get; set; }
        public bool priority { get; set; }
        [DefaultValue("{\"upload\": {\"read\": true, \"write\": true}}")]
        public JObject rights { get; set; }

        public bool IsValidRights()
        {
            var requiredKeys = new string[]{ "read", "write" };
            foreach (var it in rights) {
                if (!Regex.IsMatch(it.Key, @"^\w+$")) {
                    return false;
                }
                if (it.Value.Type != JTokenType.Object) {
                    return false;
                }
                var count = 0;
                var val = it.Value.ToObject<JObject>();
                foreach (var k in requiredKeys) {
                    if (val.ContainsKey(k)) {
                        count++;
                    }
                }

                if (count != 2 || val.Count != 2) {
                    return false;
                }
            }
            return true;
        }
    }
}
