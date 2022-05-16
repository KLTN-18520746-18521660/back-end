using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;
using System.IO;
using Common;
using Newtonsoft.Json.Linq;

namespace CoreApi.Common
{
    public class HandleBodyRequestMiddleware
    {
        private readonly RequestDelegate    Next;

        public HandleBodyRequestMiddleware(RequestDelegate Next)
        {
            this.Next = Next;
        }

        public async Task InvokeAsync(HttpContext Context)
        {
            var Stream      = Context.Request.Body;// currently holds the original stream
            var OriginBody  = await (new StreamReader(Stream).ReadToEndAsync());
            var NotModified = true;
            try {
                if (Context.Request.Path.StartsWithSegments("/api")
                    && (Context.Request.Method == Common.HTTP_METHODS.POST || Context.Request.Method == Common.HTTP_METHODS.PUT)
                    && !Context.Request.Path.StartsWithSegments("/api/upload")
                ) {
                    if (Utils.IsJsonArray(OriginBody) || Utils.IsJsonObject(OriginBody)) {
                        var ModifiedBody    = Utils.TrimJsonBodyRequest(OriginBody).ToString(Formatting.None);
                        var RequestContent  = new StringContent(ModifiedBody, Encoding.UTF8, "application/json");
                        Stream = await RequestContent.ReadAsStreamAsync();  //modified stream
                        NotModified = false;
                    }
                }
            } catch (System.Exception Ex) {
                Serilog.Log.Logger.Error(CreateLogMessage(Context, Ex.ToString()));
            }

            if (NotModified) {
                //put original data back for the downstream to read
                var requestData = Encoding.UTF8.GetBytes(OriginBody);
                Stream = new MemoryStream(requestData);
            }
            Context.Request.Body = Stream;
            await Next.Invoke(Context);
        }

        protected virtual string CreateLogMessage(HttpContext context, string Msg)
        {
            var __TraceId = Activity.Current?.Id ?? context?.TraceIdentifier;
            StringBuilder msg = new StringBuilder($"Path: { context.Request.Path }, TraceId: { __TraceId }");
            msg.Append(", ").Append(Msg);
            return msg.ToString();
        }
    }
    public static class HandleBodyRequestExtensions
    {
        public static IApplicationBuilder UseHandleTrimBodyRequest(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HandleBodyRequestMiddleware>();
        }
    }
}
