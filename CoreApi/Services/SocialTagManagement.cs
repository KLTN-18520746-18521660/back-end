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
using Microsoft.Extensions.DependencyInjection;

namespace CoreApi.Services
{
    public class SocialTagManagement : BaseService
    {
        public SocialTagManagement(DBContext _DBContext,
                                    IServiceProvider _IServiceProvider)
            : base(_DBContext, _IServiceProvider)
        {
            __ServiceName = "SocialTagManagement";
        }

        public async Task<(IReadOnlyList<SocialTag>, ErrorCodes)> GetTags()
        {
            return (
                await __DBContext.SocialTags
                    .Where(e => e.StatusStr != BaseStatus.StatusToString(SocialTagStatus.Disabled, EntityStatus.SocialTagStatus))
                    .ToListAsync(),
                ErrorCodes.NO_ERROR
            );
        }
        public async Task<(SocialTag, ErrorCodes)> FindTagByName(string Tag, Guid SocialUserId = default)
        {
            var tag = (await __DBContext.SocialTags
                    .Where(e => e.Tag == Tag
                            && e.StatusStr != BaseStatus.StatusToString(SocialCategoryStatus.Disabled, EntityStatus.SocialCategoryStatus))
                    .ToListAsync())
                    .DefaultIfEmpty(null)
                    .FirstOrDefault();
            if (tag == null) {
                return (null, ErrorCodes.NOT_FOUND);
            }

            if (SocialUserId != default) {
                // add action visted to social user_action_with_tag
            }
            return (tag, ErrorCodes.NO_ERROR);
        }
        public async Task<(SocialTag, ErrorCodes)> FindCategoryByNameIgnoreStatus(string Tag, Guid SocialUserId = default)
        {
            var tag = (await __DBContext.SocialTags
                    .Where(e => e.Tag == Tag)
                    .ToListAsync())
                    .DefaultIfEmpty(null)
                    .FirstOrDefault();
            if (tag == null) {
                return (null, ErrorCodes.NOT_FOUND);
            }

            if (SocialUserId != default) {
                // add action visted to social user_action_with_tag
            }
            return (tag, ErrorCodes.NO_ERROR);
        }

        public async Task<(SocialTag, ErrorCodes)> FindTagById(long Id)
        {
            var tag = (await __DBContext.SocialTags
                    .Where(e => e.Id == Id)
                    .ToListAsync())
                    .DefaultIfEmpty(null)
                    .FirstOrDefault();

            if (tag != null) {
                return (tag, ErrorCodes.NO_ERROR);
            }
            return (null, ErrorCodes.NOT_FOUND);
        }

        #region Tag handle
        public async Task<ErrorCodes> AddNewTag(SocialTag Tag)
        {
            await __DBContext.SocialTags.AddAsync(Tag);
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [SOCIAL] Write user activity
                var (newTag, error) = await FindTagById(Tag.Id);
                if (error == ErrorCodes.NO_ERROR) {
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialUserAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialUserAuditLogManagement>();
                        await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                            newTag.GetModelName(),
                            newTag.Id.ToString(),
                            LOG_ACTIONS.CREATE,
                            default,
                            new JObject(),
                            newTag.GetJsonObject()
                        );
                    }
                } else {
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
                #endregion
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        #endregion
    }
}