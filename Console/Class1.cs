using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class Program
{
	public class baseStatus{
		public static readonly int invalid = -1;
		protected static Dictionary<int, string> mapAction = new Dictionary<int, string>()
        {
            { invalid, "Invalid" }
        };
		public static Dictionary<int, string> MapAction {get => mapAction;}
		public static string StatusToString(int Action)
        {
            if (MapAction.ContainsKey(Action)) {
				return MapAction[Action];
			}
			return MapAction[invalid];
        }
	}
	
	public class status : baseStatus {
		public static readonly int yes = 0;
		
        protected readonly static new Dictionary<int, string> MapAction = new Dictionary<int, string>()
        {
            { invalid, "Invalid Action" },
			{ yes, "yes" }
        };
		public status() {
			MapAction.Add(1, "no");
		}
		public static int count(){ return MapAction.Count; }
	}
	public static void Main()
	{
		Console.WriteLine(status.count());
		Console.WriteLine(status.StatusToString(0));
		status a = new status();
		Console.WriteLine(status.StatusToString(0));
		Console.WriteLine(status.count());
		Console.WriteLine(status.StatusToString(1));
	}
}