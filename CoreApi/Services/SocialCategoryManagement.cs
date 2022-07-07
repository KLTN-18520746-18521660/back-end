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
    public enum GetCategoryAction {
        GetCategoriesByAction         = 1,
    }
    public class SocialCategoryManagement : BaseTransientService
    {
        public SocialCategoryManagement(IServiceProvider _IServiceProvider) : base(_IServiceProvider)
        {
            __ServiceName = "SocialCategoryManagement";
        }
        public string[] GetAllowOrderFields(GetCategoryAction action)
        {
            switch (action) {
                case GetCategoryAction.GetCategoriesByAction:
                    return new string[] {
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
        public async Task<(List<SocialCategory>, int, ErrorCodes)> GetCategoriesByAction(Guid socialUserId,
                                                                                    string action,
                                                                                    int start = 0,
                                                                                    int size = 20,
                                                                                    string search_term = default,
                                                                                    (string, bool)[] orders = default)
        {
            action = char.ToUpper(action[0]) + action.Substring(1).ToLower();
            #region validate params
            if (!EntityAction.ValidateAction(action, EntityActionType.UserActionWithCategory)) {
                return (default, default, ErrorCodes.INVALID_PARAMS);
            }
            var ColumnAllowOrder = GetAllowOrderFields(GetCategoryAction.GetCategoriesByAction);
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
                "SELECT     C.id, C.name, "
                            + "COUNT(DISTINCT CA.user_id) FILTER (WHERE CA.actions @> '[{\"action\": \"Follow\"}]') AS follows, "
                            + "COUNT(DISTINCT P.id) FILTER (WHERE P.status = 'Approved') AS posts, "
                            + "C.created_timestamp, C.last_modified_timestamp, CA.time_action "
                + "FROM "
                    + "social_category AS C JOIN "
                    + "("
                        + "SELECT   category_id, user_id, actions, "
                                    + "(jsonb_path_query_first("
                                        + "actions, "
                                        + "'$[*] ? (@.action == $action)', "
                                        + $"'{{\"action\": \"{ action }\"}}'"
                                    + ")::jsonb ->> 'date')::timestamptz AS time_action "
                        + "FROM     social_user_action_with_category "
                        + $"WHERE    actions @> '[{{\"action\":\"{ action }\"}}]' AND user_id = '{ socialUserId.ToString() }' "
                    + ") AS CA ON C.id = CA.category_id "
                    + "LEFT JOIN social_post_category AS PC ON PC.category_id = C.id "
                    + "JOIN social_post AS P ON P.id = PC.post_id "
                + $"WHERE       C.status != 'Disabled' AND LOWER(C.name) LIKE LOWER('%{ search_term }%') "
                                + $"AND LOWER(C.display_name) LIKE LOWER('%{ search_term }%') "
                                + $"AND LOWER(C.describe) LIKE LOWER('%{ search_term }%') "
                + "GROUP BY     C.id, C.name, "
                                + "C.created_timestamp, C.last_modified_timestamp, CA.time_action "
                + $"ORDER BY { orderStr }";
            var categoryIdsOrdered = await DBHelper.RawSqlQuery<long>(
                rawQuery,
                x => (long)x[0]
            );

            var queryCategories = from ids in categoryIdsOrdered
                        join category in __DBContext.SocialCategories on ids equals category.Id
                        select category;

            return (queryCategories.ToList(), categoryIdsOrdered.Count(), ErrorCodes.NO_ERROR);
        }
        
        public async Task<(List<SocialCategory>, int, ErrorCodes)> GetTrendingCategories(int time = 7, // days
                                                                                         int start = 0,
                                                                                         int size = 20,
                                                                                         string search_term = default)
        {
            // orderStr can't empty or null
            var orderStr = $"follows desc, posts desc, created_timestamp desc";
            var compareDate = DateTime.UtcNow.AddDays(-time);

            var query =
                    from ids in (
                        (from category in __DBContext.SocialCategories
                                .Where(e => e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled)
                                        && (search_term == default
                                        || e.Name.ToLower().Contains(search_term)
                                        || e.Describe.ToLower().Contains(search_term)
                                        || e.DisplayName.ToLower().Contains(search_term)
                                    )
                                    && (time == -1 || e.CreatedTimestamp >= compareDate)
                                )
                        join action in __DBContext.SocialUserActionWithCategories on category.Id equals action.CategoryId
                        into categoryWithAction
                        from c in categoryWithAction.DefaultIfEmpty()
                        group c by new {
                            category.Id,
                            category.CreatedTimestamp
                        } into gr
                        select new {
                            gr.Key,
                            Follow = gr.Count(e => EF.Functions.JsonContains(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Follow))),
                            Posts = __DBContext.SocialPosts.Count(e =>
                                e.SocialPostCategories.Count(pt => pt.CategoryId == gr.Key.Id) > 0
                                && e.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved)
                            ),
                        } into ret select new {
                            ret.Key.Id,
                            posts = ret.Posts,
                            follows = ret.Follow,
                            created_timestamp = ret.Key.CreatedTimestamp,
                        })
                        .OrderBy(orderStr)
                        .Skip(start).Take(size)
                        .Select(e => e.Id)
                    )
                    join categories in __DBContext.SocialCategories on ids equals categories.Id
                    select categories;

            var totalCount = await __DBContext.SocialCategories
                                .CountAsync(e => e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled)
                                        && (search_term == default
                                        || e.Name.ToLower().Contains(search_term)
                                        || e.Describe.ToLower().Contains(search_term)
                                        || e.DisplayName.ToLower().Contains(search_term)
                                    )
                                    && (time == -1 || e.CreatedTimestamp >= compareDate)
                                );
            return (await query.ToListAsync(), totalCount, ErrorCodes.NO_ERROR);
        }
        public async Task<(List<SocialCategory>, int)> SearchCategories(int start = 0,
                                                                        int size = 20,
                                                                        string search_term = default,
                                                                        Guid socialUserId = default,
                                                                        bool isAdmin = false)
        {
            // search_term = search_term == default ? default : search_term.Trim().ToLower();
            if (search_term != default) {
                search_term = Utils.PrepareSearchTerm(search_term);
            }

            var query =
                    from ids in (
                        (from category in __DBContext.SocialCategories
                            .Where(e => (isAdmin || e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                                    && (search_term == default
                                    || e.Name.ToLower().Contains(search_term)
                                    || e.DisplayName.ToLower().Contains(search_term)
                                    || e.Describe.ToLower().Contains(search_term)
                                )
                            )
                        join action in __DBContext.SocialUserActionWithCategories on category.Id equals action.CategoryId
                        into categoryWithAction
                        from c in categoryWithAction.DefaultIfEmpty()
                        group c by new { category.Id, category.Name } into gr
                        select new {
                            gr.Key,
                            Match = (search_term != default) ? (gr.Key.Name.ToLower() == search_term ? 1 : 0) : 0,
                            StartWith = (search_term != default) ? (gr.Key.Name.StartsWith(search_term) ? 1 : 0) : 0,
                            Follow = gr.Count(e => (socialUserId != default)
                                && (e.UserId == socialUserId)
                                && EF.Functions.JsonContains(e.ActionsStr, EntityAction.GenContainsJsonStatement(ActionType.Follow))),
                            Visited = gr.Count(e => (socialUserId != default)
                                && (e.UserId == socialUserId)
                                && EF.Functions.JsonContains(e.ActionsStr, EntityAction.GenContainsJsonStatement(ActionType.Visited))),
                        } into ret
                        orderby ret.Visited descending, ret.Follow descending, ret.StartWith descending, ret.Match descending
                        select ret.Key.Id).Skip(start).Take(size)
                    )
                    join categories in __DBContext.SocialCategories on ids equals categories.Id
                    select categories;

            var totalCount = await __DBContext.SocialCategories
                            .CountAsync(e => (isAdmin || e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                                    && (search_term == default
                                    || e.Name.ToLower().Contains(search_term)
                                    || e.DisplayName.ToLower().Contains(search_term)
                                    || e.Describe.ToLower().Contains(search_term)
                                )
                            );

            return (await query.ToListAsync(), totalCount);
        }

        public async Task<(List<SocialCategory>, ErrorCodes)> GetCategories()
        {
            return (
                await __DBContext.SocialCategories
                    .Where(e => e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                    .ToListAsync(),
                ErrorCodes.NO_ERROR
            );
        }
        public async Task<(SocialCategory, ErrorCodes)> FindCategoryBySlug(string CategorySlug, Guid SocialUserId = default)
        {
            var category = await __DBContext.SocialCategories
                    .Where(e => e.Slug == CategorySlug
                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                    .FirstOrDefaultAsync();
            if (category == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }

            if (SocialUserId != default) {
                // add action visted to social user_action_with_category
            }
            return (category, ErrorCodes.NO_ERROR);
        }
        public async Task<(SocialCategory, ErrorCodes)> FindCategoryByName(string CategoryName, Guid SocialUserId = default)
        {
            var category = await __DBContext.SocialCategories
                    .Where(e => e.Name == CategoryName
                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled))
                    .FirstOrDefaultAsync();
            if (category == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }

            if (SocialUserId != default) {
                await Visited(category.Id, SocialUserId);
            }
            return (category, ErrorCodes.NO_ERROR);
        }
        public async Task<(SocialCategory, ErrorCodes)> FindCategoryByNameIgnoreStatus(string CategoryName, Guid SocialUserId = default)
        {
            var category = await __DBContext.SocialCategories
                    .Where(e => e.Name == CategoryName)
                    .FirstOrDefaultAsync();
            if (category == default) {
                return (default, ErrorCodes.NOT_FOUND);
            }

            if (SocialUserId != default) {
                await Visited(category.Id, SocialUserId);
            }
            return (category, ErrorCodes.NO_ERROR);
        }

        public async Task<(SocialCategory, ErrorCodes)> FindCategoryById(long Id)
        {
            var category = await __DBContext.SocialCategories
                    .Where(e => e.Id == Id)
                    .FirstOrDefaultAsync();

            if (category != default) {
                return (category, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.NOT_FOUND);
        }
        public async Task<(bool, ErrorCodes)> IsCategoryExisting(string name, string slug)
        {
            var count = (await __DBContext.SocialCategories
                    .CountAsync(e => (e.Slug == slug || e.Name == name)
                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Disabled)));
            return (count > 0, ErrorCodes.NO_ERROR);
        }

        #region Category action
        public async Task<bool> IsContainsAction(long categoryId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithCategories
                .Where(e => e.CategoryId == categoryId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            return action != default ? action.Actions.Count(a => a.action == actionStr) > 0 : false;
        }
        protected async Task<ErrorCodes> AddAction(long categoryId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithCategories
                .Where(e => e.CategoryId == categoryId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            if (action != default) {
                if (!(action.Actions.Count(a => a.action == actionStr) > 0)) {
                    action.Actions.Add(new EntityAction(EntityActionType.UserActionWithCategory, actionStr));
                    if (await __DBContext.SaveChangesAsync() > 0) {
                        return ErrorCodes.NO_ERROR;
                    }
                }
                return ErrorCodes.NO_ERROR;
            } else {
                await __DBContext.SocialUserActionWithCategories
                    .AddAsync(new SocialUserActionWithCategory(){
                        UserId = socialUserId,
                        CategoryId = categoryId,
                        Actions = new List<EntityAction>(){
                            new EntityAction(EntityActionType.UserActionWithCategory, actionStr)
                        }
                    });
                if (await __DBContext.SaveChangesAsync() > 0) {
                    return ErrorCodes.NO_ERROR;
                }
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        protected async Task<ErrorCodes> RemoveAction(long categoryId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithCategories
                .Where(e => e.CategoryId == categoryId && e.UserId == socialUserId)
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
        public async Task<ErrorCodes> UnFollow(long categoryId, Guid socialUserId)
        {
            return await RemoveAction(categoryId, socialUserId, EntityAction.ActionTypeToString(ActionType.Follow));
        }
        public async Task<ErrorCodes> Follow(long categoryId, Guid socialUserId)
        {
            return await AddAction(categoryId, socialUserId,EntityAction.ActionTypeToString(ActionType.Follow));
        }
        public async Task<ErrorCodes> Visited(long categoryId, Guid socialUserId)
        {
            return await AddAction(categoryId, socialUserId,EntityAction.ActionTypeToString(ActionType.Visited));
        }
        #endregion

        #region Category handle
        public async Task<ErrorCodes> AddNewCategory(SocialCategory Category, Guid AdminUserId)
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

            await __DBContext.SocialCategories.AddAsync(Category);
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [ADMIN] Write admin audit log
                var (newCategory, error) = await FindCategoryById(Category.Id);
                if (error == ErrorCodes.NO_ERROR) {
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialAuditLogManagement>();
                        await __SocialAuditLogManagement.AddNewAuditLog(
                            newCategory.GetModelName(),
                            newCategory.Id.ToString(),
                            LOG_ACTIONS.CREATE,
                            AdminUserId,
                            new JObject(),
                            newCategory.GetJsonObject()
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
        public async Task<ErrorCodes> ModifyCategory(long CategoryId, SocialCategoryModifyModel ModelData, Guid AdminUserId)
        {
            #region Find tag info
            var (Category, Error) = await FindCategoryById(CategoryId);
            if (Error != ErrorCodes.NO_ERROR) {
                return Error;
            }
            #endregion

            var OldData = Utils.DeepClone(Category.GetJsonObjectForLog());
            #region Get data change and save
            var haveChange = false;
            if (ModelData.parent_id != default && ModelData.parent_id != Category.ParentId) {
                var (ParentCategory, ErrorGetParent) = await FindCategoryById((long) ModelData.parent_id);
                if (ErrorGetParent != ErrorCodes.NO_ERROR) {
                    return ErrorCodes.NOT_FOUND;
                }
                Category.ParentId = ParentCategory.Id;
                Category.Parent = ParentCategory;
                haveChange = true;
            }
            if (ModelData.display_name != default && ModelData.display_name != Category.DisplayName) {
                Category.DisplayName = ModelData.display_name;
                haveChange = true;
            }
            if (ModelData.describe != default && ModelData.describe != Category.Describe) {
                Category.Describe = ModelData.describe;
                haveChange = true;
            }
            if (ModelData.thumbnail != default && ModelData.thumbnail != Category.Thumbnail) {
                Category.Thumbnail = ModelData.thumbnail;
                haveChange = true;
            }
            if (ModelData.status != default && ModelData.status != Category.StatusStr) {
                Category.StatusStr = ModelData.status;
                haveChange = true;
            }
            #endregion

            if (!haveChange) {
                return ErrorCodes.NO_CHANGE_DETECTED;
            }

            Category.LastModifiedTimestamp = DateTime.UtcNow;
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region [ADMIN] Write admin audit log
                var __SocialAuditLogManagement = __ServiceProvider.GetService<SocialAuditLogManagement>();
                var (OldVal, NewVal) = Utils.GetDataChanges(OldData, Category.GetJsonObjectForLog());
                await __SocialAuditLogManagement.AddNewAuditLog(
                    Category.GetModelName(),
                    Category.Id.ToString(),
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
        #region Validate
        public bool IsValidCategory(string category) {
            return category != string.Empty && category.Count() <= 20;
        }
        public async Task<bool> IsExistingCategories(string[] categories)
        {
            var count = await __DBContext.SocialCategories
                .CountAsync(e => categories.Contains(e.Name));

            if (count != categories.Count()) {
                return false;
            }
            return true;
        }
        #endregion
    }
}