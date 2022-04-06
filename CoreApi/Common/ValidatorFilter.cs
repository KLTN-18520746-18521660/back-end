using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi.Common
{
    public class ValidatorFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid) {
                var errorsInModelState = context.ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(p => p.ErrorMessage)).ToArray();
                
                var errResp = new JObject();
                foreach (var error in errorsInModelState) {
                    errResp.Add(error.Key, error.Value.First());
                }

                var resp = new BadRequestObjectResult(new JObject() {
                    { "status", 400 },
                    { "message",  JToken.FromObject(errResp) },
                });
                resp.StatusCode = 400;
                context.Result = resp;
                return;
            }
            await next();
        }
    }
}