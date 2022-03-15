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
    public class AdminAuditLogManagement : BaseService
    {
        public AdminAuditLogManagement() : base()
        {
            __ServiceName = "AdminAuditLogManagement";
        }

        public async Task<(List<AdminAuditLog> AuditLogs, int TotalSize)> GetAllAuditLog(int Start, int Size, string SearchTerm = null)
        {
            if (SearchTerm == null || SearchTerm == "") {
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
                    .Where(e => e.SearchVector.Matches(SearchTerm))
                    .OrderBy(e => e.Id)
                    .Skip(Start)
                    .Take(Size)
                    .ToListAsync(),
                await __DBContext.AdminAuditLogs
                    .CountAsync(e => e.SearchVector.Matches(SearchTerm))
            );
        }

        public async Task AddAuditLog(
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
            log.OldValue = new LogValue(OldValue);
            log.NewValue = new LogValue(NewValue);

            __DBContext.AdminAuditLogs.Add(log);
            await __DBContext.SaveChangesAsync();
        }

        public async Task AddAuditLog(AdminAuditLog AuditLog) {
            __DBContext.AdminAuditLogs.Add(AuditLog);
            await __DBContext.SaveChangesAsync();
        }
    }
}