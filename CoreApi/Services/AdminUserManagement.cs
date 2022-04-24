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
using System.Linq.Expressions;

namespace CoreApi.Services
{
    public class AdminUserManagement : BaseTransientService
    {
        private AdminAuditLogManagement __AdminAuditLogManagement;
        public AdminUserManagement(DBContext _DBContext,
                                   IServiceProvider _IServiceProvider,
                                   AdminAuditLogManagement _AdminAuditLogManagement)
            : base(_IServiceProvider)
        {
            __AdminAuditLogManagement = _AdminAuditLogManagement;
            __ServiceName = "AdminUserManagement";
        }
        public override void SetTraceId(string TraceId)
        {
            base.SetTraceId(TraceId);
            __AdminAuditLogManagement.SetTraceId(TraceId);
        }

        public async Task UpdateDefaultAdminRoleAsync()
        {
            var rds = AdminUserRoleDetail.GetDefaultData();
            foreach (var r in rds) {
                if (await __DBContext.AdminUserRoleDetails.CountAsync(e => 
                        e.RightId == r.RightId && e.RoleId == e.RoleId
                    ) == 0
                ) {
                    await __DBContext.AdminUserRoleDetails.AddAsync(
                        new AdminUserRoleDetail(){
                            RoleId = r.RoleId,
                            RightId = r.RightId,
                            Actions = r.Actions
                        }
                    );

                    if (await __DBContext.SaveChangesAsync() <= 0) {
                        throw new Exception("UpdateDefaultAdminRole failed.");
                    }
                }
            }
        }

        public void UpdateDefaultAdminRole()
        {
            var rds = AdminUserRoleDetail.GetDefaultData();
            foreach (var r in rds) {
                if (__DBContext.AdminUserRoleDetails.Count(e => 
                        e.RightId == r.RightId && e.RoleId == e.RoleId
                    ) == 0
                ) {
                    __DBContext.AdminUserRoleDetails.Add(
                        new AdminUserRoleDetail(){
                            RoleId = r.RoleId,
                            RightId = r.RightId,
                            Actions = r.Actions
                        }
                    );

                    if (__DBContext.SaveChanges() <= 0) {
                        throw new Exception("UpdateDefaultAdminRole failed.");
                    }
                }
            }
        }

        #region Find user, handle user login
        public async Task<(AdminUser, ErrorCodes)> FindUser(string UserName, bool isEmail)
        {
            AdminUser user;
            if (isEmail) {
                user = await __DBContext.AdminUsers
                        .Where<AdminUser>(e => e.Email == UserName
                            && e.StatusStr != BaseStatus.StatusToString(AdminUserStatus.Deleted, EntityStatus.AdminUserStatus))
                        .FirstOrDefaultAsync();
            } else {
                user = await __DBContext.AdminUsers
                        .Where<AdminUser>(e => e.UserName == UserName
                            && e.StatusStr != BaseStatus.StatusToString(AdminUserStatus.Deleted, EntityStatus.AdminUserStatus))
                        .FirstOrDefaultAsync();
            }
            if (user != default) {
                return (user, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.NOT_FOUND);
        }

        public async Task<(AdminUser, ErrorCodes)> FindUserIgnoreStatus(string UserName, bool isEmail)
        {
            AdminUser user;
            if (isEmail) {
                user = await __DBContext.AdminUsers
                        .Where(e => e.Email == UserName)
                        .FirstOrDefaultAsync();
            } else {
                user = await __DBContext.AdminUsers
                        .Where(e => e.UserName == UserName)
                        .FirstOrDefaultAsync();
            }
            if (user != default) {
                return (user, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.NOT_FOUND);
        }

        public async Task<(AdminUser, ErrorCodes)> FindUserById(Guid Id)
        {
            AdminUser user;
            user = await __DBContext.AdminUsers
                    .Where(e => e.Id == Id)
                    .FirstOrDefaultAsync();

            if (user != default) {
                return (user, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.NOT_FOUND);
        }

        // username_existed, email_existed, ERROR
        public async Task<(bool, bool, ErrorCodes)> IsUserExsiting(string UserName, string Email)
        {
            var count_email = (await __DBContext.AdminUsers
                    .CountAsync(e => e.Email == UserName
                        && e.StatusStr != BaseStatus.StatusToString(AdminUserStatus.Deleted, EntityStatus.AdminUserStatus)));
            var count_username = (await __DBContext.AdminUsers
                    .CountAsync(e => e.UserName == UserName
                        && e.StatusStr != BaseStatus.StatusToString(AdminUserStatus.Deleted, EntityStatus.AdminUserStatus)));
            return (count_username > 0, count_email > 0, ErrorCodes.NO_ERROR);
        }

        public async Task<ErrorCodes> HandleLoginFail(Guid UserId, int LockTime, int NumberOfTimesAllowLoginFailure)
        {
            #region Find user info
            var (User, Error) = await FindUserById(UserId);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

            JObject config;
            if (!User.Settings.ContainsKey("__login_config")) {
                config = new JObject{
                    { "number", 1 },
                    { "last_login", DateTimeOffset.Now.ToUnixTimeSeconds() }
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
                            User.Status = AdminUserStatus.Blocked;
                        }
                    }

                    config["number"] = numberLoginFailure;
                    config["last_login"] = lastLoginFailure;
                } catch (Exception) {
                    config = new JObject(){
                        { "number", 1 },
                        { "last_login", DateTimeOffset.Now.ToUnixTimeSeconds() }
                    };
                }
            }
            User.Settings["__login_config"] = config;

            if (await __DBContext.SaveChangesAsync() > 0) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }

        public async Task<ErrorCodes> HandleLoginSuccess(Guid UserId)
        {
            #region Find user info
            var (User, Error) = await FindUserById(UserId);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

            if (User.LastAccessTimestamp == DateTime.UtcNow && !User.Settings.ContainsKey("__login_config")) {
                return ErrorCodes.NO_ERROR;
            }

            User.LastAccessTimestamp = DateTime.UtcNow;
            User.Settings.Remove("__login_config");

            if (await __DBContext.SaveChangesAsync() > 0) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        #endregion

        #region Permission
        public async Task<ErrorCodes> HaveReadPermission(Guid UserId, string Right)
        {
            #region Find user info
            var (User, Error) = await FindUserById(UserId);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

            return HaveReadPermission(User.Rights, Right);
        }

        public ErrorCodes HaveReadPermission(Dictionary<string, JObject> UserRights, string Right)
        {
            if (UserRights.ContainsKey(Right)) {
                var right = UserRights[Right];
                if (right["read"] != default &&
                    ((bool)right["read"]) == true) {
                    return ErrorCodes.NO_ERROR;
                }
            }
            return ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION;
        }

        public async Task<ErrorCodes> HaveFullPermission(Guid UserId, string Right)
        {
            #region Find user info
            var (User, Error) = await FindUserById(UserId);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

            return HaveFullPermission(User.Rights, Right);
        }

        public ErrorCodes HaveFullPermission(Dictionary<string, JObject> UserRights, string Right)
        {
            if (UserRights.ContainsKey(Right)) {
                var right = UserRights[Right];
                if (right["read"] != default && right["write"] != default &&
                    ((bool)right["read"]) == true && ((bool)right["write"]) == true) {
                    return ErrorCodes.NO_ERROR;
                }
            }
            return ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION;
        }
        #endregion

        #region Add user
        public async Task<ErrorCodes> AddNewUser(Guid UserId, AdminUser NewUser)
        {
            await __DBContext.AdminUsers.AddAsync(NewUser);
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [ADMIN] Write audit log
                (var user, var error) = await FindUserById(NewUser.Id);
                if (error == ErrorCodes.NO_ERROR) {
                    await __AdminAuditLogManagement.AddNewAuditLog(
                        user.GetModelName(),
                        user.Id.ToString(),
                        LOG_ACTIONS.CREATE,
                        UserId,
                        new JObject(),
                        user.GetJsonObject()
                    );
                } else {
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
                #endregion
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        #endregion
    }
}