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
    public class ParserAdminUserRoleDetail : IBaseParserModel
    {
        public int role_id { get; set; }
        public int right_id { get; set; }
        public JObject actions { get; set; }
    }
}
