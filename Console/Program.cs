using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DatabaseAccess.Common.Models;
using System.Text;

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
            string passCert = CoreApi.ConfigurationDefaultVariable.PASSWORD_CERTIFICATE;
            string passDb = "a";
            Console.WriteLine(CoreApi.Common.StringDecryptor.Encrypt(passCert));
            Console.WriteLine(CoreApi.Common.StringDecryptor.Encrypt(passDb));
        }
        static void Main(string[] args)
        {
            PrintPassEncrypt();
        }
    }
}
