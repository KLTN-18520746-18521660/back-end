using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess.Common.Models
{
    public class SocialReportStatus
    {
        public readonly static int InvalidStatus = -1;
        public readonly static int Pending = 0;
        public readonly static int Handled = 1;
        public readonly static int Ignored = 2;
        private readonly static Dictionary<int, string> MapStatus = new ()
        {
            { InvalidStatus, "Invalid Status" },
            { Pending, "Pending" },
            { Handled, "Handled" },
            { Ignored, "Ignored" }
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
