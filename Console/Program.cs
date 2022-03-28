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
            Console.WriteLine(StringDecryptor.Encrypt(passCert));
            Console.WriteLine(StringDecryptor.Encrypt(passDb));
            Console.WriteLine(StringDecryptor.Encrypt(passEmail));
        }
        static void Main(string[] args)
        {
            PrintPassEncrypt();
        }
    }
}
