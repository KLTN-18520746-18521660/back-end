using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Admin.AuditLog
{
    [ApiController]
    [Route("/admin/sociallog")]
    public class GetSocialAuditLogController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minutes
        #endregion

        public GetSocialAuditLogController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "GetSocialAuditLog";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = string.Empty;
            try {
                (EXTENSION_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
                (EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_ADMIN_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                __LoadConfigSuccess = true;
            } catch (Exception e) {
                __LoadConfigSuccess = false;
                StringBuilder msg = new StringBuilder(e.ToString());
                if (Error != e.Message && Error != string.Empty) {
                    msg.Append($" && Error: { Error }");
                }
                LogError($"Load config value failed, message: { msg }");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromServices] AdminUserManagement __AdminUserManagement,
                                                      [FromServices] SocialAuditLogManagement __SocialAuditLogManagement,
                                                      [FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                      [FromHeader] string session_token,
                                                      [FromQuery] int start = 0,
                                                      [FromQuery] int size = 20,
                                                      [FromQuery] string search_term = default)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __AdminUserManagement.SetTraceId(TraceId);
            __SocialAuditLogManagement.SetTraceId(TraceId);
            __SessionAdminUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session token
                if (session_token == default) {
                    LogDebug($"Missing header authorization.");
                    return Problem(403, "Missing header authorization.");
                }

                if (!CommonValidate.IsValidSessionToken(session_token)) {
                    return Problem(403, "Invalid header authorization.");
                }
                #endregion

                #region Find session for use
                SessionAdminUser session = default;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (session, error) = await __SessionAdminUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Session not found, session_token: { session_token.Substring(0, 15) }");
                        return Problem(400, "Session not found.");
                    }
                    if (error == ErrorCodes.SESSION_HAS_EXPIRED) {
                        LogInformation($"Session has expired, session_token: { session_token.Substring(0, 15) }");
                        return Problem(401, "Session has expired.");
                    }
                    if (error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        LogWarning($"User has been locked, session_token: { session_token.Substring(0, 15) }");
                        return Problem(423, "You have been locked.");
                    }
                    throw new Exception($"FindSessionForUse Failed. ErrorCode: { error }");
                }
                #endregion

                #region Check Permission
                var user = session.User;
                error = __AdminUserManagement.HaveReadPermission(user.Rights, ADMIN_RIGHTS.LOG);
                if (error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    LogInformation($"User doesn't have permission to see admin audit log, user_name: { user.UserName }");
                    return Problem(403, "User doesn't have permission to see admin audit log.");
                }
                #endregion

                #region Get all audit logs
                var (logs, totalSize) = await __SocialAuditLogManagement.GetAllAuditLog(start, size, search_term);

                List<JObject> rawReturn = new();
                logs.ForEach(e => rawReturn.Add(e.GetJsonObject()));
                var ret = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(rawReturn));
                #endregion

                #region Validate params: start, size, total_size
                if (totalSize != 0 && start >= totalSize) {
                    LogInformation($"Invalid request params for get audit log, start: { start }, size: { size }, search_term: { search_term }, total_size: { totalSize }");
                    return Problem(400, $"Invalid request params start: { start }. Total size is { totalSize }");
                }
                #endregion

                LogDebug($"Get all auditlog success, user_name: { user.UserName }, start: { start }, size: { size }, search_term: { search_term }");
                return Ok(200, "OK", new JObject(){
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
