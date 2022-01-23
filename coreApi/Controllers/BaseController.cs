using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace coreApi.Controllers
{
    [Controller]
    public class BaseController : ControllerBase
    {
        protected string __ControllerName;
        protected ILogger __Logger;
        public string ControllerName { get => __ControllerName; }
        public BaseController()
        {
            __Logger = Log.Logger;
            __ControllerName = "BaseController";
        }
    }
}