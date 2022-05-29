using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using System.ComponentModel.DataAnnotations.Schema;
using DatabaseAccess.Common.Interface;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CoreApi.Models.ModifyModels
{
    public class SocialUserRoleModifyModel
    {
        public string display_name { get; set; }
        public string describe { get; set; }
        public bool priority { get; set; }
        [DefaultValue("{1: [\"read\", \"write\"]}")]
        public JObject role_details { get; set; }

        public bool IsValidRoleDetails()
        {
            var allow_str = new string[]{ "read", "write" };
            foreach (var it in role_details) {
                if (!Regex.IsMatch(it.Key, @"^\d+$")) {
                    return false;
                }
                if (it.Value.Type != JTokenType.Array) {
                    return false;
                }
                var count = 0;
                foreach (var r in allow_str) {
                    if (it.Value.ToArray().Contains(r)) {
                        count++;
                    }
                }

                if (count != it.Value.ToArray().Count()) {
                    return false;
                }
            }
            return true;
        }
    }
}
