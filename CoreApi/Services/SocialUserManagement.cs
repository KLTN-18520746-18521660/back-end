using Serilog;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using CoreApi.Common;

namespace CoreApi.Services
{
    public class SocialUserManagement : BaseService
    {
        protected DBContext __DBContext;
        public SocialUserManagement() : base() {
            __DBContext = new DBContext();
            __ServiceName = "SocialUserManagement";
        }

        #region Find user, handle user login
        public bool FindUser(string UserName, bool isEmail, out SocialUser User, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            List<SocialUser> users;
            if (isEmail) {
                users = __DBContext.SocialUsers
                        .Where<SocialUser>(e => e.Email == UserName
                            && e.StatusStr != BaseStatus.StatusToString(SocialUserStatus.Deleted, EntityStatus.SocialUserStatus))
                        .Include(e => e.SocialUserRoleOfUsers)
                        .ToList<SocialUser>();
            } else {
                users = __DBContext.SocialUsers
                        .Where<SocialUser>(e => e.UserName == UserName
                            && e.StatusStr != BaseStatus.StatusToString(SocialUserStatus.Deleted, EntityStatus.SocialUserStatus))
                        .ToList<SocialUser>();
            }
            if (users.Count > 0) {
                User = users.First();
                return true;
            }
            Error = ErrorCodes.NOT_FOUND;
            User = null;
            return false;
        }

        public bool FindUserIgnoreStatus(string UserName, bool isEmail, out SocialUser User, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            List<SocialUser> users;
            if (isEmail) {
                users = __DBContext.SocialUsers
                        .Where<SocialUser>(e => e.Email == UserName)
                        .Include(e => e.SocialUserRoleOfUsers)
                        .ToList<SocialUser>();
            } else {
                users = __DBContext.SocialUsers
                        .Where<SocialUser>(e => e.UserName == UserName)
                        .Include(e => e.SocialUserRoleOfUsers)
                        .ToList<SocialUser>();
            }
            if (users.Count > 0) {
                User = users.First();
                return true;
            }
            Error = ErrorCodes.NOT_FOUND;
            User = null;
            return false;
        }

        public bool FindUserById(Guid Id, out SocialUser User, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            List<SocialUser> users;
            users = __DBContext.SocialUsers
                    .Where<SocialUser>(e => e.Id == Id)
                    .Include(e => e.SocialUserRoleOfUsers)
                    .ToList<SocialUser>();
            if (users.Count > 0) {
                User = users.First();
                return true;
            }
            Error = ErrorCodes.NOT_FOUND;
            User = null;
            return false;
        }

        public bool HandleLoginFail(SocialUser User, int LockTime, int NumberOfTimesAllowLoginFailure, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            JObject config;
            if (!User.Settings.ContainsKey("__login_config")) {
                config = new JObject{
                    { "number", 1 },
                    { "last_login", DateTimeOffset.Now.ToUnixTimeSeconds()}
                };
            } else {
                try {
                    config = User.Settings.Value<JObject>("__login_config");
                    int numberLoginFailure = config.Value<int>("number");
                    long lastLoginFailure = config.Value<long>("last_login");

                    if (User.Status == SocialUserStatus.Blocked) {
                        long currentUnixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                        if (currentUnixTime - lastLoginFailure > LockTime * 60) {
                            User.Status = SocialUserStatus.Activated;
                            numberLoginFailure = 1;
                            lastLoginFailure = DateTimeOffset.Now.ToUnixTimeSeconds();
                        }
                    } else {
                        numberLoginFailure++;
                        lastLoginFailure = DateTimeOffset.Now.ToUnixTimeSeconds();
                    }

                    if (numberLoginFailure >= NumberOfTimesAllowLoginFailure) {
                        User.Status = (int) SocialUserStatus.Blocked;
                    }

                    config["number"] = numberLoginFailure;
                    config["last_login"] = lastLoginFailure;
                } catch (Exception) {
                    config = new JObject(){
                        { "number", 1 },
                        { "last_login", DateTimeOffset.Now.ToUnixTimeSeconds()}
                    };
                }
            }

            User.Settings["__login_config"] = config;
            if (__DBContext.SaveChanges() > 0) {
                return true;
            }
            Error = ErrorCodes.INTERNAL_SERVER_ERROR;
            return false;
        }

        public bool HandleLoginSuccess(SocialUser User, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            User.LastAccessTimestamp = DateTime.UtcNow;
            User.Settings.Remove("__login_config");
            if (__DBContext.SaveChanges() > 0) {
                return true;
            }
            Error = ErrorCodes.INTERNAL_SERVER_ERROR;
            return false;
        }
        #endregion

        #region Permission
        public bool HaveReadPermission(SocialUser User, string Right)
        {
            var rights = User.Rights;
            if (rights.ContainsKey(Right)) {
                var right = rights[Right];
                if (right["read"] != null &&
                    ((bool)right["read"]) == true) {
                    return true;
                }
            }
            return false;
        }

        public bool HaveFullPermission(SocialUser User, string Right)
        {
            var rights = User.Rights;
            if (rights.ContainsKey(Right)) {
                var right = rights[Right];
                if (right["read"] != null && right["write"] != null &&
                    ((bool)right["read"]) == true && ((bool)right["write"]) == true) {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Add user
        public bool AddNewUser(SocialUser NewUser, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            __DBContext.SocialUsers.Add(NewUser);
            #region Add default role
            SocialUserRoleOfUser defaultRole = new SocialUserRoleOfUser();
            defaultRole.UserId = NewUser.Id;
            defaultRole.RoleId = SocialUserRole.GetDefaultRoleId();
            defaultRole.Role = __DBContext.SocialUserRoles.Where(e => e.Id == defaultRole.RoleId).ToList().First();
            NewUser.SocialUserRoleOfUsers.Add(defaultRole);
            #endregion

            if (__DBContext.SaveChanges() > 0) {
                #region  [SOCIAL] Write audit log
                SocialAuditLog log = new SocialAuditLog();
                log.Table = NewUser.GetModelName();
                log.TableKey = NewUser.Id.ToString();
                log.Action = LOG_ACTIONS.CREATE;
                log.UserId = NewUser.Id;
                log.OldValue = new LogValue();
                log.NewValue = new LogValue(NewUser.GetJsonObject());

                __DBContext.SocialAuditLogs.Add(log);
                __DBContext.SaveChanges();
                #endregion
                return true;
            }
            Error = ErrorCodes.INTERNAL_SERVER_ERROR;
            return false;
        }
        #endregion
    }
}