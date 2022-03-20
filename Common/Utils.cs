using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;

namespace Common
{
    public class Utils
    {
        // Email Format: {64}@{255} ----------- RFC 3696 - Session 3
        // Total length: 320
        public static string EmailRegex = "^[a-z0-9_\\.]{1,64}@[a-z]+\\.[a-z]{2,3}$";
        public static readonly int SeesionTokenLength = 30;
        public static readonly string SessionTokenRegex = "^[a-z-0-9]{30}$";
        public static bool IsEmail(string Input)
        {
            return Regex.IsMatch(Input, EmailRegex);
        }
        public static bool GetIpAddress(out List<string> Ip)
        {
            Ip = new List<string>();
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    Ip.Add(ip.ToString());
                }
            }
            return Ip.Count > 0;
        }
        public static string RandomString(int StringLen)
        {
            string possibleChar = "abcdefghijklmnopqrstuvwxyz0123456789";
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            StringBuilder output = new StringBuilder("");
            for (int i = 0; i < StringLen; i++) {
                int pos = rand.Next(0, possibleChar.Length);
                output.Append(possibleChar[pos].ToString());
            }
            return output.ToString();
        }

        #region Session
        public static bool IsValidSessionToken(string session_token)
        {
            if (session_token.Length != SeesionTokenLength ||
                !Regex.IsMatch(session_token, SessionTokenRegex)) {
                return false;
            }
            return true;
        }
        public static string GenerateSessionToken()
        {
            return RandomString(SeesionTokenLength);
        }
        #endregion

        #region Slug
        public static string GenerateSlug(string Original)
        {
            return "";
        }
        // [TODO] -- wait from fe
        public static bool ValidateSlug(string Slug, string Original)
        {
            return true;
        }
        #endregion

        #region Post
        public static string TakeContentForSearchFromRawContent(string RawContent)
        {
            return "";
        }
        #endregion
    }
}