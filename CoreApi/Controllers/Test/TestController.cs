using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseAccess.Context;
using Newtonsoft.Json.Linq;
using CoreApi.Common.Base;
using CoreApi.Common;
using Common;
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
        }

        [HttpGet]
        public IActionResult GET()
        {
            #region Init handler
            SetRunningFunction();
            #endregion
            return Ok(200, "OK");
        }

        [HttpPut]
        public IActionResult PUT()
        {
            #region Init handler
            SetRunningFunction();
            #endregion
            return Ok(200, "OK");
        }

        [HttpPost]
        public IActionResult POST(TestModel __ModelData)
        {
            #region Init handler
            SetRunningFunction();
            #endregion

            __ModelData.test_fied = "modify";
            WriteLog(LOG_LEVEL.FATAL, false, "This is message");
            WriteLog(LOG_LEVEL.ERROR, true, "This is message");
            return Ok(200, "OK", JObject.FromObject(__ModelData));
        }

        [HttpDelete]
        public IActionResult DELETE()
        {
            #region Init handler
            SetRunningFunction();
            #endregion
            return Ok(200, "OK");
        }
    }
}
#endif
