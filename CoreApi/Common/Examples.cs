using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CoreApi.Common
{
    #region Model Example
    public class StatusCode200OKExamples
    {
        [DefaultValue(200)]
        public int status { get; set; }
        [DefaultValue("OK")]
        public string message { get; set; }
    }
    public class SocialPostExample
    {
        [DefaultValue(12)]
        public long id { get; set; }
        [DefaultValue("{\"user_name\":\"name\",\"display_name\":\"name_pro\",\"avatar\":null,\"status\":\"Activated\"}")]
        public JObject owner { get; set; }
        [DefaultValue("Here is title.")]
        public string title { get; set; }
        [DefaultValue("here-is-title")]
        public string slug { get; set; }
        [DefaultValue("thumbail.png")]
        public string thumbnail { get; set; }
        [DefaultValue(12)]
        public int time_read { get; set; }
        [DefaultValue(1000)]
        public int views { get; set; }
        [DefaultValue(1001)]
        public int likes { get; set; }
        [DefaultValue(1)]
        public int dislikes { get; set; }
        [DefaultValue(10)]
        public int comments { get; set; }
        [DefaultValue("[\"developer\",\"left\"]")]
        public JArray tags { get; set; }
        [DefaultValue("[\"other-thing\",\"nothing\"]")]
        public JArray categories { get; set; }
        [DefaultValue(123)]
        public int visited_count { get; set; }
        [DefaultValue("The content of post. <b>ERA</b>")]
        public string content { get; set; }
        [DefaultValue("HTML")]
        public string content_type { get; set; }
        [DefaultValue("Short content.")]
        public string short_content { get; set; }
        [DefaultValue(false)]
        public bool have_pending_content { get; set; }
        [DefaultValue("Pending")]
        public string status { get; set; }
        public DateTime created_timestamp { get; set; }
        public DateTime approved_timestamp { get; set; }
        public DateTime last_modified_timestamp { get; set; }
    }
    public class GetPostByIdSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        [DefaultValue("OK")]
        public string message { get; set; }
        public GetPostByIdData data { get; set; }
        public class GetPostByIdData {
            SocialPostExample post { get; set; }
        }
    }
    #region Admin
    public class AdminUserExample
    {
        public Guid id { get; set; }
        [DefaultValue("admin")]
        public string user_name { get; set; }
        [DefaultValue("admin")]
        public string display_name { get; set; }
        [DefaultValue("admin@admin.com")]
        public string email { get; set; }
        [DefaultValue("Readonly")]
        public string status { get; set; }
        [DefaultValue("[\"admin\"]")]
        public List<string> roles { get; set; }
        [DefaultValue("{\"tst_right\":{\"read\":true,\"write\":true}}")]
        public Dictionary<string, JObject> Rights { get; set; }
        public DateTime last_access_timestamp { get; set; }
        public DateTime created_timestamp { get; set; }
        [DefaultValue("{}")]
        public JObject settings { get; set; }
    }
    public class SessionAdminUserExample
    {
        [DefaultValue("knrgrmao0yuyzy1r8oqs2j8r5otxyn")]
        public string session_token { get; set; }
        [DefaultValue(true)]
        public string saved { get; set; }
        [DefaultValue("{}")]
        public JObject data { get; set; }
        public DateTime created_timestamp { get; set; }
        public DateTime last_interaction_time { get; set; }
    }
    #endregion
    #region Social
    public class SocialUserExample
    {
        [DefaultValue("94571498-b724-47c9-b046-1e932d5ec192")]
        public Guid id { get; set; }
        [DefaultValue("social")]
        public string user_name { get; set; }
        [DefaultValue("social")]
        public string display_name { get; set; }
        [DefaultValue("social@social.com")]
        public string email { get; set; }
        [DefaultValue("Readonly")]
        public string status { get; set; }
        [DefaultValue("[\"social\"]")]
        public List<string> roles { get; set; }
        [DefaultValue("{\"tst_right\":{\"read\":true,\"write\":true}}")]
        public Dictionary<string, JObject> Rights { get; set; }
        public DateTime last_access_timestamp { get; set; }
        public DateTime created_timestamp { get; set; }
        [DefaultValue("{}")]
        public JObject settings { get; set; }
    }
    public class SessionSocialUserExample
    {
        [DefaultValue("knrgrmao0yuyzy1r8oqs2j8r5otxyn")]
        public string session_token { get; set; }
        [DefaultValue(true)]
        public string saved { get; set; }
        [DefaultValue("{}")]
        public JObject data { get; set; }
        public DateTime created_timestamp { get; set; }
        public DateTime last_interaction_time { get; set; }
    }
    #endregion
    #region Common
    public class AuditLogExample
    {
        [DefaultValue(66)]
        public long id { get; set; }
        [DefaultValue("User")]
        public string table { get; set; }
        [DefaultValue("948ab715-0b42-4657-93e5-eaa7bc081ab2")]
        public string table_key { get; set; }
        [DefaultValue("modify")]
        public string action { get; set; }
        [DefaultValue("{\"user_name\":\"abcdef\"}")]
        public JObject old_value { get; set; }
        [DefaultValue("{\"user_name\":\"123456\"}")]
        public JObject new_value { get; set; }
        [DefaultValue("{\"user_name\":\"user_name\",\"display_name\":\"user_pro_vip\",\"avatar\":null}")]
        public JObject user { get; set; }
        public DateTime timestamp { get; set; }
    }
    public class UserAuditLogExample
    {
        [DefaultValue(66)]
        public long id { get; set; }
        [DefaultValue("User")]
        public string table { get; set; }
        [DefaultValue("948ab715-0b42-4657-93e5-eaa7bc081ab2")]
        public string table_key { get; set; }
        [DefaultValue("modify")]
        public string action { get; set; }
        [DefaultValue("{\"user_name\":\"abcdef\"}")]
        public JObject old_value { get; set; }
        [DefaultValue("{\"user_name\":\"123456\"}")]
        public JObject new_value { get; set; }
        [DefaultValue("{\"user_name\":\"user_name\",\"display_name\":\"user_pro_vip\",\"avatar\":null}")]
        public JObject admin { get; set; }
        public DateTime timestamp { get; set; }
    }
    #endregion
    #endregion

    #region Controller result example
    #region Admin
    public class AdminGetConfigsSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        [DefaultValue("OK")]
        public string message { get; set; }
        public AdminGetConfigsData data { get; set; }
        public class AdminGetConfigsData {
            [DefaultValue("[{\"key\":\"value\"},{\"key_1\":\"value_1\"}]")]
            public List<JObject> configs { get; set; }
        }
    }
    public class AdminGetConfigSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        [DefaultValue("OK")]
        public string message { get; set; }
        public AdminGetConfigData data { get; set; }
        public class AdminGetConfigData {
            [DefaultValue("{\"key\":\"value\"}")]
            public JObject config { get; set; }
        }
    }
    public class GetAdminAuditLogSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        [DefaultValue("OK")]
        public string message { get; set; }
        public GetAdminAuditLogData data { get; set; }
        public class GetAdminAuditLogData {
            public AuditLogExample logs { get; set; }
            [DefaultValue(100)]
            public int total_count { get; set; }
        }
    }
    public class AdminUserLogoutSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        [DefaultValue("Success.")]
        public string message { get; set; }
    }
    public class AdminUserLoginSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        [DefaultValue("knrgrmao0yuyzy1r8oqs2j8r5otxyn")]
        public string session_id { get; set; }
        public Guid user_id { get; set; }
    }
    public class GetUserBySessionAdminSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        public AdminUserExample user { get; set; }
    }
    public class GetAdminUserByIdSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        public AdminUserExample user { get; set; }
    }
    public class ExtensionSessionAdminUserSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        [DefaultValue("Success.")]
        public string message { get; set; }
    }
    public class DeleteSessionAdminUserSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        [DefaultValue("Success.")]
        public string message { get; set; }
    }
    public class GetAllSessionAdminUserSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        public List<SessionAdminUserExample> sessions { get; set; }
    }
    public class GetSessionAdminUserSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        public SessionAdminUserExample session { get; set; }
    }
    public class CreateAdminUserSuccessExample
    {
        [DefaultValue(201)]
        public int status { get; set; }
        [DefaultValue("Success.")]
        public string message { get; set; }
        public Guid user_id { get; set; }
    }
    #endregion
    #region Social
    public class GetSocialUserAuditLogSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        [DefaultValue("OK")]
        public string message { get; set; }
        public GetSocialUserAuditLogData data { get; set; }
        public class GetSocialUserAuditLogData {
            public UserAuditLogExample logs { get; set; }
            [DefaultValue(100)]
            public int total_count { get; set; }
        }
    }
    public class SocialUserSignupSuccessExample
    {
        [DefaultValue(201)]
        public int status { get; set; }
        [DefaultValue("Success.")]
        public string message { get; set; }
        public Guid user_id { get; set; }
    }
    public class SocialUserLogoutSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        [DefaultValue("Success.")]
        public string message { get; set; }
    }
    public class SocialUserLoginSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        [DefaultValue("knrgrmao0yuyzy1r8oqs2j8r5otxyn")]
        public string session_id { get; set; }
        public Guid user_id { get; set; }
    }
    public class GetUserBySessionSocialSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        public SocialUserExample user { get; set; }
    }
    public class DeleteSessionSocialUserSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        [DefaultValue("Success.")]
        public string message { get; set; }
    }
    public class ExtensionSessionSocialUserSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        [DefaultValue("Success.")]
        public string message { get; set; }
    }
    public class GetAllSessionSocialUserSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        public List<SessionSocialUserExample> sessions { get; set; }
    }

    public class GetSessionSocialUserSuccessExample
    {
        [DefaultValue(200)]
        public int status { get; set; }
        public SessionSocialUserExample session { get; set; }
    }
    #endregion
    #endregion

    #region Examples error response
    public class StatusCode500Examples
    {
        [DefaultValue(500)]
        public int status { get; set; }
        [DefaultValue("Internal Server message.")]
        public string message { get; set; }
    }
    public class StatusCode400Examples
    {
        [DefaultValue(400)]
        public int status { get; set; }
        [DefaultValue("Bad request.")]
        public string message { get; set; }
    }
    public class StatusCode401Examples
    {
        [DefaultValue(401)]
        public int status { get; set; }
        [DefaultValue("Session has expired.")]
        public string message { get; set; }
    }
    public class StatusCode403Examples
    {
        [DefaultValue(403)]
        public int status { get; set; }
        [DefaultValue("Missing header authorization.")]
        public string message { get; set; }
    }
    public class StatusCode404Examples
    {
        [DefaultValue(404)]
        public int status { get; set; }
        [DefaultValue("Not found.")]
        public string message { get; set; }
    }
    public class StatusCode410Examples
    {
        [DefaultValue(410)]
        public int status { get; set; }
        [DefaultValue("Link has expired.")]
        public string message { get; set; }
    }
    public class StatusCode423Examples
    {
        [DefaultValue(423)]
        public int status { get; set; }
        [DefaultValue("You have been locked.")]
        public string message { get; set; }
    }
    #endregion
}