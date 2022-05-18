

using Common;

namespace CoreApi.Common.Interface
{
    public interface IBaseService
    {
        protected virtual string CreateLogMessage()
        {
            throw new System.NotImplementedException("CreateLogMessage");
        }
        protected virtual void WriteLog()
        {
            throw new System.NotImplementedException("WriteLog");
        }
    }
}