using Common;
using CoreApi.Common;
using CoreApi.Models;
using CoreApi.Services;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.Session
{
    [ApiController]
    [Route("/user/confirm")]
    public class SocialUserConfirmController : BaseController
    {
        #region Config Values
        private int REQUEST_EXPIRY_TIME; // minute
        private int NUMBER_OF_TIMES_ALLOW_CONFIRM_FAILURE; // minute
        #endregion

        public SocialUserConfirmController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            __ControllerName = "SocialUserConfirm";
            LoadConfig();
        }

        [NonAction]
        public override void LoadConfig()
        {
            string Error = "";
            try {
                (REQUEST_EXPIRY_TIME, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                (NUMBER_OF_TIMES_ALLOW_CONFIRM_FAILURE, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG, SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_CONFIRM_FAILURE);
                __LoadConfigSuccess = true;
            } catch (Exception e) {
                __LoadConfigSuccess = false;
                StringBuilder msg = new StringBuilder(e.ToString());
                if (Error != e.Message && Error != "") {
                    msg.Append($" && Error: { Error }");
                }
                LogError($"Load config value failed, message: { msg }");
            }
        }

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status410Gone, Type = typeof(StatusCode410Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetInfoConfirm([FromServices] SocialUserManagement __SocialUserManagement,
                                                        [FromQuery(Name = "i")] string i,
                                                        [FromQuery(Name = "d")] string d,
                                                        [FromQuery(Name = "s")] string state)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate params
                if (StringDecryptor.Decrypt(Uri.UnescapeDataString(i)) == default ||
                    StringDecryptor.Decrypt(Uri.UnescapeDataString(d)) == default) {
                    return Problem(400, "Invalid params.");
                }
                var now = DateTime.UtcNow;
                var Id = Utils.IsValidUUID(StringDecryptor.Decrypt(Uri.UnescapeDataString(i)));
                var Date = Utils.IsValidDateTime(StringDecryptor.Decrypt(Uri.UnescapeDataString(d)), CommonDefine.DATE_TIME_FORMAT);
                if (Id == default || Date == default || state == string.Empty || state.Length != 8) {
                    return Problem(400, "Invalid params.");
                }
                if ((now - Date.ToUniversalTime()).TotalMinutes > REQUEST_EXPIRY_TIME) {
                    LogInformation($"Request has expired, date: { Date.ToString() } ");
                    return Problem(410, "Request have expired.");
                }
                #endregion

                #region Find User
                var (user, error) = await __SocialUserManagement.FindUserById(Id);
                if (error != ErrorCodes.NO_ERROR) {
                    LogWarning($"Not found any user, id: { Id } ");
                    return Problem(400, "Invalid params.");
                }
                if (error == ErrorCodes.DELETED) {
                    LogWarning($"User has been deleted.");
                    return Problem(400, "User has been deleted.");
                }
                if (error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                    LogWarning($"User has been locked.");
                    return Problem(400, "User has been blocked.");
                }
                #endregion

                #region Validate user
                if (user.VerifiedEmail) {
                    LogInformation($"User has verified email, user_id: { Id } ");
                    return Ok(204, new JObject(){
                        { "status", 204 },
                        { "msg", "User has verified email." },
                    });
                }
                var confirm_email = user.Settings.Value<JObject>("confirm_email");
                if (confirm_email == default ||
                    confirm_email.Value<bool>("is_sending") == true ||
                    confirm_email.Value<bool>("send_success") == false) {

                    LogWarning($"Email has not been sent, user_id: { Id } ");
                    return Problem(410, "Request have expired.");
                }

                var confirmState = confirm_email.Value<string>("state");
                if (confirmState == default || confirmState == string.Empty || confirmState.Length != 8) {
                    LogWarning($"Invalid confirm state from DB, user_id: { Id } ");
                    return Problem(410, "Request have expired.");
                }
                if (confirmState != state) {
                    LogInformation($"Invalid confirm state from request, user_id: { Id } ");
                    return Problem(410, "Request have expired.");
                }

                var numberConfirmFailure = confirm_email.Value<int>("confirm_failure");
                if (numberConfirmFailure != default && numberConfirmFailure >= NUMBER_OF_TIMES_ALLOW_CONFIRM_FAILURE) {
                    LogInformation($"The request has expired because the failed confirmation exceeded the maximum allowed number of times");
                    return Problem(410, "Request have expired.");
                }
                #endregion

                return Ok(200, new JObject(){
                    { "status", 200 },
                    { "user", user.GetPublicJsonObject() },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status410Gone, Type = typeof(StatusCode410Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> Confirm([FromServices] SocialUserManagement __SocialUserManagement,
                                                 [FromBody] ConfirmUserModel parser,
                                                 [FromQuery(Name = "i")] string i,
                                                 [FromQuery(Name = "d")] string d,
                                                 [FromQuery(Name = "s")] string state)
        {
            if (!LoadConfigSuccess) {
                return Problem(500, "Internal Server error.");
            }
            #region Set TraceId for services
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Validate params
                if (StringDecryptor.Decrypt(Uri.UnescapeDataString(i)) == default ||
                    StringDecryptor.Decrypt(Uri.UnescapeDataString(d)) == default) {
                    return Problem(400, "Invalid params.");
                }
                var now = DateTime.UtcNow;
                var Id = Utils.IsValidUUID(StringDecryptor.Decrypt(Uri.UnescapeDataString(i)));
                var Date = Utils.IsValidDateTime(StringDecryptor.Decrypt(Uri.UnescapeDataString(d)), CommonDefine.DATE_TIME_FORMAT);
                if (Id == default || Date == default || state == string.Empty || state.Length != 8) {
                    return Problem(400, "Invalid params.");
                }
                if ((now - Date.ToUniversalTime()).TotalMinutes > REQUEST_EXPIRY_TIME) {
                    LogInformation($"Request has expired, date: { Date.ToString() } ");
                    return Problem(410, "Request have expired.");
                }
                #endregion

                #region Find User
                var (user, error) = await __SocialUserManagement.FindUserById(Id);
                if (error != ErrorCodes.NO_ERROR) {
                    LogWarning($"Not found any user, id: { Id } ");
                    return Problem(400, "Invalid params.");
                }
                if (error == ErrorCodes.DELETED) {
                    LogWarning($"User has been deleted.");
                    return Problem(400, "User has been deleted.");
                }
                if (error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                    LogWarning($"User has been locked.");
                    return Problem(400, "User has been blocked.");
                }
                #endregion

                #region Validate user
                if (user.VerifiedEmail) {
                    LogInformation($"User has verified email, user_id: { Id } ");
                    return Ok(204, new JObject(){
                        { "status", 204 },
                        { "msg", "User has verified email." },
                    });
                }
                var confirm_email = user.Settings.Value<JObject>("confirm_email");
                if (confirm_email == default ||
                    confirm_email.Value<bool>("is_sending") == true ||
                    confirm_email.Value<bool>("send_success") == false) {

                    LogWarning($"Email has not been sent, user_id: { Id } ");
                    return Problem(410, "Request have expired.");
                }

                var confirmState = confirm_email.Value<string>("state");
                if (confirmState == default || confirmState == string.Empty || confirmState.Length != 8) {
                    LogWarning($"Invalid confirm state from DB, user_id: { Id } ");
                    return Problem(410, "Request have expired.");
                }
                if (confirmState != state) {
                    LogInformation($"Invalid confirm state from request, user_id: { Id } ");
                    return Problem(410, "Request have expired.");
                }

                var numberConfirmFailure = confirm_email.Value<int>("confirm_failure");
                if (numberConfirmFailure != default && numberConfirmFailure >= NUMBER_OF_TIMES_ALLOW_CONFIRM_FAILURE) {
                    LogInformation($"The request has expired because the failed confirmation exceeded the maximum allowed number of times");
                    return Problem(410, "Request have expired.");
                }
                #endregion

                if (DatabaseAccess.Common.PasswordEncryptor.EncryptPassword(parser.password, user.Salt) != user.Password) {
                    numberConfirmFailure++;
                    error = await __SocialUserManagement.HandleConfirmEmailFailed(user.Id);
                    if (error != ErrorCodes.NO_ERROR) {
                        if (error == ErrorCodes.DELETED) {
                            LogWarning($"User has been deleted.");
                            return Problem(400, "User has been deleted.");
                        }
                        if (error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                            LogWarning($"User has been locked.");
                            return Problem(400, "User has been blocked.");
                        }
                        throw new Exception($"HandleConfirmEmailFailed failed, ErrorCode: { error }");
                    }
                    return Problem(401, "Incorrect password.");
                }

                error = await __SocialUserManagement.HandleConfirmEmailSuccessfully(user.Id);
                if (error != ErrorCodes.NO_ERROR) {
                    if (error == ErrorCodes.DELETED) {
                        LogWarning($"User has been deleted.");
                        return Problem(400, "User has been deleted.");
                    }
                    if (error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        LogWarning($"User has been locked.");
                        return Problem(400, "User has been blocked.");
                    }
                    throw new Exception($"HandleConfirmEmailSuccessfully failed, ErrorCode: { error }");
                }

                return Ok(200, new JObject(){
                    { "status", 200 },
                    { "user", user.GetPublicJsonObject() },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server error.");
            }
        }
    }
}
