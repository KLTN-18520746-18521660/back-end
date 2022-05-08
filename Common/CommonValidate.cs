using System;
using System.Text.RegularExpressions;

namespace Common
{
    public class CommonValidate
    {
        public static string ValidateFilePath(in string FilePath, bool CreateFileIfNotExist = true, string Error = default)
        {
            Error ??= string.Empty;
            if (!System.IO.File.Exists(FilePath)) {
                if (CreateFileIfNotExist) {
                    try {
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FilePath));
                        var fileCreated = System.IO.File.Create(FilePath);
                        fileCreated.Close();
                    } catch (Exception) { 
                        Error ??= $"Cannot create file. File path: { FilePath }";
                        return default;
                    }
                } else {
                    Error = $"File not exists. File path: { FilePath }";
                    return default;
                }
            }
            var file = System.IO.File.Open(FilePath, System.IO.FileMode.Append);
            file.Flush();
            file.Close();
            return System.IO.Path.GetFullPath(FilePath);
        }
        public static string ValidateDirectoryPath(in string DirPath, in bool CreatePathIfNotExist = true, string Error = default)
        {
            Error ??= string.Empty;
            if (!System.IO.Directory.Exists(DirPath)) {
                if (CreatePathIfNotExist) {
                    try {
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetFullPath(DirPath));
                    } catch (Exception) {
                        Error ??= $"Cannot create directory. Directory: { DirPath }";
                        return default;
                    }
                } else {
                    Error ??= $"Directory path not exists. Directory: { DirPath }";
                    return default;
                }
            }
            return System.IO.Path.GetFullPath(DirPath);
        }
        public static bool ValidatePort(in string Port, string Error = default)
        {
            Error ??= string.Empty;
            if (Regex.IsMatch(Port, "^[0-9]{0,5}$")) {
                int portInt = int.Parse(Port);
                // Valid port from 0 to 65535
                return portInt >= 0 && portInt <= 65535;
            }
            Error ??= $"{ Port } not valid. Valid port from 0 to 65535";
            return false;
        }
        public static bool IsEmail(string Input)
        {
            return Regex.IsMatch(Input, CommonDefine.EMAIL_REGEX);
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
        public static bool IsValidSessionToken(string session_token)
        {
            if (session_token.Length != CommonDefine.SESSION_TOKEN_LENGTH ||
                !Regex.IsMatch(session_token, CommonDefine.SESSION_TOKEN_REGEX)) {
                return false;
            }
            return true;
        }
        public static bool PasswordValidator(string password,
                                             int minLen,
                                             int maxLen,
                                             int minUpperChar,
                                             int minLowerChar,
                                             int minSpecialChar,
                                             string errMsg = default)
        {
            errMsg ??= string.Empty;
            if (password.Length < minLen || password.Length > maxLen) {
                errMsg ??= $"Password must less than { maxLen } and greater than { minLen }";
                return false;
            }
            // if (password)
            return true;
        }
    }
}