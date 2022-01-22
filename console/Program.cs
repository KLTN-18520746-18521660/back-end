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
            var a = 1;
            var b = a;

            b = 11;
            Console.Write(a);
            Console.Write(b);
        }
    }
}
