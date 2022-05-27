using Common;
using CoreApi.Common.Interface;
using DatabaseAccess.Context;
using Serilog;
using Serilog.Events;
using System;
using System.Text;

namespace CoreApi.Common.Base
{
    public class BaseSingletonService : IBaseService
    {
        private static readonly string[] TEMPLATE_MESSAGE = new string[]{
            "TraceId: {0}, Handler: {1}",
            "Handler: {0}"
        };
        private ILogger __Logger;
        protected string __ServiceName;
        protected IServiceProvider __ServiceProvider;
        public string ServiceName { get => __ServiceName; }
        public BaseSingletonService(IServiceProvider _IServiceProvider)
        {
            __Logger            = Log.Logger;
            __ServiceProvider   = _IServiceProvider;
            // __ServiceName = "BaseSingletonService";
            __ServiceName       = Utils.GetHandlerNameFromClassName(this.GetType().Name);
        }
        #region Loging
        protected virtual string CreateLogMessage(string Msg, string TraceId, params string[] Params)
        {
            var _Msg        = new StringBuilder();
            var ParamsStr   = string.Join(", ", Params);
            var PrefixMsg   = string.Empty;
            if (TraceId != default && TraceId != string.Empty) {
                PrefixMsg = string.Format(TEMPLATE_MESSAGE[0], TraceId, ServiceName);
            } else {
                PrefixMsg = string.Format(TEMPLATE_MESSAGE[1], ServiceName);
            }
            _Msg.Append(PrefixMsg).Append(", Message: ").Append(Msg);
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
    }
}