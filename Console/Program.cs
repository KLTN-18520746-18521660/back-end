using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MyConsole
{
    class Program
    {
        static string GenerateProjectGUID()
        {
            return Guid.NewGuid().ToString("B").ToUpper();
        }

        static void Main(string[] args)
        {
            var user = DatabaseAccess.Context.Models.AdminUser.GetDefaultData().First();
            // string salt = user.Salt;
            string salt = "2adb88f2";
            string pass = "admin";
            // string _pass = user.Password;
            string _pass = "E0F9DC1F07E91A713A471D7349B8FE13";
            string en = DatabaseAccess.Common.PasswordEncryptor.EncryptPassword(pass, salt);
            string en2 = DatabaseAccess.Common.PasswordEncryptor.EncryptPassword(pass, salt);

            Console.WriteLine(en);
            Console.WriteLine(en2);
            Console.WriteLine(_pass);
            Console.WriteLine(en == _pass);
        }
    }
}
