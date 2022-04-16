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
using DatabaseAccess.Common.Actions;
using Common;
using DatabaseAccess.Context.ParserModels;

namespace CoreApi.Services
{
    public class SocialTagManagement : BaseTransientService
    {
        public SocialTagManagement(DBContext _DBContext,
                                    IServiceProvider _IServiceProvider)
            : base(_IServiceProvider)
        {
            __ServiceName = "SocialTagManagement";
        }

        public async Task<(List<SocialTag>, int)> GetTags(int start = 0,
                                                          int size = 20,
                                                          string search_term = default,
                                                          Guid socialUserId = default)
        {
            var query =
                    from ids in (
                        (from tag in __DBContext.SocialTags
                            .Where(e => e.StatusStr != BaseStatus.StatusToString(SocialTagStatus.Disabled, EntityStatus.SocialTagStatus)
                                    && (search_term == default || (search_term != default && e.Tag.Contains(search_term))))
                        join action in __DBContext.SocialUserActionWithTags on tag.Id equals action.TagId
                        into tagWithAction
                        from t in tagWithAction.DefaultIfEmpty()
                        group t by new { tag.Id, tag.Tag } into gr
                        select new {
                            gr.Key,
                            StartWith = (search_term != default) ? (gr.Key.Tag.StartsWith(search_term) ? 1 : 0) : 0,
                            Follow = gr.Count(e => (socialUserId != default)
                                && (e.UserId == socialUserId)
                                && EF.Functions.JsonExists(e.ActionsStr, BaseAction.ActionToString(UserActionWithTag.Follow, EntityAction.UserActionWithTag))),
                            Used = gr.Count(e => (socialUserId != default)
                                && (e.UserId == socialUserId)
                                && EF.Functions.JsonExists(e.ActionsStr, BaseAction.ActionToString(UserActionWithTag.Used, EntityAction.UserActionWithTag))),
                            Visited = gr.Count(e => (socialUserId != default)
                                && (e.UserId == socialUserId)
                                && EF.Functions.JsonExists(e.ActionsStr, BaseAction.ActionToString(UserActionWithTag.Visited, EntityAction.UserActionWithTag))),
                        } into ret
                        orderby ret.Visited descending, ret.Used descending, ret.Follow descending, ret.StartWith descending
                        select ret.Key.Id).Skip(start).Take(size)
                    )
                    join tags in __DBContext.SocialTags on ids equals tags.Id
                    select tags;

            var totalCount = await __DBContext.SocialTags
                            .CountAsync(e => e.StatusStr != BaseStatus.StatusToString(SocialTagStatus.Disabled, EntityStatus.SocialTagStatus)
                                    && (search_term == default || (search_term != default && e.Tag.Contains(search_term))));

            return (await query.ToListAsync(), totalCount);
        }
        public async Task<(SocialTag, ErrorCodes)> FindTagByName(string Tag, Guid SocialUserId = default)
        {
            var tag = await __DBContext.SocialTags
                    .Where(e => e.Tag == Tag
                            && e.StatusStr != BaseStatus.StatusToString(SocialTagStatus.Disabled, EntityStatus.SocialTagStatus))
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
                var actionVisited = BaseAction.ActionToString(UserActionWithTag.Visited, EntityAction.UserActionWithTag);
                if (action != default) {
                    if (!action.Actions.Contains(actionVisited)) {
                        action.Actions.Add(actionVisited);
                        await __DBContext.SaveChangesAsync();
                    }
                } else {
                    await __DBContext.SocialUserActionWithTags
                        .AddAsync(new SocialUserActionWithTag(){
                            UserId = SocialUserId,
                            TagId = tag.Id,
                            Actions = new List<string>(){
                                actionVisited
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
        protected async Task<ErrorCodes> AddAction(long tagId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithTags
                .Where(e => e.TagId == tagId && e.UserId == socialUserId)
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
                await __DBContext.SocialUserActionWithTags
                    .AddAsync(new SocialUserActionWithTag(){
                        UserId = socialUserId,
                        TagId = tagId,
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
        protected async Task<ErrorCodes> RemoveAction(long tagId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithTags
                .Where(e => e.TagId == tagId && e.UserId == socialUserId)
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
        public async Task<ErrorCodes> UnFollow(long tagId, Guid socialUserId)
        {
            return await RemoveAction(tagId, socialUserId, BaseAction.ActionToString(UserActionWithTag.Follow, EntityAction.UserActionWithTag));
        }
        public async Task<ErrorCodes> Follow(long tagId, Guid socialUserId)
        {
            return await AddAction(tagId, socialUserId, BaseAction.ActionToString(UserActionWithTag.Follow, EntityAction.UserActionWithTag));
        }
        public async Task<ErrorCodes> Used(long tagId, Guid socialUserId)
        {
            return await AddAction(tagId, socialUserId, BaseAction.ActionToString(UserActionWithTag.Used, EntityAction.UserActionWithTag));
        }
        public async Task<ErrorCodes> RemoveUsed(long tagId, Guid socialUserId)
        {
            return await RemoveAction(tagId, socialUserId, BaseAction.ActionToString(UserActionWithTag.Used, EntityAction.UserActionWithTag));
        }
        public async Task<ErrorCodes> Visited(long tagId, Guid socialUserId)
        {
            return await AddAction(tagId, socialUserId, BaseAction.ActionToString(UserActionWithTag.Visited, EntityAction.UserActionWithTag));
        }
        #endregion

        #region Tag handle
        public async Task<ErrorCodes> AddNewTag(SocialTag Tag)
        {
            await __DBContext.SocialTags.AddAsync(Tag);
            if (await __DBContext.SaveChangesAsync() > 0) {
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
        public async Task<(bool, ErrorCodes)> IsValidTags(string[] tags)
        {
            foreach(var tag in tags) {
                if (!IsValidTag(tag)) {
                    return (false, ErrorCodes.INVALID_PARAMS);
                } else {
                    if (await __DBContext.SocialTags.CountAsync(e => e.Tag == tag) < 1) {
                        await __DBContext.SocialTags.AddAsync(new SocialTag(){
                            Tag = tag,
                            Name = tag,
                            Describe = tag,
                            Status = SocialTagStatus.Disabled,
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