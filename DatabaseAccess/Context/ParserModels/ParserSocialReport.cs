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
    public class ParserSocialReport : IBaseParserModel
    {
        public string user_name { get; set; }
        public string post_slug { get; set; }
        public long comment_id { get; set; }
        public string report_type { get; set; }
        public string content { get; set; }
    }
}
