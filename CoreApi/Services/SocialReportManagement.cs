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

        public async Task<(List<SocialTag>, int)> GetTags(int start = 0,
                                                          int size = 20,
                                                          string search_term = default,
                                                          Guid socialUserId = default)
        {
            var query =
                    from ids in (
                        (from tag in __DBContext.SocialTags
                            .Where(e => e.StatusStr != BaseStatus.StatusToString(SocialTagStatus.Disabled, EntityStatus.SocialTagStatus)
                                    && (search_term == default || (search_term != default && e.Tag.Contains(search_term))))
                        join action in __DBContext.SocialUserActionWithTags on tag.Id equals action.TagId
                        into tagWithAction
                        from t in tagWithAction.DefaultIfEmpty()
                        group t by new { tag.Id, tag.Tag } into gr
                        select new {
                            gr.Key,
                            StartWith = (search_term != default) ? (gr.Key.Tag.StartsWith(search_term) ? 1 : 0) : 0,
                            Follow = gr.Count(e => (socialUserId != default)
                                && (e.UserId == socialUserId)
                                && EF.Functions.JsonExists(e.ActionsStr, BaseAction.ActionToString(UserActionWithTag.Follow, EntityAction.UserActionWithTag))),
                            Used = gr.Count(e => (socialUserId != default)
                                && (e.UserId == socialUserId)
                                && EF.Functions.JsonExists(e.ActionsStr, BaseAction.ActionToString(UserActionWithTag.Used, EntityAction.UserActionWithTag))),
                            Visited = gr.Count(e => (socialUserId != default)
                                && (e.UserId == socialUserId)
                                && EF.Functions.JsonExists(e.ActionsStr, BaseAction.ActionToString(UserActionWithTag.Visited, EntityAction.UserActionWithTag))),
                        } into ret
                        orderby ret.Visited descending, ret.Used descending, ret.Follow descending, ret.StartWith descending
                        select ret.Key.Id).Skip(start).Take(size)
                    )
                    join tags in __DBContext.SocialTags on ids equals tags.Id
                    select tags;

            var totalCount = await __DBContext.SocialTags
                            .CountAsync(e => e.StatusStr != BaseStatus.StatusToString(SocialTagStatus.Disabled, EntityStatus.SocialTagStatus)
                                    && (search_term == default || (search_term != default && e.Tag.Contains(search_term))));

            return (await query.ToListAsync(), totalCount);
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