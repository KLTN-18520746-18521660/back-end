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
        protected DBContext __DBContext;
        public AdminAuditLogManagement() : base()
        {
            __DBContext = new DBContext();
            __ServiceName = "AdminAuditLogManagement";
        }

        public async Task<(List<AdminAuditLog> AuditLogs, int TotalSize)> GetAllAuditLog(int Start, int Size, string SearchTerm = null)
        {
            var TotalSize = 0;
            if (SearchTerm == null || SearchTerm == "") {
                TotalSize = await __DBContext.AdminAuditLogs.CountAsync();
                return (await __DBContext.AdminAuditLogs
                    .OrderBy(e => e.Id)
                    .Skip(Start)
                    .Take(Size)
                    .ToListAsync(), TotalSize);
            }
            TotalSize = await __DBContext.AdminAuditLogs
                .CountAsync(e => e.SearchVector.Matches(SearchTerm));
            return (await __DBContext.AdminAuditLogs
                .Where(e => e.SearchVector.Matches(SearchTerm))
                .OrderBy(e => e.Id)
                .Skip(Start)
                .Take(Size)
                .ToListAsync(), TotalSize);
        }

        public void AddAuditLog(
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
            __DBContext.SaveChanges();
        }

        public void AddAuditLog(AdminAuditLog AuditLog) {
            __DBContext.AdminAuditLogs.Add(AuditLog);
            __DBContext.SaveChanges();
        }
    }
}