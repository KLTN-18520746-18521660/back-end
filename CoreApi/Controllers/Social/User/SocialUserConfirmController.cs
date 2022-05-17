using Common;
using CoreApi.Common;
using CoreApi.Models;
using CoreApi.Services;
using CoreApi.Services.Background;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
                AddLogParam("request_sate", State);
                if (StringDecryptor.Decrypt(Uri.UnescapeDataString(RawUserId))  == default
                    || StringDecryptor.Decrypt(Uri.UnescapeDataString(RawDate)) == default
                ) {
                    AddLogParam("raw_user_id", RawUserId);
                    AddLogParam("raw_date", RawDate);
                    return Problem(400, "Invalid params.");
                }

                var Now     = DateTime.UtcNow;
                var UserId  = CommonValidate.IsValidUUID(StringDecryptor.Decrypt(Uri.UnescapeDataString(RawUserId)));
                var Date    = CommonValidate.IsValidDateTime(StringDecryptor.Decrypt(Uri.UnescapeDataString(RawDate)),
                                                             COMMON_DEFINE.DATE_TIME_FORMAT);
                AddLogParam("user_id", UserId);
                AddLogParam("date", Date);
                if (UserId == default || Date == default || State == string.Empty || State.Length != 8) {
                    return Problem(400, "Invalid params.");
                }
                if ((Now - Date.ToUniversalTime()).TotalMinutes > RequestExpiryTime) {
                    return Problem(410, "Request have expired.");
                }
                #endregion

                #region Find User
                var (User, Error) = await __SocialUserManagement.FindUserById(UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(400, "Invalid params.");
                    } else if (Error == ErrorCodes.DELETED) {
                        return Problem(400, "User has been deleted.");
                    } else if (Error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        return Problem(400, "User has been blocked.");
                    }
                    throw new Exception($"FindUserById failed, ErrorCode: { Error }");
                }
                #endregion

                #region Validate user
                if (User.VerifiedEmail) {
                    return Problem(400, "User has verified email.");
                }
                var ConfirmEmail    = User.Settings.Value<JObject>("confirm_email");
                var ConfirmState    = ConfirmEmail != default ? ConfirmEmail.Value<string>("state") : default;
                var FailedTimes     = ConfirmEmail != default ? ConfirmEmail.Value<int>("failed_times") : default;
                var SendDate        = ConfirmEmail != default ? ConfirmEmail.Value<DateTime>("send_date") : default;
                var IsSending       = ConfirmEmail != default ? ConfirmEmail.Value<bool>("is_sending") : default;
                var SendSuccess     = ConfirmEmail != default ? ConfirmEmail.Value<bool>("send_success") : default;

                AddLogParam("confirm_email", ConfirmEmail != default ? ConfirmEmail.ToString(Formatting.None) : default);
                if (ConfirmEmail == default || IsSending == true || SendSuccess == false) {
                    return Problem(410, "Request have expired.");
                }
                if (SendDate != Date) {
                    return Problem(410, "Request have expired.");
                }
                if (ConfirmState == default || ConfirmState == string.Empty || ConfirmState.Length != 8) {
                    return Problem(410, "Request have expired.");
                }
                if (ConfirmState != State) {
                    return Problem(410, "Request have expired.");
                }
                if (FailedTimes != default && FailedTimes >= NumberOfTimesAllowFailure) {
                    return Problem(410, "Request have expired.");
                }
                #endregion

                return Ok(200, "OK", new JObject(){
                    { "user", User.GetPublicJsonObject() },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
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
                AddLogParam("raw_user_id", __ModelData.i);
                AddLogParam("raw_date", __ModelData.d);
                AddLogParam("request_sate", __ModelData.s);
                if (StringDecryptor.Decrypt(Uri.UnescapeDataString(__ModelData.i))    == default
                    || StringDecryptor.Decrypt(Uri.UnescapeDataString(__ModelData.d)) == default
                ) {
                    return Problem(400, "Invalid params.");
                }

                var Now     = DateTime.UtcNow;
                var UserId  = CommonValidate.IsValidUUID(StringDecryptor.Decrypt(Uri.UnescapeDataString(__ModelData.i)));
                var Date    = CommonValidate.IsValidDateTime(StringDecryptor.Decrypt(Uri.UnescapeDataString(__ModelData.d)),
                                                          COMMON_DEFINE.DATE_TIME_FORMAT);
                if (UserId == default || Date == default || __ModelData.s == string.Empty || __ModelData.s.Length != 8) {
                    return Problem(400, "Invalid params.");
                }
                if ((Now - Date.ToUniversalTime()).TotalMinutes > RequestExpiryTime) {
                    return Problem(410, "Request have expired.");
                }
                #endregion

                #region Find User
                var (User, Error) = await __SocialUserManagement.FindUserById(UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(400, "Invalid params.");
                    } else if (Error == ErrorCodes.DELETED) {
                        return Problem(400, "User has been deleted.");
                    } else if (Error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        return Problem(400, "User has been blocked.");
                    }
                    throw new Exception($"FindUserById failed, ErrorCode: { Error }");
                }
                #endregion

                #region Validate user
                if (User.VerifiedEmail) {
                    return Problem(400, "User has verified email.");
                }
                var ConfirmEmail    = User.Settings.Value<JObject>("confirm_email");
                var ConfirmState    = ConfirmEmail != default ? ConfirmEmail.Value<string>("state") : default;
                var FailedTimes     = ConfirmEmail != default ? ConfirmEmail.Value<int>("failed_times") : default;
                var SendDate        = ConfirmEmail != default ? ConfirmEmail.Value<DateTime>("send_date") : default;
                var IsSending       = ConfirmEmail != default ? ConfirmEmail.Value<bool>("is_sending") : default;
                var SendSuccess     = ConfirmEmail != default ? ConfirmEmail.Value<bool>("send_success") : default;

                AddLogParam("confirm_email", ConfirmEmail != default ? ConfirmEmail.ToString(Formatting.None) : default);
                if (ConfirmEmail == default || IsSending == true || SendSuccess == false) {
                    return Problem(410, "Request have expired.");
                }
                if (SendDate != Date) {
                    return Problem(410, "Request have expired.");
                }
                if (ConfirmState == default || ConfirmState == string.Empty || ConfirmState.Length != 8) {
                    return Problem(410, "Request have expired.");
                }
                if (ConfirmState != __ModelData.s) {
                    return Problem(410, "Request have expired.");
                }
                if (FailedTimes != default && FailedTimes >= NumberOfTimesAllowFailure) {
                    return Problem(410, "Request have expired.");
                }
                #endregion

                if (DatabaseAccess.Common.PasswordEncryptor.EncryptPassword(__ModelData.password, User.Salt) != User.Password) {
                    Error = await __SocialUserManagement.HandleConfirmEmailFailed(User.Id);
                    if (Error != ErrorCodes.NO_ERROR) {
                        if (Error == ErrorCodes.DELETED) {
                            return Problem(400, "User has been deleted.");
                        }
                        if (Error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                            return Problem(400, "User has been blocked");
                        }
                        throw new Exception($"HandleConfirmEmailFailed failed, ErrorCode: { Error }");
                    }
                    return Problem(400, "Incorrect password.");
                }

                Error = await __SocialUserManagement.HandleConfirmEmailSuccessfully(User.Id);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.DELETED) {
                        return Problem(400, "User has been deleted.");
                    }
                    if (Error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        return Problem(400, "User has been blocked.");
                    }
                    throw new Exception($"HandleConfirmEmailSuccessfully failed, ErrorCode: { Error }");
                }

                return Ok(200, "OK", new JObject(){
                    { "user", User.GetPublicJsonObject() },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
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
                    return Problem(400, "User has verified email.");
                }
                var ConfirmEmail    = Session.User.Settings.Value<JObject>("confirm_email");
                var FailedTimes     = ConfirmEmail != default ? ConfirmEmail.Value<int>("failed_times") : default;
                var SendDate        = ConfirmEmail != default ? ConfirmEmail.Value<DateTime>("send_date") : default;
                var IsSending       = ConfirmEmail != default ? ConfirmEmail.Value<bool>("is_sending") : default;
                var SendSuccess     = ConfirmEmail != default ? ConfirmEmail.Value<bool>("send_success") : default;
                var Now             = DateTime.UtcNow;

                AddLogParam("confirm_email", ConfirmEmail != default ? ConfirmEmail.ToString(Formatting.None) : default);
                if (ConfirmEmail != default && (FailedTimes == default || FailedTimes < NumberOfTimesAllowFailure)) {
                    if (IsSending == true && (Now - SendDate.ToUniversalTime()).TotalMinutes <= RequestTimeout) {
                        return Problem(400, "Email is sending.");
                    }
                    if (ConfirmEmail.Value<bool>("send_success") == true && (Now - SendDate.ToUniversalTime()).TotalMinutes <= RequestExpiryTime) {
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

                return Ok(200, "OK");
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
