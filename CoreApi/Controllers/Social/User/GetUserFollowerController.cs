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
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.User
{
    [ApiController]
    [Route("/api/user")]
    public class GetUserFollowerController : BaseController
    {
        public GetUserFollowerController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpGet("follower")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetUserFollower([FromServices] SessionSocialUserManagement     __SessionSocialUserManagement,
                                                         [FromServices] SocialUserManagement            __SocialUserManagement,
                                                         [FromHeader(Name = "session_token")] string    SessionToken,
                                                         [FromQuery(Name = "start")] int                Start   = 0,
                                                         [FromQuery(Name = "size")] int                 Size    = 20)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SessionSocialUserManagement, __SocialUserManagement);
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
                AddLogParam("start", Start);
                AddLogParam("size", Size);
                if (Start < 0 || Size < 1) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                #endregion

                var (Users, TotalSize, Error) = await __SocialUserManagement.GetFollowerByName(Session.User.UserName);
                if (Error != ErrorCodes.NO_ERROR) {
                    return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "user" });
                }

                #region Validate params: start, size, total_size
                if (TotalSize != 0 && Start >= TotalSize) {
                    AddLogParam("total_size", TotalSize);
                    return Problem(400, RESPONSE_MESSAGES.INVALID_REQUEST_PARAMS_START_SIZE, new string[]{ Start.ToString(), TotalSize.ToString() });
                }
                #endregion

                List<JObject> RawRet = new List<JObject>();
                Users.ForEach(e => {
                    var r = e.GetPublicJsonObject();
                    r.Add("actions", Utils.ObjectToJsonToken(e.GetActionByUser(Session.UserId)));
                    RawRet.Add(r);
                });
                var Ret = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(RawRet));
                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "users",      Ret },
                    { "total_size", TotalSize },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
