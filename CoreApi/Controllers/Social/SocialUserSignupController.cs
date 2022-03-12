using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseAccess.Context;
using DatabaseAccess.Common;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Context.ParserModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CoreApi.Common;
// using System.Data.Entity;
using System.Diagnostics;

using System.Text;
// using System.Text.Json;
using CoreApi.Services;

namespace CoreApi.Controllers.Social
{
    [ApiController]
    [Route("/signup")]
    public class SocialUserSignupController : BaseController
    {
        private DBContext __DBContext;
        private BaseConfig __BaseConfig;
        public SocialUserSignupController(
            DBContext _DBContext,
            BaseConfig _BaseConfig
        ) : base() {
            __DBContext = _DBContext;
            __BaseConfig = _BaseConfig;
            __ControllerName = "SocialUserSignup";
            LoadConfig();
        }
        [HttpPost]
        public IActionResult SocialUserSignup(ParserSocialUser parser)
        {
            if (!LoadConfigSuccess) {
                return Problem(
                    detail: "Internal Server error.",
                    statusCode: 500,
                    instance: "/signup",
                    title: "Internal Server Error",
                    type: "/report"
                );
            }
            try {
                SocialUser user = new SocialUser();
                string Error = "";
                if (!user.Parse(parser, out Error)) {
                    LogInformation(Error);
                    return Problem(
                        detail: "Bad body data.",
                        statusCode: 400,
                        instance: "/signup",
                        title: "Bad request",
                        type: "/help"
                    );
                }
                // Check user name
                bool userNameHaveUsed = __DBContext.SocialUsers
                        .Count(e => 
                                e.UserName == user.UserName &&
                                e.StatusStr != BaseStatus.StatusToString(SocialUserStatus.Deleted, EntityStatus.SocialUserStatus)
                        ) > 0;
                if (userNameHaveUsed) {
                    LogDebug($"UserName have been used, user_name: { user.UserName }");
                    return Problem(
                        detail: "UserName have been used.",
                        statusCode: 400,
                        instance: "/signup",
                        title: "Bad request",
                        type: "/help"
                    );
                }
                // check email user
                bool userEmailHaveUsed = __DBContext.SocialUsers
                        .Count(e => 
                                e.Email == user.Email &&
                                e.StatusStr != BaseStatus.StatusToString(SocialUserStatus.Deleted, EntityStatus.SocialUserStatus)
                        ) > 0;
                if (userEmailHaveUsed) {
                    LogDebug($"Email have been used, user_name: { user.Email }");
                    return Problem(
                        detail: "Email have been used.",
                        statusCode: 400,
                        instance: "/signup",
                        title: "Bad request",
                        type: "/help"
                    );
                }
                // add user
                __DBContext.SocialUsers.Add(user);
                // add default role
                SocialUserRoleOfUser defaultRole = new SocialUserRoleOfUser();
                defaultRole.UserId = user.Id;
                defaultRole.RoleId = SocialUserRole.GetDefaultRoleId();
                defaultRole.Role = __DBContext.SocialUserRoles.Where(e => e.Id == defaultRole.RoleId).ToList().First();
                user.SocialUserRoleOfUsers.Add(defaultRole);
                // save changes
                __DBContext.SaveChanges();
                LogInformation($"Signup social user success, user_name: { user.UserName }");
                return Ok(new JObject(){
                    { "status", 200 },
                    { "message", "success" },
                });
            } catch (Exception e) {
                LogError($"Unhandle exception, message: { e.Message }");
                return Problem(
                    detail: "Internal Server error.",
                    statusCode: 500,
                    instance: "/signup",
                    title: "Internal Server Error",
                    type: "/report"
                );
            }
        }
    }
}
