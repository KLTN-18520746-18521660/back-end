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

namespace DatabaseAccess.Context.ParserModels
{
    public class ParserAdminUser : IBaseParserModel
    {
        [DefaultValue("hello")]
        public string user_name { get; set; }
        [DefaultValue("era001")]
        public string display_name { get; set; }
        [DefaultValue("hello")]
        public string password { get; set; }
        [DefaultValue("era@era.com")]
        public string email { get; set; }
        [DefaultValue("{}")]
        public JObject settings { get; set; }
    }
}
