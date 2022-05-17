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
        private string[] ALLOW_EXTENSIONS = new string[]{
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

        private string[] ALLOW_SOCIAL_PATHS = new string[]{
            "post",
            "user",
        };

        private string[] ALLOW_ADMIN_PATHS = new string[]{
            "common",
        };

        public UploadFileController(BaseConfig _BaseConfig)
            : base(_BaseConfig)
        {
            // ControllerName = "SocialUserLogin";
        }

        [NonAction]
        public (bool IsValid, IActionResult RetError) IsValidFormFile(IFormFile FormFile, int MaxLengthOfSingleFile, int Index = 0)
        {
            bool IsValid = true;
            if (FormFile == default) {
                AddLogParam($"form_file[{ Index }]", null);
                IsValid = false;
            } else if (FormFile.FileName == default) {
                AddLogParam($"form_file[{ Index }].name", null);
                IsValid = false;
            } else if (FormFile.Length == default) {
                AddLogParam($"form_file[{ Index }].length", null);
                IsValid = false;
            } else if (FormFile.Length > MaxLengthOfSingleFile) {
                AddLogParam($"form_file[{ Index }].length", FormFile.Length);
                AddLogParam("max_file_length_allow", MaxLengthOfSingleFile);
                IsValid = false;
            }

            return (IsValid, Problem(400, "Invalid request upload file.", default, LOG_LEVEL.DEBUG));
        }

#if DEBUG
        [HttpPost("public/file/{path}")]
        public async Task<IActionResult> PublicUploadFile([FromRoute(Name = "path")] string             __PrefixPath,
                                                          IFormFile                                     __FormFile)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices();
            #endregion
            try {
                #region Get config values
                var MaxLengthOfSingleFile  = GetConfigValue<int>(CONFIG_KEY.UPLOAD_FILE_CONFIG, SUB_CONFIG_KEY.MAX_LENGTH_OF_SINGLE_FILE);
                #endregion

                #region Validate params
                if (HttpContext.Request.Form.Files.Count != 1) {
                    AddLogParam("upload_files_size", HttpContext.Request.Form.Files.Count);
                    return Problem(400, "Exceed max size of files to upload.", default, LOG_LEVEL.DEBUG);
                }
                var FormFileValidate = IsValidFormFile(__FormFile, MaxLengthOfSingleFile);
                if (!FormFileValidate.IsValid) {
                    return FormFileValidate.RetError;
                }
                #endregion

                var ExtFile     = Path.GetExtension(__FormFile.FileName);
                var FileName    = Utils.GenerateSlug(__FormFile.FileName.Replace(ExtFile, string.Empty));
                FileName        = FileName.Length > 30 ? FileName.Substring(0, 30) : FileName;
                FileName        = $"{ FileName }-{ ((DateTimeOffset) DateTime.UtcNow).ToUnixTimeSeconds() }-public{ ExtFile }";
                CommonValidate.ValidateDirectoryPath($"{ Program.ServerConfiguration.UploadFilePath }/{ __PrefixPath }", true);
                var FilePath    = Path.Combine($"{ Program.ServerConfiguration.UploadFilePath }/{ __PrefixPath }", FileName);
                using (var Stream = System.IO.File.Create(FilePath))
                {
                    await __FormFile.CopyToAsync(Stream);
                    Stream.Close();
                }

                var RetUrl = $"{ Program.ServerConfiguration.PrefixPathGetUploadFile }/{ __PrefixPath }/{ FileName }";
                return Ok(200, "OK", new JObject(){
                    { "url", RetUrl }
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
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
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionSocialUserManagement,
                __SocialUserManagement
            );
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
                    AddLogParam("upload_files_size", HttpContext.Request.Form.Files.Count);
                    return Problem(400, "Exceed max size of files to upload.", default, LOG_LEVEL.DEBUG);
                }
                var FormFileValidate = IsValidFormFile(__FormFile, MaxLengthOfSingleFile);
                if (!FormFileValidate.IsValid) {
                    return FormFileValidate.RetError;
                }

                var ExtFile = Path.GetExtension(__FormFile.FileName);
                if (!ALLOW_EXTENSIONS.Contains(ExtFile)) {
                    AddLogParam("file_type", ExtFile);
                    return Problem(400, $"Not allow file type: { ExtFile }");
                }
                if (!ALLOW_SOCIAL_PATHS.Contains(__PrefixPath)) {
                    AddLogParam("prefix_upload_path", __PrefixPath);
                    return Problem(400, "Not allow upload file with path '{ __PrefixPath }.");
                }
                #endregion

                #region Check Upload Permission
                var Error = __SocialUserManagement.HaveFullPermission(Session.User.Rights, SOCIAL_RIGHTS.UPLOAD);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    return Problem(403, "User doesn't have permission to upload file.");
                }
                #endregion

                var FileName    = Utils.GenerateSlug(__FormFile.FileName.Replace(ExtFile, string.Empty));
                FileName        = FileName.Length > 30 ? FileName.Substring(0, 30) : FileName;
                FileName        = $"{ FileName }-{ ((DateTimeOffset) DateTime.UtcNow).ToUnixTimeSeconds() }{ ExtFile }";
                CommonValidate.ValidateDirectoryPath($"{ Program.ServerConfiguration.UploadFilePath }/{ __PrefixPath }", true);
                var FilePath = Path.Combine($"{ Program.ServerConfiguration.UploadFilePath }/{ __PrefixPath }", FileName);
                using (var Stream = System.IO.File.Create(FilePath))
                {
                    await __FormFile.CopyToAsync(Stream);
                    Stream.Close();
                }

                var RetUrl = $"{ Program.ServerConfiguration.PrefixPathGetUploadFile }/{ __PrefixPath }/{ FileName }";
                AddLogParam("url_file", RetUrl);
                return Ok(200, "OK", new JObject(){
                    { "url", RetUrl }
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
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
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(
                __SessionAdminUserManagement,
                __AdminUserManagement
            );
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
                    AddLogParam("upload_files_size", HttpContext.Request.Form.Files.Count);
                    return Problem(400, "Exceed max size of files to upload.", default, LOG_LEVEL.DEBUG);
                }
                var FormFileValidate = IsValidFormFile(__FormFile, MaxLengthOfSingleFile);
                if (!FormFileValidate.IsValid) {
                    return FormFileValidate.RetError;
                }

                var ExtFile = Path.GetExtension(__FormFile.FileName);
                if (!ALLOW_EXTENSIONS.Contains(ExtFile)) {
                    AddLogParam("file_type", ExtFile);
                    return Problem(400, $"Not allow file type: { ExtFile }");
                }
                if (!ALLOW_ADMIN_PATHS.Contains(__PrefixPath)) {
                    AddLogParam("prefix_upload_path", __PrefixPath);
                    return Problem(400, "Not allow upload file with path '{ __PrefixPath }.");
                }
                #endregion

                #region Check Upload Permission
                var Error = __AdminUserManagement.HaveFullPermission(Session.User.Rights, ADMIN_RIGHTS.UPLOAD);
                if (Error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    return Problem(403, "User doesn't have permission to upload file.");
                }
                #endregion

                var FileName    = Utils.GenerateSlug(__FormFile.FileName.Replace(ExtFile, string.Empty));
                FileName        = FileName.Length > 30 ? FileName.Substring(0, 30) : FileName;
                FileName        = $"{ FileName }-{ ((DateTimeOffset) DateTime.UtcNow).ToUnixTimeSeconds() }{ ExtFile }";
                CommonValidate.ValidateDirectoryPath($"{ Program.ServerConfiguration.UploadFilePath }/{ __PrefixPath }", true);
                var FilePath    = Path.Combine($"{ Program.ServerConfiguration.UploadFilePath }/{ __PrefixPath }", FileName);
                using (var Stream = System.IO.File.Create(FilePath))
                {
                    await __FormFile.CopyToAsync(Stream);
                    Stream.Close();
                }

                var RetUrl = $"{ Program.ServerConfiguration.PrefixPathGetUploadFile }/{ __PrefixPath }/{ FileName }";
                AddLogParam("url_file", RetUrl);
                return Ok(200, "OK", new JObject(){
                    { "url", RetUrl }
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error.", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
