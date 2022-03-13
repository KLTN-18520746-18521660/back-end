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
    public class SocialAuditLogManagement : BaseService
    {
        protected DBContext __DBContext;
        public SocialAuditLogManagement() : base()
        {
            __DBContext = new DBContext();
            __ServiceName = "SocialAuditLogManagement";
        }

        public List<SocialAuditLog> GetAllAuditLog(out int TotalSize, Guid UserId, int Start, int Size, string SearchTerm = null)
        {
            if (SearchTerm == null || SearchTerm == "") {
                TotalSize = __DBContext.SocialAuditLogs.Count();
                return __DBContext.SocialAuditLogs
                    .Where(e => e.UserId == UserId)
                    .OrderBy(e => e.Id)
                    .Skip(Start)
                    .Take(Size)
                    .ToList();
            }
            TotalSize = __DBContext.SocialAuditLogs
                .Count(e => e.SearchVector.Matches(SearchTerm) && e.UserId == UserId);
            return __DBContext.SocialAuditLogs
                .Where(e => e.SearchVector.Matches(SearchTerm) && e.UserId == UserId)
                .OrderBy(e => e.Id)
                .Skip(Start)
                .Take(Size)
                .ToList();
        }

        public void AddAuditLog(
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

            __DBContext.SocialAuditLogs.Add(log);
            __DBContext.SaveChanges();
        }

        public void AddAuditLog(SocialAuditLog AuditLog) {
            __DBContext.SocialAuditLogs.Add(AuditLog);
            __DBContext.SaveChanges();
        }
    }
}