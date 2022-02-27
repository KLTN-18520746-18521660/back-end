﻿using Newtonsoft.Json;
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

        static byte[] HmacSHA256(String data, byte[] key)
        {
            String algorithm = "HmacSHA256";
            KeyedHashAlgorithm kha = KeyedHashAlgorithm.Create(algorithm);
            kha.Key = key;

            return kha.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        static byte[] getSignatureKey(String key, String dateStamp, String regionName, String serviceName)
        {
            byte[] kSecret = Encoding.UTF8.GetBytes(("AWS4" + key).ToCharArray());
            byte[] kDate = HmacSHA256(dateStamp, kSecret);
            byte[] kRegion = HmacSHA256(regionName, kDate);
            byte[] kService = HmacSHA256(serviceName, kRegion);
            byte[] kSigning = HmacSHA256("aws4_request", kService);

            return kSigning;
        }

        static void Main(string[] args)
        {
            //    public JObject Settings { get; set; }
            //    [Required]
            //    [Column("settings", TypeName = "JSON")]
            //    public string SettingsStr
            //{
            //        get { return Settings.ToString(); }
            //        set { Settings = JsonConvert.DeserializeObject<JObject>(value); }
            //    }
            //var a = "[as'a','b','c']";
            ////var _a = JsonConvert.DeserializeObject<List<string>>(a);

            //var obj = "{'name': 'hi', 'age': 22}";
            //var _obj = JsonConvert.DeserializeObject<JContainer>(obj);
            //var _objj = JsonConvert.DeserializeObject<JContainer>(a);

            //Console.WriteLine(_obj.Type == JTokenType.Object);
            //Console.WriteLine(_obj.ToObject<JObject>());
            //Console.WriteLine(_objj.Type == JTokenType.Array);

            //Console.WriteLine(JsonConvert.SerializeObject(_a));
            //Console.WriteLine(_obj.Property("name").ToString());
            //Console.WriteLine(_obj.Property("abc") != null);


            //DateTime a = DateTime.Parse("2022-01-25 09:05:16");
            //Console.WriteLine(a.ToString());

            //Console.WriteLine(Guid.NewGuid());

            //string error = null;
            //error ??= "";
            //Console.WriteLine(error);

            //Console.WriteLine(DatabaseAccess.Common.UserStatus.StatusFromString("test"));

            //Console.WriteLine(DatabaseAccess.Common.UserStatus.StatusFromString("Not Activated"));
            //Console.WriteLine(DatabaseAccess.Common.UserStatus.StatusToString(4));
            //Console.WriteLine(DatabaseAccess.Common.UserStatus.StatusToString(0));


            //Console.WriteLine(Common.Password.PasswordDecryptor.GenerateSalt());
            //Console.WriteLine(Common.Password.PasswordDecryptor.GenerateSalt());
            //Console.WriteLine(Common.Password.PasswordDecryptor.GenerateSalt());
            //var adminUser = DatabaseAccess.Contexts.ConfigDB.Models.AdminUser.GetDefaultData();
            //var salt = adminUser.Salt;
            //var pass = "admin";

            //Console.WriteLine(Common.Password.PasswordEncryptor.EncryptPassword(pass, salt));
            //Console.WriteLine(Common.Password.PasswordEncryptor.EncryptPassword(pass, salt));
            //Console.WriteLine(Common.Password.PasswordEncryptor.EncryptPassword(pass, "6d5e9cdb"));
            //Console.WriteLine(adminUser.Password);

            //DatabaseAccess.Common.LogValue a;
            //a = JsonConvert.DeserializeObject<DatabaseAccess.Common.LogValue>("{Data: []}");
            //Console.WriteLine(a.ToString());
            // Dictionary<string, List<string>> Rights = new();
            // Rights.Add("test", new List<string>() { "test", "test" });
            // var val = Rights.GetValueOrDefault("test", new List<string>());
            // val.Add("test");
            // val.Add("test");
            // Console.WriteLine(JsonConvert.SerializeObject(Rights));
            // Rights.Remove("test");
            // Rights.Add("test", val.Distinct().ToList());
            // //val.Distinct().ToList();
            // Console.WriteLine(JsonConvert.SerializeObject(Rights));

            var sig = getSignatureKey(
                "L+Y/LzysRX4g9qsPan3tEqGgNfyNZqVfzcym9iGJ",
                "20220209T044253Z",
                "us-west-2",
                "execute-api"
            );

            StringBuilder s = new StringBuilder();
            foreach(var b in sig) {
                
                Console.WriteLine(b);
            }
        }
    }

    public class demoModel
    {
        public Guid Id;
        public string Name;
    }
}
