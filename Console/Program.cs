using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DatabaseAccess.Common.Models;
using Common;
using System.Text;
using CoreApi;

namespace MyConsole
{
    public class tst
    {
        public LogValue NewValue { get; set; }
        public string NewValueStr
        {
            get { return NewValue.ToString(); }
            set { NewValue = JsonConvert.DeserializeObject<LogValue>(value); }
        }
    }
    class Program
    {
        static string GenerateProjectGUID() {
            return Guid.NewGuid().ToString("B").ToUpper();
        }

        static void PrintPassEncrypt() {
            string passCert = "Ndh90768";
            string passDb = "a";
            Console.WriteLine(StringDecryptor.Encrypt(passCert));
            Console.WriteLine(StringDecryptor.Encrypt(passDb));
        }
        static void Main(string[] args)
        {
            PrintPassEncrypt();
        }
    }
}
