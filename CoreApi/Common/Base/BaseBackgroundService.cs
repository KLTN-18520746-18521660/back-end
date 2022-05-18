using Common;
using CoreApi.Common.Interface;
using DatabaseAccess.Context;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreApi.Common.Base
{
    public class BaseBackgroundService : BackgroundService, IBaseService
    {
        private ILogger             __Logger;
        protected string            __ServiceName;
        protected IServiceProvider  __ServiceProvider;
        public string ServiceName   { get => __ServiceName; }
        public BaseBackgroundService(IServiceProvider _IServiceProvider)
        {
            __Logger            = Log.Logger;
            __ServiceProvider   = _IServiceProvider;
            // __ServiceName       = "BaseBackgroundService";
            __ServiceName       = Utils.GetHandlerNameFromClassName(this.GetType().Name);
        }

        #region Loging
        protected virtual string CreateLogMessage(string Msg, string TraceId, params string[] Params)
        {
            var _Msg        = new StringBuilder(string.Format("TraceId: {0}, Handler: {1}", TraceId, ServiceName));
            var ParamsStr   = string.Join(", ", Params);
            _Msg.Append(", Message: ").Append(Msg);
            if (ParamsStr != string.Empty) {
                _Msg.Append(", ").Append(ParamsStr);
            }
            return _Msg.ToString();
        }
        protected virtual void WriteLog(LOG_LEVEL Level, string TraceId, string Message, params string[] CustomParams)
        {
            if (Message == string.Empty) {
                return;
            }
            var CustomParamsStr = new StringBuilder(string.Join(", ", CustomParams));
            __Logger.Write((LogEventLevel) Level, CreateLogMessage(Message, TraceId, CustomParamsStr.ToString()));
        }
        #endregion

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}