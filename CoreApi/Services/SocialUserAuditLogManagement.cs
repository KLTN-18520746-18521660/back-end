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

namespace CoreApi.Services
{
    public class SocialUserAuditLogManagement : BaseTransientService
    {
        public SocialUserAuditLogManagement(DBContext _DBContext,
                                        IServiceProvider _IServiceProvider)
            : base(_IServiceProvider)
        {
            __ServiceName = "SocialUserAuditLogManagement";
        }

        public async Task<(List<SocialUserAuditLog> AuditLogs, int TotalSize)> GetAuditLogs(Guid UserId,
                                                                                            string Action,
                                                                                            int Start,
                                                                                            int Size,
                                                                                            string SearchTerm = default)
        {
            string ActionStr = string.Empty;
            switch (Action) {
                case "comment":
                    ActionStr = "SocialComment";
                    break;
                case "post":
                    ActionStr = "SocialPost";
                    break;
                case "user":
                    ActionStr = "SocialUser";
                    break;
            }

            return
            (
                await __DBContext.SocialUserAuditLogs
                    .Where(
                        e => e.UserId == UserId
                        && e.Table == ActionStr
                        && (SearchTerm == default) || e.SearchVector.Matches(SearchTerm)
                    )
                    .OrderBy(e => e.Id)
                    .Skip(Start)
                    .Take(Size)
                    .ToListAsync(),
                await __DBContext.SocialUserAuditLogs
                    .CountAsync(
                        e => e.UserId == UserId
                        && (SearchTerm == default) || e.SearchVector.Matches(SearchTerm)
                    )
            );
        }

        public async Task AddNewUserAuditLog(
            string TableName,
            string TableKey,
            string Action,
            Guid UserId,
            JObject OldValue,
            JObject NewValue,
            Guid AdminUserId = default
        ) {
            SocialUserAuditLog log = new SocialUserAuditLog();
            log.Table = TableName;
            log.TableKey = TableKey;
            log.Action = Action;
            log.UserId = UserId;
            if (AdminUserId != default) {
                log.AdminUserId = AdminUserId;
            }
            log.OldValue = new LogValue(OldValue);
            log.NewValue = new LogValue(NewValue);

            await __DBContext.SocialUserAuditLogs.AddAsync(log);
            await __DBContext.SaveChangesAsync();
        }

        public async Task AddNewUserAuditLog(SocialUserAuditLog AuditLog) {
            await __DBContext.SocialUserAuditLogs.AddAsync(AuditLog);
            await __DBContext.SaveChangesAsync();
        }
    }
}