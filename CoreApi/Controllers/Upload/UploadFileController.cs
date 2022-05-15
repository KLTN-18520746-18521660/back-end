using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using CoreApi.Common;
using CoreApi.Services;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace CoreApi.Controllers.Upload
{
    [ApiController]
    [Route("/api/upload")]
    public class UploadFileController : BaseController
    {
        // https://developer.mozilla.org/en-US/docs/Web/Media/Formats/Image_types
        private string[] AllowExtensions = new string[]{
            ".apng",
            ".avif",
            ".gif",
            ".jpg",
            ".jpeg",
            ".jfif",
            ".pjpeg",
            ".pjp",
            ".png",
            ".svg",
            ".webp",
        };

        private string[] AllowSocialPaths = new string[]{
            "post",
            "user",
        };

        private string[] AllowAdminPaths = new string[]{
            "common",
        };

        public UploadFileController(BaseConfig _BaseConfig)
            : base(_BaseConfig)
        {
            ControllerName = "SocialUserLogin";
        }

#if DEBUG
        [HttpPost("public/file/{path}")]
        public async Task<IActionResult> PublicUploadFile([FromServices] SocialUserManagement           __SocialUserManagement,
                                                          [FromRoute(Name = "path")] string             __PrefixPath,
                                                          IFormFile                                     __FormFile)
        {
            #region Set TraceId for services
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get config values
                var MaxLengthOfSingleFile  = GetConfigValue<int>(CONFIG_KEY.UPLOAD_FILE_CONFIG, SUB_CONFIG_KEY.MAX_LENGTH_OF_SINGLE_FILE);
                #endregion

                #region Validate params
                if (HttpContext.Request.Form.Files.Count != 1) {
                    LogWarning($"Exceed max size of file to upload file.");
                    return Problem(400, "Invalid Request.");
                }

                __FormFile = HttpContext.Request.Form.Files[0];
                if (__FormFile == default || __FormFile.FileName == default || __FormFile.Length == default) {
                    LogDebug($"Invalid request upload file.");
                    return Problem(400, "Invalid file.");
                }
                #endregion

                if (__FormFile.Length > MaxLengthOfSingleFile) {
                    LogWarning($"Exceed max size of file to upload file.");
                    return Problem(400, "Exceed max size of file.");
                }

                var ExtFile     = Path.GetExtension(__FormFile.FileName);
                var FileName    = Utils.GenerateSlug(__FormFile.FileName.Replace(ExtFile, string.Empty));
                FileName        = FileName.Length > 30 ? FileName.Substring(0, 30) : FileName;
                FileName        = $"{ FileName }-{ ((DateTimeOffset) DateTime.UtcNow).ToUnixTimeSeconds() }-public{ ExtFile }";
                CommonValidate.ValidateDirectoryPath($"{ Program.ServerConfiguration.UploadFilePath }/{ __PrefixPath }", true);
                var FilePath    = Path.Combine($"{ Program.ServerConfiguration.UploadFilePath }/{ __PrefixPath }", FileName);
                using (var stream = System.IO.File.Create(FilePath))
                {
                    await __FormFile.CopyToAsync(stream);
                }

                var RetUrl = $"{ Program.ServerConfiguration.PrefixPathGetUploadFile }/{ __PrefixPath }/{ FileName }";
                LogInformation($"Upload public file success: { RetUrl }");
                return Ok(200, "OK", new JObject(){
                    { "url", RetUrl }
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }
#endif

        [HttpPost("file/{path}")]
        public async Task<IActionResult> SocialUploadFile([FromServices] SessionSocialUserManagement    __SessionSocialUserManagement,
                                                          [FromServices] SocialUserManagement           __SocialUserManagement,
                                                          [FromRoute(Name = "path")] string             __PrefixPath,
                                                          IFormFile                                     __FormFile,
                                                          [FromHeader(Name = "session_token")] string   SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get config values
                var MaxLengthOfSingleFile  = GetConfigValue<int>(CONFIG_KEY.UPLOAD_FILE_CONFIG, SUB_CONFIG_KEY.MAX_LENGTH_OF_SINGLE_FILE);
                #endregion

                #region Get session
                SessionToken = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, ErrRet) = await GetSessionToken(__SessionSocialUserManagement, SessionToken);
                if (ErrRet != default) {
                    return ErrRet;
                }
                if (__Session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var Session = __Session as SessionSocialUser;
                #endregion

                #region Validate params
                if (HttpContext.Request.Form.Files.Count != 1) {
                    LogDebug($"Invalid request upload file, user_name: { Session.User.UserName }");
                    return Problem(400, "Invalid Request.");
                }

                __FormFile = HttpContext.Request.Form.Files[0];
                if (__FormFile == default || __FormFile.FileName == default || __FormFile.Length == default) {
                    LogDebug($"Invalid body request upload file, user_name: { Session.User.UserName }");
                    return Problem(400, "Invalid files.");
                }

                if (!AllowSocialPaths.Contains(__PrefixPath)) {
                    LogWarning($"Not allow upload social file to path '{ __PrefixPath }', user_name: { Session.User.UserName }");
                    return Problem(400, "Not allow upload file with path '{ __PrefixPath }.");
                }

                if (__FormFile.Length > MaxLengthOfSingleFile) {
                    LogWarning($"Exceed max size of file to upload file, user_name: { Session.User.UserName }");
                    return Problem(400, "Exceed max size of file.");
                }

                var ExtFile = Path.GetExtension(__FormFile.FileName);
                if (!AllowExtensions.Contains(ExtFile)) {
                    LogWarning($"Not allow file type: { ExtFile } to upload file, user_name: { Session.User.UserName }");
                    return Problem(400, $"Not allow file type: { ExtFile }");
                }
                #endregion

                #region Check Upload Permission
                var Error = __SocialUserManagement.HaveFullPermission(Session.User.Rights, SOCIAL_RIGHTS.UPLOAD);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    LogWarning($"User doesn't have permission to upload file, user_name: { Session.User.UserName }");
                    return Problem(403, "User doesn't have permission to upload file.");
                }
                #endregion

                var FileName    = Utils.GenerateSlug(__FormFile.FileName.Replace(ExtFile, string.Empty));
                FileName        = FileName.Length > 30 ? FileName.Substring(0, 30) : FileName;
                FileName        = $"{ FileName }-{ ((DateTimeOffset) DateTime.UtcNow).ToUnixTimeSeconds() }{ ExtFile }";
                CommonValidate.ValidateDirectoryPath($"{ Program.ServerConfiguration.UploadFilePath }/{ __PrefixPath }", true);
                var FilePath = Path.Combine($"{ Program.ServerConfiguration.UploadFilePath }/{ __PrefixPath }", FileName);
                using (var stream = System.IO.File.Create(FilePath))
                {
                    await __FormFile.CopyToAsync(stream);
                }

                var RetUrl = $"{ Program.ServerConfiguration.PrefixPathGetUploadFile }/{ __PrefixPath }/{ FileName }";
                LogInformation($"Upload social file success: { RetUrl }, user_name: { Session.User.UserName }");
                return Ok(200, "OK", new JObject(){
                    { "url", RetUrl }
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }

        
        [HttpPost("file/admin/{path}")]
        public async Task<IActionResult> AdminUploadFile([FromServices] SessionAdminUserManagement          __SessionAdminUserManagement,
                                                         [FromServices] AdminUserManagement                 __AdminUserManagement,
                                                         [FromRoute(Name = "path")] string                  __PrefixPath,
                                                         IFormFile                                          __FormFile,
                                                         [FromHeader(Name = "session_token_admin")] string  SessionToken)
        {
            IsAdminController = true; // Special case
            #region Set TraceId for services
            __SessionAdminUserManagement.SetTraceId(TraceId);
            __AdminUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get config values
                var MaxLengthOfSingleFile  = GetConfigValue<int>(CONFIG_KEY.UPLOAD_FILE_CONFIG, SUB_CONFIG_KEY.MAX_LENGTH_OF_SINGLE_FILE);
                #endregion

                #region Get session
                SessionToken = SessionToken != default ? SessionToken : GetValueFromCookie(SessionTokenHeaderKey);
                var (__Session, ErrRet) = await GetSessionToken(__SessionAdminUserManagement, SessionToken);
                if (ErrRet != default) {
                    return ErrRet;
                }
                if (__Session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var Session = __Session as SessionSocialUser;
                #endregion

                #region Validate params
                if (HttpContext.Request.Form.Files.Count != 1) {
                    LogDebug($"Invalid request upload file, user_name: { Session.User.UserName }");
                    return Problem(400, "Invalid Request.");
                }

                __FormFile = HttpContext.Request.Form.Files[0];
                if (__FormFile == default || __FormFile.FileName == default || __FormFile.Length == default) {
                    LogDebug($"Invalid body request upload file, user_name: { Session.User.UserName }");
                    return Problem(400, "Invalid files.");
                }

                if (!AllowAdminPaths.Contains(__PrefixPath)) {
                    LogWarning($"Not allow upload admin file to path '{ __PrefixPath }', user_name: { Session.User.UserName }");
                    return Problem(400, "Not allow upload file with path '{ __PrefixPath }.");
                }

                if (__FormFile.Length > MaxLengthOfSingleFile) {
                    LogWarning($"Exceed max size of file to upload file, user_name: { Session.User.UserName }");
                    return Problem(400, "Exceed max size of file.");
                }

                var ExtFile = Path.GetExtension(__FormFile.FileName);
                if (!AllowExtensions.Contains(ExtFile)) {
                    LogWarning($"Not allow file type: { ExtFile } to upload file, user_name: { Session.User.UserName }");
                    return Problem(400, $"Not allow file type: { ExtFile }");
                }
                #endregion

                #region Check Upload Permission
                var error = __AdminUserManagement.HaveFullPermission(Session.User.Rights, ADMIN_RIGHTS.UPLOAD);
                if (error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    LogWarning($"User doesn't have permission to upload file, user_name: { Session.User.UserName }");
                    return Problem(403, "User doesn't have permission to upload file.");
                }
                #endregion

                var FileName    = Utils.GenerateSlug(__FormFile.FileName.Replace(ExtFile, string.Empty));
                FileName        = FileName.Length > 30 ? FileName.Substring(0, 30) : FileName;
                FileName        = $"{ FileName }-{ ((DateTimeOffset) DateTime.UtcNow).ToUnixTimeSeconds() }{ ExtFile }";
                CommonValidate.ValidateDirectoryPath($"{ Program.ServerConfiguration.UploadFilePath }/{ __PrefixPath }", true);
                var FilePath    = Path.Combine($"{ Program.ServerConfiguration.UploadFilePath }/{ __PrefixPath }", FileName);
                using (var stream = System.IO.File.Create(FilePath))
                {
                    await __FormFile.CopyToAsync(stream);
                }

                var RetUrl = $"{ Program.ServerConfiguration.PrefixPathGetUploadFile }/{ __PrefixPath }/{ FileName }";
                LogInformation($"Upload admin file success: { RetUrl }, user_name: { Session.User.UserName }");
                return Ok(200, "OK", new JObject(){
                    { "url", RetUrl }
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }
    }
}