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
    public class ParserSocialCategory : IBaseParserModel
    {
        public long? parent_id { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public string describe { get; set; }
        public string thumbnail { get; set; }
    }
}
