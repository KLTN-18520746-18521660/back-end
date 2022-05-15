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
    public class ReportController : BaseController
    {
        public ReportController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "Report";
        }

        [HttpPost("{type}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserBySessionSocialSuccessExample))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(StatusCode404Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> Report([FromServices] SessionSocialUserManagement  __SessionSocialUserManagement,
                                                [FromServices] SocialCommentManagement      __SocialCommentManagement,
                                                [FromServices] SocialUserManagement         __SocialUserManagement,
                                                [FromServices] SocialPostManagement         __SocialPostManagement,
                                                [FromServices] SocialReportManagement       __SocialReportManagement,
                                                [FromRoute(Name = "type")] string           __Type,
                                                [FromBody] ParserSocialReport               __ParserModel,
                                                [FromHeader(Name = "session_token")] string SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialCommentManagement.SetTraceId(TraceId);
            __SocialUserManagement.SetTraceId(TraceId);
            __SocialPostManagement.SetTraceId(TraceId);
            __SocialReportManagement.SetTraceId(TraceId);
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

                if (__ParserModel.report_type.ToLower() == "other" && (__ParserModel.content == default || __ParserModel.content == string.Empty)) {
                    return Problem(400, "Not accept empty content when type is 'other'");
                }

                SocialReport Report = new SocialReport(){
                    Type            = __Type,
                    ReportType      = __ParserModel.report_type,
                    Content         = __ParserModel.content,
                    ReporterId      = Session.UserId,
                };
                switch (__Type) {
                    case "user":
                        {
                            if (__ParserModel.user_name == default) {
                                LogWarning($"Missing user_name to report, user_name: { Session.User.UserName }");
                                return Problem(400, "Invaid request body.");
                            }
                            var (ReportUser, Error) = await __SocialUserManagement.FindUserIgnoreStatus(__ParserModel.user_name, false);
                            if (Error != ErrorCodes.NO_ERROR) {
                                LogWarning($"Not found user to report, user_name: { Session.User.UserName }");
                                return Problem(404, "Not found report user.");
                            }
                            if (Session.UserId != ReportUser.Id) {
                                LogWarning($"Not allow report yourself, user_name: { Session.User.UserName }");
                                return Problem(400, "Not allow.");
                            }
                            Report.UserId = ReportUser.Id;
                            break;
                        }
                    case "post":
                        {
                            if (__ParserModel.post_slug == default) {
                                LogWarning($"Missing post_slug to report, user_name: { Session.User.UserName }");
                                return Problem(400, "Invaid request body.");
                            }
                            var (ReportPost, Error) = await __SocialPostManagement.FindPostBySlug(__ParserModel.post_slug);
                            if (Error != ErrorCodes.NO_ERROR) {
                                LogWarning($"Not found post to report, user_name: { Session.User.UserName }");
                                return Problem(404, "Not found report post.");
                            }
                            if (Session.UserId != ReportPost.Owner) {
                                LogWarning($"Not allow report own post, user_name: { Session.User.UserName }");
                                return Problem(400, "Not allow.");
                            }
                            Report.PostId = ReportPost.Id;
                            break;
                        }
                    case "comment":
                        {
                            if (__ParserModel.comment_id == default) {
                                LogWarning($"Missing comment_id to report, user_name: { Session.User.UserName }");
                                return Problem(400, "Invaid request body.");
                            }
                            var (ReportComment, Error) = await __SocialCommentManagement.FindCommentById(__ParserModel.comment_id);
                            if (Error != ErrorCodes.NO_ERROR) {
                                LogWarning($"Not found comment to report, user_name: { Session.User.UserName }");
                                return Problem(404, "Not found report comment.");
                            }
                            if (Session.UserId != ReportComment.Owner) {
                                LogWarning($"Not allow report own comment, user_name: { Session.User.UserName }");
                                return Problem(400, "Not allow.");
                            }
                            Report.CommentId = ReportComment.Id;
                            break;
                        }
                    case "feedback":
                        {
                            if (__ParserModel.content == default) {
                                LogWarning($"Invalid request, type: { __Type }, content: { __ParserModel.content }");
                                return Problem(400, "Invaid request body.");
                            }
                            break;
                        }
                    default:
                        LogWarning($"Invalid request, type: { __Type }");
                        return Problem(400, "Invalid request.");
                }

                var ErrorNewReport = await __SocialReportManagement.AddNewReport(Report);
                if (ErrorNewReport != ErrorCodes.NO_ERROR) {
                    throw new Exception($"AddNewReport failed, ErrorCode: { ErrorNewReport }");
                }

                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }
    }
}
