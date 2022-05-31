using Common;
using CoreApi.Common;
using CoreApi.Common.Base;
using CoreApi.Models.ModifyModels;
using DatabaseAccess.Common.Actions;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using DatabaseAccess.Context.ParserModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CoreApi.Services
{
    public enum GetPostAction {
        GetPostsAttachedToUser          = 0,
        GetNewPosts                     = 1,
        GetTrendingPosts                = 2,
        GetPostsByUserFollowing         = 3,
        GetPostsByAdminUser             = 4,
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
                case GetPostAction.GetPostsByAdminUser:
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
                        "status",
                        "last_modified_timestamp",
                    };
                default:
                    return default;
            }
        }
        public async Task<(List<SocialPost>, int, ErrorCodes)> GetPostsByAdminUser(int start = 0,
                                                                                   int size = 20,
                                                                                   string search_term = default,
                                                                                   string owner = default,
                                                                                   string[] status = default,
                                                                                   (string, bool)[] orders = default,
                                                                                   string[] tags = default,
                                                                                   string[] categories = default)
        {
            #region validate params
            if (status != default) {
                foreach (var statusStr in status) {
                    var statusType = EntityStatus.StatusStringToType(statusStr);
                    if (statusType == default || statusType == StatusType.Deleted || statusType == StatusType.Private) {
                        return (default, default, ErrorCodes.INVALID_PARAMS);
                    }
                }
            } else {
                status = new string[]{};
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
                                .Where(e => (search_term == default || e.SearchVector.Matches(search_term))
                                    && (
                                        owner == default ||
                                        e.OwnerNavigation.DisplayName.Contains(owner, StringComparison.OrdinalIgnoreCase) ||
                                        e.OwnerNavigation.UserName.Contains(owner, StringComparison.OrdinalIgnoreCase)
                                    )
                                    && ((
                                            status.Count() == 0
                                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted)
                                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Private)
                                        ) || status.Contains(e.StatusStr)
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
                            post.StatusStr,
                            post.TimeRead,
                            post.CreatedTimestamp,
                            post.LastModifiedTimestamp
                        } into gr
                        select new {
                            gr.Key,
                            Likes = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Like))),
                            DisLikes = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Dislike))),
                            Comments = __DBContext.SocialComments.Count(e => e.PostId == gr.Key.Id),
                            Follows = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Follow))),
                            Reports = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Report))),
                        } into ret select new {
                            ret.Key.Id,
                            status                  = ret.Key.StatusStr,
                            views                   = ret.Key.Views,
                            likes                   = ret.Likes,
                            dislikes                = ret.DisLikes,
                            comments                = ret.Comments,
                            follows                 = ret.Follows,
                            reports                 = ret.Reports,
                            title                   = ret.Key.Title,
                            time_read               = ret.Key.TimeRead,
                            created_timestamp       = ret.Key.CreatedTimestamp,
                            last_modified_timestamp = ret.Key.LastModifiedTimestamp,
                        })
                        .OrderBy(orderStr)
                        .Skip(start).Take(size)
                        .Select(e => e.Id)
                    )
                    join posts in __DBContext.SocialPosts on ids equals posts.Id
                    select posts;

            var totalCount = await __DBContext.SocialPosts
                                .CountAsync(e => (search_term == default || e.SearchVector.Matches(search_term))
                                    && ((
                                            status.Count() == 0
                                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted)
                                        ) || status.Contains(e.StatusStr)
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
                    var statusType = EntityStatus.StatusStringToType(statusStr);
                    if (statusType == default || statusType == StatusType.Deleted) {
                        return (default, default, ErrorCodes.INVALID_PARAMS);
                    }
                }
            } else {
                status = new string[]{};
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
                                                && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted))
                                            || status.Contains(e.StatusStr))
                                        : e.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved)
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
                            Likes = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Like))),
                            DisLikes = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Dislike))),
                            Comments = __DBContext.SocialComments.Count(e => e.PostId == gr.Key.Id),
                            Follows = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Follow))),
                            Reports = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Report))),
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
                                                && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted))
                                            || status.Contains(e.StatusStr))
                                        : e.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved)
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
            orderStr = orderStr != string.Empty ? $"visited asc, { orderStr }, created_timestamp desc" : "visited asc, created_timestamp desc";

            var query =
                    from ids in (
                        (from post in __DBContext.SocialPosts
                                .Where(e => (search_term == default || e.SearchVector.Matches(search_term))
                                    && (e.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved)
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
                                : EF.Functions.JsonContains(gr.Key.actionStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Visited)),
                            Likes = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Like))),
                            DisLikes = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Dislike))),
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
                                    && (e.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved))
                                    && (tags.Count() == 0
                                        || e.SocialPostTags.Select(t => t.Tag.Tag).ToArray().Any(t => tags.Contains(t))
                                    )
                                    && (categories.Count() == 0
                                        || e.SocialPostCategories.Select(c => c.Category.Name).ToArray().Any(c => categories.Contains(c))
                                    )
                                );
            return (await query.ToListAsync(), totalCount, ErrorCodes.NO_ERROR);
        }

        public async Task<(List<SocialPost>, int, ErrorCodes)> GetRecommendPostsForPost(long post_id,
                                                                                        int start = 0,
                                                                                        int size = 5,
                                                                                        Guid socialUserId = default)
        {
            #region Validate params
            if (post_id < 1) {
                return (default, default, ErrorCodes.INVALID_PARAMS);
            }
            #endregion
            var (__post, error) = await FindPostById(post_id);
            if (error != ErrorCodes.NO_ERROR) {
                return (default, default, error);
            }
            var tags        = __post.SocialPostTags.Select(e => e.Tag.Tag).ToArray();
            var categories  = __post.SocialPostCategories.Select(e => e.Category.Name).ToArray();

            #region Get config
            var __BaseConfig        = __ServiceProvider.GetService<BaseConfig>();
            var VistedFactor        = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_POST_CONFIG, SUB_CONFIG_KEY.VISTED_FACTOR)
                .Value;
            var ViewsFactor         = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_POST_CONFIG, SUB_CONFIG_KEY.VIEWS_FACTOR)
                .Value;
            var LikesFactor         = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_POST_CONFIG, SUB_CONFIG_KEY.LIKES_FACTOR)
                .Value;
            var CommentsFactor         = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_POST_CONFIG, SUB_CONFIG_KEY.COMMENTS_FACTOR)
                .Value;
            var TagsFactor          = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_POST_CONFIG, SUB_CONFIG_KEY.TAGS_FACTOR)
                .Value;
            var CategoriesFactor    = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_POST_CONFIG, SUB_CONFIG_KEY.CATEGORIES_FACTOR)
                .Value;
            var CommonWordsFactor     = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_POST_CONFIG, SUB_CONFIG_KEY.COMMON_WORDS_FACTOR)
                .Value;
            var CommonWordsSize     = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_POST_CONFIG, SUB_CONFIG_KEY.COMMON_WORDS_SIZE)
                .Value;
            var MaxSize     = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_POST_CONFIG, SUB_CONFIG_KEY.MAX_SIZE)
                .Value;
            #endregion

            var CommonWords = await DBHelper.RawSqlQuery(
                "SELECT word FROM ts_stat"
                + $"('SELECT to_tsvector(''simple'', title || short_content) FROM social_post WHERE id={ post_id }')"
                + $"ORDER BY nentry DESC, ndoc DESC, word LIMIT { CommonWordsSize }",
                x => (string)x[0]
            );

            var query =
                    (from post in __DBContext.SocialPosts
                        .Where(e => e.Id != post_id && (e.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved)))
                    join action in __DBContext.SocialUserActionWithPosts on post.Id equals action.PostId
                    into postWithAction
                    from p in postWithAction.DefaultIfEmpty()
                    group p by new {
                        post.Id,
                        post.Views,
                        post.CreatedTimestamp,
                        MatchTags         = post.SocialPostTags.Select(t => t.Tag.Tag).ToArray().Any(t => tags.Contains(t)),
                        MatchCategories   = post.SocialPostCategories.Select(c => c.Category.Name).ToArray().Any(c => categories.Contains(c)),
                        MatchCommonWords  = post.SearchVector.Matches(EF.Functions.ToTsQuery(string.Join(" | ", CommonWords))),
                        actionStr         = post.SocialUserActionWithPosts
                            .Where(e => e.UserId == socialUserId)
                            .Select(e => e.ActionsStr)
                            .FirstOrDefault()
                    } into gr
                    select new {
                        gr.Key,
                        Visited = (socialUserId == default) ? false
                            : EF.Functions.JsonContains(gr.Key.actionStr,
                            EntityAction.GenContainsJsonStatement(ActionType.Visited)),
                        Likes = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                            EntityAction.GenContainsJsonStatement(ActionType.Like))),
                        Comments = __DBContext.SocialComments.Count(e => e.PostId == gr.Key.Id)
                    } into ret select new {
                        ret.Key.Id,
                        sort_factor =
                            (ret.Visited ? 0 : VistedFactor)
                            + (ret.Key.Views * ViewsFactor)
                            + (ret.Likes * LikesFactor)
                            + (ret.Comments * CommentsFactor)
                            + (ret.Key.MatchTags ? 0 : TagsFactor)
                            + (ret.Key.MatchCategories ? 0 : CategoriesFactor)
                            + (ret.Key.MatchCommonWords ? 0 : CommonWordsFactor),
                        created_timestamp = ret.Key.CreatedTimestamp,
                    })
                    .OrderBy("sort_factor desc, created_timestamp desc")
                    .Take(MaxSize);

            var queryPosts = 
                    from ids in (query.Skip(start).Take(size).Select(e => e.Id))
                    join posts in __DBContext.SocialPosts on ids equals posts.Id
                    select posts;

            return (await queryPosts.ToListAsync(), await query.CountAsync(), ErrorCodes.NO_ERROR);
        }

        public async Task<(List<SocialPost>, int, ErrorCodes)> GetRecommendPostsForUser(Guid SocialUserId,
                                                                                        int start = 0,
                                                                                        int size = 5)
        {
            var tags        = await __DBContext.SocialUserActionWithTags
                .Where(e => e.UserId == SocialUserId && EF.Functions.JsonContains(e.ActionsStr,
                            EntityAction.GenContainsJsonStatement(ActionType.Follow)))
                .Select(e => e.Tag.Tag).ToListAsync();
            var categories  = await __DBContext.SocialUserActionWithCategories
                .Where(e => e.UserId == SocialUserId && EF.Functions.JsonContains(e.ActionsStr,
                            EntityAction.GenContainsJsonStatement(ActionType.Follow)))
                .Select(e => e.Category.Name).ToListAsync();
            
            var actionPosts = await __DBContext.SocialUserActionWithPosts
                .Where(e => e.UserId == SocialUserId && (
                        EF.Functions.JsonContains(e.ActionsStr,
                            EntityAction.GenContainsJsonStatement(ActionType.Follow))
                        || EF.Functions.JsonContains(e.ActionsStr,
                            EntityAction.GenContainsJsonStatement(ActionType.Like))
                    )
                )
                .Select(e => e.Post).ToListAsync();
            var actionPostIds = actionPosts.Select(e => e.Id).ToList();
            actionPostIds.AddRange(await __DBContext.SocialPosts.Where(e => e.Owner == SocialUserId)
                .Select(e => e.Id).ToArrayAsync());
            actionPosts.ForEach(e => tags.AddRange(e.SocialPostTags.Select(e => e.Tag.Tag).ToList()));
            actionPosts.ForEach(e => categories.AddRange(e.SocialPostCategories.Select(e => e.Category.Name).ToList()));

            #region Get config
            var __BaseConfig        = __ServiceProvider.GetService<BaseConfig>();
            var VistedFactor        = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_USER_CONFIG, SUB_CONFIG_KEY.VISTED_FACTOR)
                .Value;
            var ViewsFactor         = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_USER_CONFIG, SUB_CONFIG_KEY.VIEWS_FACTOR)
                .Value;
            var LikesFactor         = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_USER_CONFIG, SUB_CONFIG_KEY.LIKES_FACTOR)
                .Value;
            var CommentsFactor         = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_USER_CONFIG, SUB_CONFIG_KEY.COMMENTS_FACTOR)
                .Value;
            var TagsFactor          = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_USER_CONFIG, SUB_CONFIG_KEY.TAGS_FACTOR)
                .Value;
            var CategoriesFactor    = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_USER_CONFIG, SUB_CONFIG_KEY.CATEGORIES_FACTOR)
                .Value;
            var CommonWordsFactor     = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_USER_CONFIG, SUB_CONFIG_KEY.COMMON_WORDS_FACTOR)
                .Value;
            var CommonWordsSize     = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_USER_CONFIG, SUB_CONFIG_KEY.COMMON_WORDS_SIZE)
                .Value;
            var MaxSize     = __BaseConfig
                .GetConfigValue<int>(CONFIG_KEY.API_GET_RECOMMEND_POSTS_FOR_USER_CONFIG, SUB_CONFIG_KEY.MAX_SIZE)
                .Value;
            #endregion

            var CommonWords = new List<string>();
            if (actionPostIds.Count != 0){
                CommonWords = await DBHelper.RawSqlQuery(
                    "SELECT word FROM ts_stat"
                    + "('SELECT to_tsvector(''simple'', title || short_content) FROM social_post "
                    + $"WHERE id IN ({ string.Join(",", actionPostIds) })') "
                    + $"ORDER BY nentry DESC, ndoc DESC, word LIMIT { CommonWordsSize }",
                    x => (string)x[0]
                );
            }

            var query =
                    (from post in __DBContext.SocialPosts
                        .Where(e => e.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved))
                    join action in __DBContext.SocialUserActionWithPosts on post.Id equals action.PostId
                    into postWithAction
                    from p in postWithAction.DefaultIfEmpty()
                    group p by new {
                        post.Id,
                        post.Views,
                        post.CreatedTimestamp,
                        MatchTags         = post.SocialPostTags.Select(t => t.Tag.Tag).ToArray().Any(t => tags.Contains(t)),
                        MatchCategories   = post.SocialPostCategories.Select(c => c.Category.Name).ToArray().Any(c => categories.Contains(c)),
                        MatchCommonWords  = post.SearchVector.Matches(EF.Functions.ToTsQuery(string.Join(" | ", CommonWords))),
                        actionStr         = post.SocialUserActionWithPosts
                            .Where(e => e.UserId == SocialUserId)
                            .Select(e => e.ActionsStr)
                            .FirstOrDefault()
                    } into gr
                    select new {
                        gr.Key,
                        Visited = (SocialUserId == default) ? false
                            : EF.Functions.JsonContains(gr.Key.actionStr,
                            EntityAction.GenContainsJsonStatement(ActionType.Visited)),
                        Likes = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                            EntityAction.GenContainsJsonStatement(ActionType.Like))),
                        Comments = __DBContext.SocialComments.Count(e => e.PostId == gr.Key.Id)
                    } into ret select new {
                        ret.Key.Id,
                        sort_factor =
                            (ret.Visited ? 0 : VistedFactor)
                            + (ret.Key.Views * ViewsFactor)
                            + (ret.Likes * LikesFactor)
                            + (ret.Comments * CommentsFactor)
                            + (ret.Key.MatchTags ? 0 : TagsFactor)
                            + (ret.Key.MatchCategories ? 0 : CategoriesFactor)
                            + (ret.Key.MatchCommonWords ? 0 : CommonWordsFactor),
                        created_timestamp = ret.Key.CreatedTimestamp,
                    })
                    .OrderBy("sort_factor desc, created_timestamp desc")
                    .Take(MaxSize);

            var queryPosts = 
                    from ids in (query.Skip(start).Take(size).Select(e => e.Id))
                    join posts in __DBContext.SocialPosts on ids equals posts.Id
                    select posts;

            return (await queryPosts.ToListAsync(), await query.CountAsync(), ErrorCodes.NO_ERROR);
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
                                    && (e.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved))
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
                                : EF.Functions.JsonContains(gr.Key.actionStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Visited)),
                            Likes = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Like))),
                            DisLikes = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Dislike))),
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
                                    && (e.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved))
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
            orderStr = orderStr != string.Empty ? $"visited asc, { orderStr }, created_timestamp desc" : "visited asc, created_timestamp desc";

            // Fllow: post, user, tag, category
            var query_post_of_following_users =
                    from user_following_ids in __DBContext.SocialUserActionWithUsers
                        .Where(e => e.UserId == socialUserId
                            && EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Follow))
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
                            && EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Follow))
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
                            && EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Follow))
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
                            && EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Follow))
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
                                        (e.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved))
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
                                : EF.Functions.JsonContains(gr.Key.actionStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Visited)),
                            Likes = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Like))),
                            DisLikes = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Dislike))),
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
                                        (e.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved))
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

        public async Task<(SocialPost, ErrorCodes)> FindPostBySlug(string Slug,
                                                                   Guid SocialUserId = default,
                                                                   bool IncreaseView = false)
        {
            if (Slug == string.Empty) {
                return (default, ErrorCodes.INVALID_PARAMS);
            }
            var post = await __DBContext.SocialPosts
                    .Where(e => e.Slug == Slug
                        && (e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted))
                    )
                    .FirstOrDefaultAsync();
            if (post == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }

            if (post.Status.Type != StatusType.Approved && (SocialUserId == default || SocialUserId != post.Owner)) {
                return (post, ErrorCodes.USER_IS_NOT_OWNER);
            }

            #region increase views + add action 'Visited'
            if (IncreaseView) {
                post.Views++;
                await __DBContext.SaveChangesAsync();
                if (SocialUserId != default && post.Status.Type == StatusType.Approved) {
                    await Visited(post.Id, SocialUserId);
                }
            }
            #endregion

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
                            && e.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved)
                            && e.StatusStr == EntityStatus.StatusTypeToString(StatusType.Private)));
            return (count > 0, ErrorCodes.NO_ERROR);
        }

        #region Post action
        public async Task<bool> IsContainsAction(long postId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithPosts
                .Where(e => e.PostId == postId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            return action != default ? action.Actions.Count(e => e.action == actionStr) > 0 : false;
        }
        protected async Task<ErrorCodes> AddAction(long postId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithPosts
                .Where(e => e.PostId == postId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            if (action != default) {
                if (!(action.Actions.Count(a => a.action == actionStr) > 0)) {
                    action.Actions.Add(new EntityAction(EntityActionType.UserActionWithPost, actionStr));
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
                        Actions = new List<EntityAction>(){
                            new EntityAction(EntityActionType.UserActionWithPost, actionStr)
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
                var _action = action.Actions.Where(a => a.action == actionStr).FirstOrDefault();
                if (_action != default) {
                    action.Actions.Remove(_action);
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
            return await RemoveAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Like));
        }
        public async Task<ErrorCodes> Like(long postId, Guid socialUserId)
        {
            await RemoveAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Dislike));
            return await AddAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Like));
        }
        public async Task<ErrorCodes> UnDisLike(long postId, Guid socialUserId)
        {
            return await RemoveAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Dislike));
        }
        public async Task<ErrorCodes> DisLike(long postId, Guid socialUserId)
        {
            await RemoveAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Like));
            return await AddAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Dislike));
        }
        public async Task<ErrorCodes> Follow(long postId, Guid socialUserId)
        {
            return await AddAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Follow));
        }
        public async Task<ErrorCodes> UnFollow(long postId, Guid socialUserId)
        {
            return await RemoveAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Follow));
        }
        public async Task<ErrorCodes> Report(long postId, Guid socialUserId)
        {
            return await AddAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Report));
        }
        public async Task<ErrorCodes> Save(long postId, Guid socialUserId)
        {
            return await AddAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Saved));
        }
        public async Task<ErrorCodes> UnSave(long postId, Guid socialUserId)
        {
            return await RemoveAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Saved));
        }
        public async Task<ErrorCodes> Comment(long postId, Guid socialUserId)
        {
            return await AddAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Comment));
        }
        public async Task<ErrorCodes> Visited(long postId, Guid socialUserId)
        {
            return await AddAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Visited));
        }
        public async Task<ErrorCodes> UnComment(long postId, Guid socialUserId)
        {
            return await RemoveAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Comment));
        }
        #endregion

        #region Post handle
        public ErrorCodes ValidateChangeStatusAction(StatusType from, StatusType to)
        {
            if ((
                    from == StatusType.Pending && (
                    to == StatusType.Approved ||
                    to == StatusType.Deleted ||
                    to == StatusType.Rejected ||
                    to == StatusType.Private)
                ) ||
                (
                    from == StatusType.Approved && (
                    to == StatusType.Deleted ||
                    to == StatusType.Rejected ||
                    to == StatusType.Private)
                ) ||
                (
                    from == StatusType.Rejected && (
                    to == StatusType.Deleted ||
                    to == StatusType.Approved)
                ) ||
                (
                    from == StatusType.Private && (
                    to == StatusType.Pending ||
                    to == StatusType.Deleted ||
                    to == StatusType.Approved)
                )
            ) {
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INVALID_ACTION;
        }

        public async Task<ErrorCodes> AddNewPost(ParserSocialPost Parser, SocialPost Post, Guid SocialUserId)
        {
            using var transaction = await __DBContext.Database.BeginTransactionAsync();
            var __SocialTagManagement = (SocialTagManagement)__ServiceProvider.GetService(typeof(SocialTagManagement));
            var __SocialCategoryManagement = (SocialCategoryManagement)__ServiceProvider.GetService(typeof(SocialCategoryManagement));

            Post.Slug = Utils.GenerateSlug(Post.Title, true);
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
                    await __SocialTagManagement.Used(tag.Id, SocialUserId);
                    #endregion
                }
                #endregion
            }

            if (!ok) {
                await transaction.RollbackAsync();
                WriteLog(LOG_LEVEL.ERROR, error);
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

        public async Task<ErrorCodes> AddPendingContent(long Id, SocialPostModifyModel Model, JObject RawModifyBody)
        {
            var (Post, Error) = await FindPostById(Id);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }

            #region Check data change
            var HaveChange = false;
            if (Model.title != default && Post.Title != Model.title) {
                HaveChange = true;
            }
            if (RawModifyBody.ContainsKey("thumbnail") && Post.Thumbnail != Model.thumbnail) {
                HaveChange = true;
            }
            if (Model.short_content != default && Post.ShortContent != Model.short_content) {
                HaveChange = true;
            }
            if (Model.content != default && Post.Content != Model.content) {
                HaveChange = true;
            }
            if (Model.time_read != default && Post.TimeRead != Model.time_read) {
                HaveChange = true;
            }
            if (Model.content_type != default && Post.ContenTypeStr != Model.content_type) {
                HaveChange = true;
            }
            if (Model.categories != default) {
                var OldCategories = Post.SocialPostCategories.Select(c => c.Category.Name);
                if (Model.categories != OldCategories) {
                    HaveChange = true;
                }
            }
            if (Model.tags != default) {
                var OldTags = Post.SocialPostTags.Select(c => c.Tag.Tag);
                if (Model.tags != OldTags) {
                    HaveChange = true;
                }
            }
            #endregion
            if (!HaveChange || Post.PendingContent == RawModifyBody) {
                return ErrorCodes.NO_CHANGE_DETECTED;
            }

            Post.PendingContent = RawModifyBody;
            if (await __DBContext.SaveChangesAsync() <= 0) {
                WriteLog(LOG_LEVEL.ERROR, "AddPendingContent failed.");
            }
            return ErrorCodes.NO_ERROR;
        }

        protected async Task<ErrorCodes> ModifyPost(SocialPost Post, SocialPostModifyModel Model, bool ApprovePost, JObject RawModifyBody)
        {
            using var Transaction = await __DBContext.Database.BeginTransactionAsync();
            var __SocialTagManagement = (SocialTagManagement)__ServiceProvider.GetService(typeof(SocialTagManagement));
            var __SocialCategoryManagement = (SocialCategoryManagement)__ServiceProvider.GetService(typeof(SocialCategoryManagement));

            #region Get data change and save
            var HaveChange = false;
            var Ok = true;
            var Error = string.Empty;
            if (Model.title != default && Post.Title != Model.title) {
                Post.Title = Model.title;
                Post.Slug = Utils.GenerateSlug(Post.Title, true);
                HaveChange = true;
            }
            if (RawModifyBody.ContainsKey("thumbnail") && Post.Thumbnail != Model.thumbnail) {
                Post.Thumbnail = Model.thumbnail;
                HaveChange = true;
            }
            if (Model.short_content != default && Post.ShortContent != Model.short_content) {
                Post.ShortContent = Model.short_content;
                HaveChange = true;
            }
            if (Model.content != default && Post.Content != Model.content) {
                Post.Content = Model.content;
                HaveChange = true;
            }
            if (Model.time_read != default && Post.TimeRead != Model.time_read) {
                Post.TimeRead = (int) Model.time_read;
                HaveChange = true;
            }
            if (Model.content_type != default && Post.ContenTypeStr != Model.content_type) {
                Post.ContenTypeStr = Model.content_type;
                HaveChange = true;
            }
            if (Ok && Model.categories != default) {
                var OldCategories = Post.SocialPostCategories.Select(c => c.Category.Name);
                if (Model.categories.Count(e => OldCategories.Contains(e)) == OldCategories.Count()) {
                    HaveChange = true;
                    __DBContext.SocialPostCategories.RemoveRange(
                        Post.SocialPostCategories
                    );
                    Ok = await __DBContext.SaveChangesAsync() >= 0;
                    if (Ok) {
                        foreach (var it in Model.categories) {
                            // No need check status of category
                            var (Category, ErrCode) = await __SocialCategoryManagement.FindCategoryByNameIgnoreStatus(it);
                            if (ErrCode != ErrorCodes.NO_ERROR) {
                                Ok = false;
                                Error = $"Not found category for add post, category: { it }";
                                break;
                            }
                            var PostCategory = new SocialPostCategory(){
                                PostId      = Post.Id,
                                Post        = Post,
                                CategoryId  = Category.Id,
                                Category    = Category,
                            };
                            await __DBContext.SocialPostCategories.AddAsync(PostCategory);
                            Post.SocialPostCategories.Add(PostCategory);
                            if (await __DBContext.SaveChangesAsync() <= 0) {
                                Ok = false;
                                Error = "Save post-category failed";
                                break;
                            }
                        }
                    } else {
                        Error = "Save post-category failed";
                    }
                }
            }
            if (Ok && Model.tags != default) {
                var OldTags = Post.SocialPostTags.Select(c => c.Tag.Tag);
                if (Model.tags.Count(e => OldTags.Contains(e)) == OldTags.Count()) {
                    HaveChange = true;
                    __DBContext.SocialPostTags.RemoveRange(
                        Post.SocialPostTags
                    );
                    Ok = await __DBContext.SaveChangesAsync() >= 0;
                    if (Ok) {
                        foreach (var it in Model.tags) {
                            // No need check status of tag
                            var (Tag, ErrCode) = await __SocialTagManagement.FindTagByNameIgnoreStatus(it);
                            if (ErrCode != ErrorCodes.NO_ERROR) {
                                Ok = false;
                                Error = $"Not found tag for add new post. tag: { it }";
                                break;
                            }
                            var PostTag = new SocialPostTag(){
                                PostId  = Post.Id,
                                Post    = Post,
                                TagId   = Tag.Id,
                                Tag     = Tag,
                            };
                            await __DBContext.SocialPostTags.AddAsync(PostTag);
                            Post.SocialPostTags.Add(PostTag);
                            if (await __DBContext.SaveChangesAsync() <= 0) {
                                Ok = false;
                                Error = "Save post-tag failed.";
                                break;
                            }

                            #region Add action used tag
                            await __SocialTagManagement.Used(Tag.Id, Post.Owner);
                            #endregion
                        }
                    } else {
                        Error = "Save post-tag failed";
                    }
                }
            }
            #endregion
            if (!HaveChange) {
                WriteLog(LOG_LEVEL.WARNING, "ModifyPost with no change detected",
                    Utils.ParamsToLog("post_id", Post.Id)
                );
            }

            if (!Ok) {
                WriteLog(LOG_LEVEL.ERROR, "ModifyPost failed",
                    Utils.ParamsToLog("error", Error)
                );
                return ErrorCodes.INTERNAL_SERVER_ERROR;
            }

            if (ApprovePost) {
                Post.PendingContent = default;
                Post.Status.ChangeStatus(StatusType.Approved);
                foreach (var it in Post.SocialPostTags) {
                    if (it.Tag.Status.Type == StatusType.Disabled) {
                        it.Tag.Status.ChangeStatus(StatusType.Enabled);
                    }
                }
            }

            Post.LastModifiedTimestamp = DateTime.UtcNow;
            Ok = await __DBContext.SaveChangesAsync() > 0;
            if (!Ok) {
                WriteLog(LOG_LEVEL.ERROR, "ModifyPost failed",
                    Utils.ParamsToLog("error", "can't update last modified timestamp.")
                );
            }
            await Transaction.CommitAsync();
            return ErrorCodes.NO_ERROR;
        }

        public async Task<ErrorCodes> ModifyPostNotApproved(long Id, SocialPostModifyModel Model, JObject RawModifyBody)
        {
            var (Post, Error) = await FindPostById(Id);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }

            var OldPost = Utils.DeepClone(Post.GetJsonObjectForLog());
            Error = await ModifyPost(Post, Model, false, RawModifyBody);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }

            #region Write social audit log
            (Post, Error) = await FindPostById(Id);
            if (Error == ErrorCodes.NO_ERROR) {
                using (var scope = __ServiceProvider.CreateScope())
                {
                    var __SocialUserAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialUserAuditLogManagement>();
                    var (OldVal, NewVal) = Utils.GetDataChanges(OldPost, Post.GetJsonObjectForLog());
                    await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                        Post.GetModelName(),
                        Post.Id.ToString(),
                        LOG_ACTIONS.MODIFY,
                        Post.Owner,
                        OldVal,
                        NewVal
                    );
                }
            } else {
                return ErrorCodes.INTERNAL_SERVER_ERROR;
            }
            #endregion
            return ErrorCodes.NO_ERROR;
        }

        public async Task<ErrorCodes> ApprovePost(long Id, Guid AdminUserId)
        {
            var (Post, Error) = await FindPostById(Id);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            var OldPost = Utils.DeepClone(Post.GetJsonObjectForLog());
            if (Post.Status.Type == StatusType.Pending
                || (Post.Status.Type == StatusType.Private && Post.PendingContent == default)
            ) {
                Post.Status.ChangeStatus(StatusType.Approved);
                Post.ApprovedTimestamp = DateTime.UtcNow;
                foreach (var it in Post.SocialPostTags) {
                    if (it.Tag.Status.Type == StatusType.Disabled) {
                        it.Tag.Status.ChangeStatus(StatusType.Enabled);
                    }
                }

                if (await __DBContext.SaveChangesAsync() <= 0) {
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
            } else {
                Error = await ModifyPost(Post,
                                         SocialPostModifyModel.FromJson(Utils.DeepClone(Post.PendingContent)),
                                         true,
                                         Utils.DeepClone(Post.PendingContent));
                if (Error != ErrorCodes.NO_ERROR) {
                    return Error;
                }
            }

            #region Write social audit log
            (Post, Error) = await FindPostById(Id);
            if (Error == ErrorCodes.NO_ERROR) {
                using (var scope = __ServiceProvider.CreateScope())
                {
                    var __SocialUserAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialUserAuditLogManagement>();
                    var (OldVal, NewVal) = Utils.GetDataChanges(OldPost, Post.GetJsonObjectForLog());
                    await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                        Post.GetModelName(),
                        Post.Id.ToString(),
                        LOG_ACTIONS.MODIFY,
                        Post.Owner,
                        OldVal,
                        NewVal,
                        AdminUserId
                    );
                }
            } else {
                return ErrorCodes.INTERNAL_SERVER_ERROR;
            }
            #endregion
            return ErrorCodes.NO_ERROR;
        }

        public async Task<ErrorCodes> RejectPost(long Id, Guid AdminUserId, bool rejectPendingContent)
        {
            var (post, error) = await FindPostById(Id);
            if (error != ErrorCodes.NO_ERROR) {
                return error;
            }
            if (rejectPendingContent == true
                && (post.StatusStr != EntityStatus.StatusTypeToString(StatusType.Approved) || post.PendingContent == default)
            ) {
                return ErrorCodes.INVALID_PARAMS;
            }

            var oldPost = Utils.DeepClone(post.GetJsonObjectForLog());
            if (rejectPendingContent) {
                post.PendingContent = default;
            } else {
                post.Status.ChangeStatus(StatusType.Rejected);
            }

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
                            LOG_ACTIONS.MODIFY,
                            post.Owner,
                            oldVal,
                            newVal,
                            AdminUserId
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
            post.Status.ChangeStatus(StatusType.Deleted);
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
                            new JObject(),
                            new JObject()
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

        public async Task<ErrorCodes> PublishPostPrivate(long Id, Guid SocialUser)
        {
            var (post, error) = await FindPostById(Id);
            if (error != ErrorCodes.NO_ERROR) {
                return error;
            }
            var oldPost = Utils.DeepClone(post.GetJsonObjectForLog());
            post.Status.ChangeStatus(StatusType.Pending);
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
                            LOG_ACTIONS.MODIFY,
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
            post.Status.ChangeStatus(StatusType.Private);
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
                            LOG_ACTIONS.MODIFY,
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