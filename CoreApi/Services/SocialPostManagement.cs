using Serilog;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Linq.Expressions;  
using System;
using NpgsqlTypes;
using Microsoft.EntityFrameworkCore;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Common.Models;
using CoreApi.Common;
using Common;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic;
using DatabaseAccess.Common.Actions;
using Npgsql.EntityFrameworkCore.PostgreSQL;

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

        /* Example
        * status = ["Private"]
        * orders = [{"views", false}, {"created_timestamp", true}]
        * ==>
        * WHERE status = 'Private' AND status != 'Deleted'
        * ORDER BY views ASC, created_timestamp DESC
        */
        // /IReadOnlyList<SocialPost>
        public async Task<(object, ErrorCodes)> GetPostsAttachedToUser(Guid socialUserId,
                                                                                                  bool isOwner,
                                                                                                  int start = 0,
                                                                                                  int size = 20,
                                                                                                  string search_term = default,
                                                                                                  string[] status = default,
                                                                                                  (string, bool)[] orders = default)
        {
            #region validate params
            if (status != default) {
                foreach (var statusStr in status) {
                    var statusInt = BaseStatus.StatusFromString(statusStr, EntityStatus.SocialPostStatus);
                    if (statusInt == BaseStatus.InvalidStatus ||
                        statusInt == SocialPostStatus.Deleted) {
                        return (default, ErrorCodes.INVALID_PARAMS);
                    }
                }
            } else {
                status = new List<string>().ToArray();
            }
            if (orders != default) {
                foreach (var order in orders) {
                    if (!SocialPost.ColumnAllowOrder.Contains(order.Item1)) {
                        return (default, ErrorCodes.INVALID_PARAMS);
                    }
                }
            } else {
                orders = new List<(string, bool)>().ToArray();
            }
            #endregion
            string orderStr = orders != default ? Utils.GenerateOrderString(orders) : default;
            // Console.WriteLine(orderStr);
            // var query = from posts in (from p in __DBContext.SocialPosts
            //             where p.Owner == socialUserId && (search_term == default || p.SearchVector.Matches(search_term))
            //                 && p.StatusStr == BaseStatus.StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus)
            //             select new { p, Likes = p.SocialUserActionWithPosts
            //                 .Count(ac => EF.Functions
            //                     .JsonExists(ac.ActionsStr, BaseAction
            //                         .ActionToString(UserActionWithPost.Like, EntityAction.UserActionWithPost)
            //                     )
            //                 )
            //             }) orderby posts.Likes select posts.p;
            // return (await query.ToListAsync(), ErrorCodes.NO_ERROR);

            if (!isOwner) {
                return (
                    await __DBContext.SocialPosts
                        .Where(e => e.Owner == socialUserId
                            && (search_term == default || e.SearchVector.Matches(search_term))
                            && e.StatusStr == BaseStatus.StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus))
                        .OrderBy(e => orderStr)
                        .Skip(start)
                        .Take(size)
                        .ToListAsync(),
                    ErrorCodes.NO_ERROR
                );
            }
            return (
                await __DBContext.SocialPosts
                    .Where(e => e.Owner == socialUserId
                        && (!isOwner || status.Contains(e.StatusStr))
                        && (search_term == default || e.SearchVector.Matches(search_term))
                        && e.StatusStr != BaseStatus.StatusToString(SocialPostStatus.Deleted, EntityStatus.SocialPostStatus))
                    .Include(e => e.Likes)
                    .Select(e => e.Likes )
                    // .OrderBy("CreatedTimestamp desc, Views asc, Likes desc")
                    .Skip(start)
                    .Take(size)
                    .ToListAsync(),
                ErrorCodes.NO_ERROR
            );
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
            if (Slug == string.Empty) {
                return (default, ErrorCodes.INVALID_PARAMS);
            }
            var post = await __DBContext.SocialPosts
                    .Where(e => e.Slug == Slug
                        && e.StatusStr == BaseStatus.StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus)
                        && e.StatusStr == BaseStatus.StatusToString(SocialPostStatus.Private, EntityStatus.SocialPostStatus))
                    .FirstOrDefaultAsync();
            if (post == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }
            post.Views++;
            _ = __DBContext.SaveChangesAsync();

            if (post.Status != SocialPostStatus.Approved) {
                if (SocialUserId == default || SocialUserId != post.Owner) {
                    return (default, ErrorCodes.USER_IS_NOT_OWNER);
                }
            }
            return (post, ErrorCodes.NO_ERROR);
        }

        public async Task<(SocialPost, ErrorCodes)> FindPostById(long Id)
        {
            var post = await __DBContext.SocialPosts
                    .Where(e => e.Id == Id)
                    .FirstOrDefaultAsync();

            if (post != default) {
                return (post, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.NOT_FOUND);
        }

        public async Task<(bool, ErrorCodes)> IsPostExisting(string Slug)
        {
            var count = (await __DBContext.SocialPosts
                    .CountAsync(e => e.Slug == Slug
                            && e.StatusStr == BaseStatus.StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus)
                            && e.StatusStr == BaseStatus.StatusToString(SocialPostStatus.Private, EntityStatus.SocialPostStatus)));
            return (count > 0, ErrorCodes.NO_ERROR);
        }

        #region Post handle
        public ErrorCodes ValidateChangeStatusAction(int from, int to)
        {
            if ((
                    from == SocialPostStatus.Pending && (
                    to == SocialPostStatus.Approved ||
                    to == SocialPostStatus.Deleted ||
                    to == SocialPostStatus.Rejected ||
                    to == SocialPostStatus.Private)
                ) ||
                (
                    from == SocialPostStatus.Approved && (
                    to == SocialPostStatus.Deleted ||
                    to == SocialPostStatus.Rejected ||
                    to == SocialPostStatus.Private)
                ) ||
                (
                    from == SocialPostStatus.Rejected && (
                    to == SocialPostStatus.Deleted ||
                    to == SocialPostStatus.Approved)
                ) ||
                (
                    from == SocialPostStatus.Private && (
                    to == SocialPostStatus.Deleted ||
                    to == SocialPostStatus.Approved)
                )
            ) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INVALID_ACTION;
        }

        public async Task<ErrorCodes> AddNewPost(SocialPost Post, Guid SocialUserId)
        {
            Post.Slug = string.Empty;
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
            var (post, error) = await FindPostById(Id);
            if (error != ErrorCodes.NO_ERROR) {
                return error;
            }
            post.Status = SocialPostStatus.Approved;
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [ADMIN] Write social audit log
                (post, error) = await FindPostById(Id);
                if (error == ErrorCodes.NO_ERROR) {
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialAuditLogManagement>();
                        await __SocialAuditLogManagement.AddNewAuditLog(
                            post.GetModelName(),
                            post.Id.ToString(),
                            LOG_ACTIONS.CREATE,
                            AdminUserId,
                            new JObject() {
                                { "status", 1 }
                            },
                            post.GetJsonObjectForLog()
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

        public async Task<ErrorCodes> RejectPost(long Id, Guid AdminUserId)
        {
            var (post, error) = await FindPostById(Id);
            if (error != ErrorCodes.NO_ERROR) {
                return error;
            }
            var oldPost = Utils.DeepClone(post);
            post.Status = SocialPostStatus.Rejected;
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [ADMIN] Write social audit log
                (post, error) = await FindPostById(Id);
                if (error == ErrorCodes.NO_ERROR) {
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialAuditLogManagement>();
                        var (oldVal, newVal) = post.GetDataChanges(oldPost);
                        await __SocialAuditLogManagement.AddNewAuditLog(
                            post.GetModelName(),
                            post.Id.ToString(),
                            LOG_ACTIONS.CREATE,
                            AdminUserId,
                            oldVal,
                            newVal
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

        public async Task<ErrorCodes> DeletedPost(long Id, Guid SocialUser)
        {
            var (post, error) = await FindPostById(Id);
            if (error != ErrorCodes.NO_ERROR) {
                return error;
            }
            var oldPost = Utils.DeepClone(post);
            post.Status = SocialPostStatus.Approved;
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [ADMIN] Write social audit log
                (post, error) = await FindPostById(Id);
                if (error == ErrorCodes.NO_ERROR) {
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialUserAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialUserAuditLogManagement>();
                        var (oldVal, newVal) = post.GetDataChanges(oldPost);
                        await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                            post.GetModelName(),
                            post.Id.ToString(),
                            LOG_ACTIONS.CREATE,
                            SocialUser,
                            oldVal,
                            newVal
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

        public async Task<ErrorCodes> MakePostPrivate(long Id, Guid SocialUser)
        {
            var (post, error) = await FindPostById(Id);
            if (error != ErrorCodes.NO_ERROR) {
                return error;
            }
            var oldPost = Utils.DeepClone(post);
            post.Status = SocialPostStatus.Private;
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [ADMIN] Write social audit log
                (post, error) = await FindPostById(Id);
                if (error == ErrorCodes.NO_ERROR) {
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialUserAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialUserAuditLogManagement>();
                        var (oldVal, newVal) = post.GetDataChanges(oldPost);
                        await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                            post.GetModelName(),
                            post.Id.ToString(),
                            LOG_ACTIONS.CREATE,
                            SocialUser,
                            oldVal,
                            newVal
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