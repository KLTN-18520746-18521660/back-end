using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.User
{
    [ApiController]
    [Route("/api/user")]
    public class GetFollowingByNameController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public GetFollowingByNameController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "GetFollowingByName";
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

        [HttpGet("following")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetFollowingByName([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                           [FromServices] SocialUserManagement __SocialUserManagement,
                                                           [FromHeader] string session_token,
                                                           [FromQuery] int start = 0,
                                                           [FromQuery] int size = 20)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session
                session_token = session_token != default ? session_token : GetValueFromCookie(SessionTokenHeaderKey);
                var (__session, errRet) = await GetSessionToken(__SessionSocialUserManagement, EXPIRY_TIME, EXTENSION_TIME, session_token);
                if (errRet != default) {
                    return errRet;
                }
                if (__session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var session = __session as SessionSocialUser;
                #endregion

                List<SocialUser> users      = default;
                int totalSize               = default;
                var error                   = ErrorCodes.NO_ERROR;
                (users, totalSize, error) = await __SocialUserManagement.GetFollowingByName(session.User.UserName);
                if (error != ErrorCodes.NO_ERROR) {
                    return Problem(404, "Not found any user.");
                }

                #region Validate params: start, size, total_size
                if (totalSize != 0 && start >= totalSize) {
                    LogWarning($"Invalid request params for get followes, start: { start }, size: { size }, total_size: { totalSize }");
                    return Problem(400, $"Invalid request params start: { start }. Total size is { totalSize }");
                }
                #endregion

                List<JObject> rawReturn = new();
                users.ForEach(e => {
                    var r = e.GetPublicJsonObject();
                    r.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(session.UserId)));
                    rawReturn.Add(r);
                });
                var ret = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(rawReturn));
                return Ok(200, "OK", new JObject(){
                    { "users", ret },
                    { "total_size", totalSize },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
