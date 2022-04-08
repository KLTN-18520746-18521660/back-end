using Serilog;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using System;
using CoreApi.Common;
using System.Threading.Tasks;

namespace CoreApi.Services
{
    public class SessionAdminUserManagement : BaseService
    {
        public SessionAdminUserManagement(DBContext _DBContext,
                                          IServiceProvider _IServiceProvider)
            : base(_DBContext, _IServiceProvider)
        {
            __ServiceName = "SessionAdminUserManagement";
        }

        public async Task<(SessionAdminUser, ErrorCodes)> NewSession(Guid UserId, bool Remember, JObject Data)
        {
            SessionAdminUser session = new();
            session.UserId = UserId;
            session.Saved = Remember;
            session.Data = Data;
            await __DBContext.SessionAdminUsers.AddAsync(session);

            if (await __DBContext.SaveChangesAsync() > 0) {
                return (session, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.INTERNAL_SERVER_ERROR);
        }

        public async Task<(SessionAdminUser, ErrorCodes)> FindSession(string SessionToken)
        {
            var session = (await __DBContext.SessionAdminUsers
                            .Where(e => e.SessionToken == SessionToken)
                            .Include(e => e.User)
                            .FirstOrDefaultAsync());
            if (session != default) {
                return (session, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.NOT_FOUND);
        }

        public async Task<(SessionAdminUser, ErrorCodes)> FindSessionForUse(string SessionToken,
                                    int ExpiryTime,
                                    int ExtensionTime)
        {
            ErrorCodes error = ErrorCodes.NO_ERROR;
            SessionAdminUser session = default;
            (session, error) = await FindSession(SessionToken);
            if (error != ErrorCodes.NO_ERROR) {
                return (session, error);
            }

            if (session.User.Status == AdminUserStatus.Blocked) {
                return (default, ErrorCodes.USER_HAVE_BEEN_LOCKED);
            }
            // Clear expired session
            error = await ClearExpiredSession(session.User.GetExpiredSessions(ExpiryTime));
            if (error != ErrorCodes.NO_ERROR) {
                return (default, ErrorCodes.INTERNAL_SERVER_ERROR);
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
                return (default, ErrorCodes.SESSION_HAS_EXPIRED);
            }
            return (default, ErrorCodes.INTERNAL_SERVER_ERROR);
        }

        public async Task<ErrorCodes> RemoveSession(string SessionToken)
        {
            ErrorCodes error = ErrorCodes.NO_ERROR;
            SessionAdminUser session = default;
            (session, error) = await FindSession(SessionToken);
            if (error != ErrorCodes.NO_ERROR) {
                return error;
            }
            __DBContext.SessionAdminUsers.Remove(session);
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
            var sessions = await __DBContext.SessionAdminUsers
                .Where(e => ExpiredSessions.Contains(e.SessionToken)).ToListAsync();
            __DBContext.SessionAdminUsers.RemoveRange(sessions);
            if (await __DBContext.SaveChangesAsync() > 0) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }

        public async Task<ErrorCodes> ExtensionSession(string SessionToken, int ExtensionTime)
        {
            var now = DateTime.UtcNow.AddMinutes(ExtensionTime);
            ErrorCodes error = ErrorCodes.NO_ERROR;
            SessionAdminUser session = default;
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

        public async Task<(List<SessionAdminUser>, ErrorCodes)> GetAllSessionOfUser(Guid UserId)
        {
            var Sessions = await __DBContext.SessionAdminUsers
                .Where(e => e.UserId == UserId).ToListAsync();
            if (Sessions.Count > 0) {
                return (Sessions, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.NOT_FOUND);
        }
    }
}