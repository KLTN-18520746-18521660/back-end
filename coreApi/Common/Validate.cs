namespace coreApi.Common
{
    public class CommonValidate
    {
        public static string ValidateFilePath(in string filePath, in bool createFileIfNotExist = true)
        {
            if (!System.IO.File.Exists(filePath)) {
                if (createFileIfNotExist) {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
                    var fileCreated = System.IO.File.Create(filePath);
                    fileCreated.Close();
                }
                return null;
            }
            var file = System.IO.File.Open(filePath, System.IO.FileMode.Append);
            file.Flush();
            file.Close();
            return System.IO.Path.GetFullPath(filePath);
        }
        public static string ValidateDirectoryPath(in string dirPath, in bool createPathIfNotExist = true)
        {
            if (!System.IO.Directory.Exists(dirPath)) {
                if (createPathIfNotExist) {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dirPath));
                }
                return null;
            }
            return System.IO.Path.GetFullPath(dirPath);
        }
    }
}