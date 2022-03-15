using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Common;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.AuditLog
{
    [ApiController]
    [Route("/auditlog")]
    public class GetSocialAuditLogController : BaseController
    {
        #region Services
        private BaseConfig __BaseConfig;
        private SocialUserManagement __SocialUserManagement;
        private SocialAuditLogManagement __SocialAuditLogManagement;
        private SessionSocialUserManagement __SessionSocialUserManagement;
        #endregion

        #region Config Value
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minutes
        #endregion

        public GetSocialAuditLogController(
            BaseConfig _BaseConfig,
            SocialUserManagement _SocialUserManagement,
            SocialAuditLogManagement _SocialAuditLogManagement,
            SessionSocialUserManagement _SessionSocialUserManagement
        ) : base() {
            __BaseConfig = _BaseConfig;
            __SocialUserManagement = _SocialUserManagement;
            __SocialAuditLogManagement = _SocialAuditLogManagement;
            __SessionSocialUserManagement = _SessionSocialUserManagement;
            __ControllerName = "GetSocialAuditLog";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                (EXTENSION_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
                (EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                __LoadConfigSuccess = true;
            } catch (Exception e) {
                __LoadConfigSuccess = false;
                StringBuilder msg = new StringBuilder(e.ToString());
                if (Error != e.Message && Error != "") {
                    msg.Append($" && Error: { Error }");
                }
                LogError($"Load config value failed, message: { msg }");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs(int start = 0, int size = 20, string search_term = null)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            try {
                #region Get session token
                string sessionToken = "";
                if (!GetHeader(HEADER_KEYS.API_KEY, out sessionToken)) {
                    LogDebug($"Missing header authorization.");
                    return Problem(403, "Missing header authorization.");
                }

                if (!Utils.IsValidSessionToken(sessionToken)) {
                    return Problem(403, "Invalid header authorization.");
                }
                #endregion

                #region Find session for use
                SessionSocialUser session = null;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (session, error) = await __SessionSocialUserManagement.FindSessionForUse(sessionToken, EXPIRY_TIME, EXTENSION_TIME);
                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Session not found, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(400, "Session not found.");
                    }
                    if (error == ErrorCodes.SESSION_HAS_EXPIRED) {
                        LogInformation($"Session has expired, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(401, "Session has expired.");
                    }
                    if (error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        LogInformation($"User has been locked, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(423, "You have been locked.");
                    }
                    throw new Exception($"FindSessionForUse Failed. ErrorCode: { error }");
                }
                #endregion

                #region Get all audit logs
                var user = session.User;
                List<SocialAuditLog> logs = null;
                int totalSize = 0;
                (logs, totalSize)= await __SocialAuditLogManagement.GetAllAuditLog(user.Id, start, size, search_term);

                List<JObject> rawReturn = new();
                logs.ForEach(e => rawReturn.Add(e.GetJsonObject()));
                var ret = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(rawReturn));
                #endregion

                #region Validate params: start, size, total_size
                if (start >= totalSize) {
                    LogInformation($"Invalid request params for get audit log, start: { start }, size: { size }, search_term: { search_term }, total_size: { totalSize }");
                    return Problem(400, $"Invalid request params start: { start }. Total size is { totalSize }");
                }
                #endregion

                LogDebug($"Get all auditlog success, user_name: { user.UserName }, start: { start }, size: { size }, search_term: { search_term }");
                return Ok( new JObject(){
                    { "status", 200 },
                    { "logs", ret },
                    { "total_size", totalSize },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
