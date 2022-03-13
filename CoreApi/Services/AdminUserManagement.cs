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
    public class AdminUserManagement : BaseService
    {
        protected DBContext __DBContext;
        public AdminUserManagement() : base() {
            __DBContext = new DBContext();
            __ServiceName = "AdminUserManagement";
        }

        #region Find user, handle user login
        public bool FindUser(string UserName, bool isEmail, out AdminUser User, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            List<AdminUser> users;
            if (isEmail) {
                users = __DBContext.AdminUsers
                        .Where<AdminUser>(e => e.Email == UserName
                            && e.StatusStr != BaseStatus.StatusToString(AdminUserStatus.Deleted, EntityStatus.AdminUserStatus))
                        .Include(e => e.AdminUserRoleOfUsers)
                        .ToList<AdminUser>();
            } else {
                users = __DBContext.AdminUsers
                        .Where<AdminUser>(e => e.UserName == UserName
                            && e.StatusStr != BaseStatus.StatusToString(AdminUserStatus.Deleted, EntityStatus.AdminUserStatus))
                        .Include(e => e.AdminUserRoleOfUsers)
                        .ToList<AdminUser>();
            }
            if (users.Count > 0) {
                User = users.First();
                return true;
            }
            Error = ErrorCodes.NOT_FOUND;
            User = null;
            return false;
        }

        public bool FindUserIgnoreStatus(string UserName, bool isEmail, out AdminUser User, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            List<AdminUser> users;
            if (isEmail) {
                users = __DBContext.AdminUsers
                        .Where<AdminUser>(e => e.Email == UserName)
                        .Include(e => e.AdminUserRoleOfUsers)
                        .ToList<AdminUser>();
            } else {
                users = __DBContext.AdminUsers
                        .Where<AdminUser>(e => e.UserName == UserName)
                        .Include(e => e.AdminUserRoleOfUsers)
                        .ToList<AdminUser>();
            }
            if (users.Count > 0) {
                User = users.First();
                return true;
            }
            Error = ErrorCodes.NOT_FOUND;
            User = null;
            return false;
        }

        public bool FindUserById(Guid Id, out AdminUser User, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            List<AdminUser> users;
            users = __DBContext.AdminUsers
                    .Where<AdminUser>(e => e.Id == Id)
                    .Include(e => e.AdminUserRoleOfUsers)
                    .ToList<AdminUser>();
            if (users.Count > 0) {
                User = users.First();
                return true;
            }
            Error = ErrorCodes.NOT_FOUND;
            User = null;
            return false;
        }

        public bool HandleLoginFail(AdminUser User, int LockTime, int NumberOfTimesAllowLoginFailure, out ErrorCodes Error)
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

                    if (User.Status == AdminUserStatus.Blocked) {
                        long currentUnixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                        if (currentUnixTime - lastLoginFailure > LockTime * 60) {
                            User.Status = AdminUserStatus.Activated;
                            numberLoginFailure = 1;
                            lastLoginFailure = DateTimeOffset.Now.ToUnixTimeSeconds();
                        }
                    } else {
                        numberLoginFailure++;
                        lastLoginFailure = DateTimeOffset.Now.ToUnixTimeSeconds();
                    }

                    if (numberLoginFailure >= NumberOfTimesAllowLoginFailure) {
                        if (User.UserName != AdminUser.GetAdminUserName()) {
                            User.Status = (int) AdminUserStatus.Blocked;
                        }
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

        public bool HandleLoginSuccess(AdminUser User, out ErrorCodes Error)
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
        public bool HaveReadPermission(AdminUser User, string Right)
        {
            var user = __DBContext.AdminUsers
                .Where(e => e.Id == User.Id)
                .Include(e => e.AdminUserRoleOfUsers)
                .First();
            var rights = user.Rights;
            if (rights.ContainsKey(Right)) {
                var right = rights[Right];
                if (right["read"] != null &&
                    ((bool)right["read"]) == true) {
                    return true;
                }
            }
            return false;
        }

        public bool HaveFullPermission(AdminUser User, string Right)
        {
            var user = __DBContext.AdminUsers
                .Where(e => e.Id == User.Id)
                .Include(e => e.AdminUserRoleOfUsers)
                .First();
            var rights = user.Rights;
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
        public bool AddNewUser(AdminUser User, AdminUser NewUser, out ErrorCodes Error)
        {
            Error = ErrorCodes.NO_ERROR;
            __DBContext.AdminUsers.Add(NewUser);
            if (__DBContext.SaveChanges() > 0) {
                #region [ADMIN] Write audit log
                AdminAuditLog log = new AdminAuditLog();
                log.Table = NewUser.GetModelName();
                log.TableKey = NewUser.Id.ToString();
                log.Action = LOG_ACTIONS.CREATE;
                log.UserId = User.Id;
                log.OldValue = new LogValue();
                log.NewValue = new LogValue(NewUser.GetJsonObject());

                __DBContext.AdminAuditLogs.Add(log);
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