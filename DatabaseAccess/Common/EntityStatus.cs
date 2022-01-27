using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess.Common
{
    public class EntityStatus
    {
        public readonly static int InvalidStatus = -1;
        public readonly static int Disabled = 0;
        public readonly static int Enabled = 1;
        public readonly static int Readonly = 2;
        private readonly static Dictionary<int, string> MapStatus = new Dictionary<int, string>()
        {
            { InvalidStatus, "Invalid Status" },
            { Disabled, "Disabled" },
            { Enabled, "Enabled" },
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