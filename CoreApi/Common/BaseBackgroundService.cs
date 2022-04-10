using CoreApi.Common.Interface;
using DatabaseAccess.Context;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreApi.Common
{
    public class BaseBackgroundService : BackgroundService, IBaseService
    {
        private ILogger __Logger;
        protected string __ServiceName;
        protected IServiceProvider __ServiceProvider;
        public string ServiceName { get => __ServiceName; }
        public BaseBackgroundService(IServiceProvider _IServiceProvider)
        {
            __Logger = Log.Logger;
            __ServiceProvider = _IServiceProvider;
            __ServiceName = "BaseBackgroundService";
        }

        public BaseBackgroundService()
        {
        }

        protected virtual string CreateLogMessage(string Msg)
        {
            string msgFormat = $"BackgroundService: { __ServiceName }";
            StringBuilder msg = new StringBuilder(msgFormat);
            msg.Append(", ").Append(Msg);
            return msg.ToString();
        }
        protected virtual void LogDebug(string Msg)
        {
            if (Msg != string.Empty) {
                __Logger.Debug(CreateLogMessage(Msg));
            }
        }
        protected virtual void LogInformation(string Msg)
        {
            if (Msg != string.Empty) {
                __Logger.Information(CreateLogMessage(Msg));
            }
        }
        protected virtual void LogWarning(string Msg)
        {
            if (Msg != string.Empty) {
                __Logger.Warning(CreateLogMessage(Msg));
            }
        }
        protected virtual void LogError(string Msg)
        {
            if (Msg != string.Empty) {
                __Logger.Error(CreateLogMessage(Msg));
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}