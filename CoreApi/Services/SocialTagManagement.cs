using Common;
using CoreApi.Common;
using CoreApi.Common.Base;
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
    public enum GetTagAction {
        GetTagsByAction         = 1,
    }
    public class SocialTagManagement : BaseTransientService
    {
        public SocialTagManagement(IServiceProvider _IServiceProvider) : base(_IServiceProvider)
        {
            __ServiceName = "SocialTagManagement";
        }
        public string[] GetAllowOrderFields(GetTagAction action)
        {
            switch (action) {
                case GetTagAction.GetTagsByAction:
                    return new string[] {
                        "used",
                        "posts",
                        "follows",
                        "time_action",
                        "created_timestamp",
                        "last_modified_timestamp",
                    };
                default:
                    return default;
            }
        }
        public async Task<(List<SocialTag>, int, ErrorCodes)> GetTagsByAction(Guid socialUserId,
                                                                                string action,
                                                                                int start = 0,
                                                                                int size = 20,
                                                                                string search_term = default,
                                                                                (string, bool)[] orders = default)
        {
            action = char.ToUpper(action[0]) + action.Substring(1).ToLower();
            #region validate params
            if (!EntityAction.ValidateAction(action, EntityActionType.UserActionWithTag)) {
                return (default, default, ErrorCodes.INVALID_PARAMS);
            }
            var ColumnAllowOrder = GetAllowOrderFields(GetTagAction.GetTagsByAction);
            if (orders != default) {
                foreach (var order in orders) {
                    if (!ColumnAllowOrder.Contains(order.Item1)) {
                        return (default, default, ErrorCodes.INVALID_PARAMS);
                    }
                }
            } else {
                orders = new (string, bool)[]{};
            }
            var __SocialUserManagement  = __ServiceProvider.GetService<SocialUserManagement>();
            var (user, error)           = await __SocialUserManagement.FindUserById(socialUserId);
            if (error != ErrorCodes.NO_ERROR) {
                return (default, default, error);
            }
            #endregion

            // orderStr can't empty or null
            string orderStr = orders != default && orders.Length != 0
                ? Utils.GenerateOrderString(orders)
                : "time_action desc";
            if (orderStr == string.Empty) {
                orderStr = "time_action desc";
            }
            search_term = search_term == default ? string.Empty : search_term;
            var rawQuery =
                "SELECT     T.id, T.tag, "
                            + "COUNT(DISTINCT TA.user_id) FILTER (WHERE TA.actions @> '[{\"action\": \"Used\"}]') AS used, "
                            + "COUNT(DISTINCT TA.user_id) FILTER (WHERE TA.actions @> '[{\"action\": \"Follow\"}]') AS follows, "
                            + "COUNT(DISTINCT P.id) FILTER (WHERE P.status = 'Approved') AS posts, "
                            + "T.created_timestamp, T.last_modified_timestamp, TA.time_action "
                + "FROM "
                    + "social_tag AS T JOIN "
                    + "("
                        + "SELECT   tag_id, user_id, actions, "
                                    + "(jsonb_path_query_first("
                                        + "actions, "
                                        + "'$[*] ? (@.action == $action)', "
                                        + $"'{{\"action\": \"{ action }\"}}'"
                                    + ")::jsonb ->> 'date')::timestamptz AS time_action "
                        + "FROM     social_user_action_with_tag "
                        + $"WHERE    actions @> '[{{\"action\":\"{ action }\"}}]' AND user_id = '{ socialUserId.ToString() }' "
                    + ") AS TA ON T.id = TA.tag_id "
                    + "LEFT JOIN social_post_tag AS PT ON PT.tag_id = T.id "
                    + "JOIN social_post AS P ON P.id = PT.post_id "
                + $"WHERE       T.status != 'Disabled' AND LOWER(T.tag) LIKE LOWER('%{ search_term }%') "
                                + $"AND LOWER(T.name) LIKE LOWER('%{ search_term }%') "
                                + $"AND LOWER(T.describe) LIKE LOWER('%{ search_term }%') "
                + "GROUP BY     T.id, T.tag, "
                                + "T.created_timestamp, T.last_modified_timestamp, TA.time_action "
                + $"ORDER BY { orderStr }";
            var tagIdsOrdered = await DBHelper.RawSqlQuery<long>(
                rawQuery,
                x => (long)x[0]
            );

            var queryTags = from ids in tagIdsOrdered
                        join tag in __DBContext.SocialTags on ids equals tag.Id
                        select tag;

            return (queryTags.ToList(), tagIdsOrdered.Count(), ErrorCodes.NO_ERROR);
        }
        public async Task<(List<SocialTag>, int, ErrorCodes)> GetTrendingTags(int time = 7, // days
                                                                              int start = 0,
                                                                              int size = 20,
                                                                              string search_term = default)
        {
            // orderStr can't empty or null
            var orderStr = $"used desc, follows desc, posts desc, created_timestamp desc";
            var compareDate = DateTime.UtcNow.AddDays(-time);

            var query =
                    from ids in (
                        (from tag in __DBContext.SocialTags
                                .Where(e => e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled)
                                        && (search_term == default
                                        || e.Tag.ToLower().Contains(search_term)
                                        || e.Name.ToLower().Contains(search_term)
                                        || e.Describe.ToLower().Contains(search_term)
                                    )
                                    && (time == -1 || e.CreatedTimestamp >= compareDate)
                                )
                        join action in __DBContext.SocialUserActionWithTags on tag.Id equals action.TagId
                        into tagWithAction
                        from t in tagWithAction.DefaultIfEmpty()
                        group t by new {
                            tag.Id,
                            tag.CreatedTimestamp
                        } into gr
                        select new {
                            gr.Key,
                            Follow = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Follow))),
                            Used = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Used))),
                            Posts = __DBContext.SocialPosts.Count(e =>
                                e.SocialPostTags.Count(pt => pt.TagId == gr.Key.Id) > 0
                                && e.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved)
                            ),
                        } into ret select new {
                            ret.Key.Id,
                            used = ret.Used,
                            posts = ret.Posts,
                            follows = ret.Follow,
                            created_timestamp = ret.Key.CreatedTimestamp,
                        })
                        .OrderBy(orderStr)
                        .Skip(start).Take(size)
                        .Select(e => e.Id)
                    )
                    join tags in __DBContext.SocialTags on ids equals tags.Id
                    select tags;

            var totalCount = await __DBContext.SocialTags
                                .CountAsync(e => e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled)
                                        && (search_term == default
                                        || e.Tag.ToLower().Contains(search_term)
                                        || e.Name.ToLower().Contains(search_term)
                                        || e.Describe.ToLower().Contains(search_term)
                                    )
                                    && (time == -1 || e.CreatedTimestamp >= compareDate)
                                );
            return (await query.ToListAsync(), totalCount, ErrorCodes.NO_ERROR);
        }
        public async Task<(List<SocialTag>, int)> SearchTags(int start = 0,
                                                             int size = 20,
                                                             string search_term = default,
                                                             Guid socialUserId = default,
                                                             bool isAdmin = false)
        {
            search_term = search_term == default ? default : search_term.Trim().ToLower();
            var query =
                    from ids in (
                        (from tag in __DBContext.SocialTags
                            .Where(e => (isAdmin || e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                                    && (search_term == default
                                    || e.Tag.ToLower().Contains(search_term)
                                    || e.Name.ToLower().Contains(search_term)
                                    || e.Describe.ToLower().Contains(search_term)
                                )
                            )
                        join action in __DBContext.SocialUserActionWithTags on tag.Id equals action.TagId
                        into tagWithAction
                        from t in tagWithAction.DefaultIfEmpty()
                        group t by new { tag.Id, tag.Tag } into gr
                        select new {
                            gr.Key,
                            Match = (search_term != default) ? (gr.Key.Tag.ToLower() == search_term ? 1 : 0) : 0,
                            StartWith = (search_term != default) ? (gr.Key.Tag.StartsWith(search_term) ? 1 : 0) : 0,
                            Follow = gr.Count(e => (socialUserId != default)
                                && (e.UserId == socialUserId)
                                && EF.Functions.JsonContains(e.ActionsStr, EntityAction.GenContainsJsonStatement(ActionType.Follow))),
                            Used = gr.Count(e => (socialUserId != default)
                                && (e.UserId == socialUserId)
                                && EF.Functions.JsonContains(e.ActionsStr, EntityAction.GenContainsJsonStatement(ActionType.Used))),
                        } into ret
                        orderby ret.Used descending, ret.Follow descending, ret.StartWith descending, ret.Match descending
                        select ret.Key.Id).Skip(start).Take(size)
                    )
                    join tags in __DBContext.SocialTags on ids equals tags.Id
                    select tags;

            var totalCount = await __DBContext.SocialTags
                            .CountAsync(e => (isAdmin || e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                                    && (search_term == default
                                    || e.Tag.ToLower().Contains(search_term)
                                    || e.Name.ToLower().Contains(search_term)
                                    || e.Describe.ToLower().Contains(search_term)
                                )
                            );

            return (await query.ToListAsync(), totalCount);
        }

        public async Task<(List<SocialTag>, int)> GetTags(int start = 0,
                                                          int size = 20,
                                                          string search_term = default,
                                                          Guid socialUserId = default)
        {
            search_term = search_term == default ? default : search_term.Trim().ToLower();
            var query =
                    from ids in (
                        (from tag in __DBContext.SocialTags
                            .Where(e => e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled)
                                    && (search_term == default
                                    || e.Tag.ToLower().Contains(search_term)
                                )
                            )
                        join action in __DBContext.SocialUserActionWithTags on tag.Id equals action.TagId
                        into tagWithAction
                        from t in tagWithAction.DefaultIfEmpty()
                        group t by new { tag.Id, tag.Tag } into gr
                        select new {
                            gr.Key,
                            Match = (search_term != default) ? (gr.Key.Tag.ToLower() == search_term ? 1 : 0) : 0,
                            StartWith = (search_term != default) ? (gr.Key.Tag.StartsWith(search_term) ? 1 : 0) : 0,
                            Follow = gr.Count(e => (socialUserId != default)
                                && (e.UserId == socialUserId)
                                && EF.Functions.JsonContains(e.ActionsStr, EntityAction.GenContainsJsonStatement(ActionType.Follow))),
                            Used = gr.Count(e => (socialUserId != default)
                                && (e.UserId == socialUserId)
                                && EF.Functions.JsonContains(e.ActionsStr, EntityAction.GenContainsJsonStatement(ActionType.Used))),
                        } into ret
                        orderby ret.Used descending, ret.Follow descending, ret.StartWith descending, ret.Match descending
                        select ret.Key.Id).Skip(start).Take(size)
                    )
                    join tags in __DBContext.SocialTags on ids equals tags.Id
                    select tags;

            var totalCount = await __DBContext.SocialTags
                            .CountAsync(e => e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled)
                                    && (search_term == default
                                    || e.Tag.ToLower().Contains(search_term)
                                )
                            );

            return (await query.ToListAsync(), totalCount);
        }
        public async Task<(SocialTag, ErrorCodes)> FindTagByName(string Tag, Guid SocialUserId = default)
        {
            var tag = await __DBContext.SocialTags
                    .Where(e => e.Tag == Tag
                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                    .FirstOrDefaultAsync();
            if (tag == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }

            if (SocialUserId != default) {
                // add action visted to social user_action_with_tag
                #region Add action used tag
                var action = await __DBContext.SocialUserActionWithTags
                    .Where(e => e.TagId == tag.Id && e.UserId == SocialUserId)
                    .FirstOrDefaultAsync();
                var actionVisited = EntityAction.ActionTypeToString(ActionType.Used);
                if (action != default) {
                    if (!(action.Actions.Count(a => a.action == actionVisited) > 0)) {
                        action.Actions.Add(new EntityAction(EntityActionType.UserActionWithTag, actionVisited));
                        await __DBContext.SaveChangesAsync();
                    }
                } else {
                    await __DBContext.SocialUserActionWithTags
                        .AddAsync(new SocialUserActionWithTag(){
                            UserId = SocialUserId,
                            TagId = tag.Id,
                            Actions = new List<EntityAction>(){
                                new EntityAction(EntityActionType.UserActionWithTag, actionVisited)
                            }
                        });
                    await __DBContext.SaveChangesAsync();
                }
                #endregion
            }
            return (tag, ErrorCodes.NO_ERROR);
        }
        public async Task<(SocialTag, ErrorCodes)> FindTagByNameIgnoreStatus(string Tag)
        {
            var tag = await __DBContext.SocialTags
                    .Where(e => e.Tag == Tag)
                    .FirstOrDefaultAsync();
            if (tag == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }
            return (tag, ErrorCodes.NO_ERROR);
        }

        public async Task<(SocialTag, ErrorCodes)> FindTagById(long Id)
        {
            var tag = await __DBContext.SocialTags
                    .Where(e => e.Id == Id)
                    .FirstOrDefaultAsync();

            if (tag != default) {
                return (tag, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.NOT_FOUND);
        }

        #region Tag action
        public async Task<bool> IsContainsAction(long tagId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithTags
                .Where(e => e.TagId == tagId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            return action != default ? action.Actions.Count(a => a.action == actionStr) > 0 : false;
        }
        protected async Task<ErrorCodes> AddAction(long tagId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithTags
                .Where(e => e.TagId == tagId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            if (action != default) {
                if (!(action.Actions.Count(a => a.action == actionStr) > 0)) {
                    action.Actions.Add(new EntityAction(EntityActionType.UserActionWithTag, actionStr));
                    if (await __DBContext.SaveChangesAsync() > 0) {
                        return ErrorCodes.NO_ERROR;
                    }
                }
                return ErrorCodes.NO_ERROR;
            } else {
                await __DBContext.SocialUserActionWithTags
                    .AddAsync(new SocialUserActionWithTag(){
                        UserId = socialUserId,
                        TagId = tagId,
                        Actions = new List<EntityAction>(){
                            new EntityAction(EntityActionType.UserActionWithTag, actionStr)
                        }
                    });
                if (await __DBContext.SaveChangesAsync() > 0) {
                    return ErrorCodes.NO_ERROR;
                }
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        protected async Task<ErrorCodes> RemoveAction(long tagId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithTags
                .Where(e => e.TagId == tagId && e.UserId == socialUserId)
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
        public async Task<ErrorCodes> UnFollow(long tagId, Guid socialUserId)
        {
            return await RemoveAction(tagId, socialUserId, EntityAction.ActionTypeToString(ActionType.Follow));
        }
        public async Task<ErrorCodes> Follow(long tagId, Guid socialUserId)
        {
            return await AddAction(tagId, socialUserId, EntityAction.ActionTypeToString(ActionType.Follow));
        }
        public async Task<ErrorCodes> Used(long tagId, Guid socialUserId)
        {
            return await AddAction(tagId, socialUserId, EntityAction.ActionTypeToString(ActionType.Used));
        }
        public async Task<ErrorCodes> RemoveUsed(long tagId, Guid socialUserId)
        {
            return await RemoveAction(tagId, socialUserId, EntityAction.ActionTypeToString(ActionType.Used));
        }
        #endregion

        #region Tag handle
        public async Task<ErrorCodes> AddNewTag(SocialTag Tag, Guid AdminUserId)
        {
            #region Find user
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __AdminUserManagement = scope.ServiceProvider.GetRequiredService<AdminUserManagement>();
                var (user, error) = await __AdminUserManagement.FindUserById(AdminUserId);
                if (error != ErrorCodes.NO_ERROR || (user.Status.Type != StatusType.Activated && user.Status.Type != StatusType.Readonly)) {
                    return error == ErrorCodes.NOT_FOUND ? error :
                        (user.Status.Type == StatusType.Deleted ? ErrorCodes.DELETED : ErrorCodes.USER_DOES_NOT_HAVE_PERMISSION);
                }
            }
            #endregion

            await __DBContext.SocialTags.AddAsync(Tag);
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [ADMIN] Write admin audit log
                var (newTag, error) = await FindTagById(Tag.Id);
                if (error == ErrorCodes.NO_ERROR) {
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialAuditLogManagement>();
                        await __SocialAuditLogManagement.AddNewAuditLog(
                            newTag.GetModelName(),
                            newTag.Id.ToString(),
                            LOG_ACTIONS.CREATE,
                            AdminUserId,
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
        public async Task<ErrorCodes> ModifyTag(long TagId, SocialTagModifyModel ModelData, Guid AdminUserId)
        {
            #region Find tag info
            var (Tag, Error) = await FindTagById(TagId);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

            var OldData = Utils.DeepClone(Tag.GetJsonObjectForLog());
            #region Get data change and save
            var haveChange = false;
            if (ModelData.name != default && ModelData.name != Tag.Name) {
                Tag.Name = ModelData.name;
                haveChange = true;
            }
            if (ModelData.describe != default && ModelData.describe != Tag.Describe) {
                Tag.Describe = ModelData.describe;
                haveChange = true;
            }
            if (ModelData.status != default && ModelData.status != Tag.StatusStr) {
                Tag.StatusStr = ModelData.status;
                haveChange = true;
            }
            #endregion

            if (!haveChange) {
                return ErrorCodes.NO_CHANGE_DETECTED;
            }

            Tag.LastModifiedTimestamp = DateTime.UtcNow;
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [ADMIN] Write admin audit log
                var __SocialAuditLogManagement = __ServiceProvider.GetService<SocialAuditLogManagement>();
                var (OldVal, NewVal) = Utils.GetDataChanges(OldData, Tag.GetJsonObjectForLog());
                await __SocialAuditLogManagement.AddNewAuditLog(
                    Tag.GetModelName(),
                    Tag.Id.ToString(),
                    LOG_ACTIONS.MODIFY,
                    AdminUserId,
                    OldVal,
                    NewVal
                );
                #endregion
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        #endregion
        #region Validation
        public bool IsValidTag(string tag)
        {
            return tag != string.Empty && tag.Count() <= 25;
        }
        public async Task<(bool, ErrorCodes)> IsValidTags(string[] tags, bool isEnableTag = false)
        {
            if (tags.Distinct().Count() != tags.Count()) {
                return (false, ErrorCodes.INVALID_PARAMS);
            }
            foreach (var tag in tags) {
                if (!IsValidTag(tag)) {
                    return (false, ErrorCodes.INVALID_PARAMS);
                } else {
                    if (await __DBContext.SocialTags.CountAsync(e => e.Tag == tag) < 1) {
                        await __DBContext.SocialTags.AddAsync(new SocialTag(){
                            Tag = tag,
                            Name = tag,
                            Describe = tag,
                            StatusStr = isEnableTag
                                ? EntityStatus.StatusTypeToString(StatusType.Enabled)
                                : EntityStatus.StatusTypeToString(StatusType.Disabled),
                        });
                        if (await __DBContext.SaveChangesAsync() <= 0) {
                            return (false, ErrorCodes.INTERNAL_SERVER_ERROR);
                        }
                    }
                }
            }

            return (true, ErrorCodes.NO_ERROR);
        }
        public async Task<bool> IsExistsTags(string[] tags)
        {
            var count = await __DBContext.SocialTags
                .CountAsync(e => tags.Contains(e.Tag));
            if (tags.Length != count) {
                return false;
            }

            return true;
        }
        #endregion
    }
}