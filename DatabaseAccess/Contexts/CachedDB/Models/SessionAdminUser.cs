using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Interface;

namespace DatabaseAccess.Contexts.CachedDB.Models
{
    public class SessionAdminUser : BaseModel
    {
        public override bool Parse(IBaseParserModel Parser, string Error = null)
        {
            throw new NotImplementedException();
        }

        public override bool PrepareExportObjectJson()
        {
            throw new NotImplementedException();
        }
    }
}
