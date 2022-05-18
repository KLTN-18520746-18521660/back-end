using Common;
using CoreApi.Common.Base;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CoreApi.Services.Background
{
    public enum RequestToSendEmailType {
        UserSignup          = 0,
        ForgotPassword      = 1,
    }

    public class EmailChannel
    {
        public RequestToSendEmailType Type { get; set; }
        public JObject Data { get; set; }
        public string TraceId { get; set; }
    }

    public class EmailDispatcher : BaseBackgroundService
    {
        private readonly Channel<EmailChannel> __Channel;
        public EmailDispatcher(IServiceProvider _IServiceProvider,
                               Channel<EmailChannel> _Channel)
            : base(_IServiceProvider)
        {
            // __ServiceName = "EmailDispatcher";
            __Channel = _Channel;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!__Channel.Reader.Completion.IsCompleted)
            {
                var reqToSendEmail = await __Channel.Reader.ReadAsync();
                var input = GetValueFromReq(reqToSendEmail.Data, reqToSendEmail.Type);
                if (input == default) {
                    throw new Exception($"TraceId: { reqToSendEmail.TraceId }, Invalid input: { reqToSendEmail.Data.ToString() }");
                }

                try {
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();
                        switch(reqToSendEmail.Type) {
                            case RequestToSendEmailType.UserSignup:
                                _ = emailSender.SendEmailUserSignUp(input.UserId, reqToSendEmail.TraceId);
                                break;
                            case RequestToSendEmailType.ForgotPassword:
                                _ = emailSender.SendEmailUserForgotPassword(input.UserId, reqToSendEmail.TraceId, input.IsAdminUser);
                                break;
                            default:
                                throw new Exception($"TraceId: { reqToSendEmail.TraceId }, Invalid Type { reqToSendEmail.Type }");
                        }
                    }
                } catch (Exception e) {
                    WriteLog(LOG_LEVEL.ERROR, reqToSendEmail.TraceId, "Unhandle exception",
                        Utils.ParamsToLog("exception_message", e.ToString())
                    );
                }
            }
        }

        protected (Guid UserId, bool IsAdminUser) GetValueFromReq(JObject Data, RequestToSendEmailType Type)
        {
            switch(Type) {
                case RequestToSendEmailType.UserSignup:
                case RequestToSendEmailType.ForgotPassword:
                    return (Data.Value<Guid>("UserId"), Data.Value<bool>("IsAdminUser"));
                default:
                    return default;
            }
        }
    }
}