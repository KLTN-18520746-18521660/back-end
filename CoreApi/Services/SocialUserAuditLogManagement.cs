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
    public class SocialUserAuditLogManagement : BaseService
    {
        public SocialUserAuditLogManagement(DBContext _DBContext,
                                        IServiceProvider _IServiceProvider)
            : base(_DBContext, _IServiceProvider)
        {
            __ServiceName = "SocialUserAuditLogManagement";
        }

        public async Task<(List<SocialUserAuditLog> AuditLogs, int TotalSize)> GetAllAuditLog(Guid UserId, int Start, int Size, string SearchTerm = null)
        {
            if (SearchTerm == null || SearchTerm == "") {
                return
                (
                    await __DBContext.SocialUserAuditLogs
                        .Where(e => e.UserId == UserId)
                        .OrderBy(e => e.Id)
                        .Skip(Start)
                        .Take(Size)
                        .ToListAsync(),
                    await __DBContext.SocialUserAuditLogs.CountAsync(e => e.UserId == UserId)
                );
            }
            return
            (
                await __DBContext.SocialUserAuditLogs
                    .Where(e => e.SearchVector.Matches(SearchTerm) && e.UserId == UserId)
                    .OrderBy(e => e.Id)
                    .Skip(Start)
                    .Take(Size)
                    .ToListAsync(),
                await __DBContext.SocialUserAuditLogs
                    .CountAsync(e => e.SearchVector.Matches(SearchTerm) && e.UserId == UserId)
            );
        }

        public async Task AddNewUserAuditLog(
            string TableName,
            string TableKey,
            string Action,
            Guid UserId,
            JObject OldValue,
            JObject NewValue
        ) {
            SocialUserAuditLog log = new SocialUserAuditLog();
            log.Table = TableName;
            log.TableKey = TableKey;
            log.Action = Action;
            log.UserId = UserId;
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