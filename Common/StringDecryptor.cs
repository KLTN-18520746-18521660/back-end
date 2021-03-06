using System;
using System.Text;
using System.Security.Cryptography;

namespace Common
{
    public class StringDecryptor
    {
        private static string key = "AbcDSEmIQSm@*$&!=)!_-";

        public static string Encrypt(string text)
        {
            var data = Encoding.UTF8.GetBytes(text);
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var keys = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
                using (var tripDes = new TripleDESCryptoServiceProvider { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    var transform = tripDes.CreateEncryptor();
                    var results = transform.TransformFinalBlock(data, 0, data.Length);
                    return Convert.ToBase64String(results, 0, results.Length);
                }
            }
        }

        public static string Decrypt(string text)
        {
            var spanData = new Span<byte>(new byte[256]);
            if (!Convert.TryFromBase64String(text, spanData, out int bytesWritten)) {
                return default;
            }
            var data = Convert.FromBase64String(text);
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var keys = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
                using (var tripDes = new TripleDESCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    var transform = tripDes.CreateDecryptor();
                    var results = transform.TransformFinalBlock(data, 0, data.Length);
                    return Encoding.UTF8.GetString(results);
                }
            }
        }
    }
}
