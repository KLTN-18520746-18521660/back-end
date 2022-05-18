using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Admin.Config
{
    [ApiController]
    [Route("/api/admin/config")]
    public class GetConfigController : BaseController
    {
        public GetConfigController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        /// <summary>
        /// Get all server configs
        /// </summary>
        /// <param name="__AdminUserManagement"></param>
        /// <param name="__SessionAdminUserManagement"></param>
        /// <param name="SessionToken"></param>
        /// <returns><b>List admin auditlog</b></returns>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header or cookie 'session_token_admin'.
        /// - User have read permission of 'config' to read all.
        /// 
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> List config.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Invalid params start, size</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="401">
        /// <b>Error case <i>(Server auto send response with will clear cookie 'session_token_admin')</i>, reasons:</b>
        /// <ul>
        /// <li>Session has expired.</li>
        /// <li>Session not found.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="423">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>User have been locked.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminGetConfigsSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetAllConfig([FromServices] AdminUserManagement __AdminUserManagement,
                                                      [FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                      [FromHeader(Name = "session_token_admin")] string SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __AdminUserManagement,
                __SessionAdminUserManagement
            );
            #endregion
            try {
                #region Get session
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, ErrRet) = await GetSessionToken(__SessionAdminUserManagement, SessionToken);
                if (ErrRet != default) {
                    return ErrRet;
                }
                if (__Session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var Session             = __Session as SessionAdminUser;
                #endregion

                #region Check Permission
                var IsHaveReadPermission = true;
                var Error = __AdminUserManagement.HaveReadPermission(Session.User.Rights, ADMIN_RIGHTS.CONFIG);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    IsHaveReadPermission = false;
                }
                AddLogParam("have_read_permission", IsHaveReadPermission);
                #endregion

                #region Get all config
                var (Ret, ErrMsg) = IsHaveReadPermission ? __BaseConfig.GetAllConfig() : __BaseConfig.GetAllPublicConfig();
                if (ErrMsg != string.Empty) {
                    throw new Exception($"GetAllConfig Failed. ErrorMsg: { ErrMsg }");
                }
                #endregion

                return Ok(200, "OK", new JObject(){
                    { "configs", Ret },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }

        /// <summary>
        /// Get config by config_key
        /// </summary>
        /// <param name="__AdminUserManagement"></param>
        /// <param name="__SessionAdminUserManagement"></param>
        /// <param name="__ConfigKey"></param>
        /// <param name="SessionToken"></param>
        /// <returns><b>config value</b></returns>
        ///
        /// <remarks>
        /// <b>Using endpoint need:</b>
        /// 
        /// - Need header or cookie 'session_token_admin'.
        /// - User have read permission of 'config'.
        /// 
        /// </remarks>
        ///
        /// <response code="200">
        /// <b>Success Case:</b> config value.
        /// </response>
        /// 
        /// <response code="400">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>Invalid params start, size</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="401">
        /// <b>Error case <i>(Server auto send response with will clear cookie 'session_token_admin')</i>, reasons:</b>
        /// <ul>
        /// <li>Session has expired.</li>
        /// <li>Session not found.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="404">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>User doesn't have permission to read non pubic configkey.</li>
        /// <li>config_key not in valid config_keys</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="423">
        /// <b>Error case, reasons:</b>
        /// <ul>
        /// <li>User have been locked.</li>
        /// </ul>
        /// </response>
        /// 
        /// <response code="500">
        /// <b>Unexpected case, reason:</b> Internal Server Error.<br/><i>See server log for detail.</i>
        /// </response>
        [HttpGet("{config_key}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminGetConfigSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetConfigByConfigKey([FromServices] AdminUserManagement            __AdminUserManagement,
                                                              [FromServices] SessionAdminUserManagement     __SessionAdminUserManagement,
                                                              [FromRoute(Name = "config_key")] string       __ConfigKey,
                                                              [FromHeader(Name = "session_token")] string   SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__AdminUserManagement, __SessionAdminUserManagement);
            #endregion
            try {
                #region Get session
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, ErrRet) = await GetSessionToken(__SessionAdminUserManagement, SessionToken);
                if (ErrRet != default) {
                    return ErrRet;
                }
                if (__Session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var Session             = __Session as SessionAdminUser;
                #endregion

                #region Validate params
                AddLogParam("raw_config_key", __ConfigKey);
                if (__ConfigKey == default || __ConfigKey == string.Empty) {
                    return Problem(400, "Invalid config_key.");
                }
                #endregion

                #region Check Permission
                var IsHaveReadPermission = true;
                var Error = __AdminUserManagement.HaveReadPermission(Session.User.Rights, ADMIN_RIGHTS.CONFIG);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    IsHaveReadPermission = false;
                }
                AddLogParam("have_read_permission", IsHaveReadPermission);
                #endregion

                #region Get config by key
                var ConfigKey = DEFAULT_BASE_CONFIG.StringToConfigKey(__ConfigKey);
                AddLogParam("config_key", ConfigKey);
                if (ConfigKey == CONFIG_KEY.INVALID) {
                    return Problem(404, "Not found config_key.");
                }
                if (!IsHaveReadPermission && !__BaseConfig.IsPublicConfig(ConfigKey)) {
                    return Problem(404, "Not found config_key.");
                }

                var (Ret, ErrMsg) = __BaseConfig.GetConfigValue(ConfigKey);
                if (ErrMsg != string.Empty) {
                    throw new Exception($"GetConfigValue Failed. ErrorMsg: { ErrMsg }");
                }
                #endregion

                return Ok(200, "OK", new JObject(){
                    { "config", Ret },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
