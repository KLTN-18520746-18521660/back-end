using CoreApi.Common;
using CoreApi.Common.Base;
using DatabaseAccess.Common.Actions;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi.Services
{
    public class SocialCategoryManagement : BaseTransientService
    {
        public SocialCategoryManagement(IServiceProvider _IServiceProvider) : base(_IServiceProvider)
        {
            __ServiceName = "SocialCategoryManagement";
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