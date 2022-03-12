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

        public List<AdminAuditLog> GetAllAuditLog(out int TotalSize, int Start, int Size, string SearchTerm = null)
        {
            if (SearchTerm == null || SearchTerm == "") {
                TotalSize = __DBContext.AdminAuditLogs.Count();
                return __DBContext.AdminAuditLogs
                    .OrderBy(e => e.Id)
                    .Skip(Start)
                    .Take(Size)
                    .ToList();
            }
            TotalSize = __DBContext.AdminAuditLogs
                .Count(e => e.SearchVector.Matches(SearchTerm));
            return __DBContext.AdminAuditLogs
                .Where(e => e.SearchVector.Matches(SearchTerm))
                .OrderBy(e => e.Id)
                .Skip(Start)
                .Take(Size)
                .ToList();
        }

        public void AddAuditLog(
            string TableName,
            string TableKey,
            string Action,
            string UserName,
            JObject OldValue,
            JObject NewValue
        ) {
            AdminAuditLog log = new AdminAuditLog();
            log.Table = TableName;
            log.TableKey = TableKey;
            log.Action = Action;
            log.User = UserName;
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