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
        
        }
    }
}
