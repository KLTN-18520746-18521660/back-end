using Common;
using CoreApi.Common;
using CoreApi.Common.Base;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi.Services
{
    public class RedirectUrlManagement : BaseTransientService
    {
        public RedirectUrlManagement(IServiceProvider _IServiceProvider) : base(_IServiceProvider)
        {
            __ServiceName = "RedirectUrlManagement";
        }

        public async Task<(RedirectUrl, ErrorCodes)> GetUrl(string Url)
        {
            var item = await __DBContext.RedirectUrls.Where(e => e.Url == Url).FirstOrDefaultAsync();
            if (item == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }
            return (item, ErrorCodes.NO_ERROR);
        }

        public async Task<ErrorCodes> IncreaseTimesGoToUrl(string Url)
        {
            var (item, error) = await GetUrl(Url);
            if (error == ErrorCodes.NO_ERROR) {
                item.Times++;
            } else {
                __DBContext.RedirectUrls.Add(
                    new RedirectUrl(){
                        Url = Url,
                        Times = 1
                    }
                );
            }

            if (__DBContext.SaveChanges() > 0) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
    }
}