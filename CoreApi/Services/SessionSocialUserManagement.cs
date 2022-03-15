using Serilog;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using System;
using System.Threading.Tasks;
using CoreApi.Common;

namespace CoreApi.Services
{
    public class SessionSocialUserManagement : BaseService
    {
        public SessionSocialUserManagement() : base()
        {
            __ServiceName = "SessionSocialUserManagement";
        }

        public async Task<(SessionSocialUser, ErrorCodes)> NewSession(Guid UserId, bool Remember, JObject Data)
        {
            SessionSocialUser session = new();
            session.UserId = UserId;
            session.Saved = Remember;
            session.Data = Data;
            __DBContext.SessionSocialUsers.Add(session);

            if (await __DBContext.SaveChangesAsync() > 0) {
                return (session, ErrorCodes.NO_ERROR);
            }
            return (null, ErrorCodes.INTERNAL_SERVER_ERROR);
        }

        public async Task<(SessionSocialUser, ErrorCodes)> FindSession(string SessionToken)
        {
            var session = (await __DBContext.SessionSocialUsers
                            .Where(e => e.SessionToken == SessionToken)
                            .Include(e => e.User)
                            .ToListAsync())
                            .DefaultIfEmpty(null)
                            .FirstOrDefault();
            if (session != null) {
                return (session, ErrorCodes.NO_ERROR);
            }
            return (null, ErrorCodes.NOT_FOUND);
        }

        public async Task<(SessionSocialUser, ErrorCodes)> FindSessionForUse(string SessionToken,
                                    int ExpiryTime,
                                    int ExtensionTime)
        {
            ErrorCodes error = ErrorCodes.NO_ERROR;
            SessionSocialUser session = null;
            (session, error) = await FindSession(SessionToken);
            if (error != ErrorCodes.NO_ERROR) {
                return (session, error);
            }

            if (session.User.Status == SocialUserStatus.Blocked) {
                return (null, ErrorCodes.USER_HAVE_BEEN_LOCKED);
            }
            // Clear expired session
            error = await ClearExpiredSession(session.User.GetExpiredSessions(ExpiryTime));
            if (error != ErrorCodes.NO_ERROR) {
                return (null, ErrorCodes.INTERNAL_SERVER_ERROR);
            }
            // Find session again if session if expired
            (session, error) = await FindSession(SessionToken);
            if (error == ErrorCodes.NO_ERROR) {
                error = await ExtensionSession(SessionToken, ExtensionTime);
                if (error == ErrorCodes.NO_ERROR) {
                    session.User.LastAccessTimestamp = session.LastInteractionTime;
                    if (await __DBContext.SaveChangesAsync() > 0) {
                        return (session, ErrorCodes.NO_ERROR);
                    }
                }
            } else if (error == ErrorCodes.NOT_FOUND) {
                return (null, ErrorCodes.SESSION_HAS_EXPIRED);
            }
            return (null, ErrorCodes.INTERNAL_SERVER_ERROR);
        }

        public async Task<ErrorCodes> RemoveSession(string SessionToken)
        {
            ErrorCodes error = ErrorCodes.NO_ERROR;
            SessionSocialUser session = null;
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
            SessionSocialUser session = null;
            (session, error) = await FindSession(SessionToken);
            if (error != ErrorCodes.NO_ERROR) {
                return error;
            }
            session.LastInteractionTime = now;
            if (await __DBContext.SaveChangesAsync() > 0) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }

        public async Task<(List<SessionSocialUser>, ErrorCodes)> GetAllSessionOfUser(Guid UserId)
        {
            var Sessions = await __DBContext.SessionSocialUsers
                .Where(e => e.UserId == UserId).ToListAsync();
            if (Sessions.Count > 0) {
                return (Sessions, ErrorCodes.NO_ERROR);
            }
            return (null, ErrorCodes.NOT_FOUND);
        }
    }
}