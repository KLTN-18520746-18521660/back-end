using Common;
using CoreApi.Common.Base;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using CoreApi.Models;

namespace CoreApi.Controllers.Social
{
    [ApiController]
    [Route("/api/goto")]
    public class CountRedirectUrlController : BaseController
    {
        public CountRedirectUrlController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SocialUserLoginSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(StatusCode423Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> CountRedirectUrl([FromServices] RedirectUrlManagement          __RedirectUrlManagement,
                                                          [FromServices] SessionSocialUserManagement    __SessionSocialUserManagement,
                                                          [FromBody] CountRedirectUrlModel              __ModelData,
                                                          [FromHeader(Name = "session_token")] string   SessionToken)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__RedirectUrlManagement, __SessionSocialUserManagement);
            #endregion
            try {
                #region Get session (not required)
                SessionToken            = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, _)      = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                var IsValidSession      = __Session != default;
                var Session             = __Session as SessionSocialUser;
                #endregion

                var error = await __RedirectUrlManagement.IncreaseTimesGoToUrl(__ModelData.url);
                if (error != ErrorCodes.NO_ERROR) {
                    AddLogParam("url", Url);
                    throw new Exception($"IncreaseTimesGoToUrl faied, ErrorCode: { error }");
                }

                return Ok(200, RESPONSE_MESSAGES.OK, default);
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
