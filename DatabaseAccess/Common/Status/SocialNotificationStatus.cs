using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess.Common.Models
{
    public class SocialNotificationStatus
    {
        public readonly static int InvalidStatus = -1;
        public readonly static int Sent = 0;
        public readonly static int Read = 1;
        public readonly static int Deleted = 2;
        private readonly static Dictionary<int, string> MapStatus = new ()
        {
            { InvalidStatus, "Invalid Status" },
            { Sent, "Sent" },
            { Read, "Read" },
            { Deleted, "Deleted" }
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
