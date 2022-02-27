using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess.Common.Models
{
    public class SocialCategoryStatus
    {
        public readonly static int InvalidStatus = -1;
        public readonly static int Enabled = 0;
        public readonly static int Disabled = 1;
        public readonly static int Readonly = 2;
        private readonly static Dictionary<int, string> MapStatus = new ()
        {
            { InvalidStatus, "Invalid Status" },
            { Enabled, "Pending" },
            { Disabled, "Approved" },
            { Readonly, "Deleted" }
        };

        public static string StatusToString(int Status)
        {
            return MapStatus.GetValueOrDefault(Status, MapStatus[InvalidStatus]);
        }

        public static int StatusFromString(string Status)
        {
            return MapStatus.FirstOrDefault(x => x.Value == Status).Key;
        }
    }
}
