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
using Common;

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
                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted))
                        .FirstOrDefaultAsync();
            } else {
                user = await __DBContext.AdminUsers
                        .Where<AdminUser>(e => e.UserName == UserName
                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted))
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
                        && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted)));
            var count_username = (await __DBContext.AdminUsers
                    .CountAsync(e => e.UserName == UserName
                        && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted)));
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

                    if (User.Status.Type == StatusType.Blocked) {
                        long currentUnixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                        if (currentUnixTime - lastLoginFailure > LockTime * 60) {
                            User.Status.ChangeStatus(StatusType.Activated);
                            numberLoginFailure = 1;
                            lastLoginFailure = DateTimeOffset.Now.ToUnixTimeSeconds();
                        }
                    } else {
                        numberLoginFailure++;
                        lastLoginFailure = DateTimeOffset.Now.ToUnixTimeSeconds();
                    }

                    if (numberLoginFailure >= NumberOfTimesAllowLoginFailure) {
                        if (User.UserName != AdminUser.GetAdminUserName()) {
                            User.Status.ChangeStatus(StatusType.Blocked);
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

        #region CURD user
        public string ValidatePasswordWithPolicy(string Password)
        {
            #region Get password policy
            var __BaseConfig                    = (BaseConfig)__ServiceProvider.GetService(typeof(BaseConfig));
            var MinLen                          = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.ADMIN_PASSWORD_POLICY, SUB_CONFIG_KEY.MIN_LEN)
                .Value;
            var MaxLen                          = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.ADMIN_PASSWORD_POLICY, SUB_CONFIG_KEY.MAX_LEN)
                .Value;
            var MinUpperChar                    = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.ADMIN_PASSWORD_POLICY, SUB_CONFIG_KEY.MIN_UPPER_CHAR)
                .Value;
            var MinLowerChar                    = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.ADMIN_PASSWORD_POLICY, SUB_CONFIG_KEY.MIN_LOWER_CHAR)
                .Value;
            var MinNumberChar                   = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.ADMIN_PASSWORD_POLICY, SUB_CONFIG_KEY.MIN_NUMBER_CHAR)
                .Value;
            var MinSpecialChar                  = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.ADMIN_PASSWORD_POLICY, SUB_CONFIG_KEY.MIN_SPECIAL_CHAR)
                .Value;
            #endregion

            return CommonValidate.ValidatePassword(Password, MinLen, MaxLen, MinUpperChar, MinLowerChar, MinNumberChar, MinSpecialChar);
        }
        public async Task<ErrorCodes> ChangePassword(Guid AdminUserId, string NewPassword, Guid AdminUserIdAction)
        {
            var (User, Error) = await FindUserById(AdminUserId);
            var (UserAction, TmpError) = await FindUserById(AdminUserIdAction);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            if (TmpError != ErrorCodes.NO_ERROR) {
                return Error;
            }
            var OldUser = Utils.DeepClone(User.GetJsonObjectForLog());

            if (User.Password == PasswordEncryptor.EncryptPassword(NewPassword, User.Salt)) {
                return ErrorCodes.NO_CHANGE_DETECTED;
            }
            #region Modify password
            User.Password = NewPassword;
            if (!User.Settings.ContainsKey("password")) {
                User.Settings.Add("password", new JObject(){
                    "last_change_password", DateTime.UtcNow
                });
            } else {
                var PasswordSetting = User.Settings.ContainsKey("password")
                                        ? User.Settings.SelectToken("password").ToObject<JObject>()
                                        : new JObject();
                if (PasswordSetting.ContainsKey("last_change_password")) {
                    PasswordSetting.SelectToken("last_change_password").Replace(DateTime.UtcNow);
                } else {
                    PasswordSetting.Add("last_change_password", DateTime.UtcNow);
                }
                if (User.Settings.ContainsKey("password")) {
                    User.Settings.SelectToken("password").Replace(PasswordSetting);
                } else {
                    User.Settings.Add("password", PasswordSetting);
                }
            }
            #endregion

            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [SOCIAL] Write admin audit log
                (User, Error) = await FindUserById(User.Id);
                var (OldVal, NewVal) = Utils.GetDataChanges(OldUser, User.GetJsonObjectForLog());
                if (Error == ErrorCodes.NO_ERROR) {
                    await __AdminAuditLogManagement.AddNewAuditLog(
                        User.GetModelName(),
                        User.Id.ToString(),
                        LOG_ACTIONS.MODIFY,
                        AdminUserIdAction,
                        OldVal,
                        NewVal
                    );
                } else {
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
                #endregion
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
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

        #region Forgot password
        public async Task<ErrorCodes> HandleNewPasswordSuccessfully(Guid Id)
        {
            #region Find user info
            var (User, Error) = await FindUserById(Id);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

            var OldUser = Utils.DeepClone(User.GetJsonObjectForLog());
            if (User.Status.Type == StatusType.Deleted) {
                return ErrorCodes.DELETED;
            } else if (User.Status.Type == StatusType.Blocked) {
                return ErrorCodes.USER_HAVE_BEEN_LOCKED;
            }

            #region Update user info
            if (!User.Settings.ContainsKey("forgot_password")) {
                return ErrorCodes.INTERNAL_SERVER_ERROR;
            }
            User.Settings.Remove("forgot_password");
            #endregion

            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [SOCIAL] Write user audit log
                (User, Error) = await FindUserById(Id);
                var (OldVal, NewVal) = Utils.GetDataChanges(OldUser, User.GetJsonObjectForLog());
                if (Error == ErrorCodes.NO_ERROR) {
                    await __AdminAuditLogManagement.AddNewAuditLog(
                        User.GetModelName(),
                        User.Id.ToString(),
                        LOG_ACTIONS.MODIFY,
                        User.Id,
                        OldVal,
                        NewVal
                    );
                } else {
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
                #endregion
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        public async Task<ErrorCodes> HandleNewPasswordFailed(Guid Id)
        {
            #region Find user info
            var (User, Error) = await FindUserById(Id);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

            if (User.Status.Type == StatusType.Deleted) {
                return ErrorCodes.DELETED;
            } else if (User.Status.Type == StatusType.Blocked) {
                return ErrorCodes.USER_HAVE_BEEN_LOCKED;
            }

            #region Update user info
            var ForgotPassword = User.Settings.Value<JObject>("forgot_password");
            if (ForgotPassword == default) {
                return ErrorCodes.INTERNAL_SERVER_ERROR;
            }
            var FailedTimes = ForgotPassword.Value<int>("failed_times") + 1;
            if (ForgotPassword.ContainsKey("failed_times")) {
                ForgotPassword.SelectToken("failed_times").Replace(FailedTimes);
            } else {
                ForgotPassword.Add("failed_times", FailedTimes);
            }
            User.Settings.SelectToken("forgot_password").Replace(ForgotPassword);
            #endregion

            if (await __DBContext.SaveChangesAsync() > 0) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        #endregion
    }
}