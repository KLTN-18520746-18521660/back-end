using Common;
using CoreApi.Common.Base;
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

namespace CoreApi.Controllers.Admin.User
{
    [ApiController]
    [Route("/api/admin/user/forgotpassword")]
    public class AdminUserForgotPasswordController : BaseController
    {
        public AdminUserForgotPasswordController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status410Gone, Type = typeof(StatusCode410Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> SendRequestForgotPassword([FromServices] AdminUserManagement   __AdminUserManagement,
                                                                   [FromServices] Channel<EmailChannel> __EmailChannel,
                                                                   [FromBody] ForgotPasswordModel       __ModelData)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__AdminUserManagement);
            #endregion
            try {
                #region Get config values
                var RequestExpiryTime                   = GetConfigValue<int>(CONFIG_KEY.ADMIN_FORGOT_PASSWORD_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                var RequestTimeout                      = GetConfigValue<int>(CONFIG_KEY.ADMIN_FORGOT_PASSWORD_CONFIG, SUB_CONFIG_KEY.TIMEOUT);
                var NumberOfTimesAllowFailure           = GetConfigValue<int>(CONFIG_KEY.ADMIN_FORGOT_PASSWORD_CONFIG,
                                                                              SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_FAILURE);
                #endregion

                #region Find User
                AddLogParam("user_name", __ModelData.user_name);
                var IsEmail         = CommonValidate.IsEmail(__ModelData.user_name);
                var (User, Error)   = await __AdminUserManagement.FindUser(__ModelData.user_name, IsEmail);
                if (Error != ErrorCodes.NO_ERROR) {
                        return Problem(404, RESPONSE_MESSAGES.NOT_FOUND, new string[]{ "user" });
                }
                #endregion

                #region Validate user
                var FogotPasswordSetting    = User.Settings.Value<JObject>("forgot_password");
                var FailedTimes             = FogotPasswordSetting != default ? FogotPasswordSetting.Value<int>("failed_times") : default;
                var SendDate                = FogotPasswordSetting != default ? FogotPasswordSetting.Value<DateTime>("send_date") : default;
                var IsSending               = FogotPasswordSetting != default ? FogotPasswordSetting.Value<bool>("is_sending") : default;
                var SendSuccess             = FogotPasswordSetting != default ? FogotPasswordSetting.Value<bool>("send_success") : default;
                var Now                     = DateTime.UtcNow;

                AddLogParam("forgot_password", FogotPasswordSetting != default ? FogotPasswordSetting.ToString(Formatting.None) : default);
                if (FogotPasswordSetting != default && (FailedTimes == default || FailedTimes < NumberOfTimesAllowFailure)) {
                    if (IsSending == true && (Now - SendDate.ToUniversalTime()).TotalMinutes <= RequestTimeout) {
                        return Problem(400, RESPONSE_MESSAGES.EMAIL_IS_SENDING);
                    }
                    if (SendSuccess == true && (Now - SendDate.ToUniversalTime()).TotalMinutes <= RequestExpiryTime) {
                        return Problem(400, RESPONSE_MESSAGES.EMAIL_IS_SENT_SUCCESSFULLY);
                    }
                }
                #endregion

                await __EmailChannel.Writer.WriteAsync(new EmailChannel() {
                    TraceId     = TraceId,
                    Type        = RequestToSendEmailType.ForgotPassword,
                    Data        = new JObject() {
                        { "UserId",         User.Id },
                        { "IsAdminUser",    true },
                    }
                });

                return Ok(200, RESPONSE_MESSAGES.OK);
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status410Gone, Type = typeof(StatusCode410Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> GetInfoRequest([FromServices] AdminUserManagement __AdminUserManagement,
                                                        [FromQuery(Name = "i")] string      RawUserId,
                                                        [FromQuery(Name = "d")] string      RawDate,
                                                        [FromQuery(Name = "s")] string      State)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__AdminUserManagement);
            #endregion
            try {
                #region Get config values
                var RequestExpiryTime                   = GetConfigValue<int>(CONFIG_KEY.ADMIN_FORGOT_PASSWORD_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                var NumberOfTimesAllowFailure           = GetConfigValue<int>(CONFIG_KEY.ADMIN_FORGOT_PASSWORD_CONFIG,
                                                                              SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_FAILURE);
                #endregion

                #region Validate params
                AddLogParam("request_sate", State);
                if (StringDecryptor.Decrypt(Uri.UnescapeDataString(RawUserId))  == default
                    || StringDecryptor.Decrypt(Uri.UnescapeDataString(RawDate)) == default
                ) {
                    AddLogParam("raw_user_id", RawUserId);
                    AddLogParam("raw_date", RawDate);
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }

                var Now     = DateTime.UtcNow;
                var UserId  = CommonValidate.IsValidUUID(StringDecryptor.Decrypt(Uri.UnescapeDataString(RawUserId)));
                var Date    = CommonValidate.IsValidDateTime(StringDecryptor.Decrypt(Uri.UnescapeDataString(RawDate)),
                                                             COMMON_DEFINE.DATE_TIME_FORMAT);
                AddLogParam("user_id", UserId);
                AddLogParam("date", Date);
                if (UserId == default || Date == default || State == string.Empty || State.Length != 8) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                if ((Now - Date.ToUniversalTime()).TotalMinutes > RequestExpiryTime) {
                    return Problem(410, RESPONSE_MESSAGES.REQUEST_HAS_EXPIRED);
                }
                #endregion

                #region Find User
                var (User, Error) = await __AdminUserManagement.FindUserById(UserId);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NOT_FOUND) {
                        return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                    } else if (Error == ErrorCodes.DELETED) {
                        return Problem(400, RESPONSE_MESSAGES.USER_HAS_BEEN_DELETED);
                    } else if (Error == ErrorCodes.USER_HAVE_BEEN_LOCKED) {
                        return Problem(400, RESPONSE_MESSAGES.USER_HAS_BEEN_LOCKED);
                    }
                    throw new Exception($"FindUserById failed, ErrorCode: { Error }");
                }
                #endregion

                #region Validate user
                var FogotPasswordSetting    = User.Settings.Value<JObject>("forgot_password");
                var RequestState            = FogotPasswordSetting != default ? FogotPasswordSetting.Value<string>("state") : default;
                var FailedTimes             = FogotPasswordSetting != default ? FogotPasswordSetting.Value<int>("failed_times") : default;
                var SendDate                = FogotPasswordSetting != default ? FogotPasswordSetting.Value<DateTime>("send_date") : default;
                var IsSending               = FogotPasswordSetting != default ? FogotPasswordSetting.Value<bool>("is_sending") : default;
                var SendSuccess             = FogotPasswordSetting != default ? FogotPasswordSetting.Value<bool>("send_success") : default;

                AddLogParam("forgot_password", FogotPasswordSetting != default ? FogotPasswordSetting.ToString(Formatting.None) : default);
                if (SendDate != Date) {
                    return Problem(410, RESPONSE_MESSAGES.REQUEST_HAS_EXPIRED);
                }
                if (FogotPasswordSetting == default || IsSending == true || SendSuccess == false) {
                    return Problem(410, RESPONSE_MESSAGES.REQUEST_HAS_EXPIRED);
                }
                if (RequestState == default || RequestState == string.Empty || RequestState.Length != 8) {
                    return Problem(410, RESPONSE_MESSAGES.REQUEST_HAS_EXPIRED);
                }
                if (RequestState != State) {
                    return Problem(410, RESPONSE_MESSAGES.REQUEST_HAS_EXPIRED);
                }
                if (FailedTimes != default && FailedTimes >= NumberOfTimesAllowFailure) {
                    return Problem(410, RESPONSE_MESSAGES.REQUEST_HAS_EXPIRED);
                }
                #endregion

                return Ok(200, RESPONSE_MESSAGES.OK, default, new JObject(){
                    { "user", User.GetPublicJsonObject() },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, RESPONSE_MESSAGES.INTERNAL_SERVER_ERROR, default, default, LOG_LEVEL.ERROR);
            }
        }
    }
}
