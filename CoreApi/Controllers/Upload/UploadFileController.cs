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
    // [Produces("multipart/form-data")]
    public class UploadFileController : BaseController
    {
        #region Config Values
        private int MAX_LENGTH_OF_SINGLE_FILE;
        private int EXTENSION_TIME; // minutes
        private int EXPIRY_TIME; // minute
        #endregion

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

        private string[] SocialPath = new string[]{
            "post",
            "user",
        };

        private string[] AdminPath = new string[]{
            "common",
        };

        public UploadFileController(BaseConfig _BaseConfig)
            : base(_BaseConfig)
        {
            __ControllerName = "SocialUserLogin";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = string.Empty;
            try {
                (MAX_LENGTH_OF_SINGLE_FILE, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.UPLOAD_FILE_CONFIG, SUB_CONFIG_KEY.MAX_LENGTH_OF_SINGLE_FILE);
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

#if DEBUG
        [HttpPost("public/file/{path}")]
        public async Task<IActionResult> PublicUploadFile([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                          [FromServices] SocialUserManagement __SocialUserManagement,
                                                          IFormFile formFile,
                                                          [FromRoute] string path,
                                                          [FromHeader] string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            #endregion
            try {
                #region Validate params
                if (HttpContext.Request.Form.Files.Count != 1) {
                    return Problem(400, "Invalid Request.");
                }

                formFile = HttpContext.Request.Form.Files[0];
                if (formFile == default || formFile.FileName == default || formFile.Length == default) {
                    return Problem(400, "Invalid file.");
                }
                #endregion

                if (formFile.Length > MAX_LENGTH_OF_SINGLE_FILE) {
                    return Problem(400, "Exceed max size of file.");
                }

                var ext = Path.GetExtension(formFile.FileName);

                var fileName = Utils.GenerateSlug(formFile.FileName.Replace(ext, string.Empty));
                fileName = fileName.Length > 30 ? fileName.Substring(0, 30) : fileName;
                fileName = $"{ fileName }-{ ((DateTimeOffset) DateTime.UtcNow).ToUnixTimeSeconds() }-public{ ext }";
                CommonValidate.ValidateDirectoryPath($"{ Program.ServerConfiguration.UploadFilePath }/{ path }", true);
                var filePath = Path.Combine($"{ Program.ServerConfiguration.UploadFilePath }/{ path }", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await formFile.CopyToAsync(stream);
                }

                return Ok(200, "OK", new JObject(){
                    { "url", $"{ Program.ServerConfiguration.PrefixPathGetUploadFile }/{ path }/{ fileName }" }
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
#endif

        [HttpPost("file/{path}")]
        public async Task<IActionResult> SocialUploadFile([FromServices] SessionSocialUserManagement __SessionSocialUserManagement,
                                                          [FromServices] SocialUserManagement __SocialUserManagement,
                                                          IFormFile formFile,
                                                          [FromRoute] string path,
                                                          [FromHeader] string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            #endregion
            try {
                #region Validate params
                if (HttpContext.Request.Form.Files.Count != 1) {
                    return Problem(400, "Invalid Request.");
                }

                formFile = HttpContext.Request.Form.Files[0];
                if (formFile == default || formFile.FileName == default || formFile.Length == default) {
                    return Problem(400, "Invalid files.");
                }
                #endregion

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

                #region Check Upload Permission
                var error = __SocialUserManagement.HaveFullPermission(session.User.Rights, SOCIAL_RIGHTS.UPLOAD);
                if (error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    LogInformation($"User doesn't have permission to upload file, user_name: { session.User.UserName }");
                    return Problem(403, "User doesn't have permission to upload file.");
                }
                #endregion

                if (formFile.Length > MAX_LENGTH_OF_SINGLE_FILE) {
                    return Problem(400, "Exceed max size of file.");
                }

                var ext = Path.GetExtension(formFile.FileName);
                if (!AllowExtensions.Contains(ext)) {
                    return Problem(400, $"Not allow file type: { ext }");
                }

                var fileName = Utils.GenerateSlug(formFile.FileName.Replace(ext, string.Empty));
                fileName = fileName.Length > 30 ? fileName.Substring(0, 30) : fileName;
                fileName = $"{ fileName }-{ ((DateTimeOffset) DateTime.UtcNow).ToUnixTimeSeconds() }{ ext }";
                CommonValidate.ValidateDirectoryPath($"{ Program.ServerConfiguration.UploadFilePath }/{ path }", true);
                var filePath = Path.Combine($"{ Program.ServerConfiguration.UploadFilePath }/{ path }", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await formFile.CopyToAsync(stream);
                }

                return Ok(200, "OK", new JObject(){
                    { "url", $"{ Program.ServerConfiguration.PrefixPathGetUploadFile }/{ path }/{ fileName }" }
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }

        
        [HttpPost("file/admin/{path}")]
        public async Task<IActionResult> AdminUploadFile([FromServices] SessionAdminUserManagement __SessionAdminUserManagement,
                                                         [FromServices] AdminUserManagement __AdminUserManagement,
                                                         IFormFile formFile,
                                                         [FromRoute] string path,
                                                         [FromHeader(Name = "session_token_admin")] string session_token)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            #endregion
            try {
                #region Validate params
                if (HttpContext.Request.Form.Files.Count != 1) {
                    return Problem(400, "Invalid Request.");
                }

                formFile = HttpContext.Request.Form.Files[0];
                if (formFile == default || formFile.FileName == default || formFile.Length == default) {
                    return Problem(400, "Invalid files.");
                }
                #endregion

                #region Get session
                session_token = session_token != default ? session_token : GetValueFromCookie(SessionTokenHeaderKey);
                var (__session, errRet) = await GetSessionToken(__SessionAdminUserManagement, EXPIRY_TIME, EXTENSION_TIME, session_token);
                if (errRet != default) {
                    return errRet;
                }
                if (__session == default) {
                    throw new Exception($"GetSessionToken failed.");
                }
                var session = __session as SessionAdminUser;
                #endregion

                #region Check Upload Permission
                var error = __AdminUserManagement.HaveFullPermission(session.User.Rights, ADMIN_RIGHTS.UPLOAD);
                if (error == ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION) {
                    LogInformation($"User doesn't have permission to upload file, user_name: { session.User.UserName }");
                    return Problem(403, "User doesn't have permission to upload file.");
                }
                #endregion

                if (formFile.Length > MAX_LENGTH_OF_SINGLE_FILE) {
                    return Problem(400, "Exceed max size of file.");
                }

                var ext = Path.GetExtension(formFile.FileName);
                if (!AllowExtensions.Contains(ext)) {
                    return Problem(400, $"Not allow file type: { ext }");
                }

                var fileName = Utils.GenerateSlug(formFile.FileName.Replace(ext, string.Empty));
                fileName = fileName.Length > 30 ? fileName.Substring(0, 30) : fileName;
                fileName = $"{ fileName }-{ ((DateTimeOffset) DateTime.UtcNow).ToUnixTimeSeconds() }{ ext }";
                CommonValidate.ValidateDirectoryPath($"{ Program.ServerConfiguration.UploadFilePath }/{ path }", true);
                var filePath = Path.Combine($"{ Program.ServerConfiguration.UploadFilePath }/{ path }", fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await formFile.CopyToAsync(stream);
                }

                return Ok(200, "OK", new JObject(){
                    { "url", $"{ Program.ServerConfiguration.PrefixPathGetUploadFile }/{ path }/{ fileName }" }
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}