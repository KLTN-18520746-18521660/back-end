using System;
using System.Text.RegularExpressions;

namespace Common.Validate
{
    public class CommonValidate
    {
        public static string ValidateFilePath(in string FilePath, in bool CreateFileIfNotExist = true, string Error = null)
        {
            Error ??= "";
            if (!System.IO.File.Exists(FilePath)) {
                if (CreateFileIfNotExist) {
                    try {
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FilePath));
                        var fileCreated = System.IO.File.Create(FilePath);
                        fileCreated.Close();
                    } catch (Exception) { 
                        Error ??= $"Cannot create file. File path: { FilePath }";
                        return null;
                    }
                }
                Error = $"File not exists. File path: { FilePath }";
                return null;
            }
            var file = System.IO.File.Open(FilePath, System.IO.FileMode.Append);
            file.Flush();
            file.Close();
            return System.IO.Path.GetFullPath(FilePath);
        }
        public static string ValidateDirectoryPath(in string DirPath, in bool CreatePathIfNotExist = true, string Error = null)
        {
            Error ??= "";
            if (!System.IO.Directory.Exists(DirPath)) {
                if (CreatePathIfNotExist) {
                    try {
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(DirPath));
                    } catch (Exception) {
                        Error ??= $"Cannot create directory. Directory: { DirPath }";
                        return null;
                    }
                }
                Error ??= $"Directory path not exists. Directory: { DirPath }";
                return null;
            }
            return System.IO.Path.GetFullPath(DirPath);
        }
        public static bool ValidatePort(in string Port, string Error = null)
        {
            Error ??= "";
            if (Regex.IsMatch(Port, "^[0-9]{0,5}$")) {
                int portInt = int.Parse(Port);
                // Valid port from 0 to 65535
                return portInt >= 0 && portInt <= 65535;
            }
            Error ??= $"{ Port } not valid. Valid port from 0 to 65535";
            return false;
        }
    }
}