using Common;
using CoreApi.Common;
using CoreApi.Common.Base;
using CoreApi.Models.ModifyModels;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Actions;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Context.ParserModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CoreApi.Services
{
    public enum GetUserAction {
        SearchUsers                     = 0,
    }
    public class SocialUserManagement : BaseTransientService
    {
        private SocialUserAuditLogManagement __SocialUserAuditLogManagement;
        public SocialUserManagement(IServiceProvider _IServiceProvider,
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

        public string[] GetAllowOrderFields(GetUserAction action)
        {
            switch (action) {
                case GetUserAction.SearchUsers:
                    return new string[]{
                        "following",
                        "follower"
                    };
                default:
                    return default;
            }
        }

        #region Find user, handle user login
        public async Task<(SocialUser, ErrorCodes)> FindUser(string UserName, bool isEmail)
        {
            SocialUser user;
            if (isEmail) {
                user = await __DBContext.SocialUsers
                        .Where(e => e.Email.ToLower() == UserName.ToLower()
                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted))
                        .FirstOrDefaultAsync();
            } else {
                user = await __DBContext.SocialUsers
                        .Where(e => e.UserName.ToLower() == UserName.ToLower()
                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted))
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
                        .Where(e => e.Email.ToLower() == UserName.ToLower())
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

        public async Task<(List<SocialUser>, int, ErrorCodes)> SearchUsers(Guid SocialUserId = default,
                                                                           int Start = 0,
                                                                           int Size = 20,
                                                                           string SearchTerm = default,
                                                                           (string, bool)[] Orders = default)
        {
            #region validate params
            var ColumnAllowOrder = GetAllowOrderFields(GetUserAction.SearchUsers);
            if (Orders != default) {
                foreach (var order in Orders) {
                    if (!ColumnAllowOrder.Contains(order.Item1)) {
                        return (default, default, ErrorCodes.INVALID_PARAMS);
                    }
                }
            } else {
                Orders = new (string, bool)[]{};
            }
            #endregion
            // SearchTerm = SearchTerm == default ? default : SearchTerm.Trim().ToLower();

            if (SearchTerm != default) {
                SearchTerm = Utils.PrepareSearchTerm(SearchTerm);
            }

            // orderStr can't empty or null
            // string OrderStr = Orders != default && Orders.Length != 0 ? Utils.GenerateOrderString(Orders) : "following desc, follower desc";
            var Query = __DBContext.SocialUsers
                            .Where(e => (e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted))
                                && (SearchTerm == default
                                    || e.SearchVector.Matches(SearchTerm)
                                    || e.UserName.ToLower().Contains(SearchTerm)
                                    || e.DisplayName.ToLower().Contains(SearchTerm)
                                )
                            );
            var TotalCount = await __DBContext.SocialUsers
                                .CountAsync(e => (e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted))
                                    && (SearchTerm == default
                                        || e.SearchVector.Matches(SearchTerm)
                                        || e.UserName.ToLower().Contains(SearchTerm)
                                        || e.DisplayName.ToLower().Contains(SearchTerm)
                                    )
                                );
            return (await Query.ToListAsync(), TotalCount, ErrorCodes.NO_ERROR);
        }

        public async Task<(List<SocialUser>, int TotalSize)> GetUsers(int Start, int Size, string SearchTerm = default)
        {
            return
            (
                await __DBContext.SocialUsers
                    .Where(e =>
                        e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted)
                        && (
                            SearchTerm == default || SearchTerm.Trim() == string.Empty
                            || e.SearchVector.Matches(SearchTerm)
                            || e.UserName.ToLower().Contains(SearchTerm.Trim().ToLower())
                            || e.DisplayName.ToLower().Contains(SearchTerm.Trim().ToLower())
                            || e.Email.ToLower().Contains(SearchTerm.Trim().ToLower())
                        )
                    )
                    .OrderByDescending(e => e.CreatedTimestamp)
                    .Skip(Start)
                    .Take(Size)
                    .ToListAsync(),
                await __DBContext.SocialUsers
                    .CountAsync(e =>
                        e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted)
                        && (
                            SearchTerm == default || SearchTerm.Trim() == string.Empty
                            || e.SearchVector.Matches(SearchTerm)
                            || e.UserName.ToLower().Contains(SearchTerm.Trim().ToLower())
                            || e.DisplayName.ToLower().Contains(SearchTerm.Trim().ToLower())
                            || e.Email.ToLower().Contains(SearchTerm.Trim().ToLower())
                        )
                    )
            );
        }
        // username_existed, email_existed, ERROR
        public async Task<(bool, bool, ErrorCodes)> IsUserExsiting(string UserName, string Email)
        {
            var count_email = (await __DBContext.SocialUsers
                    .CountAsync(e => e.Email == Email
                        && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted)));
            var count_username = (await __DBContext.SocialUsers
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
                        User.Status.ChangeStatus(StatusType.Blocked);
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

        public async Task<bool> IsExpiredBlockTime(Guid UserId, int LockTime)
        {
            #region Find user info
            var (User, Error) = await FindUserById(UserId);
            if (Error != ErrorCodes.NO_ERROR) {
                return false;
            }
            #endregion

            if (User.Settings.ContainsKey("__login_config")) {
                try {
                    JObject config = User.Settings.Value<JObject>("__login_config");
                    if (!config.ContainsKey("last_login")) {
                        return false;
                    }

                    long lastLoginFailure = config.Value<long>("last_login");
                    if (User.Status.Type == StatusType.Blocked) {
                        long currentUnixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                        if (currentUnixTime - lastLoginFailure > LockTime * 60) {
                            config["number"] = 0;
                            User.Settings["__login_config"] = config;
                            User.Status.ChangeStatus(StatusType.Activated);
                            await __DBContext.SaveChangesAsync();
                            return true;
                        }
                    }
                } catch (Exception) {}
            }

            return false;
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

        public async Task<ErrorCodes> HaveWritePermission(Guid UserId, string Right)
        {
            #region Find user info
            var (User, Error) = await FindUserById(UserId);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

            return HaveWritePermission(User.Rights, Right);
        }

        public ErrorCodes HaveWritePermission(Dictionary<string, JObject> UserRights, string Right)
        {
            if (UserRights.ContainsKey(Right)) {
                var right = UserRights[Right];
                if (right["write"] != default &&
                    ((bool)right["write"]) == true) {
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
                .GetConfigValue<int>(CONFIG_KEY.SOCIAL_PASSWORD_POLICY, SUB_CONFIG_KEY.MIN_LEN)
                .Value;
            var MaxLen                          = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.SOCIAL_PASSWORD_POLICY, SUB_CONFIG_KEY.MAX_LEN)
                .Value;
            var MinUpperChar                    = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.SOCIAL_PASSWORD_POLICY, SUB_CONFIG_KEY.MIN_UPPER_CHAR)
                .Value;
            var MinLowerChar                    = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.SOCIAL_PASSWORD_POLICY, SUB_CONFIG_KEY.MIN_LOWER_CHAR)
                .Value;
            var MinNumberChar                   = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.SOCIAL_PASSWORD_POLICY, SUB_CONFIG_KEY.MIN_NUMBER_CHAR)
                .Value;
            var MinSpecialChar                  = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.SOCIAL_PASSWORD_POLICY, SUB_CONFIG_KEY.MIN_SPECIAL_CHAR)
                .Value;
            #endregion

            return CommonValidate.ValidatePassword(Password, MinLen, MaxLen, MinUpperChar, MinLowerChar, MinNumberChar, MinSpecialChar);
        }
        public async Task<ErrorCodes> ChangePassword(Guid UserId, string NewPassword)
        {
            var (User, Error) = await FindUserById(UserId);
            if (Error != ErrorCodes.NO_ERROR) {
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
                    { "last_change_password", DateTime.UtcNow },
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
                #region [SOCIAL] Write user audit log
                (User, Error) = await FindUserById(User.Id);
                var (OldVal, NewVal) = Utils.GetDataChanges(OldUser, User.GetJsonObjectForLog());
                if (Error == ErrorCodes.NO_ERROR) {
                    await __SocialUserAuditLogManagement.AddNewUserAuditLog(
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
                #region [SOCIAL] Write user audit log
                (var user, var error) = await FindUserById(NewUser.Id);
                if (error == ErrorCodes.NO_ERROR) {
                    await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                        user.GetModelName(),
                        user.Id.ToString(),
                        LOG_ACTIONS.CREATE,
                        user.Id,
                        new JObject(),
                        user.GetJsonObjectForLog()
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
            var oldUser = Utils.DeepClone(user.GetJsonObjectForLog());
            #region Get data change and save
            var haveChange = false;
            if (modelModify.user_name != default && modelModify.user_name != user.UserName) {
                user.UserName = modelModify.user_name;
                haveChange = true;
            }
            if (modelModify.first_name != default && modelModify.first_name != user.FirstName) {
                user.FirstName = modelModify.first_name;
                haveChange = true;
            }
            if (modelModify.last_name != default && modelModify.last_name != user.LastName) {
                user.LastName = modelModify.last_name;
                haveChange = true;
            }
            if (modelModify.display_name != default && modelModify.display_name != user.DisplayName) {
                user.DisplayName = modelModify.display_name;
                haveChange = true;
            }
            if (modelModify.description != default && modelModify.description != user.Description) {
                user.Description = modelModify.description;
                haveChange = true;
            }
            if (modelModify.email != default && modelModify.email.ToLower() != user.Email) {
                user.Email = modelModify.email.ToLower();
                haveChange = true;
            }
            if (modelModify.avatar != default && modelModify.avatar != user.Avatar) {
                user.Avatar = modelModify.avatar;
                haveChange = true;
            }
            if (modelModify.sex != default && modelModify.sex != user.Sex) {
                user.Sex = modelModify.sex;
                haveChange = true;
            }
            if (modelModify.phone != default && modelModify.phone != user.Phone) {
                user.Phone = modelModify.phone;
                haveChange = true;
            }
            if (modelModify.city != default && modelModify.city != user.City) {
                user.City = modelModify.city;
                haveChange = true;
            }
            if (modelModify.province != default && modelModify.province != user.Province) {
                user.Province = modelModify.province;
                haveChange = true;
            }
            if (modelModify.country != default && modelModify.country != user.Country) {
                user.Country = modelModify.country;
                haveChange = true;
            }
            if (modelModify.ui_settings != default) {
                haveChange = true;
                if (user.Settings.ContainsKey("ui_settings")) {
                    if (user.Settings.SelectToken("ui_settings").ToString() != modelModify.ui_settings.ToString()) {
                        user.Settings.SelectToken("ui_settings").Replace(Utils.ObjectToJsonToken(modelModify.ui_settings));
                    } else {
                        haveChange = false;
                    }
                } else {
                    user.Settings.Add("ui_settings", Utils.ObjectToJsonToken(modelModify.ui_settings));
                }
            }
            if (modelModify.publics != default) {
                if (modelModify.publics.Count(e => user.Publics.Contains(e)) != user.Publics.Count()) {
                    haveChange = true;
                    user.Publics = JArray.FromObject(modelModify.publics);
                }
            }
            #endregion

            if (!haveChange) {
                return ErrorCodes.NO_CHANGE_DETECTED;
            }

            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [SOCIAL] Write user audit log
                (user, error) = await FindUserById(user.Id);
                var (oldVal, newVal) = Utils.GetDataChanges(oldUser, user.GetJsonObjectForLog());
                if (error == ErrorCodes.NO_ERROR) {
                    await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                        user.GetModelName(),
                        user.Id.ToString(),
                        LOG_ACTIONS.MODIFY,
                        user.Id,
                        oldVal,
                        newVal
                    );
                } else {
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
                #endregion
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }

        public async Task<ErrorCodes> DeleteUser(Guid UserId)
        {
            var (User, Error) = await FindUserById(UserId);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            User.StatusStr = EntityStatus.StatusTypeToString(StatusType.Deleted);

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
        public async Task<ErrorCodes> ModifyUser(Guid UserId, SocialUserModifyModelByAdmin ModelData, Guid AdminUserId)
        {
            #region Find user info
            var (User, Error) = await FindUserById(UserId);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

            var OldData = Utils.DeepClone(User.GetJsonObjectForLog());
            #region Get data change and save
            var haveChange = false;
            if (ModelData.status != default && ModelData.status != User.StatusStr) {
                User.StatusStr = ModelData.status;
                haveChange = true;
            }
            if (ModelData.roles != default) {
                User.SocialUserRoleOfUsers.Clear();
                foreach (var It in ModelData.roles) {
                    var (Role, Err) =  await GetRoleIgnoreStatus(It.ToString());
                    if (Err != ErrorCodes.NO_ERROR) {
                        return ErrorCodes.NOT_FOUND;
                    }
                    User.SocialUserRoleOfUsers.Add(new SocialUserRoleOfUser(){
                        RoleId      = Role.Id,
                        Role        = Role,
                        UserId      = User.Id,
                        User        = User
                    });
                }
                haveChange = true;
            }
            #endregion

            if (!haveChange) {
                return ErrorCodes.NO_CHANGE_DETECTED;
            }
            var Ret = await __DBContext.SaveChangesAsync();
            if (Ret == 0) {
                return ErrorCodes.NO_CHANGE_DETECTED;
            }
            if (Ret > 0) {
                #region [ADMIN] Write audit log
                var __SocialUserAuditLogManagement = __ServiceProvider.GetService<SocialUserAuditLogManagement>();
                var (OldVal, NewVal) = Utils.GetDataChanges(OldData, User.GetJsonObjectForLog());
                await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                    User.GetModelName(),
                    User.Id.ToString(),
                    LOG_ACTIONS.MODIFY,
                    User.Id,
                    OldVal,
                    NewVal,
                    AdminUserId
                );
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
                    await __SocialUserAuditLogManagement.AddNewUserAuditLog(
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

        #region Confirm Email
        public async Task<ErrorCodes> HandleConfirmEmailSuccessfully(Guid Id)
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
            var ConfirmEmail = User.Settings.Value<JObject>("confirm_email");
            if (ConfirmEmail == default) {
                return ErrorCodes.INTERNAL_SERVER_ERROR;
            }
            if (ConfirmEmail.ContainsKey("confirm_date")) {
                ConfirmEmail.SelectToken("confirm_date").Replace(DateTime.UtcNow);
            } else {
                ConfirmEmail.Add("confirm_date", DateTime.UtcNow);
            }
            if (ConfirmEmail.ContainsKey("failed_times")) {
                ConfirmEmail.Remove("failed_times");
            }
            User.Settings.SelectToken("confirm_email").Replace(ConfirmEmail);
            User.VerifiedEmail = true;
            #endregion

            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [SOCIAL] Write user audit log
                (User, Error) = await FindUserById(Id);
                var (OldVal, NewVal) = Utils.GetDataChanges(OldUser, User.GetJsonObjectForLog());
                if (Error == ErrorCodes.NO_ERROR) {
                    await __SocialUserAuditLogManagement.AddNewUserAuditLog(
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
        public async Task<ErrorCodes> HandleConfirmEmailFailed(Guid Id)
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
            var ConfirmEmail = User.Settings.Value<JObject>("confirm_email");
            if (ConfirmEmail == default) {
                return ErrorCodes.INTERNAL_SERVER_ERROR;
            }
            var FailedTimes = ConfirmEmail.Value<int>("failed_times") + 1;
            if (ConfirmEmail.ContainsKey("failed_times")) {
                ConfirmEmail.SelectToken("failed_times").Replace(FailedTimes);
            } else {
                ConfirmEmail.Add("failed_times", FailedTimes);
            }
            User.Settings.SelectToken("confirm_email").Replace(ConfirmEmail);
            #endregion

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
            return action != default ? action.Actions.Count(a => a.action == actionStr) > 0 : false;
        }
        protected async Task<ErrorCodes> AddAction(Guid userId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithUsers
                .Where(e => e.UserIdDes == userId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            if (action != default) {
                if (!(action.Actions.Count(a => a.action == actionStr) > 0)) {
                    action.Actions.Add(new EntityAction(EntityActionType.UserActionWithUser, actionStr));
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
                        Actions = new List<EntityAction>(){
                            new EntityAction(EntityActionType.UserActionWithUser, actionStr)
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
                var _action = action.Actions.Where(a => a.action == actionStr).FirstOrDefault();
                if (_action != default) {
                    action.Actions.Remove(_action);
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
                EntityAction.ActionTypeToString(ActionType.Follow)
            );
        }
        public async Task<ErrorCodes> Follow(Guid userId, Guid socialUserId)
        {
            return await AddAction(
                userId, socialUserId,
                EntityAction.ActionTypeToString(ActionType.Follow)
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
                .Where(e => e.Actions.Count(a => a.action == EntityAction.ActionTypeToString(ActionType.Follow)) > 0
                    && e.User.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted)
                )
                .Select(e => e.User)
                .Skip(start).Take(size)
                .ToList();
            var total_size = user.SocialUserActionWithUserUserIdDesNavigations
                .Count(e => e.Actions.Count(a => a.action == EntityAction.ActionTypeToString(ActionType.Follow)) > 0
                    && e.User.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted)
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
                .Where(e => e.Actions.Count(a => a.action == EntityAction.ActionTypeToString(ActionType.Follow)) > 0
                    && e.UserIdDesNavigation.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted)
                )
                .Select(e => e.UserIdDesNavigation)
                .Skip(start).Take(size)
                .ToList();
            var total_size = user.SocialUserActionWithUserUsers
                .Count(e => e.Actions.Count(a => a.action == EntityAction.ActionTypeToString(ActionType.Follow)) > 0
                    && e.UserIdDesNavigation.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted)
                );
            return (ret, total_size, ErrorCodes.NO_ERROR);
        }

        #region Roles, Rights
        public async Task<(SocialUserRole, ErrorCodes)> GetRole(string RoleName)
        {
            var Right = await __DBContext.SocialUserRoles
                    .Where(e => e.RoleName == RoleName && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                    .FirstOrDefaultAsync();
            if (Right == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }
            return (Right, ErrorCodes.NO_ERROR);
        }
        public async Task<(SocialUserRole, ErrorCodes)> GetRoleIgnoreStatus(string RoleName)
        {
            var Right = await __DBContext.SocialUserRoles
                    .Where(e => e.RoleName == RoleName)
                    .FirstOrDefaultAsync();
            if (Right == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }
            return (Right, ErrorCodes.NO_ERROR);
        }
        public async Task<(SocialUserRole, ErrorCodes)> GetRoleById(int Id)
        {
            var Role = await __DBContext.SocialUserRoles
                    .Where(e => e.Id == Id)
                    .FirstOrDefaultAsync();
            if (Role == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }
            return (Role, ErrorCodes.NO_ERROR);
        }

        public async Task<(List<SocialUserRole>, int)> GetRoles()
        {
            return (
                await __DBContext.SocialUserRoles
                    .Where(e => e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                    .ToListAsync(),
                await __DBContext.SocialUserRoles
                    .CountAsync(e => e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
            );
        }
        public async Task<ErrorCodes> NewRole(SocialUserRole Role, ParserSocialUserRole Parser, Guid UserId)
        {
            foreach (var It in Parser.rights) {
                var Actions = new JObject();
                Actions["write"] = It.Value.ToObject<JObject>()["write"];
                Actions["read"] = It.Value.ToObject<JObject>()["read"];
                var (Right, Err) =  await GetRight(It.Key);
                if (Err != ErrorCodes.NO_ERROR) {
                    return ErrorCodes.NOT_FOUND;
                }
                Role.SocialUserRoleDetails.Add(new SocialUserRoleDetail(){
                    RightId     = Right.Id,
                    Right       = Right,
                    Role        = Role,
                    Actions     = Actions
                });
            }
            await __DBContext.SocialUserRoles.AddAsync(Role);

            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [SOCIAL] Write admin audit log
                var __AdminAuditLogManagement = __ServiceProvider.GetService<AdminAuditLogManagement>();
                var Error = ErrorCodes.NO_ERROR;
                (Role, Error) = await GetRole(Role.RoleName);
                await __AdminAuditLogManagement.AddNewAuditLog(
                    Role.GetModelName(),
                    Role.Id.ToString(),
                    LOG_ACTIONS.CREATE,
                    UserId,
                    new JObject(),
                    Role.GetJsonObjectForLog()
                );
                #endregion
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        public async Task<ErrorCodes> ModifyRole(int RoleId, SocialUserRoleModifyModel ModelData, Guid UserId)
        {
            var (Role, Error) =  await GetRoleById(RoleId);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }

            var OldData = Utils.DeepClone(Role.GetJsonObjectForLog());
            #region Get data change and save
            var haveChange = false;
            if (ModelData.display_name != default && ModelData.display_name != Role.DisplayName) {
                Role.DisplayName = ModelData.display_name;
                haveChange = true;
            }
            if (ModelData.describe != default && ModelData.describe != Role.Describe) {
                Role.Describe = ModelData.describe;
                haveChange = true;
            }
            if (ModelData.priority != default && ModelData.priority != Role.Priority) {
                Role.Priority = (bool) ModelData.priority;
                haveChange = true;
            }
            if (ModelData.rights != default) {
                Role.SocialUserRoleDetails.Clear();
                foreach (var It in ModelData.rights) {
                    var Actions = new JObject();
                    Actions["write"] = It.Value.ToObject<JObject>()["write"];
                    Actions["read"] = It.Value.ToObject<JObject>()["read"];
                    var (Right, Err) =  await GetRight(It.Key);
                    if (Err != ErrorCodes.NO_ERROR) {
                        return ErrorCodes.NOT_FOUND;
                    }
                    Role.SocialUserRoleDetails.Add(new SocialUserRoleDetail(){
                        RightId     = Right.Id,
                        Right       = Right,
                        Role        = Role,
                        Actions     = Actions
                    });
                }
                haveChange = true;
            }
            #endregion

            if (!haveChange) {
                return ErrorCodes.NO_CHANGE_DETECTED;
            }

            var Ret = await __DBContext.SaveChangesAsync();
            if (Ret == 0) {
                return ErrorCodes.NO_CHANGE_DETECTED;
            }
            if (Ret > 0) {
                #region [SOCIAL] Write admin audit log
                var __AdminAuditLogManagement = __ServiceProvider.GetService<AdminAuditLogManagement>();
                var (OldVal, NewVal) = Utils.GetDataChanges(OldData, Role.GetJsonObjectForLog());
                await __AdminAuditLogManagement.AddNewAuditLog(
                    Role.GetModelName(),
                    Role.Id.ToString(),
                    LOG_ACTIONS.MODIFY,
                    UserId,
                    OldVal,
                    NewVal
                );
                #endregion
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        public async Task<(SocialUserRight, ErrorCodes)> GetRight(string RightName)
        {
            var Right = await __DBContext.SocialUserRights
                    .Where(e => e.RightName == RightName && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                    .FirstOrDefaultAsync();
            if (Right == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }
            return (Right, ErrorCodes.NO_ERROR);
        }
        public async Task<(SocialUserRight, ErrorCodes)> GetRightById(int Id)
        {
            var Right = await __DBContext.SocialUserRights
                    .Where(e => e.Id == Id)
                    .FirstOrDefaultAsync();
            if (Right == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }
            return (Right, ErrorCodes.NO_ERROR);
        }
        public async Task<(List<SocialUserRight>, int)> GetRights()
        {
            return (
                await __DBContext.SocialUserRights
                    .Where(e => e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                    .ToListAsync(),
                await __DBContext.SocialUserRights
                    .CountAsync(e => e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
            );
        }
        public async Task<ErrorCodes> NewRight(SocialUserRight Right, Guid UserId)
        {
            await __DBContext.SocialUserRights.AddAsync(Right);

            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [SOCIAL] Write admin audit log
                var __AdminAuditLogManagement = __ServiceProvider.GetService<AdminAuditLogManagement>();
                await __AdminAuditLogManagement.AddNewAuditLog(
                    Right.GetModelName(),
                    Right.Id.ToString(),
                    LOG_ACTIONS.CREATE,
                    UserId,
                    new JObject(),
                    Right.GetJsonObjectForLog()
                );
                #endregion
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        public async Task<ErrorCodes> ModifyRight(int RightId, SocialUserRightModifyModel ModelData, Guid UserId)
        {
            var (Right, Error) =  await GetRightById(RightId);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            var OldData = Utils.DeepClone(Right.GetJsonObjectForLog());
            #region Get data change and save
            var haveChange = false;
            if (ModelData.display_name != default && ModelData.display_name != Right.Describe) {
                Right.DisplayName = ModelData.display_name;
                haveChange = true;
            }
            if (ModelData.describe != default && ModelData.describe != Right.Describe) {
                Right.Describe = ModelData.describe;
                haveChange = true;
            }
            #endregion

            if (!haveChange) {
                return ErrorCodes.NO_CHANGE_DETECTED;
            }

            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [SOCIAL] Write admin audit log
                var __AdminAuditLogManagement = __ServiceProvider.GetService<AdminAuditLogManagement>();
                var (OldVal, NewVal) = Utils.GetDataChanges(OldData, Right.GetJsonObjectForLog());
                await __AdminAuditLogManagement.AddNewAuditLog(
                    Right.GetModelName(),
                    Right.Id.ToString(),
                    LOG_ACTIONS.MODIFY,
                    UserId,
                    OldVal,
                    NewVal
                );
                #endregion
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        #endregion
    }
}