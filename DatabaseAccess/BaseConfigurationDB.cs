
namespace DatabaseAccess
{
    public class IBaseConfigurationDB
    {
        protected static readonly string CONNECT_STRING = "Host={0};Username={1};Database={2};Password={3};Port={4}";
        protected static string _Host = "localhost";
        protected static string _User = "postgres";
        protected static string _Password = "a";
        protected static string _Port = "5432";

        public static string Host { get => _Host; }
        public static string User { get => _User; }
        public static string Password { get => _Password; }
        public static string Port { get => _Port; }
    }

    public class BaseConfigurationDB : IBaseConfigurationDB
    {
        private static string _DBName = "db_pro_vip";
        protected static bool _IsConfigured = false;

        public static string DBName { get => _DBName; }
        public static bool IsConfigured { get => _IsConfigured; }
        public static bool Configure()
        {
            _IsConfigured = true;
            return true;
        }
        public static bool Configure(string Host,
                                     string User,
                                     string Password,
                                     string Port)
        {
            // Allow run one time
            if (_IsConfigured) {
                return false;
            }
            
            _Host = Host;
            _User = User;
            _Password = Password;
            _Port = Port;
            _IsConfigured = true;
            return true;
        }
        public static bool Configure(string Host,
                                     string User,
                                     string Password,
                                     string Port,
                                     string DBName)
        {
            // Allow run one time
            if (_IsConfigured) {
                return false;
            }
            
            _Host = Host;
            _User = User;
            _Password = Password;
            _Port = Port;
            _DBName = DBName;
            _IsConfigured = true;
            return true;
        }
        public static string GetConnectStringToDB()
        {
            return string.Format(CONNECT_STRING, Host, User, DBName, Password, Port);
        }
    }
}
