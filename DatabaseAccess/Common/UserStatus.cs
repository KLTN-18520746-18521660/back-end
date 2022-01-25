using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess.Common
{
    public class UserStatus
    {
        public readonly static int InvalidStatus = -1;
        public readonly static int Deleted = 0;
        public readonly static int NotActivated = 1;
        public readonly static int Activated = 2;
        public readonly static int Readonly = 3;
        private readonly static Dictionary<int, string> MapStatus = new Dictionary<int, string>()
        {
            { InvalidStatus, "Invalid Status" },
            { Deleted, "Deleted" },
            { NotActivated, "Not Activated" },
            { Activated, "Activated" },
            { Readonly, "Readonly" }
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
