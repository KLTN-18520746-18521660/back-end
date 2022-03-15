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
        public SocialUserManagement() : base()
        {
            __ServiceName = "SocialUserManagement";
        }

        #region Find user, handle user login
        public async Task<(SocialUser, ErrorCodes)> FindUser(string UserName, bool isEmail)
        {
            SocialUser user;
            if (isEmail) {
                user = (await __DBContext.SocialUsers
                        .Where(e => e.Email == UserName
                            && e.StatusStr != BaseStatus.StatusToString(SocialUserStatus.Deleted, EntityStatus.SocialUserStatus))
                        .ToListAsync())
                        .DefaultIfEmpty(null)
                        .FirstOrDefault();
            } else {
                user = (await __DBContext.SocialUsers
                        .Where(e => e.UserName == UserName
                            && e.StatusStr != BaseStatus.StatusToString(SocialUserStatus.Deleted, EntityStatus.SocialUserStatus))
                        .ToListAsync())
                        .DefaultIfEmpty(null)
                        .FirstOrDefault();
            }
            if (user != null) {
                return (user, ErrorCodes.NO_ERROR);
            }
            return (null, ErrorCodes.NOT_FOUND);
        }

        public async Task<(SocialUser, ErrorCodes)> FindUserIgnoreStatus(string UserName, bool isEmail)
        {
            SocialUser user;
            if (isEmail) {
                user = (await __DBContext.SocialUsers
                        .Where<SocialUser>(e => e.Email == UserName)
                        .ToListAsync())
                        .DefaultIfEmpty(null)
                        .FirstOrDefault();
            } else {
                user = (await __DBContext.SocialUsers
                        .Where<SocialUser>(e => e.UserName == UserName)
                        .ToListAsync())
                        .DefaultIfEmpty(null)
                        .FirstOrDefault();
            }
            if (user != null) {
                return (user, ErrorCodes.NO_ERROR);
            }
            return (null, ErrorCodes.NOT_FOUND);
        }

        public async Task<(SocialUser, ErrorCodes)> FindUserById(Guid Id)
        {
            SocialUser user;
            user = (await __DBContext.SocialUsers
                    .Where(e => e.Id == Id)
                    .ToListAsync())
                    .DefaultIfEmpty(null)
                    .FirstOrDefault();

            if (user != null) {
                return (user, ErrorCodes.NO_ERROR);
            }
            return (null, ErrorCodes.NOT_FOUND);
        }

        public async Task<ErrorCodes> HandleLoginFail(Guid UserId, int LockTime, int NumberOfTimesAllowLoginFailure)
        {
            #region Find user info
            ErrorCodes Error = ErrorCodes.NO_ERROR;
            SocialUser User = null;
            (User, Error) = await FindUserById(UserId);
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
            ErrorCodes Error = ErrorCodes.NO_ERROR;
            SocialUser User = null;
            (User, Error) = await FindUserById(UserId);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

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
            ErrorCodes Error = ErrorCodes.NO_ERROR;
            SocialUser User = null;
            (User, Error) = await FindUserById(UserId);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

            var rights = User.Rights;
            if (rights.ContainsKey(Right)) {
                var right = rights[Right];
                if (right["read"] != null &&
                    ((bool)right["read"]) == true) {
                    return ErrorCodes.NO_ERROR;
                }
            }
            return ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION;
        }

        public async Task<ErrorCodes> HaveFullPermission(Guid UserId, string Right)
        {
            #region Find user info
            ErrorCodes Error = ErrorCodes.NO_ERROR;
            SocialUser User = null;
            (User, Error) = await FindUserById(UserId);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

            var rights = User.Rights;
            if (rights.ContainsKey(Right)) {
                var right = rights[Right];
                if (right["read"] != null && right["write"] != null &&
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
            __DBContext.SocialUsers.Add(NewUser);
            #region Add default role
            SocialUserRoleOfUser defaultRole = new SocialUserRoleOfUser();
            defaultRole.UserId = NewUser.Id;
            defaultRole.RoleId = SocialUserRole.GetDefaultRoleId();
            defaultRole.Role = __DBContext.SocialUserRoles.Where(e => e.Id == defaultRole.RoleId).ToList().First();
            NewUser.SocialUserRoleOfUsers.Add(defaultRole);
            #endregion

            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [SOCIAL] Write audit log
                SocialAuditLog log = new SocialAuditLog();
                log.Table = NewUser.GetModelName();
                log.TableKey = NewUser.Id.ToString();
                log.Action = LOG_ACTIONS.CREATE;
                log.UserId = NewUser.Id;
                log.OldValue = new LogValue();
                log.NewValue = new LogValue(NewUser.GetJsonObject());

                __DBContext.SocialAuditLogs.Add(log);
                await __DBContext.SaveChangesAsync();
                #endregion
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        #endregion
    }
}