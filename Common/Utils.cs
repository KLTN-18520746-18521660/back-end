using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using System.Web;

namespace Common
{
    public class Utils
    {
        // Email Format: {64}@{255} ----------- RFC 3696 - Session 3
        // Total length: 320
        public static string EmailRegex = "^[a-z0-9_\\.]{1,64}@[a-z]+\\.[a-z]{2,3}$";
        public static readonly int SeesionTokenLength = 30;
        public static readonly string SessionTokenRegex = "^[a-z-0-9]{30}$";
        public static readonly string PrefixUrlConfirmSignup = "/user/confirm";
        #region Common validate
        public static bool IsEmail(string Input)
        {
            return Regex.IsMatch(Input, EmailRegex);
        }
        public static Guid IsValidUUID(string id)
        {
            if (!Guid.TryParse(id, out var tmpRs)) {
                return default;
            }
            var parser = new Guid(id);
            if (parser.ToString() != id) {
                return default;
            }
            return parser;
        }
        public static DateTime IsValidDateTime(string date, string format)
        {
            if (!DateTime.TryParseExact(date, format, null, System.Globalization.DateTimeStyles.None, out var tmpDate)) {
                return default;
            }
            var parser = DateTime.ParseExact(date, format, null);
            if (parser.ToString(format) != date) {
                return default;
            }
            return parser;
        }
        public static bool IsValidUrl(string URL, bool AllowHttp = true)
        {
            bool result = Uri.TryCreate(URL, UriKind.Absolute, out var uriResult);
            if (AllowHttp) {
                return result && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            }
            return result && uriResult.Scheme == Uri.UriSchemeHttps;
        }
        public static bool IsValidDomainName(string name)
        {
            return Uri.CheckHostName(name) != UriHostNameType.Unknown;
        }
        #endregion
        #region Common functions]
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
        #endregion
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
        public static string TakeShortContentFromContentSearch(string ContentSearch)
        {
            return "";
        }
        #endregion

        #region User
        public static (string url, string state) GenerateUrlConfirm(Guid id, string host)
        {
            string state = RandomString(8);
            StringBuilder path = new StringBuilder(PrefixUrlConfirmSignup);
            path.Append($"?i={ Uri.EscapeDataString(StringDecryptor.Encrypt(id.ToString())) }");
            path.Append($"&d={ Uri.EscapeDataString(StringDecryptor.Encrypt(DateTime.UtcNow.ToString(CommonDefine.DATE_TIME_FORMAT))) }");
            path.Append($"&s={ state }");
            return (Uri.EscapeUriString($"{ host }{ path.ToString() }"), state);
        }
        public static (Guid, DateTime) ParseParamsFromUserlConfirm(string url)
        {
            return (Guid.NewGuid(), DateTime.UtcNow);
        }
        #endregion
    }
}