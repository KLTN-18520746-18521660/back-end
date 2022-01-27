using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using System.ComponentModel.DataAnnotations.Schema;
using DatabaseAccess.Common.Interface;

namespace DatabaseAccess.Contexts.ConfigDB.ParserModels
{
    public class ParserAdminUser : IBaseParserModel
    {
        public string user_name { get; set; }
        public string display_name { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public JObject settings { get; set; }
    }
}
