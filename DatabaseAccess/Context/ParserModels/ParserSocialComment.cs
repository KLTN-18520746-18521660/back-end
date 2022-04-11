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
    public class ParserSocialComment : IBaseParserModel
    {
        public long parent_id { get; set; }
        [DefaultValue("New comment")]
        public string content { get; set; }
    }
}
