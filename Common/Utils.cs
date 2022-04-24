using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System.Linq;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Common
{
    public class Utils
    {
        #region Common functions
        public static T DeepClone<T>(T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null)) return default;

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings {ObjectCreationHandling = ObjectCreationHandling.Replace};

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }
        public static JToken ObjectToJsonToken(object source)
        {
            return JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(source));
        }
        public static (JObject, JObject) GetDataChanges(JObject oldObj, JObject newObj) {
            var oldKeys = oldObj.Properties().Select(e => e.Name).ToArray();
            foreach (var key in oldKeys) {
                if (newObj.ContainsKey(key) && oldObj.GetValue(key).ToString() == newObj.GetValue(key).ToString()) {
                    oldObj.Remove(key);
                    newObj.Remove(key);
                }
            }
            return (oldObj, newObj);
        }
        // public static EncryptUserId
        public static (List<T> items, string errMsg) LoadListJsonFromFile<T>(string filePath) where T : class
        {
            string errMsg = default;
            List<T> items = default;
            var fullPath = CommonValidate.ValidateFilePath(filePath, false, errMsg);
            if (errMsg != default || fullPath == default) {
                return (items, errMsg);
            }
            using (StreamReader r = new StreamReader(fullPath))
            {
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<List<T>>(json);
            }
            return (items, errMsg);
        }
        public static string RandomString(int StringLen)
        {
            string possibleChar = "abcdefghijklmnopqrstuvwxyz0123456789";
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            StringBuilder output = new StringBuilder(string.Empty);
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
        public static string GenerateOrderString((string, bool)[] orders)
        {
            /* Example
            * orders = [{"views", false}, {"created_timestamp", true}]
            * ==>
            * views asc, created_timestamp desc
            */
            var orderStrs = new List<string>();
            foreach (var order in orders) {
                orderStrs.Add($"{ order.Item1 } {( order.Item2 ? "desc" : "asc" )}");
            }
            return string.Join(", ", orderStrs).Trim();
        }
        public string BindModelToString<T>(string template, T model) where T : class
        {
            var ret = template;
            var findProperties = new Regex(@"@Model.([a-zA-Z]+[0-9]*)");
            var res = findProperties.Matches(template)
                .Cast<Match>()
                .OrderByDescending(i => i.Index);
            foreach (Match item in res) {
                var allGroup = item.Groups[0];

                var foundPropGrRoup = item.Groups[1];
                var propName = foundPropGrRoup.Value;

                object value = string.Empty;

                try {
                    var prop = typeof(T).GetProperty(propName);

                    if (prop != null) {
                        value = prop.GetValue(model, null);
                    }
                } catch (Exception) {
                    throw new Exception("Missing value for binding model.");
                }

                ret = ret.Remove(allGroup.Index, allGroup.Length)
                    .Insert(allGroup.Index, value.ToString());
            }
            return ret;
        }
        #endregion
        #region Session
        public static string GenerateSessionToken()
        {
            return RandomString(CommonDefine.SESSION_TOKEN_LENGTH);
        }
        #endregion
        #region Slug
        public static string GenerateSlug(string Original, bool appendTimeStamp = false)
        {
            var rawStr = Original
                .Normalize(System.Text.NormalizationForm.FormD)
                .ToCharArray()
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray();
            var retStr = new string(rawStr);
            retStr = retStr.ToLower();
            retStr = Regex.Replace(retStr, "\\s\\s+", " ");
            retStr = Regex.Replace(retStr, "[\\u0300-\\u036f]", "");
            retStr = Regex.Replace(retStr, "đ", "d");
            retStr = Regex.Replace(retStr, "[-,\\/]+", "-");
            retStr = Regex.Replace(retStr, "[~`!@#$%^&*()+={}\\[\\];:\\'\\\"<>.,\\/\\\\?-_]", "");
            retStr = Regex.Replace(retStr, "\\s", "-");
            retStr = Regex.Replace(retStr, "\\-\\-+", "-");
            if (appendTimeStamp) {
                return $"{ retStr }-{ ((DateTimeOffset) DateTime.UtcNow).ToUnixTimeSeconds() }";
            }
            return Uri.EscapeDataString(retStr);
        }
        #endregion
        #region Post
        public static string TakeContentForSearchFromRawContent(string RawContent)
        {
            return string.Empty;
        }
        public static string TakeShortContentFromContentSearch(string ContentSearch)
        {
            return "demo_short_content";
        }
        #endregion
        #region User
        public static string GenerateUserName()
        {
            string possibleChar = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var raw = StringDecryptor.Encrypt(Guid.NewGuid().ToString()).Take(5);
            var raw2 = RandomString(5);
            var ret = new StringBuilder();
            foreach(var chr in raw) {
                if (possibleChar.Contains(chr)){
                    ret.Append(chr);
                }
            }
            return ret.Append(raw2).ToString();
        }
        public static (string url, string state) GenerateUrlConfirm(Guid id, string host, string prefixUrl)
        {
            string state = RandomString(8);
            StringBuilder path = new StringBuilder(prefixUrl);
            path.Append($"?i={ Uri.EscapeDataString(StringDecryptor.Encrypt(id.ToString())) }");
            path.Append($"&d={ Uri.EscapeDataString(StringDecryptor.Encrypt(DateTime.UtcNow.ToString(CommonDefine.DATE_TIME_FORMAT))) }");
            path.Append($"&s={ state }");
            return (Uri.EscapeUriString($"{ host }{ path.ToString() }"), state);
        }
        #endregion
    }
}