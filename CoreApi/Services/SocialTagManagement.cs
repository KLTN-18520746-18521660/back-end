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
         public async Task<(SocialCategory, ErrorCodes)> FindCategoryByName(string CategoryName, Guid SocialUserId = default)
        {
            var category = (await __DBContext.SocialCategories
                    .Where(e => e.Name == CategoryName
                            && e.StatusStr != BaseStatus.StatusToString(SocialCategoryStatus.Disabled, EntityStatus.SocialCategoryStatus))
                    .ToListAsync())
                    .DefaultIfEmpty(null)
                    .FirstOrDefault();
            if (category == null) {
                return (null, ErrorCodes.NOT_FOUND);
            }

            if (SocialUserId != default) {
                // add action visted to social user_action_with_category
            }
            return (category, ErrorCodes.NO_ERROR);
        }
        public async Task<(SocialCategory, ErrorCodes)> FindCategoryByNameIgnoreStatus(string CategoryName, Guid SocialUserId = default)
        {
            var category = (await __DBContext.SocialCategories
                    .Where(e => e.Name == CategoryName)
                    .ToListAsync())
                    .DefaultIfEmpty(null)
                    .FirstOrDefault();
            if (category == null) {
                return (null, ErrorCodes.NOT_FOUND);
            }

            if (SocialUserId != default) {
                // add action visted to social user_action_with_category
            }
            return (category, ErrorCodes.NO_ERROR);
        }

        public async Task<(SocialCategory, ErrorCodes)> FindCategoryById(long Id)
        {
            var category = (await __DBContext.SocialCategories
                    .Where(e => e.Id == Id)
                    .ToListAsync())
                    .DefaultIfEmpty(null)
                    .FirstOrDefault();

            if (category != null) {
                return (category, ErrorCodes.NO_ERROR);
            }
            return (null, ErrorCodes.NOT_FOUND);
        }

        public async Task<(bool, ErrorCodes)> IsSlugExisting(string Slug)
        {
            var count = (await __DBContext.SocialCategories
                    .CountAsync(e => e.Slug == Slug
                            && e.StatusStr != BaseStatus.StatusToString(SocialCategoryStatus.Disabled, EntityStatus.SocialCategoryStatus)));
            return (count > 0, ErrorCodes.NO_ERROR);
        }

        #region Tag handle
        // public async Task<ErrorCodes> AddNewTag(SocialPost Post, Guid SocialUserId)
        // {
        //     #region Find user
        //     using (var scope = __ServiceProvider.CreateScope())
        //     {
        //         var __SocialUserManagement = scope.ServiceProvider.GetRequiredService<SocialUserManagement>();
        //         var (user, error) = await __SocialUserManagement.FindUserById(SocialUserId);
        //         if (error != ErrorCodes.NO_ERROR || user.Status != SocialUserStatus.Activated) {
        //             // return error == ErrorCodes.NOT_FOUND ? error : ErrorCodes.INVALID_USER;
        //         }
        //     }
        //     #endregion
        //     LogInformation(Post.ToJsonString());
        //     await __DBContext.SocialPosts.AddAsync(Post);
        //     if (await __DBContext.SaveChangesAsync() > 0) {
        //         #region [SOCIAL] Write user activity
        //         var (newPost, error) = await FindPostById(Post.Id);
        //         if (error == ErrorCodes.NO_ERROR) {
        //             using (var scope = __ServiceProvider.CreateScope())
        //             {
        //                 var __SocialUserAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialUserAuditLogManagement>();
        //                 await __SocialUserAuditLogManagement.AddNewUserAuditLog(
        //                     newPost.GetModelName(),
        //                     newPost.Id.ToString(),
        //                     LOG_ACTIONS.CREATE,
        //                     SocialUserId,
        //                     new JObject(),
        //                     newPost.GetJsonObject()
        //                 );
        //             }
        //         } else {
        //             return ErrorCodes.INTERNAL_SERVER_ERROR;
        //         }
        //         #endregion
        //         return ErrorCodes.NO_ERROR;
        //     }
        //     return ErrorCodes.INTERNAL_SERVER_ERROR;
        // }
        #endregion
    }
}