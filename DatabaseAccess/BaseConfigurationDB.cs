
namespace DatabaseAccess
{
    public class IBaseConfigurationDB
    {
#pragma warning disable CA2211 // Non-constant fields should not be visible
        protected static readonly string CONNECT_STRING = "Host={0};Username={1};Database={2};Password={3};Port={4}";
        protected static string _Host = "localhost";
        protected static string _User = "postgres";
        protected static string _Password = "a";
        protected static string _Port = "5432";
#pragma warning restore CA2211 // Non-constant fields should not be visible

        public static string Host { get => _Host; }
        public static string User { get => _User; }
        public static string Password { get => _Password; }
        public static string Port { get => _Port; }
    }

    public class BaseConfigurationDB : IBaseConfigurationDB
    {
        private static string _ConfigDBName = "config_db";
        private static string _SocialDBName = "social_db";
        private static string _InventoryDBName = "inventory_db";
        private static string _CachedDBName = "cached_db";
#pragma warning disable CA2211 // Non-constant fields should not be visible
        protected static bool _IsConfigured = false;
#pragma warning restore CA2211 // Non-constant fields should not be visible

        public static string ConfigDBName { get => _ConfigDBName; }
        public static string SocialDBName { get => _SocialDBName; }
        public static string InventoryDBName { get => _InventoryDBName; }
        public static string CachedDBName { get => _CachedDBName; }
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
                                     string ConfigDBName,
                                     string SocialDBname,
                                     string InventoryDBName,
                                     string CachedDBName)
        {
            // Allow run one time
            if (_IsConfigured) {
                return false;
            }
            
            _Host = Host;
            _User = User;
            _Password = Password;
            _Port = Port;
            _ConfigDBName = ConfigDBName;
            _SocialDBName = SocialDBname;
            _InventoryDBName = InventoryDBName;
            _CachedDBName = CachedDBName;
            
            _IsConfigured = true;
            return true;
        }
        public static string GetConnectStringToConfigDB()
        {
            return string.Format(CONNECT_STRING, Host, User, ConfigDBName, Password, Port);
        }
        public static string GetConnectStringToSocialDB()
        {
            return string.Format(CONNECT_STRING, Host, User, SocialDBName, Password, Port);
        }
        public static string GetConnectStringToInventoryDB()
        {
            return string.Format(CONNECT_STRING, Host, User, InventoryDBName, Password, Port);
        }
        public static string GetConnectStringToCachedDB()
        {
            return string.Format(CONNECT_STRING, Host, User, CachedDBName, Password, Port);
        }
    }
}
