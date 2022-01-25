using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var adminUser = DatabaseAccess.Contexts.ConfigDB.Models.AdminUser.GetUserDefault();
            var salt = adminUser.Salt;
            var pass = "admin";

            Console.WriteLine(Common.Password.PasswordEncryptor.EncryptPassword(pass, salt));
            Console.WriteLine(Common.Password.PasswordEncryptor.EncryptPassword(pass, salt));
            Console.WriteLine(Common.Password.PasswordEncryptor.EncryptPassword(pass, "6d5e9cdb"));
            Console.WriteLine(adminUser.Password);
        }
    }

    public class demoModel
    {
        public Guid Id;
        public string Name;
    }
}
