namespace Common
{
    public static class CommonDefine
    {
        // Email Format: {64}@{255} ----------- RFC 3696 - Session 3
        // Total length: 320
        public static readonly string EMAIL_REGEX = "^[a-z0-9_\\.]{1,64}@[a-z]+\\.[a-z]{2,3}$";
        public static readonly int SESSION_TOKEN_LENGTH = 30;
        public static readonly string SESSION_TOKEN_REGEX = "^[a-z-0-9]{30}$";
        public static readonly string DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss tt";
    }
}