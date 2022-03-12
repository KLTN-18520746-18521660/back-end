using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseAccess.Context;
using Newtonsoft.Json.Linq;
using CoreApi.Common;
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


        [HttpGet]
        public IActionResult GetUserAdminById()
        {
            ObjectResult obj = new(new JObject(){
                { "status", 200 },
            });
            obj.StatusCode = 500;

            return obj;
        }
    }
}
