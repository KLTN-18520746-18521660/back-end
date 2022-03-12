using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApi.Controllers.Admin.AuditLog
{
    [ApiController]
    [Route("/admin/auditlog")]
    public class GetAdminAuditLogController : BaseController
    {
        #region Services
        private BaseConfig __BaseConfig;
        private AdminUserManagement __AdminUserManagement;
        private AdminAuditLogManagement __AdminAuditLogManagement;
        private SessionAdminUserManagement __SessionAdminUserManagement;
        #endregion

        #region Config Value
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minutes
        #endregion

        public GetAdminAuditLogController(
            BaseConfig _BaseConfig,
            AdminUserManagement _AdminUserManagement,
            AdminAuditLogManagement _AdminAuditLogManagement,
            SessionAdminUserManagement _SessionAdminUserManagement
        ) : base() {
            __BaseConfig = _BaseConfig;
            __AdminUserManagement = _AdminUserManagement;
            __AdminAuditLogManagement = _AdminAuditLogManagement;
            __SessionAdminUserManagement = _SessionAdminUserManagement;
            __ControllerName = "GetAdminAuditLog";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                EXTENSION_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, "extension_time", out Error);
                EXPIRY_TIME = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, "expiry_time", out Error);
                __LoadConfigSuccess = true;
            } catch (Exception e) {
                __LoadConfigSuccess = false;
                StringBuilder msg = new StringBuilder(e.Message);
                if (Error != e.Message && Error != "") {
                    msg.Append($" && Error: { Error }");
                }
                LogError($"Load config value fail, message: { msg }");
            }
        }

        [HttpGet]
        public IActionResult GetAuditLogs(int start = 0, int size = 20, string search_term = null)
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

                if (!CoreApi.Common.Utils.IsValidSessionToken(sessionToken)) {
                    return Problem(403, "Invalid header authorization.");
                }
                #endregion

                #region Find session for use
                SessionAdminUser session = null;
                ErrorCodes error = ErrorCodes.NO_ERROR;

                if (!__SessionAdminUserManagement.FindSessionForUse(sessionToken, EXPIRY_TIME, EXTENSION_TIME, out session, out error)) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Session not found, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(400, "Session not found.");
                    }
                    if (error == ErrorCodes.SESSION_HAS_EXPIRED) {
                        LogInformation($"Session has expired, session_token: { sessionToken.Substring(0, 15) }");
                        return Problem(401, "Session has expired.");
                    }
                    throw new Exception("Internal Server Error. FindSessionForUse Failed.");
                }
                #endregion

                #region Check Permission
                var user = session.User;
                if (!__AdminUserManagement.HaveReadPermission(user, ADMIN_RIGHTS.LOG)) {
                    LogInformation($"User doesn't have permission to see admin audit log, user_name: { user.UserName }");
                    return Problem(403, "User doesn't have permission to see admin audit log.");
                }
                #endregion

                #region Get all audit logs
                int totalSize = 0;
                var logs = __AdminAuditLogManagement.GetAllAuditLog(out totalSize, start, size, search_term);

                List<JObject> rawReturn = new();
                logs.ForEach(e => rawReturn.Add(e.GetJsonObject()));
                var ret = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(rawReturn));
                #endregion

                #region Validate params: start, size, total_size
                if (start + size > totalSize) {
                    LogInformation($"Invalid request params for get audit log, start: { start }, size: { size }, search_term: { search_term }, total_size: { totalSize }");
                    return Problem(400, $"Invalid request params 'start', 'size'. Total size is { totalSize }");
                }
                #endregion

                LogDebug($"Get all auditlog success, user_name: { user.UserName }, start: { start }, size: { size }, search_term: { search_term }");
                return Ok( new JObject(){
                    { "status", 200 },
                    { "logs", ret },
                    { "total_size", totalSize },
                });
            } catch (Exception e) {
                LogError($"Unhandle exception, message: { e.Message }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
