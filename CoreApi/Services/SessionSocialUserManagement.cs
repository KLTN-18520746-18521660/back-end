using CoreApi.Common;
using CoreApi.Common.Base;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi.Services
{
    public class SessionSocialUserManagement : BaseTransientService
    {
        public SessionSocialUserManagement(IServiceProvider _IServiceProvider) : base(_IServiceProvider)
        {
            __ServiceName = "SessionSocialUserManagement";
        }

        public async Task<(SessionSocialUser, ErrorCodes)> NewSession(Guid UserId, bool Remember, JObject Data)
        {
            SessionSocialUser session = new();
            session.UserId = UserId;
            session.Saved = Remember;
            session.Data = Data;
            await __DBContext.SessionSocialUsers.AddAsync(session);

            if (await __DBContext.SaveChangesAsync() > 0) {
                return (session, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.INTERNAL_SERVER_ERROR);
        }

        public async Task<(SessionSocialUser, ErrorCodes)> FindSession(string SessionToken)
        {
            var session = (await __DBContext.SessionSocialUsers
                            .Where(e => e.SessionToken == SessionToken)
                            .Include(e => e.User)
                            .FirstOrDefaultAsync());
            if (session != default) {
                return (session, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.NOT_FOUND);
        }

        public ErrorCodes IsNeedChangePassword(SocialUser User)
        {
            #region Get password policy
            var __BaseConfig                    = (BaseConfig)__ServiceProvider.GetService(typeof(BaseConfig));
            var ExpiryTime                      = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.SOCIAL_PASSWORD_POLICY, SUB_CONFIG_KEY.EXPIRY_TIME)
                .Value;
            var RequiredChangeExpiredPassword   = __BaseConfig
                .GetConfigValue<bool>(CONFIG_KEY.SOCIAL_PASSWORD_POLICY, SUB_CONFIG_KEY.REQUIRED_CHANGE_EXPIRED_PASSWORD)
                .Value;
            #endregion

            #region Get value from user settings
            DateTime LastChangePassword = default;
            bool IsExpired              = false;
            var Now                     = DateTime.UtcNow;
            var NeedChangePassword      = false;
            var PasswordSetting         = User.Settings.ContainsKey("password")
                                            ? User.Settings.SelectToken("password").ToObject<JObject>()
                                            : new JObject();
            LastChangePassword = PasswordSetting.ContainsKey("last_change_password")
                                    ? PasswordSetting.Value<DateTime>("last_change_password")
                                    : User.CreatedTimestamp;
            IsExpired = PasswordSetting.ContainsKey("is_expired")
                                    ? PasswordSetting.Value<bool>("is_expired")
                                    : false;
            #endregion

            NeedChangePassword = IsExpired && RequiredChangeExpiredPassword;
            IsExpired = (Now - LastChangePassword.ToUniversalTime()).TotalDays > ExpiryTime;

            #region Update user value
            if (PasswordSetting.ContainsKey("last_change_password")) {
                PasswordSetting.SelectToken("last_change_password").Replace(LastChangePassword);
            } else {
                PasswordSetting.Add("last_change_password", LastChangePassword);
            }
            if (PasswordSetting.ContainsKey("is_expired")) {
                PasswordSetting.SelectToken("is_expired").Replace(IsExpired);
            } else {
                PasswordSetting.Add("is_expired", IsExpired);
            }
            if (User.Settings.ContainsKey("password")) {
                User.Settings.SelectToken("password").Replace(PasswordSetting);
            } else {
                User.Settings.Add("password", PasswordSetting);
            }
            #endregion
            return NeedChangePassword ? ErrorCodes.PASSWORD_IS_EXPIRED : ErrorCodes.NO_ERROR;
        }

        public async Task<(SessionSocialUser, ErrorCodes)> FindSessionForUse(string SessionToken,
                                                                             int ExpiryTime,
                                                                             int ExtensionTime)
        {
            var (Session, Error) = await FindSession(SessionToken);
            if (Error != ErrorCodes.NO_ERROR) {
                return (Session, Error);
            }

            if (Session.User.Status.Type == StatusType.Blocked) {
                return (default, ErrorCodes.USER_HAVE_BEEN_LOCKED);
            }
            // Clear expired session
            Error = await ClearExpiredSession(Session.User.GetExpiredSessions(ExpiryTime));
            if (Error != ErrorCodes.NO_ERROR) {
                return (default, ErrorCodes.INTERNAL_SERVER_ERROR);
            }
            // Find session again if session if expired
            (Session, Error) = await FindSession(SessionToken);
            if (Error == ErrorCodes.NO_ERROR) {
                Error = await ExtensionSession(SessionToken, ExtensionTime);
                if (Error == ErrorCodes.NO_ERROR) {
                    Error = IsNeedChangePassword(Session.User);
                    Session.User.LastAccessTimestamp = Session.LastInteractionTime;
                    if (await __DBContext.SaveChangesAsync() > 0) {
                        return (Session, Error);
                    }
                }
            } else if (Error == ErrorCodes.NOT_FOUND) {
                return (default, ErrorCodes.SESSION_HAS_EXPIRED);
            }
            return (default, ErrorCodes.INTERNAL_SERVER_ERROR);
        }

        public async Task<ErrorCodes> RemoveSession(string SessionToken)
        {
            ErrorCodes error = ErrorCodes.NO_ERROR;
            SessionSocialUser session = default;
            (session, error) = await FindSession(SessionToken);
            if (error != ErrorCodes.NO_ERROR) {
                return error;
            }
            __DBContext.SessionSocialUsers.Remove(session);
            if (await __DBContext.SaveChangesAsync() > 0) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }

        public async Task<ErrorCodes> RemoveAllSession(Guid UserId, string[] IgnoreSessions)
        {
            var Sessions = await __DBContext.SessionSocialUsers
                .Where(e => e.UserId == UserId && !IgnoreSessions.Contains(e.SessionToken))
                .ToArrayAsync();
            if (Sessions.Length == 0) {
                return ErrorCodes.NO_CHANGE_DETECTED;
            }
            __DBContext.SessionSocialUsers.RemoveRange(Sessions);
            if (await __DBContext.SaveChangesAsync() > 0) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }

        public async Task<ErrorCodes> ClearExpiredSession(List<string> ExpiredSessions)
        {
            if (ExpiredSessions.Count == 0) {
                return ErrorCodes.NO_ERROR;
            }
            var sessions = await __DBContext.SessionSocialUsers
                .Where(e => ExpiredSessions.Contains(e.SessionToken)).ToListAsync();
            __DBContext.SessionSocialUsers.RemoveRange(sessions);
            if (await __DBContext.SaveChangesAsync() > 0) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }

        public async Task<ErrorCodes> ExtensionSession(string SessionToken, int ExtensionTime)
        {
            var now = DateTime.UtcNow.AddMinutes(ExtensionTime);
            ErrorCodes error = ErrorCodes.NO_ERROR;
            SessionSocialUser session = default;
            (session, error) = await FindSession(SessionToken);
            if (error != ErrorCodes.NO_ERROR) {
                return error;
            }
            session.LastInteractionTime = now.ToUniversalTime();
            if (await __DBContext.SaveChangesAsync() > 0) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }

        public async Task<(List<SessionSocialUser>, ErrorCodes)> GetAllSessionOfUser(Guid UserId)
        {
            var Sessions = await __DBContext.SessionSocialUsers
                .Where(e => e.UserId == UserId)
                .OrderByDescending(e => e.LastInteractionTime)
                .ToListAsync();
            if (Sessions.Count > 0) {
                return (Sessions, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.NOT_FOUND);
        }
    }
}