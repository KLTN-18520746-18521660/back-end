using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using Common;

namespace CoreApi.Common.Middlerware
{
    public class ValidatorFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext Context, ActionExecutionDelegate Next)
        {
            if (!Context.ModelState.IsValid) {
                var ErrorsInModelState = Context.ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(p => p.ErrorMessage)).ToArray();
                
                var ErrResp = new JObject();
                foreach (var Error in ErrorsInModelState) {
                    ErrResp.Add(Error.Key, Error.Value.First());
                }

                var Resp = new BadRequestObjectResult(new JObject() {
                    { "status",         400 },
                    { "message",        RESPONSE_MESSAGES.INVALID_REQUEST_BODY.GetMessage() },
                    { "message_code",   RESPONSE_MESSAGES.INVALID_REQUEST_BODY.CODE },
                    { "data",           JToken.FromObject(ErrResp) },
                });
                Resp.StatusCode = 400;
                Context.Result = Resp;
                return;
            }
            await Next();
        }
    }
}