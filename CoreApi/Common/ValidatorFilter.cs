using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

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
                    // var firstErr = error.Value.First();
                    errResp.Add(error.Key, error.Value.First());
                    // foreach(var subError in error.Value) {
                    //     errors.Add(new JObject(){
                    //         { "Field name", error.Key },
                    //         { "Error", subError },
                    //     });
                    // }
                }

                var resp = new BadRequestObjectResult(new JObject(){
                    { "status", 400 },
                    { "error",  JToken.FromObject(errResp) },
                });
                resp.StatusCode = 400;
                context.Result = resp;
                return;
            }
            await next();
        }
    }
}