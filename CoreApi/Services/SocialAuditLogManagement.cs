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
    public class SocialAuditLogManagement : BaseTransientService
    {
        public SocialAuditLogManagement(DBContext _DBContext,
                                        IServiceProvider _IServiceProvider)
            : base(_IServiceProvider)
        {
            __ServiceName = "SocialAuditLogManagement";
        }

        public async Task<(List<SocialAuditLog> AuditLogs, int TotalSize)> GetAuditLogs(int Start, int Size, string SearchTerm = default)
        {
            if (SearchTerm == default || SearchTerm == string.Empty) {
                return 
                (
                    await __DBContext.SocialAuditLogs
                        .OrderBy(e => e.Id)
                        .Skip(Start)
                        .Take(Size)
                        .ToListAsync(),
                    await __DBContext.SocialAuditLogs.CountAsync()
                );
            }
            return
            (
                await __DBContext.SocialAuditLogs
                    .Where(e => e.SearchVector.Matches(SearchTerm))
                    .OrderBy(e => e.Id)
                    .Skip(Start)
                    .Take(Size)
                    .ToListAsync(),
                await __DBContext.SocialAuditLogs
                    .CountAsync(e => e.SearchVector.Matches(SearchTerm))
            );
        }

        public async Task AddNewAuditLog(
            string TableName,
            string TableKey,
            string Action,
            Guid UserId,
            JObject OldValue,
            JObject NewValue
        ) {
            SocialAuditLog log = new SocialAuditLog();
            log.Table = TableName;
            log.TableKey = TableKey;
            log.Action = Action;
            log.UserId = UserId;
            log.OldValue = new LogValue(OldValue);
            log.NewValue = new LogValue(NewValue);

            await __DBContext.SocialAuditLogs.AddAsync(log);
            await __DBContext.SaveChangesAsync();
        }

        public async Task AddNewAuditLog(SocialAuditLog AuditLog) {
            await __DBContext.SocialAuditLogs.AddAsync(AuditLog);
            await __DBContext.SaveChangesAsync();
        }
    }
}