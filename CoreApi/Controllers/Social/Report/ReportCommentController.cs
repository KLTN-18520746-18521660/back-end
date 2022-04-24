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
    [Route("/api/report")]
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

        [HttpPost("{action}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> Report([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                [FromServices] SocialCommentManagement __SocialCommentManagement,
                                                [FromServices] SocialUserManagement __SocialUserManagement,
                                                [FromServices] SocialPostManagement __SocialPostManagement,
                                                [FromServices] SocialReportManagement __SocialReportManagement,
                                                [FromHeader] string session_token,
                                                [FromRoute] string action,
                                                [FromBody] ParserSocialReport Parser)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get session token
                if (session_token == default) {
                    LogDebug($"Missing header authorization.");
                    return Problem(401, "Missing header authorization.");
                }

                if (!CommonValidate.IsValidSessionToken(session_token)) {
                    return Problem(401, "Invalid header authorization.");
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

                if (Parser.report_type.ToLower() == "other" && Parser.content == default) {
                    return Problem(400, "Not accept empty content when type is 'other'");
                }

                SocialReport report = new SocialReport();
                report.Type = action;
                report.ReportType = Parser.report_type;
                report.Content = Parser.content;
                report.ReporterId = session.UserId;
                switch (action) {
                    case "user":
                        {
                            if (Parser.user_name == default) {
                                return Problem(400, "Invaid request body.");
                            }
                            SocialUser reportUser = default;
                            (reportUser, error) = await __SocialUserManagement.FindUserIgnoreStatus(Parser.user_name, false);
                            if (error != ErrorCodes.NO_ERROR) {
                                return Problem(404, "Not found report user.");
                            }
                            if (session.UserId != reportUser.Id) {
                                return Problem(400, "Not allow.");
                            }
                            report.UserId = reportUser.Id;
                            break;
                        }
                    case "post":
                        {
                            if (Parser.post_slug == default) {
                                return Problem(400, "Invaid request body.");
                            }
                            SocialPost reportPost = default;
                            (reportPost, error) = await __SocialPostManagement.FindPostBySlug(Parser.post_slug);
                            if (error != ErrorCodes.NO_ERROR) {
                                return Problem(404, "Not found report post.");
                            }
                            if (session.UserId != reportPost.Owner) {
                                return Problem(400, "Not allow.");
                            }
                            report.PostId = reportPost.Id;
                            break;
                        }
                    case "comment":
                        {
                            if (Parser.comment_id == default) {
                                return Problem(400, "Invaid request body.");
                            }
                            SocialComment reportComment = default;
                            (reportComment, error) = await __SocialCommentManagement.FindCommentById(Parser.comment_id);
                            if (error != ErrorCodes.NO_ERROR) {
                                return Problem(404, "Not found report comment.");
                            }
                            if (session.UserId != reportComment.Owner) {
                                return Problem(400, "Not allow.");
                            }
                            report.CommentId = reportComment.Id;
                            break;
                        }
                    case "feedback":
                        {
                            if (Parser.content == default) {
                                return Problem(400, "Invaid request body.");
                            }
                            break;
                        }
                    default:
                        return Problem(400, "Invalid request.");
                }

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
