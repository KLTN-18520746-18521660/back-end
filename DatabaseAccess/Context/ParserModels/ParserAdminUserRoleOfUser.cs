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
    public class ParserAdminUserRoleOfUser : IBaseParserModel
    {
        public Guid user_id { get; set; }
        public int role_id { get; set; }
    }
}
