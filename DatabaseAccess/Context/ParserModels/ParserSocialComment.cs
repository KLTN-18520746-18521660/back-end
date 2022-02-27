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
    public class ParserSocialComment : IBaseParserModel
    {
        public long parent_id { get; set; }
        public long post_id { get; set; }
        public Guid owner { get; set; }
        public string content { get; set; }
    }
}
