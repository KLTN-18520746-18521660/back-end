using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseAccess.Contexts.ConfigDB.ParserModels;
using DatabaseAccess.Contexts.ConfigDB.Models;
using DatabaseAccess.Contexts.ConfigDB;
// using System.Data.Entity;
using System.Diagnostics;

namespace CoreApi.Controllers.Test
{
    [ApiController]
    [Route("[controller]/test/adminuser")]
    public class TestController : BaseController
    {
        private DBContext __ConfigDB;
        public TestController(
            DBContext ConfigDB
        ) : base() {
            __ConfigDB = ConfigDB;
            __ControllerName = "TestController";
        }

        [HttpPost]
        public IActionResult CreateUserAdmin(ParserAdminUser Parser)
        {
            var traceId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
            __Logger.Information($"CreateUserAdmin, TraceId {traceId}");
            AdminUser AdminUserModel = new AdminUser();
            AdminUserModel.Parse(Parser);

            __ConfigDB.AdminUsers.Add(AdminUserModel);

            try {
                if (__ConfigDB.SaveChanges() > 0) {
                    __Logger.Information($"CreateUserAdmin. Successfulley. user_name: {AdminUserModel.UserName}");
                    return Ok(AdminUserModel.ToJsonString());
                }
            } catch (Exception ex) {
                if (ex is DbUpdateException) {
                    // error while save changes
                }
                else if (ex is DbUpdateConcurrencyException) {
                    // rows affected not equals with expected rows
                }
                throw;
            }

            __Logger.Warning($"CreateUserAdmin. Failed. user_name: {AdminUserModel.UserName}");
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails?view=aspnetcore-6.0
            return Problem(
                detail: "detail", // Error detail
                statusCode: 500,
                instance: "/test/adminuser", // Link send request
                title: "title", // Short title about error
                type: "type" // link to page can identifies problem
            );
        }

        [HttpGet]
        public IActionResult GetUserAdminById(string id)
        {
            var traceId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
            __Logger.Information($"GetUserAdminById, TraceId {traceId}");

            var users = __ConfigDB.AdminUsers.Where<AdminUser>(e => e.Id.ToString() == id).ToList();
            if (users.Count < 1) {
                return Problem(
                    detail: "User not found", // Error detail
                    statusCode: 400,
                    instance: "/test/adminuser", // Link send request
                    title: "title", // Short title about error
                    type: "type" // link to page can identifies problem
                );
            }
            // HttpContext.Response.Headers.Add("das", "Dasd");
            if (users.Count > 1) {
                __Logger.Warning($"Find duplicate user_id: {id}");
            }
            return Ok(users[0].ToJsonString());
        }
    }
}
