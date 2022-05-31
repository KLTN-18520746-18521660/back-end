using Common;
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
    public class SocialAuditLogManagement : BaseTransientService
    {
        public SocialAuditLogManagement(IServiceProvider _IServiceProvider) : base(_IServiceProvider)
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
            log.OldValue = new LogValue(Utils.CensorSensitiveDate(OldValue));
            log.NewValue = new LogValue(Utils.CensorSensitiveDate(NewValue));

            await __DBContext.SocialAuditLogs.AddAsync(log);
            await __DBContext.SaveChangesAsync();
        }

        public async Task AddNewAuditLog(SocialAuditLog AuditLog) {
            await __DBContext.SocialAuditLogs.AddAsync(AuditLog);
            await __DBContext.SaveChangesAsync();
        }
    }
}