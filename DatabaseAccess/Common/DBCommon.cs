using System;

namespace Common
{
    public static class DBCommon
    {
        public static readonly int MAX_LENGTH_OF_TEXT = 65535;
        public static readonly DateTime DEFAULT_DATETIME_FOR_DATA_SEED = new DateTime(2022, 02, 20, 13, 13, 13).ToUniversalTime();
        public static readonly string FIRST_ADMIN_USER_NAME = "admin";
#if DEBUG
        public static readonly Guid FIRST_ADMIN_USER_ID = new Guid("1afc27e9-85c3-4e48-89ab-dd997621ab32");
#else
        public static readonly Guid FIRST_ADMIN_USER_ID = new Guid();
#endif
    }
}