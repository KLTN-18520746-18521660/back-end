using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess.Common
{
    public class CommonValidator
    {
        /**
         * All valid case:
         * {
         *    "right_with_all_abilities": ["write", "read"],
         *    "right_with_write": ["write"],
         *    "right_with_read": ["read"],
         *    "right_with_non_abilities": []
         * }
         **/
        public static bool ValidateRightsAbilities(Dictionary<string, List<string>> Rights)
        {
            foreach(var value in Rights.Values)
            {
                if ((value.Count == 1 && (value[0] == "write" || value[0] == "read")) ||
                    (value.Count == 2 && value.Contains("write") && value.Contains("read")) ||
                    value.Count == 0) {
                    continue;
                }
                return false;
            }
            return true;
        }
    }
}
