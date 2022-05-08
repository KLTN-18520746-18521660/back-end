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

namespace CoreApi.Common
{
    // From [https://github.com/domaindrivendev/Swashbuckle.WebApi/issues/384#issuecomment-410117400]
    public class SwaggerBasicAuthMiddleware
    {
        private readonly RequestDelegate next;

        public SwaggerBasicAuthMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try {
                //Make sure we are hitting the swagger path, and not doing it locally as it just gets annoying :-)
                if (context.Request.Path.StartsWithSegments(Program.SwaggerDocumentConfiguration.Path)) {
                    string authHeader = context.Request.Headers["Authorization"];
                    if (authHeader != null && authHeader.StartsWith("Basic ")) {
                        // Get the encoded username and password
                        var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();

                        // Decode from Base64 to string
                        var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));

                        // Split username and password
                        var username = decodedUsernamePassword.Split(':', 2)[0];
                        var password = decodedUsernamePassword.Split(':', 2)[1];

                        // Check if login is correct
                        if (IsAuthorized(username, password)) {
                            await next.Invoke(context);
                            return;
                        }
                    }

                    // Return authentication type (causes browser to show login dialog)
                    context.Response.Headers["WWW-Authenticate"] = "Basic";

                    // Return unauthorized
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    // await context.ChallengeAsync();
                } else {
                    await next.Invoke(context);
                }
            } catch (Exception ex) {
                Serilog.Log.Logger.Error(CreateLogMessage(context, ex.ToString()));
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            if (!context.Response.HasStarted) {
                string result;

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                result = JsonConvert.SerializeObject(new { error = "An error has occured" });

                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(result);
            } else {
                return context.Response.WriteAsync(string.Empty);
            }
        }

        public bool IsAuthorized(string username, string password)
        {
            // Check that username and password are correct
            return username.Equals(Program.SwaggerDocumentConfiguration.Username)
                && password.Equals(Program.SwaggerDocumentConfiguration.Password);
        }

        protected virtual string CreateLogMessage(HttpContext context, string Msg)
        {
            var __TraceId = Activity.Current?.Id ?? context?.TraceIdentifier;
            StringBuilder msg = new StringBuilder($"Path: { context.Request.Path }, TraceId: { __TraceId }");
            msg.Append(", ").Append(Msg);
            return msg.ToString();
        }
    }
    public static class SwaggerAuthorizeExtensions
    {
        public static IApplicationBuilder UseSwaggerAuthorized(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SwaggerBasicAuthMiddleware>();
        }
    }



    // public class SwaggerAccessMessageHandler : DelegatingHandler
    // {
    //     protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    //     {
    //         if (IsSwagger(request)) {
    //             IEnumerable<string> authHeaderValues = null;

    //             request.Headers.TryGetValues("Authorization", out authHeaderValues);
    //             var authHeader = authHeaderValues?.FirstOrDefault();

    //             if (authHeader != null && authHeader.StartsWith("Basic ")) {
    //                 // Get the encoded username and password
    //                 var encodedUsernamePassword = authHeader.Split(' ')[1]?.Trim();

    //                 // Decode from Base64 to string
    //                 var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));

    //                 // Split username and password
    //                 var username = decodedUsernamePassword.Split(':')[0];
    //                 var password = decodedUsernamePassword.Split(':')[1];

    //                 // Check if login is correct
    //                 if (IsAuthorized(username, password)) {
    //                     return await base.SendAsync(request, cancellationToken);
    //                 }
    //             }

    //             var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
    //             //response.Headers.Location = new Uri("http://www.google.com.au");
    //             response.Headers.Add("WWW-Authenticate", "Basic");


    //             return response;
    //         } else {
    //             return await base.SendAsync(request, cancellationToken);
    //         }
    //     }

    //     public bool IsAuthorized(string username, string password)
    //     {
    //         // Check that username and password are correct
    //         return username.Equals(Program.SwaggerDocumentConfiguration.Username)
    //             && password.Equals(Program.SwaggerDocumentConfiguration.Password);
    //     }

    //     private bool IsSwagger(HttpRequestMessage request)
    //     {
    //         return request.RequestUri.PathAndQuery.StartsWith(Program.SwaggerDocumentConfiguration.Path, StringComparison.OrdinalIgnoreCase);
    //     }
    // }
}