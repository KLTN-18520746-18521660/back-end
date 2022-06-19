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

namespace DatabaseAccess.Context.ParserModels
{
    public class ParserSocialTag : IBaseParserModel
    {
        [DefaultValue("palv")]
        public string tag { get; set; }
        [DefaultValue("name of aplv")]
        public string name { get; set; }
        [DefaultValue("describe of palv")]
        public string describe { get; set; }
    }
}
