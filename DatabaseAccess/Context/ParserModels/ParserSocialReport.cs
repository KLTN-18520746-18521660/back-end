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
        public Guid user_id { get; set; }
        public long post_id { get; set; }
        public long comment_id { get; set; }
        public string content { get; set; }
    }
}
