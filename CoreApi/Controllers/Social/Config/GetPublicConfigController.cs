using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.Config
{
    [ApiController]
    [Route("/api/config")]
    public class GetPublicConfigController : BaseController
    {
        public GetPublicConfigController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "GetPublicConfig";
        }

        private string[] NotAllowConfigKeyContains = new string[]{
            "admin",
            "api"
        };

        private bool IsNotAllowConfigKey(string Key)
        {
            string KeyLower = Key.ToLower();
            return NotAllowConfigKeyContains.Count(e => KeyLower.Contains(e)) > 0;
        }

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetAllPublicConfig([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                            [FromHeader(Name = "session_token")] string SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session (not required)
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, _)      = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                var IsValidSession      = __Session != default;
                var Session             = __Session as SessionSocialUser;
                #endregion

                #region Get all public config
                var (RawRet, ErrMsg) = __BaseConfig.GetAllPublicConfig();
                if (ErrMsg != string.Empty) {
                    throw new Exception($"GetAllConfig Failed. ErrorMsg: { ErrMsg }");
                }
                JObject Ret = new JObject();
                foreach (var it in RawRet) {
                    if (IsNotAllowConfigKey(it.Key)) {
                        continue;
                    }
                    Ret.Add(it.Key, it.Value);
                }
                #endregion

                LogInformation($"Get all config success.");
                return Ok(200, "OK", new JObject(){
                    { "configs", Ret },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }

        [HttpGet("{config_key}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(StatusCode403Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetPublicConfigByConfigKey([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                                    [FromRoute(Name = "config_key")] string     __ConfigKey,
                                                                    [FromHeader(Name = "session_token")] string SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session (not required)
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, _)      = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                var IsValidSession      = __Session != default;
                var Session             = __Session as SessionSocialUser;
                #endregion

                #region Validate config_key
                if (__ConfigKey == default
                    || __ConfigKey == string.Empty
                    || DEFAULT_BASE_CONFIG.StringToConfigKey(__ConfigKey) == CONFIG_KEY.INVALID
                    || IsNotAllowConfigKey(__ConfigKey) == true
                ) {
                    LogDebug($"Invalid config key.");
                    return Problem(400, "Invalid config_key.");
                }
                #endregion

                #region Get public config by key
                if (DEFAULT_BASE_CONFIG.StringToConfigKey(__ConfigKey) == CONFIG_KEY.INVALID
                    || __BaseConfig.IsPublicConfig(DEFAULT_BASE_CONFIG.StringToConfigKey(__ConfigKey))) {
                    return Problem(400, "Invalid config_key.");
                }
                var (Ret, ErrMsg) = __BaseConfig.GetPublicConfig(DEFAULT_BASE_CONFIG.StringToConfigKey(__ConfigKey));
                if (ErrMsg != string.Empty) {
                    LogWarning($"GetPublicConfig failed, ErrorMessage: { ErrMsg }");
                    return Problem(400, "Invalid config_key or config_key not exist.");
                }
                #endregion

                LogInformation($"Get public config by key success.");
                return Ok(200, "OK", new JObject(){
                    { "config", Ret },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }
    }
}
