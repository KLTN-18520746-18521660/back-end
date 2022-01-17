using System;

namespace console
{
    class Program
    {
        static string GenerateProjectGUID()
        {
            return System.Guid.NewGuid().ToString("B").ToUpper();
        }
        static void Main(string[] args)
        {
            // Console.WriteLine("Hello World!");
            // var guid = GenerateProjectGUID();
            // Console.WriteLine($"GUID: {guid}");
            demo_lib.demo tst = new demo_lib.demo();
            tst.Hello();
        
        }
    }
}
