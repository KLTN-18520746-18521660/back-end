using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace DatabaseAccess.Common
{
    public class PasswordEncryptor
    {
        public static string GenerateSalt()
        {
            string uuid = Guid.NewGuid().ToString();
            return string.Join("", uuid.Split("-")).Substring(0, 8);
        }

        public static string EncryptPassword(string Password, string Salt)
        {
            string mixed = $"{CreateHash256(Password)}{CreateMD5(Salt)}";
            return CreateMD5(CreateHash256(CreateHash256(mixed)));
        }

        // ref: https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.md5
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        // ref: https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256
        public static string CreateHash256(string input)
        {
            // Use input string to calculate SHA256 hash
            SHA256 sha256 = SHA256.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
