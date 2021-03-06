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
        public static string RemoveSign4VietnameseString(string str)
        {
            for (int i = 1; i < COMMON_DEFINE.VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < COMMON_DEFINE.VietnameseSigns[i].Length; j++)
                    str = str.Replace(COMMON_DEFINE.VietnameseSigns[i][j], COMMON_DEFINE.VietnameseSigns[0][i - 1]);
            }
            return str;
        }
        public static string PrepareSearchTerm(string RawSearchTerm)
        {
            return RemoveSign4VietnameseString(RawSearchTerm).ToLower().Trim();
        }
        public static string CensorString(string Value)
        {
            return Value.Length > 3
                    ? $"{ Value.Substring(0, 3) }{ new string('*', Value.Length - 3) }"
                    : new string('*', Value.Length);
        }
        public static string HideString(string Value)
        {
            return new string('*', 5);
        }
        public static string GetHandlerNameFromClassName(string ClassName)
        {
            var RemoveStrs  = new string[]{ "service", "controller" };
            var Ret         = ClassName;
            foreach (var Str in RemoveStrs) {
                Ret = Regex.Replace(Ret, Str, "", RegexOptions.IgnoreCase);
            }
            return Ret;
        }
        public static string ParamsToLog<T>(string PName, T PValue)
        {
            var Value = "null";
            if (PValue != null) {
                Value = JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(PValue)).ToString(Formatting.None);
                if (COMMON_DEFINE.SENSITIVE_KEY.Contains(PName)) {
                    Value = new string('*', 5);
                } else if (COMMON_DEFINE.CENSOR_KEY.Contains(PName)) {
                    Value = Value.Length > 3
                        ? $"{ Value.Substring(0, 3) }{ new string('*', Value.Length > 5 ? 5 : Value.Length - 3 ) }"
                        : new string('*', Value.Length);
                }
            }
            return string.Format(COMMON_DEFINE.PARAM_LOG_TEMPLATE,
                                 PName,
                                 Value
            );
        }
        public static bool IsJsonArray(string Data)
        {
            try {
                return JToken.Parse(Data).Type == JTokenType.Array;
            } catch (JsonReaderException) {
                return false;
            }
            // $regex.= '\[(?:(?1)(?:,(?1))*)?\s*\]|'; //arrays
            // $regex.= '\{(?:\s*'.$regexString.'\s*:(?1)(?:,\s*'.$regexString.'\s*:(?1))*)?\s*\}';    //objects
        }
        public static bool IsJsonObject(string Data)
        {
            try {
                return JToken.Parse(Data).Type == JTokenType.Object;
            } catch (JsonReaderException) {
                return false;
            }
        }
        public static JToken TrimJsonBodyRequest(JArray Array)
        {
            for (int i = 0; i < Array.Count; i++) {
                if (Array[i].Type == JTokenType.Object) {
                    Array[i] = TrimJsonBodyRequest((JObject) Array[i]);
                } else if (Array[i].Type == JTokenType.Array) {
                    Array[i] = TrimJsonBodyRequest((JArray) Array[i]);
                } else if (Array[i].Type == JTokenType.String) {
                    Array[i] = JToken.FromObject(Array[i].ToString().Trim());
                }
            }
            return Array;
        }
        public static JToken TrimJsonBodyRequest(JObject Object)
        {
            foreach (var It in Object) {
                if (It.Value.Type == JTokenType.Object) {
                    Object[It.Key] = TrimJsonBodyRequest((JObject) It.Value);
                } else if (It.Value.Type == JTokenType.Array) {
                    Object[It.Key] = TrimJsonBodyRequest((JArray) It.Value);
                } else if (It.Value.Type == JTokenType.String) {
                    if (!COMMON_DEFINE.NOT_TRIM_KEYS.Contains(It.Key.ToLower())) {
                        Object[It.Key] = JToken.FromObject(It.Value.ToString().Trim());
                    }
                }
            }
            return Object;
        }
        public static JToken TrimJsonBodyRequest(string OriginBody)
        {
            if (Utils.IsJsonArray(OriginBody)) {
                return TrimJsonBodyRequest(JArray.Parse(OriginBody));
            } else if (Utils.IsJsonObject(OriginBody)) {
                return TrimJsonBodyRequest(JObject.Parse(OriginBody));
            }
            return default;
        }
        public static JArray MakeValueJSonEmpty(JArray OriginArray, string[] IgnoreKeys = default)
        {
            for (int i = 0; i < OriginArray.Count; i++) {
                if (OriginArray[i].Type == JTokenType.Object) {
                    OriginArray[i] = MakeValueJSonEmpty((JObject) OriginArray[i], IgnoreKeys);
                } else if (OriginArray[i].Type == JTokenType.Array) {
                    OriginArray[i] = MakeValueJSonEmpty((JArray) OriginArray[i], IgnoreKeys);
                } else {
                    OriginArray[i] = "";
                }
            }
            return JArray.FromObject(OriginArray.ToHashSet());
        }
        public static JObject MakeValueJSonEmpty(JObject OriginObject, string[] IgnoreKeys = default)
        {
            foreach (var It in OriginObject) {
                if (IgnoreKeys != default && IgnoreKeys.Contains(It.Key)) {
                    continue;
                }
                if (It.Value.Type == JTokenType.Object) {
                    OriginObject[It.Key] = MakeValueJSonEmpty((JObject) It.Value, IgnoreKeys);
                } else if (It.Value.Type == JTokenType.Array) {
                    OriginObject[It.Key] = MakeValueJSonEmpty((JArray) It.Value, IgnoreKeys);
                } else {
                    OriginObject[It.Key] = "";
                }
            }
            return OriginObject;
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
        public static (JObject, JObject) GetDataChanges(JObject OldObj, JObject NewObj) {
            var OldKeys = OldObj.Properties().Select(e => e.Name).ToArray();
            foreach (var Key in OldKeys) {
                if (NewObj.ContainsKey(Key) && OldObj.GetValue(Key).ToString() == NewObj.GetValue(Key).ToString()) {
                    OldObj.Remove(Key);
                    NewObj.Remove(Key);
                }
            }
            return (OldObj, NewObj);
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
        public static string RandomString(int StringLen, int? Seed = default)
        {
            string PossibleChar = "abcdefghijklmnopqrstuvwxyz0123456789";
            Random Rand = new Random();
            if (Seed != default) {
                Rand = new Random((int)Seed);
            }
            StringBuilder Output = new StringBuilder(string.Empty);
            for (int i = 0; i < StringLen; i++) {
                int pos = Rand.Next(0, PossibleChar.Length);
                Output.Append(PossibleChar[pos].ToString());
            }
            return Output.ToString();
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
        public static string GenerateOrderString((string, bool)[] Orders, string[] IgnoreFields = default)
        {
            /* Example
            * orders = [{"views", false}, {"created_timestamp", true}]
            * ==>
            * views asc, created_timestamp desc
            */
            var OrderStatements = new List<string>();
            foreach (var Order in Orders) {
                if (IgnoreFields != default && IgnoreFields.Contains(Order.Item1)) {
                    continue;
                }
                OrderStatements.Add($"{ Order.Item1 } {( Order.Item2 ? "desc" : "asc" )}");
            }
            return string.Join(", ", OrderStatements).Trim();
        }
        public static string[] GetModelProperties(string Template)
        {
            var FindProperties = new Regex(@"@Model\.([a-zA-Z]+[a-zA-Z-0-9]*)");
            return FindProperties.Matches(Template)
                .Cast<Match>()
                .OrderByDescending(i => i.Index)
                .Select(e => e.Value).ToArray();
        }
        public static string BindModelToString<T>(string Template, T Model) where T : class
        {
            var Ret = Template;
            var FindProperties = new Regex(@"@Model\.([a-zA-Z]+[a-zA-Z-0-9]*)");
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
        public static string SimpleDecryptGuid(Guid Id)
        {
            var RandomStr   = RandomString(5, Id.GetHashCode());
            var Prefix      = Id.ToString().Skip(1).Take(5).ToArray();

            var _Prefix      = new string(Prefix);

            return $"{ RandomStr }{ _Prefix }";
        }
        #endregion
        #region Session
        public static string GenerateSessionToken()
        {
            return RandomString(COMMON_DEFINE.SESSION_TOKEN_LENGTH);
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
            retStr = Regex.Replace(retStr, "??", "d");
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
        public static string TakeContentForSearchFromRawContent(string RawContent, bool IsMarkdown) // default is HTML
        {
            var Ret = DeepClone<string>(RawContent);
            if (IsMarkdown) {
                Ret = Regex.Replace(Ret, "$>#[*`!\\[\\]-_]+", " ");
                Ret = Regex.Replace(Ret, "(.*?)", " ");
            }
            Ret = Regex.Replace(Ret, "<[\\/a-zA-Z0-9= \"\\\"'\'#;:()$_-]*?>", " ");
            Ret = Regex.Replace(Ret, "&.*;", " ");
            var ArrChr = Ret.ToArray();
            for (var i = 0; i < ArrChr.Length; i++) {
                if (!Char.IsLetterOrDigit(ArrChr[i])) {
                    ArrChr[i] = ' ';
                }
            }
            Ret = new string(ArrChr);
            Ret = Regex.Replace(Ret, "\\s+", " ");
            return Ret.Trim();
        }
        public static string TakeShortContentFromContentSearch(string ContentSearch)
        {
            if (ContentSearch.Length <= 200) {
                return ContentSearch;
            }
            int Ind;
            for (Ind = 200; Ind < ContentSearch.Length; Ind++) {
                if (ContentSearch[Ind] == ' ') {
                    break;
                }
            }
            return ContentSearch.Substring(0, Ind + 1);
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
        public static (string url, string state) GenerateUrl(Guid id, string host, string prefixUrl)
        {
            string state = RandomString(8);
            StringBuilder path = new StringBuilder(prefixUrl);
            path.Append($"?i={ Uri.EscapeDataString(StringDecryptor.Encrypt(id.ToString())) }");
            path.Append($"&d={ Uri.EscapeDataString(StringDecryptor.Encrypt(DateTime.UtcNow.ToString(COMMON_DEFINE.DATE_TIME_FORMAT))) }");
            path.Append($"&s={ state }");
            return (Uri.EscapeUriString($"{ host }{ path.ToString() }"), state);
        }
        #endregion
        #region Log
        public static JArray CensorSensitiveDate(JArray Array)
        {
            for (int i = 0; i < Array.Count; i++) {
                if (Array[i].Type == JTokenType.Object) {
                    Array[i] = CensorSensitiveDate((JObject) Array[i]);
                } else if (Array[i].Type == JTokenType.Array) {
                    Array[i] = CensorSensitiveDate((JArray) Array[i]);
                }
            }
            return Array;
        }
        public static JObject CensorSensitiveDate(JObject Obj)
        {
            foreach (var It in Obj) {
                if (It.Value.Type == JTokenType.Object) {
                    Obj[It.Key] = CensorSensitiveDate((JObject) Obj[It.Key]);
                } else if (It.Value.Type == JTokenType.Array) {
                    Obj[It.Key] = CensorSensitiveDate((JArray) Obj[It.Key]);
                } else if (It.Value.Type == JTokenType.String) {
                    Obj[It.Key] = JToken.FromObject(It.Value.ToString().Trim());
                    if (COMMON_DEFINE.SENSITIVE_KEY.Contains(It.Key)) {
                        Obj[It.Key] = HideString(Obj[It.Key].ToString());
                    }
                    if (COMMON_DEFINE.CENSOR_KEY.Contains(It.Key)) {
                        Obj[It.Key] = CensorString(Obj[It.Key].ToString());
                    }
                }
            }
            return Obj;
        }
        #endregion
    }
}