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
    public class ModifyConfigKeyController : BaseController
    {
        public ModifyConfigKeyController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        [NonAction]
        private string ValidateJsonObject(JObject Input, JObject Format)
        {
            if (Format.ContainsKey("any") && (string)Format["any"] == "any") {
                return string.Empty;
            }

            if (Input.Count != Format.Count) {
                return "Missing field";
            }
            foreach (var It in Format) {
                if (!Input.ContainsKey(It.Key)) {
                    return "Missing field";
                }
                var Type = (string) It.Value["type"];
                if (Type == (string) JTokenType.String.ToString().ToLower() || Type == "text") {
                    var Val = (string) Input[It.Key];
                    if (Val == default || Val == string.Empty) {
                        return "Invalid string field";
                    }
                } else if (Type == (string) JTokenType.Integer.ToString().ToLower()) {
                    var Val = (int?) Input[It.Key];
                    if (Val == default) {
                        return "Invalid integer field";
                    }
                    if (It.Value["min"] != default && Val < (int)It.Value["min"]) {
                        return "Exceed minium integer.";
                    }
                } else if (Type == Input[It.Key].Type.ToString().ToLower()) {
                    var Val = Input[It.Key];
                    if (Val == default) {
                        return "Invalid field is null";
                    }
                } else {
                    return "Invalid type";
                }
            }
            return string.Empty;
        }

        [HttpPut("{config_key}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminGetConfigSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(StatusCode401Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ModifyConfigKey([FromServices] AdminUserManagement            __AdminUserManagement,
                                                         [FromServices] SessionAdminUserManagement     __SessionAdminUserManagement,
                                                         [FromRoute(Name = "config_key")] string       __ConfigKey,
                                                         [FromBody] JObject                            __ModelData,
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
                    return Problem(400, RESPONSE_MESSAGES.INVALID_CONFIG_KEY, new string[]{ __ConfigKey });
                }
                if (__ModelData == default) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_BODY);
                }
                #endregion

                #region Check Permission
                var Error = __AdminUserManagement.HaveFullPermission(Session.User.Rights, ADMIN_RIGHTS.CONFIG);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    return Problem(404, RESPONSE_MESSAGES.NOT_ALLOW_TO_DO, new string[]{ "modify config key" });
                }
                #endregion

                #region Get config by key
                var ConfigKey = DEFAULT_BASE_CONFIG.StringToConfigKey(__ConfigKey);
                AddLogParam("config_key", ConfigKey.ToString());
                if (ConfigKey == CONFIG_KEY.INVALID) {
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "config_key" });
                }

                var ErrMsg = string.Empty;
                var Format = DEFAULT_BASE_CONFIG.GetValueFormatOfConfigKey(ConfigKey, ErrMsg);
                if (ErrMsg != string.Empty) {
                    throw new Exception($"GetValueFormatOfConfigKey failed. ErrorMsg: { ErrMsg }");
                }
                #endregion

                ErrMsg = ValidateJsonObject(__ModelData, Format);
                if (ErrMsg != string.Empty) {
                    AddLogParam("err_validate", ErrMsg);
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_BODY);
                }

                Error = await __BaseConfig.UpdateConfig(ConfigKey, __ModelData, Session.UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NO_CHANGE_DETECTED) {
                        return Problem(400, RESPONSE_MESSAGES.NO_CHANGES_DETECTED);
                    }
                    throw new Exception($"UpdateConfig failed, ErrorCode: { Error }");
                }

                #region Reload all config
                var Errors = new string[]{};
                (Error, Errors) = await __BaseConfig.ReLoadConfig();
                if (Error != ErrorCodes.NO_ERROR) {
                    var ErrResp = new JObject(){
                       { "errors", JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(Errors)) }
                    };
                    return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, ErrResp, LOG_LEVEL.ERROR);
                }
                #endregion

                return Ok(200, RESPONSE_MESSAGES.OK, default);
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
