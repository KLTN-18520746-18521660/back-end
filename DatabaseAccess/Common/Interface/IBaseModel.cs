using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess.Common.Interface
{
    public interface IBaseModel
    {
        public bool PrepareExportObjectJson();
        public bool Parse(IBaseParserModel Parser, out string Error);
    }
}
