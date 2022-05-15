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
        public static bool IsJsonArray(string Data)
        {
            try {
                return JToken.Parse(Data).Type == JTokenType.Array;
            } catch (JsonReaderException) {
                return false;
            }
        }
        public static bool IsJsonObject(string Data)
        {
            try {
                return JToken.Parse(Data).Type == JTokenType.Object;
            } catch (JsonReaderException) {
                return false;
            }
        }
        public static JToken TrimJsonBodyRequest(string OriginBody)
        {
            if (Utils.IsJsonArray(OriginBody)) {
                var RetRaw = new JArray();
                JArray Array = JArray.Parse(OriginBody);
                foreach (var Val in Array) {
                    if (Val.Type == JTokenType.Array || Val.Type == JTokenType.Object) {
                        RetRaw.Add(TrimJsonBodyRequest(Val.ToString()));
                    } else if (Val.Type == JTokenType.String) {
                        RetRaw.Add(JToken.FromObject(Val.ToString().Trim()));
                    } else {
                        RetRaw.Add(Val);
                    }
                }
                return RetRaw;
            } else {
                var RetRaw = new JObject();
                JObject Object = JObject.Parse(OriginBody);
                foreach (var Val in Object) {
                    if (Val.Value.Type == JTokenType.Array || Val.Value.Type == JTokenType.Object) {
                        RetRaw.Add(Val.Key, TrimJsonBodyRequest(Val.Value.ToString()));
                    } else if (Val.Value.Type == JTokenType.String) {
                        if (CommonDefine.NOT_TRIM_KEYS.Contains(Val.Key.ToLower())) {
                            RetRaw.Add(Val.Key, Val.Value);
                        } else {
                            RetRaw.Add(Val.Key, Val.Value.ToString().Trim());
                        }
                    } else {
                        RetRaw.Add(Val.Key, Val.Value);
                    }
                }
                return RetRaw;
            }
        }
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
        public static JToken ObjectToJsonToken(object Source)
        {
            return JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(Source));
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
        public static string BindModelToString<T>(string Template, T Model) where T : class
        {
            var Ret = Template;
            var FindProperties = new Regex(@"@Model.([a-zA-Z]+[a-zA-Z-0-9]*)");
            var Res = FindProperties.Matches(Template)
                .Cast<Match>()
                .OrderByDescending(i => i.Index);
            foreach (Match Item in Res) {
                var AllGroup = Item.Groups[0];

                var FoundPropGroup = Item.Groups[1];
                var PropName = FoundPropGroup.Value;

                object Value = string.Empty;
                try {
                    var Prop = typeof(T).GetProperty(PropName);

                    if (Prop != null) {
                        Value = Prop.GetValue(Model, null);
                    }
                } catch (Exception) {
                    return string.Empty;
                }

                Ret = Ret.Remove(AllGroup.Index, AllGroup.Length)
                    .Insert(AllGroup.Index, Value.ToString());
            }
            return Ret;
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
            retStr = Regex.Replace(retStr, "[\\u0300-\\u036f]", string.Empty);
            retStr = Regex.Replace(retStr, "đ", "d");
            retStr = Regex.Replace(retStr, "[-,\\/]+", "-");
            retStr = Regex.Replace(retStr, "[~`'!@#$%^&*()+={}\\[\\];:\\\"<>.,\\/\\\\?-_]", string.Empty);
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
            foreach (var chr in raw) {
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
        #region Log
        public static JObject CensorSensitiveDate(JObject Obj)
        {
            var RetObj          = Obj;
            var SensitiveKey    = new string[]{
                "password",
                "salt",
            };
            foreach (var Key in SensitiveKey) {
                if (RetObj.ContainsKey(Key)) {
                    // All value of sensitive key is string
                    var SensitiveDataLength = RetObj.Value<string>(Key).Length;
                    SensitiveDataLength     = SensitiveDataLength == 0 ? 3 : SensitiveDataLength;
                    RetObj.SelectToken(Key).Replace(new string('*', SensitiveDataLength));
                }
            }
            return RetObj;
        }
        #endregion
    }
}