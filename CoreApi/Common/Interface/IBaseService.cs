

namespace CoreApi.Common.Interface
{
    public interface IBaseService
    {
        protected virtual string CreateLogMessage(string Msg)
        {
            throw new System.NotImplementedException("CreateLogMessage");
        }
        protected virtual void LogDebug(string Msg)
        {
            throw new System.NotImplementedException("LogDebug");
        }
        protected virtual void LogInformation(string Msg)
        {
            throw new System.NotImplementedException("LogInformation");
        }
        protected virtual void LogWarning(string Msg)
        {
            throw new System.NotImplementedException("LogWarning");
        }
        protected virtual void LogError(string Msg)
        {
            throw new System.NotImplementedException("LogError");
        }
    }
}