using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseAccess.Contexts.ConfigDB.ParserModels;
using DatabaseAccess.Contexts.ConfigDB.Models;
using DatabaseAccess.Contexts.ConfigDB;
using System.Data.Entity;
using System.Diagnostics;

namespace CoreApi.Controllers.Test
{
    [ApiController]
    [Route("[controller]/test/baseconfig")]
    public class TestBaseConfigController : BaseController
    {
        private DBContext __ConfigDB;
        public TestBaseConfigController(
            DBContext ConfigDB
        ) : base()
        {
            __ConfigDB = ConfigDB;
            __ControllerName = "TestBaseConfigController";
        }

        [HttpPost]
        public IActionResult CreateBaseConfig(ParserBaseConfig Parser)
        {
            var traceId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
            __Logger.Information($"CreateBaseConfig, TraceId {traceId}");
            BaseConfig BaseConfigModel = new BaseConfig();
            BaseConfigModel.Parse(Parser);

            __ConfigDB.BaseConfigs.Add(BaseConfigModel);
            try {
                if (__ConfigDB.SaveChanges() > 0) {
                    __Logger.Information($"CreateBaseConfig. Successfulley. base_config_key: {BaseConfigModel.ConfigKey}");
                    return Ok(BaseConfigModel.ToJsonString());
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

            __Logger.Information("CreateBaseConfig");

            return Problem(
                detail: "detail", // Error detail
                statusCode: 500,
                instance: "/test/baseconfig", // Link send request
                title: "title", // Short title about error
                type: "type" // link to page can identifies problem
            );
        }

        [HttpGet]
        public IActionResult GetBaseConfigById(string id)
        {
            var traceId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
            __Logger.Information($"GetBaseConfigById, TraceId {traceId}");

            var configs = __ConfigDB.BaseConfigs.Where<BaseConfig>(e => e.Id.ToString() == id).ToList();
            if (configs.Count < 1) {
                return Problem(
                    detail: "BaseConfig not found", // Error detail
                    statusCode: 400,
                    instance: "/test/baseconfig", // Link send request
                    title: "title", // Short title about error
                    type: "type" // link to page can identifies problem
                );
            }
            
            if (configs.Count > 1) {
                __Logger.Warning($"Find duplicate user_id: {id}");
            }
            return Ok(configs[0].ToJsonString());
        }
    }
}
