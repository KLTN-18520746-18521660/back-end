using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using System.ComponentModel.DataAnnotations.Schema;
using DatabaseAccess.Common.Interface;

namespace DatabaseAccess.Context.ParserModels
{
    public class ParserSocialUser : IBaseParserModel
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string display_name { get; set; }
        public string user_name { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string sex { get; set; }
        public string phone { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string avatar { get; set; }
        public JObject settings { get; set; }
    }
}
