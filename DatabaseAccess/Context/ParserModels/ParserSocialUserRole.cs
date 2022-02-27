using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using DatabaseAccess.Common.Interface;

namespace DatabaseAccess.Context.ParserModels
{
    public class ParserSocialUserRole : IBaseParserModel
    {
        public string role_name { get; set; }
        public string display_name { get; set; }
        public string describe { get; set; }
        public Dictionary<string, List<string>> rights { get; set; }
    }
}
