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
    public class GetUserFollowingController : BaseController
    {
        public GetUserFollowingController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "GetUserFollowing";
        }

        [HttpGet("following")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetUserFollowing([FromServices] SessionSocialUserManagement    __SessionSocialUserManagement,
                                                          [FromServices] SocialUserManagement           __SocialUserManagement,
                                                          [FromHeader(Name = "session_token")] string   SessionToken,
                                                          [FromQuery(Name = "start")] int               Start   = 0,
                                                          [FromQuery(Name = "size")] int                Size    = 20)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, ErrRet) = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                if (ErrRet != default) {
                    return ErrRet;
                }
                if (__Session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var Session             = __Session as SessionSocialUser;
                #endregion

                #region Validate params
                if (Start < 0 || Size < 1) {
                    LogDebug(
                        "Invalid request params for get followes, "
                        + $"start: { Start }, size: { Size }"
                    );
                    return Problem(400, "Bad request params.");
                }
                #endregion

                var (Users, TotalSize, Error) = await __SocialUserManagement.GetFollowingByName(Session.User.UserName);
                if (Error != ErrorCodes.NO_ERROR) {
                    LogWarning($"Not found any user, user_name: { Session.User.UserName }");
                    return Problem(404, "Not found any user.");
                }

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    LogWarning(
                        $"Invalid request params for get following, "
                        + $"start: { Start }, size: { Size }, total_size: { TotalSize }"
                    );
                    return Problem(400, $"Invalid request params start: { Start }. Total size is { TotalSize }");
                }
                #endregion

                List<JObject> RawRet = new List<JObject>();
                Users.ForEach(e => {
                    var r = e.GetPublicJsonObject();
                    r.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                    RawRet.Add(r);
                });
                var Ret = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(RawRet));
                return Ok(200, "OK", new JObject(){
                    { "users",      Ret },
                    { "total_size", TotalSize },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }
    }
}
