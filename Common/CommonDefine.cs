using Serilog.Events;

namespace Common
{
    public enum LOG_LEVEL
    {
        VERBOSE     = LogEventLevel.Verbose,
        DEBUG       = LogEventLevel.Debug,
        INFO        = LogEventLevel.Information,
        WARNING     = LogEventLevel.Warning,
        ERROR       = LogEventLevel.Error,
        FATAL       = LogEventLevel.Fatal,
    }
    public static class COMMON_DEFINE
    {
        // Email Format: {64}@{255} ----------- RFC 3696 - Session 3
        // Total length: 320
        public static readonly string       EMAIL_REGEX             = "^[a-z0-9_\\.]{1,64}@[a-z]+\\.[a-z]{2,3}$";
        public static readonly int          SESSION_TOKEN_LENGTH    = 30;
        public static readonly string       SESSION_TOKEN_REGEX     = "^[a-z-0-9]{30}$";
        public static readonly string       DATE_TIME_FORMAT        = "yyyy-MM-dd HH:mm:ss tt";
        public static readonly string       PARAM_LOG_TEMPLATE      = "{0}: {1}";
        public static readonly string[]     NOT_TRIM_KEYS           = new string[]{
            "password",
        };
        // Value with key is sensitive. Ex: {"key": "value"} --> {"key": "*****"}
        public static readonly string[]     SENSITIVE_KEY           = new string[]{
            "password",
            "salt",
        };
        // Value with key is sensitive. Ex: {"key": "value"} --> {"key": "val**"}
        public static readonly string[]     CENSOR_KEY              = new string[]{
            "session_token",
            "session_token_admin",
            "delete_session_token",
            "get_session_token",
        };
    }
}