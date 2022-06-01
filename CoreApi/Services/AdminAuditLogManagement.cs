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
    public class AdminAuditLogManagement : BaseTransientService
    {
        public AdminAuditLogManagement(IServiceProvider _IServiceProvider) : base(_IServiceProvider)
        {
            __ServiceName = "AdminAuditLogManagement";
        }

        public async Task<(List<AdminAuditLog> AuditLogs, int TotalSize)> GetAuditLogs(int Start, int Size, string SearchTerm = default)
        {
            if (SearchTerm == default || SearchTerm == string.Empty) {
                return 
                (
                    await __DBContext.AdminAuditLogs
                        .OrderBy(e => e.Id)
                        .Skip(Start)
                        .Take(Size)
                        .ToListAsync(),
                    await __DBContext.AdminAuditLogs.CountAsync()
                );
            }
            return
            (
                await __DBContext.AdminAuditLogs
                    .Where(e => e.SearchVector.Matches(SearchTerm.Trim()))
                    .OrderBy(e => e.Id)
                    .Skip(Start)
                    .Take(Size)
                    .ToListAsync(),
                await __DBContext.AdminAuditLogs
                    .CountAsync(e => e.SearchVector.Matches(SearchTerm.Trim()))
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
            AdminAuditLog log = new AdminAuditLog();
            log.Table = TableName;
            log.TableKey = TableKey;
            log.Action = Action;
            log.UserId = UserId;
            log.OldValue = new LogValue(Utils.CensorSensitiveDate(OldValue));
            log.NewValue = new LogValue(Utils.CensorSensitiveDate(NewValue));

            await __DBContext.AdminAuditLogs.AddAsync(log);
            await __DBContext.SaveChangesAsync();
        }

        public async Task AddNewAuditLog(AdminAuditLog AuditLog) {
            await __DBContext.AdminAuditLogs.AddAsync(AuditLog);
            await __DBContext.SaveChangesAsync();
        }
    }
}