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
using CoreApi.Services;

#if DEBUG
namespace CoreApi.Controllers.Test
{
    [ApiController]
    [Route("/api/test")]
    public class TestController : BaseController
    {
        public TestController(BaseConfig _BaseConfig) : base(_BaseConfig) {
            ControllerName = "TestController";
        }

        [HttpGet]
        public IActionResult GET()
        {
            return Ok(200, "OK");
        }

        [HttpPut]
        public IActionResult PUT()
        {
            return Ok(200, "OK");
        }

        [HttpPost]
        public IActionResult POST(TestModel __ModelData)
        {
            return Ok(200, "OK", JObject.FromObject(__ModelData));
        }

        [HttpDelete]
        public IActionResult DELETE()
        {
            return Ok(200, "OK");
        }
    }
}
#endif
