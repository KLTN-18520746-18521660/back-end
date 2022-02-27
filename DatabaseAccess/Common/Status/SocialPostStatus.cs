using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess.Common.Models
{
    public class SocialPostStatus
    {
        public readonly static int InvalidStatus = -1;
        public readonly static int Pending = 0;
        public readonly static int Approved = 1;
        public readonly static int Deleted = 2;
        public readonly static int Private = 3;
        private readonly static Dictionary<int, string> MapStatus = new ()
        {
            { InvalidStatus, "Invalid Status" },
            { Pending, "Pending" },
            { Approved, "Approved" },
            { Deleted, "Deleted" },
            { Private, "Blocked" }
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
