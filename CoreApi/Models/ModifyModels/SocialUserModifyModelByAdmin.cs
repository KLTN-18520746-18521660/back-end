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
    public class SocialUserModifyModelByAdmin
    {
        [DefaultValue("Block")]
        public string status { get; set; }
        [DefaultValue("[\"upload\"]")]
        public JArray roles { get; set; }

        public bool IsValidRights()
        {
            foreach (var it in roles) {
                if (it.Type != JTokenType.String) {
                    return false;
                }
                if (!Regex.IsMatch(it.ToString(), @"^\w+$")) {
                    return false;
                }
            }
            return true;
        }
    }
}
