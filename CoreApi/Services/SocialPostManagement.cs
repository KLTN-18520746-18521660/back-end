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
using DatabaseAccess.Context.ParserModels;
using CoreApi.Models.ModifyModels;
using System.Reflection;

namespace CoreApi.Services
{
    public enum GetPostAction {
        GetPostsAttachedToUser          = 0,
        GetNewPosts                     = 1,
        GetTrendingPosts                = 2,
        GetPostsByUserFollowing         = 3,
    }
    public class SocialPostManagement : BaseTransientService
    {
        public SocialPostManagement(DBContext _DBContext,
                                    IServiceProvider _IServiceProvider)
            : base(_IServiceProvider)
        {
            __ServiceName = "SocialPostManagement";
        }

        public string[] GetAllowOrderFields(GetPostAction action)
        {
            switch (action) {
                case GetPostAction.GetPostsAttachedToUser:
                    return new string[] {
                        "views",
                        "likes",
                        "dislikes",
                        "comments",
                        "follows",
                        "reports",
                        "title",
                        "time_read",
                        "created_timestamp",
                        "last_modified_timestamp",
                    };
                case GetPostAction.GetNewPosts:
                    return new string[] {
                        "views",
                        "likes",
                        "dislikes",
                        "comments",
                    };
                case GetPostAction.GetTrendingPosts:
                    return new string[] {
                    };
                case GetPostAction.GetPostsByUserFollowing:
                    return new string[] {
                        "views",
                        "likes",
                        "dislikes",
                        "comments",
                    };
                default:
                    return default;
            }
        }

        /* Example
        * status = ["Private"]
        * orders = [{"views", false}, {"created_timestamp", true}]
        * ==>
        * WHERE status = 'Private' AND status != 'Deleted'
        * ORDER BY views ASC, created_timestamp DESC
        */
        public async Task<(List<SocialPost>, int, ErrorCodes)> GetPostsAttachedToUser(Guid socialUserId,
                                                                                      bool isOwner,
                                                                                      int start = 0,
                                                                                      int size = 20,
                                                                                      string search_term = default,
                                                                                      string[] status = default,
                                                                                      (string, bool)[] orders = default,
                                                                                      string[] tags = default,
                                                                                      string[] categories = default)
        {
            #region validate params
            if (status != default) {
                foreach (var statusStr in status) {
                    var statusInt = BaseStatus.StatusFromString(statusStr, EntityStatus.SocialPostStatus);
                    if (statusInt == BaseStatus.InvalidStatus ||
                        statusInt == SocialPostStatus.Deleted) {
                        return (default, default, ErrorCodes.INVALID_PARAMS);
                    }
                }
            }

            if (tags == default) {
                tags = new string[]{};
            }

            if (categories == default) {
                categories = new string[]{};
            }

            var ColumnAllowOrder = GetAllowOrderFields(GetPostAction.GetPostsAttachedToUser);
            if (orders != default) {
                foreach (var order in orders) {
                    if (!ColumnAllowOrder.Contains(order.Item1)) {
                        return (default, default, ErrorCodes.INVALID_PARAMS);
                    }
                }
            } else {
                orders = new (string, bool)[]{};
            }
            #endregion

            // orderStr can't empty or null
            string orderStr = orders != default && orders.Length != 0 ? Utils.GenerateOrderString(orders) : "created_timestamp desc";
            var query =
                    from ids in (
                        (from post in __DBContext.SocialPosts
                                .Where(e => e.Owner == socialUserId
                                    && (search_term == default || e.SearchVector.Matches(search_term))
                                    && (isOwner
                                        ? ((status.Count() == 0
                                                && e.StatusStr != BaseStatus
                                                    .StatusToString(SocialPostStatus.Deleted, EntityStatus.SocialPostStatus))
                                            || status.Contains(e.StatusStr))
                                        : e.StatusStr == BaseStatus
                                                    .StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus)
                                    )
                                    && (tags.Count() == 0
                                        || e.SocialPostTags.Select(t => t.Tag.Tag).ToArray().Any(t => tags.Contains(t))
                                    )
                                    && (categories.Count() == 0
                                        || e.SocialPostCategories.Select(c => c.Category.Name).ToArray().Any(c => categories.Contains(c))
                                    )
                                )
                        join action in __DBContext.SocialUserActionWithPosts on post.Id equals action.PostId
                        into postWithAction
                        from p in postWithAction.DefaultIfEmpty()
                        group p by new {
                            post.Id,
                            post.Views,
                            post.Title,
                            post.TimeRead,
                            post.CreatedTimestamp,
                            post.LastModifiedTimestamp
                        } into gr
                        select new {
                            gr.Key,
                            Likes = gr.Count(e => EF.Functions.JsonExists(e.ActionsStr,
                                BaseAction.ActionToString(UserActionWithPost.Like, EntityAction.UserActionWithPost))),
                            DisLikes = gr.Count(e => EF.Functions.JsonExists(e.ActionsStr,
                                BaseAction.ActionToString(UserActionWithPost.Dislike, EntityAction.UserActionWithPost))),
                            Comments = __DBContext.SocialComments.Count(e => e.PostId == gr.Key.Id),
                            Follows = gr.Count(e => EF.Functions.JsonExists(e.ActionsStr,
                                BaseAction.ActionToString(UserActionWithPost.Follow, EntityAction.UserActionWithPost))),
                            Reports = gr.Count(e => EF.Functions.JsonExists(e.ActionsStr,
                                BaseAction.ActionToString(UserActionWithPost.Report, EntityAction.UserActionWithPost))),
                        } into ret select new {
                            ret.Key.Id,
                            views = ret.Key.Views,
                            likes = ret.Likes,
                            dislikes = ret.DisLikes,
                            comments = ret.Comments,
                            follows = ret.Follows,
                            reports = ret.Reports,
                            title = ret.Key.Title,
                            time_read = ret.Key.TimeRead,
                            created_timestamp = ret.Key.CreatedTimestamp,
                            last_modified_timestamp = ret.Key.LastModifiedTimestamp,
                        })
                        .OrderBy(orderStr)
                        .Skip(start).Take(size)
                        .Select(e => e.Id)
                    )
                    join posts in __DBContext.SocialPosts on ids equals posts.Id
                    select posts;

            var totalCount = await __DBContext.SocialPosts
                                .CountAsync(e => e.Owner == socialUserId
                                    && (search_term == default || e.SearchVector.Matches(search_term))
                                    && (isOwner
                                        ? ((status.Count() == 0
                                                && e.StatusStr != BaseStatus
                                                    .StatusToString(SocialPostStatus.Deleted, EntityStatus.SocialPostStatus))
                                            || status.Contains(e.StatusStr))
                                        : e.StatusStr == BaseStatus
                                                    .StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus)
                                    )
                                    && (tags.Count() == 0
                                        || e.SocialPostTags.Select(t => t.Tag.Tag).ToArray().Any(t => tags.Contains(t))
                                    )
                                    && (categories.Count() == 0
                                        || e.SocialPostCategories.Select(c => c.Category.Name).ToArray().Any(c => categories.Contains(c))
                                    )
                                );
            return (await query.ToListAsync(), totalCount, ErrorCodes.NO_ERROR);
        }

        public async Task<(List<SocialPost>, int, ErrorCodes)> GetNewPosts(Guid socialUserId = default,
                                                                           int start = 0,
                                                                           int size = 20,
                                                                           string search_term = default,
                                                                           (string, bool)[] orders = default,
                                                                           string[] tags = default,
                                                                           string[] categories = default)
        {
            #region validate params
            if (tags == default) {
                tags = new string[]{};
            }

            if (categories == default) {
                categories = new string[]{};
            }

            var ColumnAllowOrder = GetAllowOrderFields(GetPostAction.GetNewPosts);
            if (orders != default) {
                foreach (var order in orders) {
                    if (!ColumnAllowOrder.Contains(order.Item1)) {
                        return (default, default, ErrorCodes.INVALID_PARAMS);
                    }
                }
            } else {
                orders = new (string, bool)[]{};
            }
            #endregion

            // orderStr can't empty or null
            string orderStr = orders != default && orders.Length != 0 ? Utils.GenerateOrderString(orders) : string.Empty;
            orderStr = $"visited asc, { orderStr }, created_timestamp desc";

            var query =
                    from ids in (
                        (from post in __DBContext.SocialPosts
                                .Where(e => (search_term == default || e.SearchVector.Matches(search_term))
                                    && (e.StatusStr == BaseStatus
                                                    .StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus)
                                    )
                                    && (tags.Count() == 0
                                        || e.SocialPostTags.Select(t => t.Tag.Tag).ToArray().Any(t => tags.Contains(t))
                                    )
                                    && (categories.Count() == 0
                                        || e.SocialPostCategories.Select(c => c.Category.Name).ToArray().Any(c => categories.Contains(c))
                                    )
                                )
                        join action in __DBContext.SocialUserActionWithPosts on post.Id equals action.PostId
                        into postWithAction
                        from p in postWithAction.DefaultIfEmpty()
                        group p by new {
                            post.Id,
                            post.Views,
                            post.CreatedTimestamp,
                            actionStr = post.SocialUserActionWithPosts
                                .Where(e => e.UserId == socialUserId)
                                .Select(e => e.ActionsStr)
                                .FirstOrDefault()
                        } into gr
                        select new {
                            gr.Key,
                            Visited = (socialUserId == default) ? false
                                : EF.Functions.JsonExists(gr.Key.actionStr,
                                BaseAction.ActionToString(UserActionWithPost.Visited, EntityAction.UserActionWithPost)),
                            Likes = gr.Count(e => EF.Functions.JsonExists(e.ActionsStr,
                                BaseAction.ActionToString(UserActionWithPost.Like, EntityAction.UserActionWithPost))),
                            DisLikes = gr.Count(e => EF.Functions.JsonExists(e.ActionsStr,
                                BaseAction.ActionToString(UserActionWithPost.Dislike, EntityAction.UserActionWithPost))),
                            Comments = __DBContext.SocialComments.Count(e => e.PostId == gr.Key.Id),
                        } into ret select new {
                            ret.Key.Id,
                            visited = ret.Visited,
                            views = ret.Key.Views,
                            likes = ret.Likes,
                            dislikes = ret.DisLikes,
                            comments = ret.Comments,
                            created_timestamp = ret.Key.CreatedTimestamp,
                        })
                        .OrderBy(orderStr)
                        .Skip(start).Take(size)
                        .Select(e => e.Id)
                    )
                    join posts in __DBContext.SocialPosts on ids equals posts.Id
                    select posts;

            var totalCount = await __DBContext.SocialPosts
                                .CountAsync(e => (search_term == default || e.SearchVector.Matches(search_term))
                                    && (e.StatusStr == BaseStatus
                                                    .StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus)
                                    )
                                    && (tags.Count() == 0
                                        || e.SocialPostTags.Select(t => t.Tag.Tag).ToArray().Any(t => tags.Contains(t))
                                    )
                                    && (categories.Count() == 0
                                        || e.SocialPostCategories.Select(c => c.Category.Name).ToArray().Any(c => categories.Contains(c))
                                    )
                                );
            return (await query.ToListAsync(), totalCount, ErrorCodes.NO_ERROR);
        }

        public async Task<(List<SocialPost>, int, ErrorCodes)> GetTrendingPosts(Guid socialUserId = default,
                                                                                int time = 7, // days
                                                                                int start = 0,
                                                                                int size = 20,
                                                                                string search_term = default,
                                                                                string[] tags = default,
                                                                                string[] categories = default)
        {
            #region validate params
            if (tags == default) {
                tags = new string[]{};
            }

            if (categories == default) {
                categories = new string[]{};
            }
            #endregion

            // orderStr can't empty or null
            var orderStr = $"visited asc, likes desc, comments desc, created_timestamp desc";
            var compareDate = DateTime.UtcNow.AddDays(-time);

            var query =
                    from ids in (
                        (from post in __DBContext.SocialPosts
                                .Where(e => (search_term == default || e.SearchVector.Matches(search_term))
                                    && (e.StatusStr == BaseStatus
                                                    .StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus)
                                    )
                                    && (tags.Count() == 0
                                        || e.SocialPostTags.Select(t => t.Tag.Tag).ToArray().Any(t => tags.Contains(t))
                                    )
                                    && (categories.Count() == 0
                                        || e.SocialPostCategories.Select(c => c.Category.Name).ToArray().Any(c => categories.Contains(c))
                                    )
                                    && e.CreatedTimestamp >= compareDate
                                )
                        join action in __DBContext.SocialUserActionWithPosts on post.Id equals action.PostId
                        into postWithAction
                        from p in postWithAction.DefaultIfEmpty()
                        group p by new {
                            post.Id,
                            post.Views,
                            post.CreatedTimestamp,
                            actionStr = post.SocialUserActionWithPosts
                                .Where(e => e.UserId == socialUserId)
                                .Select(e => e.ActionsStr)
                                .FirstOrDefault()
                        } into gr
                        select new {
                            gr.Key,
                            Visited = (socialUserId == default) ? false
                                : EF.Functions.JsonExists(gr.Key.actionStr,
                                BaseAction.ActionToString(UserActionWithPost.Visited, EntityAction.UserActionWithPost)),
                            Likes = gr.Count(e => EF.Functions.JsonExists(e.ActionsStr,
                                BaseAction.ActionToString(UserActionWithPost.Like, EntityAction.UserActionWithPost))),
                            DisLikes = gr.Count(e => EF.Functions.JsonExists(e.ActionsStr,
                                BaseAction.ActionToString(UserActionWithPost.Dislike, EntityAction.UserActionWithPost))),
                            Comments = __DBContext.SocialComments.Count(e => e.PostId == gr.Key.Id),
                        } into ret select new {
                            ret.Key.Id,
                            visited = ret.Visited,
                            views = ret.Key.Views,
                            likes = ret.Likes,
                            dislikes = ret.DisLikes,
                            comments = ret.Comments,
                            created_timestamp = ret.Key.CreatedTimestamp,
                        })
                        .OrderBy(orderStr)
                        .Skip(start).Take(size)
                        .Select(e => e.Id)
                    )
                    join posts in __DBContext.SocialPosts on ids equals posts.Id
                    select posts;

            var totalCount = await __DBContext.SocialPosts
                                .CountAsync(e => (search_term == default || e.SearchVector.Matches(search_term))
                                    && (e.StatusStr == BaseStatus
                                                    .StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus)
                                    )
                                    && (tags.Count() == 0
                                        || e.SocialPostTags.Select(t => t.Tag.Tag).ToArray().Any(t => tags.Contains(t))
                                    )
                                    && (categories.Count() == 0
                                        || e.SocialPostCategories.Select(c => c.Category.Name).ToArray().Any(c => categories.Contains(c))
                                    )
                                    && e.CreatedTimestamp >= compareDate
                                );
            return (await query.ToListAsync(), totalCount, ErrorCodes.NO_ERROR);
        }

        public async Task<(List<SocialPost>, int, ErrorCodes)> GetPostsByUserFollowing(Guid socialUserId,
                                                                                       int start = 0,
                                                                                       int size = 20,
                                                                                       (string, bool)[] orders = default,
                                                                                       string[] tags = default,
                                                                                       string[] categories = default)
        {
            #region validate params
            if (tags == default) {
                tags = new string[]{};
            }

            if (categories == default) {
                categories = new string[]{};
            }
            var ColumnAllowOrder = GetAllowOrderFields(GetPostAction.GetNewPosts);
            if (orders != default) {
                foreach (var order in orders) {
                    if (!ColumnAllowOrder.Contains(order.Item1)) {
                        return (default, default, ErrorCodes.INVALID_PARAMS);
                    }
                }
            } else {
                orders = new (string, bool)[]{};
            }
            #endregion

            // orderStr can't empty or null
            string orderStr = orders != default && orders.Length != 0 ? Utils.GenerateOrderString(orders) : string.Empty;
            orderStr = $"visited asc, { orderStr }, created_timestamp desc";

            var query_post_of_following_users =
                    // Fllow: post, user, tag, category
                    from user_following_ids in __DBContext.SocialUserActionWithUsers
                        .Where(e => e.UserId == socialUserId
                            && EF.Functions.JsonExists(e.ActionsStr,
                                BaseAction.ActionToString(UserActionWithUser.Follow, EntityAction.UserActionWithUser))
                        )
                        .Join(
                            __DBContext.SocialPosts,
                            ac => ac.UserIdDes,
                            p => p.Owner,
                            (ac, p) => p.Id
                        )
                    select user_following_ids;

            var query_post_of_following_tags =
                    from tag_following_ids in __DBContext.SocialUserActionWithTags
                        .Where(e => e.UserId == socialUserId
                            && EF.Functions.JsonExists(e.ActionsStr,
                                BaseAction.ActionToString(UserActionWithTag.Follow, EntityAction.UserActionWithTag))
                        )
                        .Join(
                            __DBContext.SocialPostTags,
                            ac => ac.TagId,
                            pt => pt.TagId,
                            (ac, pt) => pt.PostId
                        )
                    select tag_following_ids;

            var query_post_of_following_categories =
                    from category_following_ids in __DBContext.SocialUserActionWithCategories
                        .Where(e => e.UserId == socialUserId
                            && EF.Functions.JsonExists(e.ActionsStr,
                                BaseAction.ActionToString(UserActionWithCategory.Follow, EntityAction.UserActionWithCategory))
                        )
                        .Join(
                            __DBContext.SocialPostCategories,
                            ac => ac.CategoryId,
                            pc => pc.CategoryId,
                            (ac, pc) => pc.PostId
                        )
                    select category_following_ids;

            var query_post_of_following_posts =
                    from post_following_ids in __DBContext.SocialUserActionWithPosts
                        .Where(e => e.UserId == socialUserId
                            && EF.Functions.JsonExists(e.ActionsStr,
                                BaseAction.ActionToString(UserActionWithPost.Follow, EntityAction.UserActionWithPost))
                        )
                        .Select(e => e.PostId)
                    select post_following_ids;

            var post_ids = query_post_of_following_users
                                .Union(query_post_of_following_tags)
                                .Union(query_post_of_following_categories)
                                .Union(query_post_of_following_posts);

            var query = 
                    from ids in (
                        (from follow_post in
                            (from post in __DBContext.SocialPosts
                                    .Where(e =>
                                        (e.StatusStr == BaseStatus
                                                        .StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus)
                                        )
                                        && (tags.Count() == 0
                                            || e.SocialPostTags.Select(t => t.Tag.Tag).ToArray().Any(t => tags.Contains(t))
                                        )
                                        && (categories.Count() == 0
                                            || e.SocialPostCategories.Select(c => c.Category.Name).ToArray().Any(c => categories.Contains(c))
                                        )
                                    )
                            join follow_ids in post_ids on post.Id equals follow_ids
                            select post)
                        join action in __DBContext.SocialUserActionWithPosts on follow_post.Id equals action.PostId
                        into postWithAction
                        from p in postWithAction.DefaultIfEmpty()
                        group p by new {
                            follow_post.Id,
                            follow_post.Views,
                            follow_post.CreatedTimestamp,
                            actionStr = follow_post.SocialUserActionWithPosts
                                .Where(e => e.UserId == socialUserId)
                                .Select(e => e.ActionsStr)
                                .FirstOrDefault()
                        } into gr
                        select new {
                            gr.Key,
                            Visited = (socialUserId == default) ? false
                                : EF.Functions.JsonExists(gr.Key.actionStr,
                                BaseAction.ActionToString(UserActionWithPost.Visited, EntityAction.UserActionWithPost)),
                            Likes = gr.Count(e => EF.Functions.JsonExists(e.ActionsStr,
                                BaseAction.ActionToString(UserActionWithPost.Like, EntityAction.UserActionWithPost))),
                            DisLikes = gr.Count(e => EF.Functions.JsonExists(e.ActionsStr,
                                BaseAction.ActionToString(UserActionWithPost.Dislike, EntityAction.UserActionWithPost))),
                            Comments = __DBContext.SocialComments.Count(e => e.PostId == gr.Key.Id),
                        } into ret select new {
                            ret.Key.Id,
                            visited = ret.Visited,
                            views = ret.Key.Views,
                            likes = ret.Likes,
                            dislikes = ret.DisLikes,
                            comments = ret.Comments,
                            created_timestamp = ret.Key.CreatedTimestamp,
                        })
                        .OrderBy(orderStr)
                        .Skip(start).Take(size)
                        .Select(e => e.Id)
                    )
                    join posts in __DBContext.SocialPosts on ids equals posts.Id
                    select posts;

            var totalCount = await (from post in __DBContext.SocialPosts
                                    .Where(e =>
                                        (e.StatusStr == BaseStatus
                                                        .StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus)
                                        )
                                        && (tags.Count() == 0
                                            || e.SocialPostTags.Select(t => t.Tag.Tag).ToArray().Any(t => tags.Contains(t))
                                        )
                                        && (categories.Count() == 0
                                            || e.SocialPostCategories.Select(c => c.Category.Name).ToArray().Any(c => categories.Contains(c))
                                        )
                                    )
                            join follow_ids in post_ids on post.Id equals follow_ids
                            select post).CountAsync();
            return (await query.ToListAsync(), totalCount, ErrorCodes.NO_ERROR);
        }

        public async Task<(SocialPost, ErrorCodes)> FindPostBySlug(string Slug)
        {
            if (Slug == string.Empty) {
                return (default, ErrorCodes.INVALID_PARAMS);
            }
            var post = await __DBContext.SocialPosts
                    .Where(e => e.Slug == Slug
                        && (e.StatusStr == BaseStatus.StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus)
                        || e.StatusStr == BaseStatus.StatusToString(SocialPostStatus.Private, EntityStatus.SocialPostStatus)))
                    .FirstOrDefaultAsync();
            if (post == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }

            return (post, ErrorCodes.NO_ERROR);
        }

        public async Task<(SocialPost, ErrorCodes)> FindPostBySlug(string Slug, Guid SocialUserId)
        {
            if (Slug == string.Empty) {
                return (default, ErrorCodes.INVALID_PARAMS);
            }
            var post = await __DBContext.SocialPosts
                    .Where(e => e.Slug == Slug
                        && (e.StatusStr == BaseStatus.StatusToString(SocialPostStatus.Approved, EntityStatus.SocialPostStatus)
                        || e.StatusStr == BaseStatus.StatusToString(SocialPostStatus.Private, EntityStatus.SocialPostStatus)))
                    .FirstOrDefaultAsync();
            if (post == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }

            #region increase views + add action 'Visited'
            post.Views++;
            await __DBContext.SaveChangesAsync();
            if (SocialUserId != default && post.Status == SocialPostStatus.Approved) {
                var action = await __DBContext.SocialUserActionWithPosts
                    .Where(e => e.PostId == post.Id && e.UserId == SocialUserId)
                    .FirstOrDefaultAsync();
                var actionVisited = BaseAction.ActionToString(UserActionWithPost.Visited, EntityAction.UserActionWithPost);
                if (action != default) {
                    if (!action.Actions.Contains(actionVisited)) {
                        action.Actions.Add(actionVisited);
                        await __DBContext.SaveChangesAsync();
                    }
                } else {
                    await __DBContext.SocialUserActionWithPosts
                        .AddAsync(new SocialUserActionWithPost(){
                            UserId = SocialUserId,
                            PostId = post.Id,
                            Actions = new List<string>(){
                                actionVisited
                            }
                        });
                    await __DBContext.SaveChangesAsync();
                }
            }
            #endregion

            if (post.Status != SocialPostStatus.Approved && (SocialUserId == default || SocialUserId != post.Owner)) {
                return (default, ErrorCodes.NOT_FOUND);
            } else {
                if (SocialUserId == default || SocialUserId != post.Owner) {
                    return (post, ErrorCodes.USER_IS_NOT_OWNER);
                }
            }
            return (post, ErrorCodes.NO_ERROR);
        }

        public async Task<(SocialPost, ErrorCodes)> FindPostById(long Id)
        {
            var post = await __DBContext.SocialPosts
                    .Where(e => e.Id == Id)
                    .Include(e => e.OwnerNavigation)
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

        #region Post action
        public async Task<bool> IsContainsAction(long postId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithPosts
                .Where(e => e.PostId == postId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            return action != default ? action.Actions.Contains(actionStr) : false;
        }
        protected async Task<ErrorCodes> AddAction(long postId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithPosts
                .Where(e => e.PostId == postId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            if (action != default) {
                if (!action.Actions.Contains(actionStr)) {
                    action.Actions.Add(actionStr);
                    if (await __DBContext.SaveChangesAsync() > 0) {
                        return ErrorCodes.NO_ERROR;
                    }
                }
                return ErrorCodes.NO_ERROR;
            } else {
                await __DBContext.SocialUserActionWithPosts
                    .AddAsync(new SocialUserActionWithPost(){
                        UserId = socialUserId,
                        PostId = postId,
                        Actions = new List<string>(){
                            actionStr
                        }
                    });
                if (await __DBContext.SaveChangesAsync() > 0) {
                    return ErrorCodes.NO_ERROR;
                }
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        protected async Task<ErrorCodes> RemoveAction(long postId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithPosts
                .Where(e => e.PostId == postId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            if (action != default) {
                if (action.Actions.Contains(actionStr)) {
                    action.Actions.Remove(actionStr);
                    if (await __DBContext.SaveChangesAsync() > 0) {
                        return ErrorCodes.NO_ERROR;
                    }
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.NO_ERROR;
        }
        public async Task<ErrorCodes> UnLike(long postId, Guid socialUserId)
        {
            return await RemoveAction(postId, socialUserId, BaseAction.ActionToString(UserActionWithPost.Like, EntityAction.UserActionWithPost));
        }
        public async Task<ErrorCodes> Like(long postId, Guid socialUserId)
        {
            await RemoveAction(postId, socialUserId, BaseAction.ActionToString(UserActionWithPost.Dislike, EntityAction.UserActionWithPost));
            return await AddAction(postId, socialUserId, BaseAction.ActionToString(UserActionWithPost.Like, EntityAction.UserActionWithPost));
        }
        public async Task<ErrorCodes> UnDisLike(long postId, Guid socialUserId)
        {
            return await RemoveAction(postId, socialUserId, BaseAction.ActionToString(UserActionWithPost.Dislike, EntityAction.UserActionWithPost));
        }
        public async Task<ErrorCodes> DisLike(long postId, Guid socialUserId)
        {
            await RemoveAction(postId, socialUserId, BaseAction.ActionToString(UserActionWithPost.Like, EntityAction.UserActionWithPost));
            return await AddAction(postId, socialUserId, BaseAction.ActionToString(UserActionWithPost.Dislike, EntityAction.UserActionWithPost));
        }
        public async Task<ErrorCodes> Follow(long postId, Guid socialUserId)
        {
            return await AddAction(postId, socialUserId, BaseAction.ActionToString(UserActionWithPost.Follow, EntityAction.UserActionWithPost));
        }
        public async Task<ErrorCodes> UnFollow(long postId, Guid socialUserId)
        {
            return await RemoveAction(postId, socialUserId, BaseAction.ActionToString(UserActionWithPost.Follow, EntityAction.UserActionWithPost));
        }
        public async Task<ErrorCodes> Report(long postId, Guid socialUserId)
        {
            return await AddAction(postId, socialUserId, BaseAction.ActionToString(UserActionWithPost.Report, EntityAction.UserActionWithPost));
        }
        public async Task<ErrorCodes> Save(long postId, Guid socialUserId)
        {
            return await AddAction(postId, socialUserId, BaseAction.ActionToString(UserActionWithPost.Saved, EntityAction.UserActionWithPost));
        }
        public async Task<ErrorCodes> UnSave(long postId, Guid socialUserId)
        {
            return await RemoveAction(postId, socialUserId, BaseAction.ActionToString(UserActionWithPost.Saved, EntityAction.UserActionWithPost));
        }
        public async Task<ErrorCodes> Comment(long postId, Guid socialUserId)
        {
            return await AddAction(postId, socialUserId, BaseAction.ActionToString(UserActionWithPost.Comment, EntityAction.UserActionWithPost));
        }
        public async Task<ErrorCodes> UnComment(long postId, Guid socialUserId)
        {
            return await RemoveAction(postId, socialUserId, BaseAction.ActionToString(UserActionWithPost.Comment, EntityAction.UserActionWithPost));
        }
        #endregion

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

        public async Task<ErrorCodes> AddNewPost(ParserSocialPost Parser, SocialPost Post, Guid SocialUserId)
        {
            using var transaction = await __DBContext.Database.BeginTransactionAsync();
            Post.Slug = string.Empty;
            await __DBContext.SocialPosts.AddAsync(Post);
            var ok = await __DBContext.SaveChangesAsync() > 0;
            var error = string.Empty;
            if (ok) {
                #region Add foregin key
                foreach (var it in Parser.categories) {
                    // No need check status of category
                    var category = await __DBContext.SocialCategories
                        .Where(c => c.Name == it)
                        .FirstOrDefaultAsync();
                    if (category == default) {
                        ok = false;
                        error = $"Not found category for add post, category: { it }";
                        break;
                    }
                    var postCategory = new SocialPostCategory(){
                        PostId = Post.Id,
                        Post = Post,
                        CategoryId = category.Id,
                        Category = category,
                    };
                    await __DBContext.SocialPostCategories.AddAsync(postCategory);
                    Post.SocialPostCategories.Add(postCategory);
                    if (await __DBContext.SaveChangesAsync() <= 0) {
                        ok = false;
                        error = "Save post-category failed";
                        break;
                    }
                }
                foreach (var it in Parser.tags) {
                    // No need check status of tag
                    var tag = await __DBContext.SocialTags
                        .Where(t => t.Tag == it)
                        .FirstOrDefaultAsync();
                    if (tag == default) {
                        ok = false;
                        error = $"Not found tag for add new post. tag: { it }";
                        break;
                    }
                    var postTag = new SocialPostTag(){
                        PostId = Post.Id,
                        Post = Post,
                        TagId = tag.Id,
                        Tag = tag,
                    };
                    await __DBContext.SocialPostTags.AddAsync(postTag);
                    Post.SocialPostTags.Add(postTag);
                    if (await __DBContext.SaveChangesAsync() <= 0) {
                        ok = false;
                        error = "Save post-tag failed.";
                        break;
                    }

                    #region Add action used tag
                    var action = await __DBContext.SocialUserActionWithTags
                        .Where(e => e.TagId == tag.Id && e.UserId == SocialUserId)
                        .FirstOrDefaultAsync();
                    var actionUsed = BaseAction.ActionToString(UserActionWithTag.Used, EntityAction.UserActionWithTag);
                    if (action != default) {
                        if (!action.Actions.Contains(actionUsed)) {
                            action.Actions.Add(actionUsed);
                            await __DBContext.SaveChangesAsync();
                        }
                    } else {
                        await __DBContext.SocialUserActionWithTags
                            .AddAsync(new SocialUserActionWithTag(){
                                UserId = SocialUserId,
                                TagId = tag.Id,
                                Actions = new List<string>(){
                                    actionUsed
                                }
                            });
                        await __DBContext.SaveChangesAsync();
                    }
                    #endregion
                }
                #endregion
            }

            if (!ok) {
                await transaction.RollbackAsync();
                LogError(error);
                return ErrorCodes.INTERNAL_SERVER_ERROR;
            }
            await transaction.CommitAsync();

            #region [SOCIAL] Write user activity
            var (newPost, errCode) = await FindPostById(Post.Id);
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __SocialUserAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialUserAuditLogManagement>();
                await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                    newPost.GetModelName(),
                    newPost.Id.ToString(),
                    LOG_ACTIONS.CREATE,
                    SocialUserId,
                    new JObject(),
                    newPost.GetJsonObjectForLog()
                );
            }
            #endregion
            return ErrorCodes.NO_ERROR;
        }

        public async Task<ErrorCodes> ModifyPost(SocialPost post, SocialPostModifyModel model)
        {
            using var transaction = await __DBContext.Database.BeginTransactionAsync();
            
            #region Get data change and save
            var haveChange = false;
            var ok = true;
            var error = string.Empty;
            if (model.title != default && post.Title != model.title) {
                post.Title = model.title;
                haveChange = true;
            }
            if (model.thumbnail != default && post.Thumbnail != model.thumbnail) {
                post.Thumbnail = model.thumbnail;
                haveChange = true;
            }
            if (model.short_content != default && post.ShortContent != model.short_content) {
                post.ShortContent = model.short_content;
                haveChange = true;
            }
            if (model.content != default && post.Content != model.content) {
                post.Content = model.content;
                haveChange = true;
            }
            if (model.time_read != default && post.TimeRead != model.time_read) {
                post.TimeRead = (int)model.time_read;
                haveChange = true;
            }
            if (model.content_type != default && post.ContenTypeStr != model.content_type) {
                post.ContenTypeStr = model.content_type;
                haveChange = true;
            }
            if (ok && model.categories != default) {
                var old_categories = post.SocialPostCategories.Select(c => c.Category.Name);
                if (model.categories != old_categories) {
                    haveChange = true;
                    __DBContext.SocialPostCategories.RemoveRange(
                        post.SocialPostCategories
                    );
                    ok = await __DBContext.SaveChangesAsync() >= 0;
                    if (ok) {
                        foreach (var it in model.categories) {
                            // No need check status of category
                            var category = await __DBContext.SocialCategories
                                .Where(c => c.Name == it)
                                .FirstOrDefaultAsync();
                            if (category == default) {
                                ok = false;
                                error = $"Not found category for add post, category: { it }";
                                break;
                            }
                            var postCategory = new SocialPostCategory(){
                                PostId = post.Id,
                                Post = post,
                                CategoryId = category.Id,
                                Category = category,
                            };
                            await __DBContext.SocialPostCategories.AddAsync(postCategory);
                            post.SocialPostCategories.Add(postCategory);
                            if (await __DBContext.SaveChangesAsync() <= 0) {
                                ok = false;
                                error = "Save post-category failed";
                                break;
                            }
                        }
                    } else {
                        error = "Save post-category failed";
                    }
                }
            }

            if (ok && model.tags != default) {
                var old_tags = post.SocialPostTags.Select(c => c.Tag.Tag);
                if (model.tags != old_tags) {
                    haveChange = true;
                    __DBContext.SocialPostTags.RemoveRange(
                        post.SocialPostTags
                    );
                    ok = await __DBContext.SaveChangesAsync() >= 0;
                    if (ok) {
                        foreach (var it in model.tags) {
                            // No need check status of tag
                            var tag = await __DBContext.SocialTags
                                .Where(t => t.Tag == it)
                                .FirstOrDefaultAsync();
                            if (tag == default) {
                                ok = false;
                                error = $"Not found tag for add new post. tag: { it }";
                                break;
                            }
                            var postTag = new SocialPostTag(){
                                PostId = post.Id,
                                Post = post,
                                TagId = tag.Id,
                                Tag = tag,
                            };
                            await __DBContext.SocialPostTags.AddAsync(postTag);
                            post.SocialPostTags.Add(postTag);
                            if (await __DBContext.SaveChangesAsync() <= 0) {
                                ok = false;
                                error = "Save post-tag failed.";
                                break;
                            }

                            #region Add action used tag
                            var action = await __DBContext.SocialUserActionWithTags
                                .Where(e => e.TagId == tag.Id && e.UserId == post.Owner)
                                .FirstOrDefaultAsync();
                            var actionUsed = BaseAction.ActionToString(UserActionWithTag.Used, EntityAction.UserActionWithTag);
                            if (action != default) {
                                if (!action.Actions.Contains(actionUsed)) {
                                    action.Actions.Add(actionUsed);
                                    await __DBContext.SaveChangesAsync();
                                }
                            } else {
                                await __DBContext.SocialUserActionWithTags
                                    .AddAsync(new SocialUserActionWithTag(){
                                        UserId = post.Owner,
                                        TagId = tag.Id,
                                        Actions = new List<string>(){
                                            actionUsed
                                        }
                                    });
                                await __DBContext.SaveChangesAsync();
                            }
                            #endregion
                        }
                    } else {
                        error = "Save post-tag failed";
                    }
                }
            }
            #endregion
            if (!haveChange) {
                return ErrorCodes.NO_CHANGE_DETECTED;
            }

            if (!ok) {
                LogError($"ModifyPost failed. error: { error }");
                return ErrorCodes.INTERNAL_SERVER_ERROR;
            }
            post.LastModifiedTimestamp = DateTime.UtcNow;
            ok = await __DBContext.SaveChangesAsync() > 0;
            if (!ok) {
                LogError($"ModifyPost failed. error: can't update last modified timestamp.");
            }
            await transaction.CommitAsync();
            return ErrorCodes.NO_ERROR;
        }

        public async Task<ErrorCodes> ApprovePost(long Id, Guid AdminUserId)
        {
            var (post, error) = await FindPostById(Id);
            if (error != ErrorCodes.NO_ERROR) {
                return error;
            }
            var oldPost = Utils.DeepClone(post.GetJsonObjectForLog());
            post.Status = SocialPostStatus.Approved;
            post.Slug = Utils.GenerateSlug(post.Title, true);
            foreach (var it in post.SocialPostTags) {
                if (it.Tag.Status == SocialTagStatus.Disabled) {
                    it.Tag.Status = SocialTagStatus.Enabled;
                }
            }

            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [ADMIN] Write social audit log
                (post, error) = await FindPostById(Id);
                if (error == ErrorCodes.NO_ERROR) {
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialAuditLogManagement>();
                        var (oldVal, newVal) = Utils.GetDataChanges(oldPost, post.GetJsonObjectForLog());
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

        public async Task<ErrorCodes> RejectPost(long Id, Guid AdminUserId)
        {
            var (post, error) = await FindPostById(Id);
            if (error != ErrorCodes.NO_ERROR) {
                return error;
            }
            var oldPost = Utils.DeepClone(post.GetJsonObjectForLog());
            post.Status = SocialPostStatus.Rejected;
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [ADMIN] Write social audit log
                (post, error) = await FindPostById(Id);
                if (error == ErrorCodes.NO_ERROR) {
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialAuditLogManagement>();
                        var (oldVal, newVal) = Utils.GetDataChanges(oldPost, post.GetJsonObjectForLog());
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
            var oldPost = Utils.DeepClone(post.GetJsonObjectForLog());
            post.Status = SocialPostStatus.Deleted;
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [ADMIN] Write social audit log
                (post, error) = await FindPostById(Id);
                if (error == ErrorCodes.NO_ERROR) {
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialUserAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialUserAuditLogManagement>();
                        var (oldVal, newVal) = Utils.GetDataChanges(oldPost, post.GetJsonObjectForLog());
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
            var oldPost = Utils.DeepClone(post.GetJsonObjectForLog());
            post.Status = SocialPostStatus.Private;
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [ADMIN] Write social audit log
                (post, error) = await FindPostById(Id);
                if (error == ErrorCodes.NO_ERROR) {
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialUserAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialUserAuditLogManagement>();
                        var (oldVal, newVal) = Utils.GetDataChanges(oldPost, post.GetJsonObjectForLog());
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