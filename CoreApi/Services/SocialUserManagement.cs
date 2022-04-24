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
using Common;
using DatabaseAccess.Common.Actions;
using CoreApi.Models.ModifyModels;

namespace CoreApi.Services
{
    public class SocialUserManagement : BaseTransientService
    {
        private SocialUserAuditLogManagement __SocialUserAuditLogManagement;
        public SocialUserManagement(DBContext _DBContext,
                                    IServiceProvider _IServiceProvider,
                                    SocialUserAuditLogManagement _SocialUserAuditLogManagement)
            : base(_IServiceProvider)
        {
            __SocialUserAuditLogManagement = _SocialUserAuditLogManagement;
            __ServiceName = "SocialUserManagement";
        }
        public override void SetTraceId(string TraceId)
        {
            base.SetTraceId(TraceId);
            __SocialUserAuditLogManagement.SetTraceId(TraceId);
        }

        public void UpdateDefaultSocialRole()
        {
            var rds = SocialUserRoleDetail.GetDefaultData();
            foreach (var r in rds) {
                if (__DBContext.SocialUserRoleDetails.Count(e => 
                        e.RightId == r.RightId && e.RoleId == e.RoleId
                    ) == 0
                ) {
                    __DBContext.SocialUserRoleDetails.Add(
                        new SocialUserRoleDetail(){
                            RoleId = r.RoleId,
                            RightId = r.RightId,
                            Actions = r.Actions
                        }
                    );

                    if (__DBContext.SaveChanges() <= 0) {
                        throw new Exception("UpdateDefaultSocialRole failed.");
                    }
                }
            }
        }

        public async Task UpdateDefaultSocialRoleAsync()
        {
            var rds = SocialUserRoleDetail.GetDefaultData();
            foreach (var r in rds) {
                if (await __DBContext.SocialUserRoleDetails.CountAsync(e => 
                        e.RightId == r.RightId && e.RoleId == e.RoleId
                    ) == 0
                ) {
                    await __DBContext.SocialUserRoleDetails.AddAsync(
                        new SocialUserRoleDetail(){
                            RoleId = r.RoleId,
                            RightId = r.RightId,
                            Actions = r.Actions
                        }
                    );

                    if (await __DBContext.SaveChangesAsync() <= 0) {
                        throw new Exception("UpdateDefaultSocialRole failed.");
                    }
                }
            }
        }

        #region Find user, handle user login
        public async Task<(SocialUser, ErrorCodes)> FindUser(string UserName, bool isEmail)
        {
            SocialUser user;
            if (isEmail) {
                user = await __DBContext.SocialUsers
                        .Where(e => e.Email == UserName
                            && e.StatusStr != BaseStatus.StatusToString(SocialUserStatus.Deleted, EntityStatus.SocialUserStatus))
                        .FirstOrDefaultAsync();
            } else {
                user = await __DBContext.SocialUsers
                        .Where(e => e.UserName == UserName
                            && e.StatusStr != BaseStatus.StatusToString(SocialUserStatus.Deleted, EntityStatus.SocialUserStatus))
                        .FirstOrDefaultAsync();
            }
            if (user != default) {
                return (user, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.NOT_FOUND);
        }

        public async Task<(SocialUser, ErrorCodes)> FindUserIgnoreStatus(string UserName, bool isEmail)
        {
            SocialUser user;
            if (isEmail) {
                user = await __DBContext.SocialUsers
                        .Where(e => e.Email == UserName)
                        .FirstOrDefaultAsync();
            } else {
                user = await __DBContext.SocialUsers
                        .Where(e => e.UserName == UserName)
                        .FirstOrDefaultAsync();
            }
            if (user != default) {
                return (user, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.NOT_FOUND);
        }

        public async Task<(SocialUser, ErrorCodes)> FindUserById(Guid Id)
        {
            SocialUser user;
            user = await __DBContext.SocialUsers
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
            var count_email = (await __DBContext.SocialUsers
                    .CountAsync(e => e.Email == UserName
                        && e.StatusStr != BaseStatus.StatusToString(SocialUserStatus.Deleted, EntityStatus.SocialUserStatus)));
            var count_username = (await __DBContext.SocialUsers
                    .CountAsync(e => e.UserName == UserName
                        && e.StatusStr != BaseStatus.StatusToString(SocialUserStatus.Deleted, EntityStatus.SocialUserStatus)));
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
                        User.Status = SocialUserStatus.Blocked;
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
        public async Task<ErrorCodes> AddNewUser(SocialUser NewUser)
        {
            await __DBContext.SocialUsers.AddAsync(NewUser);
            #region Add default role
            SocialUserRoleOfUser defaultRole = new SocialUserRoleOfUser();
            defaultRole.UserId = NewUser.Id;
            defaultRole.RoleId = SocialUserRole.GetDefaultRoleId();
            defaultRole.Role = __DBContext.SocialUserRoles.Where(e => e.Id == defaultRole.RoleId).ToList().First();
            NewUser.SocialUserRoleOfUsers.Add(defaultRole);
            #endregion

            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [SOCIAL] Write user activity
                (var user, var error) = await FindUserById(NewUser.Id);
                if (error == ErrorCodes.NO_ERROR) {
                    await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                        user.GetModelName(),
                        user.Id.ToString(),
                        LOG_ACTIONS.CREATE,
                        user.Id,
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

        public async Task<ErrorCodes> ModifyUser(Guid userId, SocialUserModifyModel modelModify)
        {
            var (user, error) = await FindUserById(userId);
            if (error != ErrorCodes.NO_ERROR) {
                return error;
            }
            #region Get data change and save
            var haveChange = false;
            if (modelModify.user_name != default) {
                user.UserName = modelModify.user_name;
                haveChange = true;
            }
            if (modelModify.first_name != default) {
                user.FirstName = modelModify.first_name;
                haveChange = true;
            }
            if (modelModify.last_name != default) {
                user.LastName = modelModify.last_name;
                haveChange = true;
            }
            if (modelModify.display_name != default) {
                user.DisplayName = modelModify.display_name;
                haveChange = true;
            }
            if (modelModify.description != default) {
                user.Description = modelModify.description;
                haveChange = true;
            }
            if (modelModify.email != default) {
                user.Email = modelModify.email;
                haveChange = true;
            }
            if (modelModify.avatar != default) {
                user.Avatar = modelModify.avatar;
                haveChange = true;
            }
            if (modelModify.sex != default) {
                user.Sex = modelModify.sex;
                haveChange = true;
            }
            if (modelModify.phone != default) {
                user.Phone = modelModify.phone;
                haveChange = true;
            }
            if (modelModify.city != default) {
                user.City = modelModify.city;
                haveChange = true;
            }
            if (modelModify.province != default) {
                user.Province = modelModify.province;
                haveChange = true;
            }
            if (modelModify.country != default) {
                user.Country = modelModify.country;
                haveChange = true;
            }
            if (modelModify.ui_settings != default) {
                haveChange = true;
                if (user.Settings.ContainsKey("ui_settings")) {
                    user.Settings.SelectToken("ui_settings").Replace(Utils.ObjectToJsonToken(modelModify.ui_settings));
                }
                user.Settings.Add("ui_settings", Utils.ObjectToJsonToken(modelModify.ui_settings));
            }
            if (modelModify.publics != default) {
                user.Publics = JArray.FromObject(modelModify.publics);
            }
            #endregion

            if (!haveChange) {
                return ErrorCodes.NO_CHANGE_DETECTED;
            }

            if (await __DBContext.SaveChangesAsync() > 0) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }

        public async Task<ErrorCodes> DeleteUser(SocialUser User)
        {
            __DBContext.SocialUsers.Remove(User);

            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [SOCIAL] Write user activity
                await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                    User.GetModelName(),
                    User.Id.ToString(),
                    LOG_ACTIONS.DELETE,
                    User.Id,
                    new JObject(),
                    new JObject()
                );
                #endregion
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        #endregion

        #region Confirm Email
        public async Task<ErrorCodes> HandleConfirmEmailSuccessfully(Guid Id)
        {
            #region Find user info
            var (User, Error) = await FindUserById(Id);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

            if (User.Status == SocialUserStatus.Deleted) {
                return ErrorCodes.DELETED;
            } else if (User.Status == SocialUserStatus.Blocked) {
                return ErrorCodes.USER_HAVE_BEEN_LOCKED;
            }

            var confirm_email = User.Settings.Value<JObject>("confirm_email");
            if (confirm_email == default) {
                return ErrorCodes.INTERNAL_SERVER_ERROR;
            }
            confirm_email.Remove("confirm_date");
            confirm_email.Add("confirm_date", DateTime.UtcNow.ToString(CommonDefine.DATE_TIME_FORMAT));

            User.Settings.Remove("confirm_email");
            User.Settings.Add("confirm_email", confirm_email);

            User.VerifiedEmail = true;

            if (await __DBContext.SaveChangesAsync() > 0) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        public async Task<ErrorCodes> HandleConfirmEmailFailed(Guid Id)
        {
            #region Find user info
            var (User, Error) = await FindUserById(Id);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

            if (User.Status == SocialUserStatus.Deleted) {
                return ErrorCodes.DELETED;
            } else if (User.Status == SocialUserStatus.Blocked) {
                return ErrorCodes.USER_HAVE_BEEN_LOCKED;
            }

            var confirm_email = User.Settings.Value<JObject>("confirm_email");
            if (confirm_email == default) {
                return ErrorCodes.INTERNAL_SERVER_ERROR;
            }
            var numberConfirmFailure = confirm_email.Value<int>("confirm_failure") + 1;
            confirm_email.Remove("confirm_failure");
            confirm_email.Add("confirm_failure", numberConfirmFailure);

            User.Settings.Remove("confirm_email");
            User.Settings.Add("confirm_email", confirm_email);

            if (await __DBContext.SaveChangesAsync() > 0) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        #endregion

        
        #region User action
        public async Task<bool> IsContainsAction(Guid userId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithUsers
                .Where(e => e.UserIdDes == userId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            return action != default ? action.Actions.Contains(actionStr) : false;
        }
        protected async Task<ErrorCodes> AddAction(Guid userId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithUsers
                .Where(e => e.UserIdDes == userId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            if (action != default) {
                if (!action.Actions.Contains(actionStr)) {
                    action.Actions.Add(actionStr);
                    if (await __DBContext.SaveChangesAsync() > 0) {
                        return ErrorCodes.NO_ERROR;
                    }
                }
                return ErrorCodes.NO_ERROR;
            } else {
                await __DBContext.SocialUserActionWithUsers
                    .AddAsync(new SocialUserActionWithUser(){
                        UserId = socialUserId,
                        UserIdDes = userId,
                        Actions = new List<string>(){
                            actionStr
                        }
                    });
                if (await __DBContext.SaveChangesAsync() > 0) {
                    return ErrorCodes.NO_ERROR;
                }
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        protected async Task<ErrorCodes> RemoveAction(Guid userId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithUsers
                .Where(e => e.UserIdDes == userId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            if (action != default) {
                if (action.Actions.Contains(actionStr)) {
                    action.Actions.Remove(actionStr);
                    if (await __DBContext.SaveChangesAsync() > 0) {
                        return ErrorCodes.NO_ERROR;
                    }
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.NO_ERROR;
        }
        public async Task<ErrorCodes> UnFollow(Guid userId, Guid socialUserId)
        {
            return await RemoveAction(
                userId, socialUserId,
                BaseAction.ActionToString(UserActionWithUser.Follow, EntityAction.UserActionWithUser)
            );
        }
        public async Task<ErrorCodes> Follow(Guid userId, Guid socialUserId)
        {
            return await AddAction(
                userId, socialUserId,
                BaseAction.ActionToString(UserActionWithUser.Follow, EntityAction.UserActionWithUser)
            );
        }
        #endregion

        public async Task<(List<SocialUser>, int, ErrorCodes)> GetFollowerByName(string UserName, int start = 0, int size = 20)
        {
            var (user, error) = await FindUserIgnoreStatus(UserName, false);
            if (error != ErrorCodes.NO_ERROR) {
                return (default, default, error);
            }

            var ret = user.SocialUserActionWithUserUserIdDesNavigations
                .Where(e => e.ActionsStr.Contains(
                        BaseAction.ActionToString(UserActionWithUser.Follow, EntityAction.UserActionWithUser)
                    )
                )
                .Select(e => e.User)
                .Skip(start).Take(size)
                .ToList();
            var total_size = user.SocialUserActionWithUserUserIdDesNavigations
                .Count(e => e.ActionsStr.Contains(
                        BaseAction.ActionToString(UserActionWithUser.Follow, EntityAction.UserActionWithUser)
                    )
                );
            return (ret, total_size, ErrorCodes.NO_ERROR);
        }

        public async Task<(List<SocialUser>, int, ErrorCodes)> GetFollowingByName(string UserName, int start = 0, int size = 20)
        {
            var (user, error) = await FindUserIgnoreStatus(UserName, false);
            if (error != ErrorCodes.NO_ERROR) {
                return (default, default, error);
            }

            var ret = user.SocialUserActionWithUserUsers
                .Where(e => e.ActionsStr.Contains(
                    BaseAction.ActionToString(UserActionWithUser.Follow, EntityAction.UserActionWithUser))
                )
                .Select(e => e.UserIdDesNavigation)
                .Skip(start).Take(size)
                .ToList();
            var total_size = user.SocialUserActionWithUserUsers
                .Count(e => e.ActionsStr.Contains(
                        BaseAction.ActionToString(UserActionWithUser.Follow, EntityAction.UserActionWithUser)
                    )
                );
            return (ret, total_size, ErrorCodes.NO_ERROR);
        }
    }
}