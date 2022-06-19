using Common;
using CoreApi.Common.Interface;
using DatabaseAccess.Context;
using Serilog;
using Serilog.Events;
using System;
using System.Text;

namespace CoreApi.Common.Base
{
    public class BaseTransientService : IBaseService
    {
        private ILogger __Logger;
        protected string __ServiceName;
        protected string __TraceId;
        protected DBContext __DBContext;
        protected IServiceProvider __ServiceProvider;
        public string ServiceName { get => __ServiceName; }
        public string TraceId { get => __TraceId; }
        public BaseTransientService(IServiceProvider _IServiceProvider)
        {
            __Logger            = Log.Logger;
            __TraceId           = string.Empty;
            __DBContext         = new DBContext();
            __ServiceProvider   = _IServiceProvider;
            // __ServiceName = "BaseTransientService";
            __ServiceName       = Utils.GetHandlerNameFromClassName(this.GetType().Name);
        }
        public virtual void SetTraceId(string TraceId)
        {
            __TraceId = TraceId;
        }
        #region Loging
        protected virtual string CreateLogMessage(string Msg, params string[] Params)
        {
            var _Msg        = new StringBuilder(string.Format("TraceId: {0}, Handler: {1}", TraceId, ServiceName));
            var ParamsStr   = string.Join(", ", Params);
            _Msg.Append(", Message: ").Append(Msg);
            if (ParamsStr != string.Empty) {
                _Msg.Append(", ").Append(ParamsStr);
            }
            return _Msg.ToString();
        }
        protected virtual void WriteLog(LOG_LEVEL Level, string Message, params string[] CustomParams)
        {
            if (Message == string.Empty) {
                return;
            }
            var CustomParamsStr = new StringBuilder(string.Join(", ", CustomParams));
            __Logger.Write((LogEventLevel) Level, CreateLogMessage(Message, CustomParamsStr.ToString()));
        }
        #endregion
    }
}