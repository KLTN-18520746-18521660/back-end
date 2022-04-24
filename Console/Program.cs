using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
// using DatabaseAccess.Common.Models;
using Common;
using System.Text;
// using CoreApi;
using System.Threading;

namespace MyConsole
{
    public class tst
    {
        public SemaphoreSlim __Gate;
        public int __GateLimit;
        public tst()
        {
            __GateLimit = 1;
            __Gate = new SemaphoreSlim(__GateLimit);
        }
        public bool ChangeGateLimit(int value)
        {
            int diff = Math.Abs(value - __GateLimit);
            if (diff == 0 || value < 1) {
                return false;
            }
            for (int i = 0; i < diff; i++) {
                if (value > __GateLimit) {
                    __Gate.Release();
                } else {
                    __Gate.WaitAsync();
                }
            }
            __GateLimit = value;
            return true;
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
            string passEmail = "wecxrnzqcwkldrla";
            string passSwagger = "admin";
            Console.WriteLine(StringDecryptor.Encrypt(passCert));
            Console.WriteLine(StringDecryptor.Encrypt(passDb));
            Console.WriteLine(StringDecryptor.Encrypt(passEmail));
            Console.WriteLine(StringDecryptor.Encrypt(passSwagger));
        }
        static void TestGenerateSlug()
        {
            var tests = new List<string>(){
                "Xuất khẩu khí ---đốt từ Nga thủy đến châu Âu thông qua Ukraine tăng gần 40%",
                "Vĩnh   Ngọc Trịnh@#%^*())  142612819$^&*(   Vĩnh 新年快",
                "Hoàng tử bé và hành tinh B612: Kết nối những tâm hồn 'từng là trẻ con'",
                "Bạn quen trên mạng nhờ 'giữ giùm' 1,5 triệu đô, đô đâu chưa thấy, mất luôn 151 triệu đồng 27-2/2000",
            };
            var results = new List<string>(){
                "xuat-khau-khi-dot-tu-nga-thuy-den-chau-au-thong-qua-ukraine-tang-gan-40",
                "vinh-ngoc-trinh-142612819-vinh-%E6%96%B0%E5%B9%B4%E5%BF%AB",
                "hoang-tu-be-va-hanh-tinh-b612-ket-noi-nhung-tam-hon-tung-la-tre-con",
                "ban-quen-tren-mang-nho-giu-gium-1-5-trieu-do-do-dau-chua-thay-mat-luon-151-trieu-dong-27-2-2000",
            };

            for (int i = 0; i < tests.Count; i++) {
                var r = Utils.GenerateSlug(tests[i]);
                Console.WriteLine(r == results[i]);
            }
        }
        class test {
            public string action;
            public DateTime date;

            public test(string a, DateTime d) {
                action = a;
                date = d;
            }
        }
        static void Main(string[] args)
        {
            // List<(string action, DateTime date)> tst = new List<(string action, DateTime date)>();
            List<EntityAction> tst = new List<EntityAction>();

            var ac = new EntityAction(EntityActionType.UserActionWithCategory, ActionType.Follow);
            tst.Add(ac);

            Console.WriteLine(JArray.FromObject(tst));


            List<EntityAction> tst_ser = new List<EntityAction>();
            var obj = "[{\"action\": \"Follow\",\"date\": \"2022-04-24T10:20:15.2954809Z\"}]";
            var arr = JsonConvert.DeserializeObject<JArray>(obj);

            foreach (var a in arr) {
                tst_ser.Add(new(EntityActionType.UserActionWithCategory,
                                (a as JObject).Value<string>("action"))
                {
                        date = (a as JObject).Value<DateTime>("date")
                    }
                );
            }

            Console.WriteLine(JArray.FromObject(tst_ser));
        }
    }
}
