using CoreApi.Common;
using CoreApi.Common.Base;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using System;
using System.Threading.Tasks;

namespace CoreApi.Services
{
    public class SocialReportManagement : BaseTransientService
    {
        public SocialReportManagement(DBContext _DBContext,
                                    IServiceProvider _IServiceProvider)
            : base(_IServiceProvider)
        {
            __ServiceName = "SocialReportManagement";
        }

        #region Report handle
        public async Task<ErrorCodes> AddNewReport(SocialReport Report)
        {
            await __DBContext.SocialReports.AddAsync(Report);
            if (await __DBContext.SaveChangesAsync() > 0) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        #endregion
    }
}