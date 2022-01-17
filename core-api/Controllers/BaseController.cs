

using Microsoft.AspNetCore.Mvc;

namespace core_api.Controllers
{
    [Controller]
    public class BaseController : ControllerBase
    {
        protected string __ControllerName;
        public string ControllerName { get => __ControllerName; }
        public BaseController()
        {
            __ControllerName = "BaseController";
        }
    }
}