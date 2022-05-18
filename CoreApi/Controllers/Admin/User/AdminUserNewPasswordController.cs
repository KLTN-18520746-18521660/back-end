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

namespace CoreApi.Controllers.Admin.User
{
    [ApiController]
    [Route("/api/admin/user/newpassword")]
    public class AdminUserNewPasswordController : BaseController
    {
        public AdminUserNewPasswordController(BaseConfig _BaseConfig) : base(_BaseConfig, true)
        {
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(StatusCode400Examples))]
        [ProducesResponseType(StatusCodes.Status410Gone, Type = typeof(StatusCode410Examples))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(StatusCode500Examples))]
        public async Task<IActionResult> NewPassword([FromServices] AdminUserManagement    __AdminUserManagement,
                                                     [FromBody] NewPasswordModel            __ModelData)
        {
            #region Init Handler
            SetRunningFunction();
            SetTraceIdForServices(__AdminUserManagement);
            #endregion
            try {
                #region Get config values
                var RequestExpiryTime                   = GetConfigValue<int>(CONFIG_KEY.FORGOT_PASSWORD_CONFIG, SUB_CONFIG_KEY.EXPIRY_TIME);
                var NumberOfTimesAllowFailure           = GetConfigValue<int>(CONFIG_KEY.FORGOT_PASSWORD_CONFIG,
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

                var UserId  = CommonValidate.IsValidUUID(StringDecryptor.Decrypt(Uri.UnescapeDataString(__ModelData.i)));
                var Date    = CommonValidate.IsValidDateTime(StringDecryptor.Decrypt(Uri.UnescapeDataString(__ModelData.d)),
                                                             COMMON_DEFINE.DATE_TIME_FORMAT);
                if (UserId == default || Date == default || __ModelData.s == string.Empty || __ModelData.s.Length != 8) {
                    return Problem(400, "Invalid params.");
                }
                if ((DateTime.UtcNow - Date.ToUniversalTime()).TotalMinutes > RequestExpiryTime) {
                    return Problem(410, "Request have expired.");
                }
                #endregion

                #region Find User
                var (User, Error) = await __AdminUserManagement.FindUserById(UserId);
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
                var FogotPasswordSetting    = User.Settings.Value<JObject>("forgot_password");
                var RequestState            = FogotPasswordSetting != default ? FogotPasswordSetting.Value<string>("state") : default;
                var FailedTimes             = FogotPasswordSetting != default ? FogotPasswordSetting.Value<int>("failed_times") : default;
                var SendDate                = FogotPasswordSetting != default ? FogotPasswordSetting.Value<DateTime>("send_date") : default;
                var IsSending               = FogotPasswordSetting != default ? FogotPasswordSetting.Value<bool>("is_sending") : default;
                var SendSuccess             = FogotPasswordSetting != default ? FogotPasswordSetting.Value<bool>("send_success") : default;

                if (SendDate != Date) {
                    return Problem(410, "Request have expired.");
                }
                if (FogotPasswordSetting == default || IsSending == true || SendSuccess == false) {
                    return Problem(410, "Request have expired.");
                }
                if (RequestState == default || RequestState == string.Empty || RequestState.Length != 8) {
                    return Problem(410, "Request have expired.");
                }
                if (RequestState != __ModelData.s) {
                    return Problem(410, "Request have expired.");
                }
                if (FailedTimes != default && FailedTimes >= NumberOfTimesAllowFailure) {
                    return Problem(410, "Request have expired.");
                }
                #endregion

                Error = await __AdminUserManagement.ChangePassword(User.Id, __ModelData.new_password, User.Id);
                if (Error != ErrorCodes.NO_ERROR) {
                    if (Error == ErrorCodes.NO_CHANGE_DETECTED) {
                        return Problem(400, "No change detected.");
                    }
                    var ErrHandle = await __AdminUserManagement.HandleNewPasswordFailed(User.Id);
                    if (ErrHandle != ErrorCodes.NO_ERROR) {
                        WriteLog(LOG_LEVEL.ERROR, false, $"HandleNewPasswordFailed Failed, ErrorCode: { ErrHandle }");
                    }
                    throw new Exception($"ChangePassword Failed, ErrorCode: { Error }");
                }

                Error = await __AdminUserManagement.HandleNewPasswordSuccessfully(User.Id);
                if (Error != ErrorCodes.NO_ERROR) {
                    WriteLog(LOG_LEVEL.ERROR, false, $"HandleNewPasswordSuccessfully Failed, ErrorCode: { Error }");
                }

                return Ok(200, "OK", new JObject(){
                    { "user", User.GetPublicJsonObject() },
                });
            } catch (Exception e) {
                AddLogParam("exception_message", e.ToString());
                return Problem(500, "Internal Server Error", default, LOG_LEVEL.ERROR);
            }
        }
    }
}
