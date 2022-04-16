using Common;
using CoreApi.Common;
using CoreApi.Models.ModifyModels;
using CoreApi.Services;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Context.ParserModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.Report
{
    [ApiController]
    [Route("/report")]
    public class ReportCommentController : BaseController
    {
        #region Config Values
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

        public ReportCommentController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "ReportComment";
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

        [HttpPost("comment")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> ReportComment([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                       [FromServices] SocialCommentManagement __SocialCommentManagement,
                                                       [FromServices] SocialUserManagement __SocialUserManagement,
                                                       [FromServices] SocialPostManagement __SocialPostManagement,
                                                       [FromServices] SocialReportManagement __SocialReportManagement,
                                                       [FromHeader] string session_token,
                                                       [FromBody] ParserSocialReport Parser)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate parser
                if (Parser.user_name == default || Parser.post_slug == default || Parser.comment_id == default) {
                    return Problem(400, "Bad body request.");
                }
                #endregion

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
                SessionSocialUser session = default;
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (session, error) = await __SessionSocialUserManagement.FindSessionForUse(session_token, EXPIRY_TIME, EXTENSION_TIME);

                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.NOT_FOUND) {
                        LogDebug($"Session not found, session_token: { session_token.Substring(0, 15) }");
                        return Problem(401, "Session not found.");
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

                #region Get report user
                SocialUser reportUser = default;
                (reportUser, error) = await __SocialUserManagement.FindUser(Parser.user_name, false);
                if (error != ErrorCodes.NO_ERROR) {
                    return Problem(404, "Not found report user.");
                }
                #endregion

                #region Get post info
                SocialPost reportPost = default;
                (reportPost, error) = await __SocialPostManagement.FindPostBySlug(Parser.post_slug);
                if (error != ErrorCodes.NO_ERROR || reportPost.Owner != reportUser.Id) {
                    return Problem(404, "Not found report post.");
                }
                #endregion

                #region Get comment info
                SocialComment reportComment = default;
                (reportComment, error) = await __SocialCommentManagement.FindCommentById(Parser.comment_id);
                if (error != ErrorCodes.NO_ERROR
                    || reportComment.PostId != reportPost.Id
                    || reportComment.Status == SocialCommentStatus.Deleted
                ) {
                    return Problem(404, "Not found report comment.");
                }
                #endregion

                var report = new SocialReport();
                report.Parse(Parser, out var errMsg);
                if (errMsg != string.Empty) {
                    throw new Exception($"Parse social report model failed, error: { errMsg }");
                }
                report.UserId = reportUser.Id;
                report.PostId = reportPost.Id;
                report.CommentId = reportComment.Id;

                error = await __SocialReportManagement.AddNewReport(report);
                if (error != ErrorCodes.NO_ERROR) {
                    throw new Exception($"AddNewReport failed, ErrorCode: { error }");
                }

                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
