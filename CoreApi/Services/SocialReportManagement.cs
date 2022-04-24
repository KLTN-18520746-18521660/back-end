using Serilog;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using NpgsqlTypes;
using Microsoft.EntityFrameworkCore;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Common.Models;
using CoreApi.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DatabaseAccess.Common.Actions;
using Common;
using DatabaseAccess.Context.ParserModels;

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