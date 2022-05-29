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
                                                                                            string Key,
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
                    Key = default;
                    break;
            }

            return
            (
                await __DBContext.SocialUserAuditLogs
                    .Where(
                        e => e.UserId == UserId
                        && e.Table == ActionStr
                        && ((SearchTerm == default) || e.SearchVector.Matches(SearchTerm))
                        && ((Key == default) || e.TableKey == Key)
                    )
                    .OrderBy(e => e.Id)
                    .Skip(Start)
                    .Take(Size)
                    .ToListAsync(),
                await __DBContext.SocialUserAuditLogs
                    .CountAsync(
                        e => e.UserId == UserId
                        && e.Table == ActionStr
                        && (SearchTerm == default) || e.SearchVector.Matches(SearchTerm)
                        && (Key == default) || e.TableKey == Key
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
            log.OldValue = new LogValue(Utils.CensorSensitiveDate(OldValue));
            log.NewValue = new LogValue(Utils.CensorSensitiveDate(NewValue));

            await __DBContext.SocialUserAuditLogs.AddAsync(log);
            await __DBContext.SaveChangesAsync();
        }

        public async Task AddNewUserAuditLog(SocialUserAuditLog AuditLog) {
            await __DBContext.SocialUserAuditLogs.AddAsync(AuditLog);
            await __DBContext.SaveChangesAsync();
        }
    }
}