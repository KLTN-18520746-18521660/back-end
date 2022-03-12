using Serilog;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using System;
using CoreApi.Common;

namespace CoreApi.Services
{
    public class SessionAdminUserManagement : BaseService
    {
        protected DBContext __DBContext;
        public SessionAdminUserManagement() : base()
        {
            __DBContext = new DBContext();
        }

        public bool NewSession(Guid UserId, bool Remember, JObject Data, out SessionAdminUser Session, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            Session = new();
            Session.UserId = UserId;
            Session.Saved = Remember;
            Session.Data = Data;
            __DBContext.SessionAdminUsers.Add(Session);
            if (__DBContext.SaveChanges() > 0) {
                return true;
            }
            Error = ErrorCodes.INTERNAL_SERVER_ERROR;
            return false;
        }

        public bool FindSession(string SessionToken, out SessionAdminUser Session, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            var sessions = __DBContext.SessionAdminUsers
                            .Where(e => e.SessionToken == SessionToken)
                            .ToList();
            if (sessions.Count > 0) {
                Session = sessions.First();
                return true;
            }
            Error = ErrorCodes.NOT_FOUND;
            Session = null;
            return false;
        }

        public bool FindSessionForUse(string SessionToken,
                                    int ExpiryTime,
                                    int ExtensionTime,
                                    out SessionAdminUser Session,
                                    out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            if (FindSession(SessionToken, out Session, out Error)) {
                if (!ClearExpiredSession(Session.User, ExpiryTime, out Error)) {
                    Error = ErrorCodes.INTERNAL_SERVER_ERROR;
                    return false;
                }
                if (FindSession(SessionToken, out Session, out Error)) {
                    if (ExtensionSession(SessionToken, ExtensionTime, out Error)) {
                        return true;
                    }
                    Error = ErrorCodes.INTERNAL_SERVER_ERROR;
                    return false;
                }
                Error = ErrorCodes.SESSION_HAS_EXPIRED;
                return false;
            }
            Error = ErrorCodes.NOT_FOUND;
            Session = null;
            return false;
        }

        public bool RemoveSession(SessionAdminUser Session, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            __DBContext.SessionAdminUsers.Remove(Session);
            if (__DBContext.SaveChanges() > 0) {
                return true;
            }
            Error = ErrorCodes.INTERNAL_SERVER_ERROR;
            return false;
        }

        public bool ClearExpiredSession(AdminUser User, int ExpiryTime, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            var expiredSessions = User.GetExpiredSessions(ExpiryTime);
            if (expiredSessions.Count == 0) {
                return true;
            }
            var sessions = __DBContext.SessionAdminUsers.Where(e => expiredSessions.Contains(e.SessionToken));
            __DBContext.SessionAdminUsers.RemoveRange(sessions);
            if (__DBContext.SaveChanges() > 0) {
                return true;
            }
            Error = ErrorCodes.INTERNAL_SERVER_ERROR;
            return false;
        }

        public bool ExtensionSession(string SessionToken, int ExtensionTime, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            var now = DateTime.UtcNow.AddMinutes(ExtensionTime);
            var session = __DBContext.SessionAdminUsers.Where<SessionAdminUser>(e => e.SessionToken == SessionToken).ToList().First();
            session.LastInteractionTime = now;
            if (__DBContext.SaveChanges() > 0) {
                return true;
            }
            Error = ErrorCodes.INTERNAL_SERVER_ERROR;
            return false;
        }

        public bool GetAllSessionOfUser(Guid UserId, out List<SessionAdminUser> Sessions, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            Sessions = __DBContext.SessionAdminUsers
                .Where(e => e.UserId == UserId).ToList();
            if (Sessions.Count > 0) {
                return true;
            }
            Error = ErrorCodes.INTERNAL_SERVER_ERROR;
            return false;
        }
    }
}