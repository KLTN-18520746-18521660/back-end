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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CoreApi.Controllers.Social.User
{
    [ApiController]
    [Route("/api/user/newpassword")]
    public class SocialUserNewPasswordController : BaseController
    {
        public SocialUserNewPasswordController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status410Gone, Type = typeof(StatusCode410Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> NewPassword([FromServices] SocialUserManagement    __SocialUserManagement,
                                                     [FromBody] NewPasswordModel            __ModelData)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__SocialUserManagement);
            #endregion
            try {
                #region Get config values
                var RequestExpiryTime                   = GetConfigValue<int>(CONFIG_KEY.SOCIAL_FORGOT_PASSWORD_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                var NumberOfTimesAllowFailure           = GetConfigValue<int>(CONFIG_KEY.SOCIAL_FORGOT_PASSWORD_CONFIG,
                                                                              SUB_CONFIG_KEY.NUMBER_OF_TIMES_ALLOW_FAILURE);
                #endregion

                #region Validate params
                AddLogParam("raw_user_id", __ModelData.i);
                AddLogParam("raw_date", __ModelData.d);
                AddLogParam("request_sate", __ModelData.s);
                if (StringDecryptor.Decrypt(Uri.UnescapeDataString(__ModelData.i))    == default
                    || StringDecryptor.Decrypt(Uri.UnescapeDataString(__ModelData.d)) == default
                ) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }

                var UserId  = CommonValidate.IsValidUUID(StringDecryptor.Decrypt(Uri.UnescapeDataString(__ModelData.i)));
                var Date    = CommonValidate.IsValidDateTime(StringDecryptor.Decrypt(Uri.UnescapeDataString(__ModelData.d)),
                                                             COMMON_DEFINE.DATE_TIME_FORMAT);
                if (UserId == default || Date == default || __ModelData.s == string.Empty || __ModelData.s.Length != 8) {
                    return Problem(400, RESPONSE_MESSAGES.BAD_REQUEST_PARAMS);
                }
                if ((DateTime.UtcNow - Date.ToUniversalTime()).TotalMinutes > RequestExpiryTime) {
                    return Problem(410, RESPONSE_MESSAGES.REQUEST_HAS_EXPIRED);
                }
                #endregion

                #region Find User
                var (User, Error) = await __SocialUserManagement.FindUserById(UserId);
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
                var EmailSend               = FogotPasswordSetting != default ? FogotPasswordSetting.Value<string>("email") : default;

                if (SendDate.ToString(COMMON_DEFINE.DATE_TIME_FORMAT) != Date.ToString(COMMON_DEFINE.DATE_TIME_FORMAT)) {
                    return Problem(410, RESPONSE_MESSAGES.REQUEST_HAS_EXPIRED);
                }
                if (FogotPasswordSetting == default || EmailSend == default || IsSending == true || SendSuccess == false) {
                    return Problem(410, RESPONSE_MESSAGES.REQUEST_HAS_EXPIRED);
                }
                if (EmailSend != User.Email) {
                    return Problem(410, RESPONSE_MESSAGES.REQUEST_HAS_EXPIRED);
                }
                if (RequestState == default || RequestState == string.Empty || RequestState.Length != 8) {
                    return Problem(410, RESPONSE_MESSAGES.REQUEST_HAS_EXPIRED);
                }
                if (RequestState != __ModelData.s) {
                    return Problem(410, RESPONSE_MESSAGES.REQUEST_HAS_EXPIRED);
                }
                if (FailedTimes != default && FailedTimes >= NumberOfTimesAllowFailure) {
                    return Problem(410, RESPONSE_MESSAGES.REQUEST_HAS_EXPIRED);
                }
                #endregion

                Error = await __SocialUserManagement.ChangePassword(User.Id, __ModelData.new_password);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NO_CHANGE_DETECTED) {
                        return Problem(400, RESPONSE_MESSAGES.NO_CHANGES_DETECTED);
                    }
                    var ErrHandle = await __SocialUserManagement.HandleNewPasswordFailed(User.Id);
                    if (ErrHandle != ErrorCodes.NO_ERROR) {
                        WriteLog(LOG_LEVEL.ERROR, false, $"HandleNewPasswordFailed failed, ErrorCode: { ErrHandle }");
                    }
                    throw new Exception($"ChangePassword failed, ErrorCode: { Error }");
                }

                Error = await __SocialUserManagement.HandleNewPasswordSuccessfully(User.Id);
                if (Error != ErrorCodes.NO_ERROR) {
                    WriteLog(LOG_LEVEL.ERROR, false, $"HandleNewPasswordSuccessfully failed, ErrorCode: { Error }");
                }

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
