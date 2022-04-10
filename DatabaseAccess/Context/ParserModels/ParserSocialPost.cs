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
    public class ParserSocialPost : IBaseParserModel
    {
        [DefaultValue("Demo post title must have lenght geater than 20")]
        public string title { get; set; }
        [DefaultValue("http://localhost/img/thumbnail.png")]
        public string thumbnail { get; set; }
        [DefaultValue("<p>Demo post content must lenght > 20</p>")]
        public string content { get; set; }
        [DefaultValue("Post short content must have lenght geater than 20")]
        public string short_content { get; set; }
        [DefaultValue(5)]
        public int time_read { get; set; }
        [DefaultValue("HTML")]
        public string content_type { get; set; }
        [DefaultValue("[\"technology\"]")]
        public List<string> categories { get; set; }
        [DefaultValue("[]")]
        public List<string> tags { get; set; }
    }
}
