using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CoreApi.Common
{
    #region Model Example
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
    #endregion

    #region Controller result example
    #region Admin
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
        [DefaultValue("Internal Server error.")]
        public string error { get; set; }
    }
    public class StatusCode400Examples
    {
        [DefaultValue(400)]
        public int status { get; set; }
        [DefaultValue("Bad request.")]
        public string error { get; set; }
    }
    public class StatusCode401Examples
    {
        [DefaultValue(401)]
        public int status { get; set; }
        [DefaultValue("Session has expired.")]
        public string error { get; set; }
    }
    public class StatusCode403Examples
    {
        [DefaultValue(403)]
        public int status { get; set; }
        [DefaultValue("Missing header authorization.")]
        public string error { get; set; }
    }
    public class StatusCode404Examples
    {
        [DefaultValue(404)]
        public int status { get; set; }
        [DefaultValue("Not found.")]
        public string error { get; set; }
    }
    public class StatusCode410Examples
    {
        [DefaultValue(410)]
        public int status { get; set; }
        [DefaultValue("Link has expired.")]
        public string error { get; set; }
    }
    public class StatusCode423Examples
    {
        [DefaultValue(423)]
        public int status { get; set; }
        [DefaultValue("You have been locked.")]
        public string error { get; set; }
    }
    #endregion
}