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
    public class SocialPostManagement : BaseService
    {
        public SocialPostManagement(DBContext _DBContext,
                                    IServiceProvider _IServiceProvider)
            : base(_DBContext, _IServiceProvider)
        {
            __ServiceName = "SocialPostManagement";
        }

        /// <summary>
        /// Using function will increase views if UserId is valid and get info success.
        /// Only select post with status is 'Approved'
        /// </summary>
        /// <param name="Slug"></param>
        /// <param name="SocialUserId"></param>
        /// <returns>(SocialPost, ErrorCodes)</returns>
        public async Task<(SocialPost, ErrorCodes)> FindPostBySlug(string Slug, Guid SocialUserId = default)
        {
            var post = (await __DBContext.SocialPosts
                    .Where(e => e.Slug == Slug 
                            && e.StatusStr != BaseStatus.StatusToString(SocialPostStatus.Deleted, EntityStatus.SocialPostStatus))
                    .ToListAsync())
                    .DefaultIfEmpty(null)
                    .FirstOrDefault();
            if (post == null) {
                return (null, ErrorCodes.NOT_FOUND);
            }

            if (post.Status != SocialPostStatus.Approved) {
                if (SocialUserId == default || SocialUserId != post.Owner) {
                    return (null, ErrorCodes.USER_IS_NOT_OWNER);
                }
            }
            return (post, ErrorCodes.NO_ERROR);
        }

        public async Task<(SocialPost, ErrorCodes)> FindPostById(long Id)
        {
            var post = (await __DBContext.SocialPosts
                    .Where(e => e.Id == Id)
                    .ToListAsync())
                    .DefaultIfEmpty(null)
                    .FirstOrDefault();

            if (post != null) {
                return (post, ErrorCodes.NO_ERROR);
            }
            return (null, ErrorCodes.NOT_FOUND);
        }

        public async Task<(bool, ErrorCodes)> IsSlugExisting(string Slug)
        {
            return (false, ErrorCodes.INTERNAL_SERVER_ERROR);
        }

        #region Post handle
        public async Task<ErrorCodes> AddNewPost(SocialPost Post, Guid SocialUserId)
        {
            #region Find user
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __SocialUserManagement = scope.ServiceProvider.GetRequiredService<SocialUserManagement>();
                var (user, error) = await __SocialUserManagement.FindUserById(SocialUserId);
                if (error != ErrorCodes.NO_ERROR || user.Status != SocialUserStatus.Activated) {
                    return error == ErrorCodes.NOT_FOUND ? error : ErrorCodes.INVALID_USER;
                }
            }
            #endregion
            LogInformation(Post.ToJsonString());
            await __DBContext.SocialPosts.AddAsync(Post);
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [SOCIAL] Write user activity
                var (newPost, error) = await FindPostById(Post.Id);
                if (error == ErrorCodes.NO_ERROR) {
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialUserAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialUserAuditLogManagement>();
                        await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                            newPost.GetModelName(),
                            newPost.Id.ToString(),
                            LOG_ACTIONS.CREATE,
                            SocialUserId,
                            new JObject(),
                            newPost.GetJsonObject()
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

        public async Task<ErrorCodes> ApprovePost(long Id, Guid AdminUserId)
        {
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }

        public async Task<ErrorCodes> RejectPost(long Id, Guid AdminUserId)
        {
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }

        public async Task<ErrorCodes> DeletedPost(long Id, Guid SocialUser)
        {
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        #endregion
    }
}