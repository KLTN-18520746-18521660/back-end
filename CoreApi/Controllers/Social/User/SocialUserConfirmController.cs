using Common;
using CoreApi.Common;
using CoreApi.Models;
using CoreApi.Services;
using CoreApi.Services.Background;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.User
{
    [ApiController]
    [Route("/api/user/confirm")]
    public class SocialUserConfirmController : BaseController
    {
        public SocialUserConfirmController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "SocialUserConfirm";
        }

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status410Gone, Type = typeof(StatusCode410Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetInfoConfirm([FromServices] SocialUserManagement __SocialUserManagement,
                                                        [FromQuery(Name = "i")] string      RawUserId,
                                                        [FromQuery(Name = "d")] string      RawDate,
                                                        [FromQuery(Name = "s")] string      State)
        {
            #region Set TraceId for services
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get config values
                var RequestExpiryTime                   = GetConfigValue<int>(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                var NumberOfTimesAllowFailure           = GetConfigValue<int>(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG,
                                                                              SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_FAILURE);
                #endregion

                #region Validate params
                if (StringDecryptor.Decrypt(Uri.UnescapeDataString(RawUserId))  == default
                    || StringDecryptor.Decrypt(Uri.UnescapeDataString(RawDate)) == default
                ) {
                    LogWarning(
                        "Invalid request confirm user, "
                        + $"raw_user_id:, { RawUserId }, "
                        + $"raw_date: { RawDate }"
                    );
                    return Problem(400, "Invalid params.");
                }

                var Now     = DateTime.UtcNow;
                var UserId  = CommonValidate.IsValidUUID(StringDecryptor.Decrypt(Uri.UnescapeDataString(RawUserId)));
                var Date    = CommonValidate.IsValidDateTime(StringDecryptor.Decrypt(Uri.UnescapeDataString(RawDate)),
                                                             CommonDefine.DATE_TIME_FORMAT);
                if (UserId == default || Date == default || State == string.Empty || State.Length != 8) {
                    LogWarning(
                        "Invalid request confirm user, "
                        + $"raw_user_id:, { RawUserId }, "
                        + $"raw_date: { RawDate }, state: { State }"
                    );
                    return Problem(400, "Invalid params.");
                }
                if ((Now - Date.ToUniversalTime()).TotalMinutes > RequestExpiryTime) {
                    LogWarning($"Request has expired, date: { Date.ToString() } ");
                    return Problem(410, "Request have expired.");
                }
                #endregion

                #region Find User
                var (User, Error) = await __SocialUserManagement.FindUserById(UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        LogWarning($"Not found any user, id: { UserId } ");
                        return Problem(400, "Invalid params.");
                    } else if (Error == ErrorCodes.DELETED) {
                        LogWarning($"User has been deleted, id: { UserId }");
                        return Problem(400, "User has been deleted.");
                    } else if (Error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        LogWarning($"User has been locked, id: { UserId }");
                        return Problem(400, "User has been blocked.");
                    }
                    throw new Exception($"FindUserById failed, ErrorCode: { Error }");
                }
                #endregion

                #region Validate user
                if (User.VerifiedEmail) {
                    LogWarning($"User has verified email, user_id: { UserId } ");
                    return Problem(400, "User has verified email.");
                }
                var ConfirmEmail    = User.Settings.Value<JObject>("confirm_email");
                var ConfirmState    = ConfirmEmail != default ? ConfirmEmail.Value<string>("state") : default;
                var FailedTimes     = ConfirmEmail != default ? ConfirmEmail.Value<int>("failed_times") : default;
                var SendDate        = ConfirmEmail != default ? ConfirmEmail.Value<DateTime>("send_date") : default;

                if (ConfirmEmail == default
                    || ConfirmEmail.Value<bool>("is_sending") == true
                    || ConfirmEmail.Value<bool>("send_success") == false
                ) {
                    LogWarning($"Email has not been sent, user_id: { UserId } ");
                    return Problem(410, "Request have expired.");
                }
                if (SendDate != Date) {
                    LogWarning(
                        $"Invalid send date from request, user_id: { UserId }, "
                        + $"send_date: { SendDate.ToString() }, date_from_request: { SendDate.ToString() } "
                    );
                    return Problem(410, "Request have expired.");
                }
                if (ConfirmState == default || ConfirmState == string.Empty || ConfirmState.Length != 8) {
                    LogWarning($"Invalid confirm state from DB, user_id: { UserId } ");
                    return Problem(410, "Request have expired.");
                }
                if (ConfirmState != State) {
                    LogWarning($"Invalid confirm state from request, user_id: { UserId } ");
                    return Problem(410, "Request have expired.");
                }
                if (FailedTimes != default && FailedTimes >= NumberOfTimesAllowFailure) {
                    LogWarning($"The request has expired because the failed confirmation exceeded the maximum allowed number of times");
                    return Problem(410, "Request have expired.");
                }
                #endregion

                LogInformation($"Get user info confirm successfully, user_name: { User.UserName }");
                return Ok(200, "OK", new JObject(){
                    { "user", User.GetPublicJsonObject() },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status410Gone, Type = typeof(StatusCode410Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> Confirm([FromServices] SocialUserManagement    __SocialUserManagement,
                                                 [FromBody] ConfirmUserModel            __ModelData)
        {
            #region Set TraceId for services
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get config values
                var RequestExpiryTime                   = GetConfigValue<int>(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                var NumberOfTimesAllowFailure           = GetConfigValue<int>(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG,
                                                                              SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_FAILURE);
                #endregion

                #region Validate params
                if (StringDecryptor.Decrypt(Uri.UnescapeDataString(__ModelData.i))    == default
                    || StringDecryptor.Decrypt(Uri.UnescapeDataString(__ModelData.d)) == default
                ) {
                    LogWarning(
                        "Invalid request confirm user, "
                        + $"raw_user_id:, { __ModelData.i }, "
                        + $"raw_date: { __ModelData.d }"
                    );
                    return Problem(400, "Invalid params.");
                }

                var Now     = DateTime.UtcNow;
                var UserId  = CommonValidate.IsValidUUID(StringDecryptor.Decrypt(Uri.UnescapeDataString(__ModelData.i)));
                var Date    = CommonValidate.IsValidDateTime(StringDecryptor.Decrypt(Uri.UnescapeDataString(__ModelData.d)),
                                                          CommonDefine.DATE_TIME_FORMAT);
                if (UserId == default || Date == default || __ModelData.s == string.Empty || __ModelData.s.Length != 8) {
                    LogWarning(
                        "Invalid request confirm user, "
                        + $"raw_user_id:, { __ModelData.i }, "
                        + $"raw_date: { __ModelData.d }, state: { __ModelData.s }"
                    );
                    return Problem(400, "Invalid params.");
                }
                if ((Now - Date.ToUniversalTime()).TotalMinutes > RequestExpiryTime) {
                    LogInformation($"Request has expired, date: { Date.ToString() } ");
                    return Problem(410, "Request have expired.");
                }
                #endregion

                #region Find User
                var (User, Error) = await __SocialUserManagement.FindUserById(UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        LogWarning($"Not found any user, id: { UserId } ");
                        return Problem(400, "Invalid params.");
                    } else if (Error == ErrorCodes.DELETED) {
                        LogWarning($"User has been deleted, id: { UserId }");
                        return Problem(400, "User has been deleted.");
                    } else if (Error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        LogWarning($"User has been locked, id: { UserId }");
                        return Problem(400, "User has been blocked.");
                    }
                    throw new Exception($"FindUserById failed, ErrorCode: { Error }");
                }
                #endregion

                #region Validate user
                if (User.VerifiedEmail) {
                    LogWarning($"User has verified email, user_id: { UserId }");
                    return Problem(400, "User has verified email.");
                }
                var ConfirmEmail    = User.Settings.Value<JObject>("confirm_email");
                var ConfirmState    = ConfirmEmail != default ? ConfirmEmail.Value<string>("state") : default;
                var FailedTimes     = ConfirmEmail != default ? ConfirmEmail.Value<int>("failed_times") : default;
                var SendDate        = ConfirmEmail != default ? ConfirmEmail.Value<DateTime>("send_date") : default;
                var IsSending       = ConfirmEmail != default ? ConfirmEmail.Value<bool>("is_sending") : default;
                var SendSuccess     = ConfirmEmail != default ? ConfirmEmail.Value<bool>("send_success") : default;

                if (ConfirmEmail == default || IsSending == true || SendSuccess == false) {
                    LogWarning($"Email has not been sent, user_id: { UserId }");
                    return Problem(410, "Request have expired.");
                }
                if (SendDate != Date) {
                    LogWarning(
                        $"Invalid send date from request, user_id: { UserId }, "
                        + $"send_date: { SendDate.ToString() }, date_from_request: { SendDate.ToString() } "
                    );
                    return Problem(410, "Request have expired.");
                }
                if (ConfirmState == default || ConfirmState == string.Empty || ConfirmState.Length != 8) {
                    LogWarning($"Invalid confirm state from DB, user_id: { UserId }");
                    return Problem(410, "Request have expired.");
                }
                if (ConfirmState != __ModelData.s) {
                    LogInformation($"Invalid confirm state from request, user_id: { UserId }");
                    return Problem(410, "Request have expired.");
                }
                if (FailedTimes != default && FailedTimes >= NumberOfTimesAllowFailure) {
                    LogInformation(
                        "The request has expired because the failed confirmation exceeded the maximum allowed number of times, "
                        + $"user_id: { UserId }"
                    );
                    return Problem(410, "Request have expired.");
                }
                #endregion

                if (DatabaseAccess.Common.PasswordEncryptor.EncryptPassword(__ModelData.password, User.Salt) != User.Password) {
                    Error = await __SocialUserManagement.HandleConfirmEmailFailed(User.Id);
                    if (Error != ErrorCodes.NO_ERROR) {
                        if (Error == ErrorCodes.DELETED) {
                            LogWarning($"User has been deleted, id: { UserId }");
                            return Problem(400, "User has been deleted.");
                        }
                        if (Error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                            LogWarning($"User has been locked, id: { UserId }");
                            return Problem(400, "User has been blocked");
                        }
                        throw new Exception($"HandleConfirmEmailFailed failed, ErrorCode: { Error }");
                    }
                    return Problem(400, "Incorrect password.");
                }

                Error = await __SocialUserManagement.HandleConfirmEmailSuccessfully(User.Id);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.DELETED) {
                        LogWarning($"User has been deleted, id: { UserId }");
                        return Problem(400, "User has been deleted.");
                    }
                    if (Error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        LogWarning($"User has been locked, id: { UserId }");
                        return Problem(400, "User has been blocked.");
                    }
                    throw new Exception($"HandleConfirmEmailSuccessfully failed, ErrorCode: { Error }");
                }

                LogInformation($"User confirm successfully, user_name: { User.UserName }");
                return Ok(200, "OK", new JObject(){
                    { "user", User.GetPublicJsonObject() },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }

        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status410Gone, Type = typeof(StatusCode410Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> SendEmailConfirm([FromServices] SessionSocialUserManagement    __SessionSocialUserManagement,
                                                          [FromServices] SocialUserManagement           __SocialUserManagement,
                                                          [FromServices] Channel<EmailChannel>          __EmailChannel,
                                                          [FromHeader(Name = "session_token")] string   SessionToken)
        {
            #region Set TraceId for services
            __SessionSocialUserManagement.SetTraceId(TraceId);
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get config values
                var RequestExpiryTime                   = GetConfigValue<int>(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                var RequestTimeout                      = GetConfigValue<int>(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG, SUB_CONFIG_KEY.TIMEOUT);
                var NumberOfTimesAllowFailure           = GetConfigValue<int>(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG,
                                                                              SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_FAILURE);
                #endregion

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

                #region Validate user
                if (Session.User.VerifiedEmail) {
                    LogWarning($"User has verified email, user_id: { Session.UserId }");
                    return Problem(400, "User has verified email.");
                }
                var ConfirmEmail    = Session.User.Settings.Value<JObject>("confirm_email");
                var FailedTimes     = ConfirmEmail != default ? ConfirmEmail.Value<int>("failed_times") : default;
                var SendDate        = ConfirmEmail != default ? ConfirmEmail.Value<DateTime>("send_date") : default;
                var IsSending       = ConfirmEmail != default ? ConfirmEmail.Value<bool>("is_sending") : default;
                var SendSuccess     = ConfirmEmail != default ? ConfirmEmail.Value<bool>("send_success") : default;
                var Now             = DateTime.UtcNow;

                if (ConfirmEmail != default && (FailedTimes == default || FailedTimes < NumberOfTimesAllowFailure)) {
                    if (IsSending == true && (Now - SendDate.ToUniversalTime()).TotalMinutes <= RequestTimeout) {
                        LogWarning($"Email is sending, user_id: { Session.UserId }");
                        return Problem(400, "Email is sending.");
                    }
                    if (ConfirmEmail.Value<bool>("send_success") == true && (Now - SendDate.ToUniversalTime()).TotalMinutes <= RequestExpiryTime) {
                        LogWarning($"Email is sent successfully, user_id: { Session.UserId }");
                        return Problem(400, "Email is sent successfully.");
                    }
                }
                #endregion

                await __EmailChannel.Writer.WriteAsync(new EmailChannel() {
                    TraceId     = TraceId,
                    Type        = RequestToSendEmailType.UserSignup,
                    Data        = new JObject() {
                        { "UserId",         Session.UserId },
                        { "IsAdminUser",    false },
                    }
                });

                LogInformation($"Make request email user-confirm successfully, user_name: { Session.User.UserName }");
                return Ok(200, "OK");
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }
    }
}
