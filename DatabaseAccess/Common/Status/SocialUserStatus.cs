using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess.Common.Models
{
    public class SocialUserStatus
    {
        public readonly static int InvalidStatus = -1;
        public readonly static int Deleted = 0;
        public readonly static int Activated = 1;
        public readonly static int Blocked = 2;
        private readonly static Dictionary<int, string> MapStatus = new ()
        {
            { InvalidStatus, "Invalid Status" },
            { Deleted, "Deleted" },
            { Activated, "Activated" },
            { Blocked, "Blocked" }
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
