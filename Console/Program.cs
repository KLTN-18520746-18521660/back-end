using System;
using System.Collections.Generic;
using Common;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DatabaseAccess.Context.Models;
using CoreApi.Services;

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
                "What's your name?",
                "Vĩnh   Ngọc Trịnh@#%^*())  142612819$^&*(   Vĩnh 新年快",
                "Hoàng tử bé và hành tinh B612: Kết nối những tâm hồn 'từng là trẻ con'",
                "Bạn quen trên mạng nhờ 'giữ giùm' 1,5 triệu đô, đô đâu chưa thấy, mất luôn 151 triệu đồng 27-2/2000",
            };
            var results = new List<string>(){
                "xuat-khau-khi-dot-tu-nga-thuy-den-chau-au-thong-qua-ukraine-tang-gan-40",
                "whats-your-name",
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
        static void Way01(int loop)
        {
            var var1 = "hi";
            var var2 = "hi_1";
            var _t = new StringBuilder();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < loop; i++) {
                _t.Append($"heelo{ var1 }, { var2 }");
            }
            stopwatch.Stop();
            Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds);
        }
        static void Way02(int loop)
        {
            var var1 = "hi";
            var var2 = "hi_1";
            var _t = new StringBuilder();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < loop; i++) {
                _t.Append($"heelo{ var1 }" + $" { var2 }");
            }
            stopwatch.Stop();
            Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds);
        }
        static void Main(string[] args)
        {
            var r = DEFAULT_BASE_CONFIG.GetValueFormatOfConfigKey(CONFIG_KEY.UI_CONFIG);
            Console.WriteLine(r);
            r = DEFAULT_BASE_CONFIG.GetValueFormatOfConfigKey(CONFIG_KEY.EMAIL_CLIENT_CONFIG);
            Console.WriteLine(r);
            r = DEFAULT_BASE_CONFIG.GetValueFormatOfConfigKey(CONFIG_KEY.ADMIN_PASSWORD_POLICY);
            Console.WriteLine(r);
            r = DEFAULT_BASE_CONFIG.GetValueFormatOfConfigKey(CONFIG_KEY.API_GET_COMMENT_CONFIG);
            Console.WriteLine(r);
            r = DEFAULT_BASE_CONFIG.GetValueFormatOfConfigKey(CONFIG_KEY.PUBLIC_CONFIG);
            Console.WriteLine(r);
        }
    }
}
