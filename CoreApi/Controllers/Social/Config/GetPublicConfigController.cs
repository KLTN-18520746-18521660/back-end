using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.Config
{
    [ApiController]
    [Route("/api/config")]
    public class GetPublicConfigController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public GetPublicConfigController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "GetPublicConfig";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = string.Empty;
            try {
                (EXTENSION_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXTENSION_TIME);
                (EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SESSION_SOCIAL_USER_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
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

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetAllPublicConfig([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                            [FromHeader] string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                bool isSessionInvalid = true;
                #region Get session token
                session_token = session_token != default ? session_token : GetValueFromCookie(SessionTokenHeaderKey);
                if (session_token == default) {
                    LogDebug($"Missing header authorization.");
                    isSessionInvalid = false;
                }

                if (isSessionInvalid && !CommonValidate.IsValidSessionToken(session_token)) {
                    LogDebug("Invalid header authorization.");
                }
                #endregion

                #region Find session for use
                SessionSocialUser session = default;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                if (isSessionInvalid) {
                    (session, error) = await __SessionSocialUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                    if (error != ErrorCodes.NO_ERROR) {
                        isSessionInvalid = false;
                    }
                }
                #endregion

                #region Get all public config
                var (ret, errMsg) = __BaseConfig.GetAllPublicConfig();
                if (errMsg != string.Empty) {
                    throw new Exception($"GetAllConfig Failed. ErrorMsg: { errMsg }");
                }
                #endregion

                LogInformation($"Get all config success.");
                return Ok(200, "OK", new JObject(){
                    { "configs", ret },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }

        [HttpGet("{config_key}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetPublicConfigByConfigKey([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                                    [FromHeader] string session_token,
                                                                    [FromRoute] string config_key)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate config_key
                if (config_key == default || config_key == string.Empty || DefaultBaseConfig.StringToConfigKey(config_key) == CONFIG_KEY.INVALID) {
                    LogDebug($"Invalid config key.");
                    return Problem(400, "Invalid config_key.");
                }
                #endregion

                bool isSessionInvalid = false;
                #region Get session token
                session_token = session_token != default ? session_token : GetValueFromCookie(SessionTokenHeaderKey);
                if (session_token == default) {
                    LogDebug($"Missing header authorization.");
                }

                if (!CommonValidate.IsValidSessionToken(session_token)) {
                    LogDebug("Invalid header authorization.");
                }
                #endregion

                #region Find session for use
                SessionSocialUser session = default;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                if (isSessionInvalid) {
                    (session, error) = await __SessionSocialUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                    if (error != ErrorCodes.NO_ERROR) {
                        isSessionInvalid = false;
                    }
                }
                #endregion

                #region Get public config by key
                if (DefaultBaseConfig.StringToConfigKey(config_key) == CONFIG_KEY.INVALID
                    || __BaseConfig.IsPublicConfig(DefaultBaseConfig.StringToConfigKey(config_key))) {
                    return Problem(400, "Invalid config_key.");
                }
                var (ret, errMsg) = __BaseConfig.GetPublicConfig(DefaultBaseConfig.StringToConfigKey(config_key));
                if (errMsg != string.Empty) {
                    LogWarning($"GetPublicConfig failed, ErrorMessage: { errMsg }");
                    return Problem(400, "Invalid config_key or config_key not exist.");
                }
                #endregion

                LogInformation($"Get public config by key success.");
                return Ok(200, "OK", new JObject(){
                    { "config", ret },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
