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
    [Route("/api/user/newpassword")]
    public class SocialUserNewPasswordController : BaseController
    {
        public SocialUserNewPasswordController(BaseConfig _BaseConfig) : base(_BaseConfig)
        {
            ControllerName = "SocialUserNewPassword";
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status410Gone, Type = typeof(StatusCode410Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> NewPassword([FromServices] SocialUserManagement    __SocialUserManagement,
                                                     [FromBody] NewPasswordModel            __ModelData)
        {
            #region Set TraceId for services
            __SocialUserManagement.SetTraceId(TraceId);
            #endregion
            try {
                #region Get config values
                var RequestExpiryTime                   = GetConfigValue<int>(CONFIG_KEY.FORGOT_PASSWORD_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                var NumberOfTimesAllowFailure           = GetConfigValue<int>(CONFIG_KEY.FORGOT_PASSWORD_CONFIG,
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
                if ((DateTime.UtcNow - Date.ToUniversalTime()).TotalMinutes > RequestExpiryTime) {
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
                var FogotPasswordSetting    = User.Settings.Value<JObject>("forgot_password");
                var RequestState            = FogotPasswordSetting != default ? FogotPasswordSetting.Value<string>("state") : default;
                var FailedTimes             = FogotPasswordSetting != default ? FogotPasswordSetting.Value<int>("failed_times") : default;
                var SendDate                = FogotPasswordSetting != default ? FogotPasswordSetting.Value<DateTime>("send_date") : default;

                if (SendDate != Date) {
                    LogWarning(
                        $"Invalid send date from request, user_id: { UserId }, "
                        + $"send_date: { SendDate.ToString() }, date_from_request: { SendDate.ToString() } "
                    );
                    return Problem(410, "Request have expired.");
                }
                if (FogotPasswordSetting == default
                    || FogotPasswordSetting.Value<bool>("is_sending") == true
                    || FogotPasswordSetting.Value<bool>("send_success") == false
                ) {
                    LogWarning($"Email has not been sent, user_id: { UserId } ");
                    return Problem(410, "Request have expired.");
                }
                if (RequestState == default || RequestState == string.Empty || RequestState.Length != 8) {
                    LogWarning($"Invalid request state from DB, user_id: { UserId } ");
                    return Problem(410, "Request have expired.");
                }
                if (RequestState != __ModelData.s) {
                    LogWarning($"Invalid request state from request, user_id: { UserId } ");
                    return Problem(410, "Request have expired.");
                }
                if (FailedTimes != default && FailedTimes >= NumberOfTimesAllowFailure) {
                    LogWarning($"The request has expired because the times of failed exceeded max times allowed, max: { NumberOfTimesAllowFailure }");
                    return Problem(410, "Request have expired.");
                }
                #endregion

                Error = await __SocialUserManagement.ChangePassword(User.Id, __ModelData.new_password);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NO_CHANGE_DETECTED) {
                        LogWarning($"No change detected when change password, user_name: { User.UserName }");
                        return Problem(400, "No change detected.");
                    }
                    var ErrHandle = await __SocialUserManagement.HandleNewPasswordFailed(User.Id);
                    if (ErrHandle != ErrorCodes.NO_ERROR) {
                        LogError($"HandleNewPasswordFailed Failed, ErrorCode: { ErrHandle }");
                    }
                    throw new Exception($"ChangePassword Failed, ErrorCode: { Error }");
                }

                Error = await __SocialUserManagement.HandleNewPasswordSuccessfully(User.Id);
                if (Error != ErrorCodes.NO_ERROR) {
                    LogError($"HandleNewPasswordSuccessfully Failed, ErrorCode: { Error }");
                }

                LogInformation($"New password successfully, user_name: { User.UserName }");
                return Ok(200, "OK", new JObject(){
                    { "user", User.GetPublicJsonObject() },
                });
            } catch (Exception e) {
                LogError($"Unexpected exception, message: { e.ToString() }");
                return Problem(500, "Internal Server Error.");
            }
        }
    }
}
