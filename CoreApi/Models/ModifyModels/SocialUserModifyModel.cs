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

namespace CoreApi.Models.ModifyModels
{
    public class SocialUserModifyModel
    {
        [DefaultValue("user_name")]
        public string user_name { get; set; }
        [DefaultValue("first_name")]
        public string first_name { get; set; }
        [DefaultValue("last_name")]
        public string last_name { get; set; }
        [DefaultValue("display_name")]
        public string display_name { get; set; }
        [DefaultValue("description")]
        public string description { get; set; }
        [DefaultValue("email@email.com")]
        public string email { get; set; }
        [DefaultValue("male")]
        public string sex { get; set; }
        [DefaultValue("0965690984")]
        public string phone { get; set; }
        [DefaultValue("country")]
        public string country { get; set; }
        [DefaultValue("city")]
        public string city { get; set; }
        [DefaultValue("province")]
        public string province { get; set; }
        [DefaultValue("avatar")]
        public string avatar { get; set; }
        [DefaultValue("[\"display_name\", \"user_name\", \"email\", \"description\", \"sex\", \"country\", \"avatar\", \"status\", \"ranks\", \"publics\", \"followers\", \"posts\", \"views\", \"likes\"]")]
        public string[] publics { get; set; }
        [DefaultValue("{}")]
        public JObject ui_settings { get; set; }
    }
}
