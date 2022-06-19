using DatabaseAccess.Common.Models;

namespace CoreApi.Common.Interface
{
    public interface IModifyModel
    {
        public abstract ErrorCodes GetDataChange(BaseModel model);
    }
}