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
    public class SocialAuditLogManagement : BaseService
    {
        public SocialAuditLogManagement() : base()
        {
            __ServiceName = "SocialAuditLogManagement";
        }

        public async Task<(List<SocialAuditLog> AuditLogs, int TotalSize)> GetAllAuditLog(Guid UserId, int Start, int Size, string SearchTerm = null)
        {
            if (SearchTerm == null || SearchTerm == "") {
                return
                (
                    await __DBContext.SocialAuditLogs
                        .Where(e => e.UserId == UserId)
                        .OrderBy(e => e.Id)
                        .Skip(Start)
                        .Take(Size)
                        .ToListAsync(),
                    await __DBContext.SocialAuditLogs.CountAsync(e => e.UserId == UserId)
                );
            }
            return
            (
                await __DBContext.SocialAuditLogs
                    .Where(e => e.SearchVector.Matches(SearchTerm) && e.UserId == UserId)
                    .OrderBy(e => e.Id)
                    .Skip(Start)
                    .Take(Size)
                    .ToListAsync(),
                await __DBContext.SocialAuditLogs
                    .CountAsync(e => e.SearchVector.Matches(SearchTerm) && e.UserId == UserId)
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
            SocialAuditLog log = new SocialAuditLog();
            log.Table = TableName;
            log.TableKey = TableKey;
            log.Action = Action;
            log.UserId = UserId;
            log.OldValue = new LogValue(OldValue);
            log.NewValue = new LogValue(NewValue);

            __DBContext.SocialAuditLogs.Add(log);
            await __DBContext.SaveChangesAsync();
        }

        public async Task AddAuditLog(SocialAuditLog AuditLog) {
            __DBContext.SocialAuditLogs.Add(AuditLog);
            await __DBContext.SaveChangesAsync();
        }
    }
}