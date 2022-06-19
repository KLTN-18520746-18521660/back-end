
namespace DatabaseAccess
{
    public class IBaseConfigurationDB
    {
        protected static readonly string CONNECT_STRING = "Host={0};Username={1};Database={2};Password={3};Port={4}";
        protected static string __Host      = "localhost";
        protected static string __User      = "postgres";
        protected static string __Port      = "5432";
        protected static string __Password  = "a";

        public static string Host       { get => __Host; }
        public static string User       { get => __User; }
        public static string Port       { get => __Port; }
        public static string Password   { get => __Password; }
    }

    public class BaseConfigurationDB : IBaseConfigurationDB
    {
        private static string __APP_NAME        = "oOwlet Blog";
        private static string __DBName          = "db_pro_vip";
        protected static bool __IsConfigured    = false;

        public static string APP_NAME           { get => __APP_NAME; }
        public static string DBName             { get => __DBName; }
        public static bool IsConfigured         { get => __IsConfigured; }
        public static bool Configure()
        {
            __IsConfigured = true;
            return true;
        }
        public static bool Configure(string AppName,
                                     string Host,
                                     string User,
                                     string Password,
                                     string Port,
                                     string DBName)
        {
            // Allow run one time
            if (__IsConfigured) {
                return false;
            }
            __APP_NAME  = AppName;
            __DBName    = DBName;
            __Host       = Host;
            __User       = User;
            __Password   = Password;
            __Port       = Port;
            __IsConfigured = true;
            return true;
        }
        public static string GetConnectStringToDB()
        {
            return string.Format(CONNECT_STRING, Host, User, DBName, Password, Port);
        }
    }
}
