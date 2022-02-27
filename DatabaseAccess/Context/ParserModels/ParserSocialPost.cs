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
    public class ParserSocialPost : IBaseParserModel
    {
        public Guid owner { get; set; }
        public string title { get; set; }
        public string thumbnail { get; set; }
        public string content { get; set; }
    }
}
