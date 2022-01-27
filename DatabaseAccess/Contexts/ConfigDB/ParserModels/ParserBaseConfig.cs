using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using DatabaseAccess.Common.Interface;

namespace DatabaseAccess.Contexts.ConfigDB.ParserModels
{
    public class ParserBaseConfig : IBaseParserModel
    {
        public string config_key { get; set; }
        public JObject value { get; set; }
    }
}
