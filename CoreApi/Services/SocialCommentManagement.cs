using Common;
using CoreApi.Common;
using CoreApi.Models.ModifyModels;
using DatabaseAccess.Common.Actions;
using DatabaseAccess.Common.Models;
using DatabaseAccess.Common.Status;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CoreApi.Services
{
    public enum GetCommentAction {
        GetCommentsAttachedToPost = 0,
    }
    public class SocialCommentManagement : BaseTransientService
    {
        public SocialCommentManagement(DBContext _DBContext,
                                       IServiceProvider _IServiceProvider)
            : base(_IServiceProvider)
        {
            __ServiceName = "SocialCommentManagement";
        }

        public string[] GetAllowOrderFields(GetCommentAction action)
        {
            switch (action) {
                case GetCommentAction.GetCommentsAttachedToPost:
                    return new string[] {
                        "likes",
                        "dislikes",
                        "replies",
                        "reports",
                        "created_timestamp",
                        "last_modified_timestamp",
                    };
                default:
                    return default;
            }
        }

        public async Task<(List<SocialComment>, int, ErrorCodes)> GetCommentsAttachedToPost(long postId,
                                                                                            long? parrent_comment_id = default,
                                                                                            int start = 0,
                                                                                            int size = 20,
                                                                                            string search_term = default,
                                                                                            string[] status = default,
                                                                                            (string, bool)[] orders = default)
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

            var ColumnAllowOrder = GetAllowOrderFields(GetCommentAction.GetCommentsAttachedToPost);
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
                        (from comment in __DBContext.SocialComments
                                .Where(e => e.PostId == postId
                                    && (search_term == default || e.SearchVector.Matches(search_term))
                                    && (e.ParentId == parrent_comment_id)
                                    && ((status.Count() == 0
                                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted))
                                        || status.Contains(e.StatusStr)
                                    )
                                )
                        join action in __DBContext.SocialUserActionWithComments on comment.Id equals action.CommentId
                        into commentWithAction
                        from c in commentWithAction.DefaultIfEmpty()
                        group c by new {
                            comment.Id,
                            comment.CreatedTimestamp,
                            comment.LastModifiedTimestamp
                        } into gr
                        select new {
                            gr.Key,
                            Likes = gr.Count(e => EF.Functions.JsonExists(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Like))),
                            DisLikes = gr.Count(e => EF.Functions.JsonExists(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Dislike))),
                            Replies = gr.Count(e => EF.Functions.JsonExists(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Reply))),
                            Reports = gr.Count(e => EF.Functions.JsonExists(e.ActionsStr,
                                EntityAction.GenContainsJsonStatement(ActionType.Report))),
                        } into ret select new {
                            ret.Key.Id,
                            likes = ret.Likes,
                            dislikes = ret.DisLikes,
                            replies = ret.Replies,
                            reports = ret.Reports,
                            created_timestamp = ret.Key.CreatedTimestamp,
                            last_modified_timestamp = ret.Key.LastModifiedTimestamp,
                        })
                        .OrderBy(orderStr)
                        .Skip(start).Take(size)
                        .Select(e => e.Id)
                    )
                    join comments in __DBContext.SocialComments on ids equals comments.Id
                    select comments;
            var totalCount = await __DBContext.SocialComments
                                .CountAsync(e => e.PostId == postId
                                    && (search_term == default || e.SearchVector.Matches(search_term))
                                    && (e.ParentId == parrent_comment_id)
                                    && ((status.Count() == 0
                                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted))
                                        || status.Contains(e.StatusStr)
                                    )
                                );
            return (await query.ToListAsync(), totalCount, ErrorCodes.NO_ERROR);
        }

        public async Task<(SocialComment, ErrorCodes)> FindCommentById(long Id)
        {
            var comment = await __DBContext.SocialComments
                    .Where(e => e.Id == Id)
                    .FirstOrDefaultAsync();

            if (comment != default) {
                return (comment, ErrorCodes.NO_ERROR);
            }
            return (default, ErrorCodes.NOT_FOUND);
        }

        #region Comment action
        public async Task<bool> IsContainsAction(long commentId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithComments
                .Where(e => e.CommentId == commentId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            return action != default ? action.Actions.Count(a => a.action == actionStr) > 0 : false;
        }
        protected async Task<ErrorCodes> AddAction(long commentId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithComments
                .Where(e => e.CommentId == commentId && e.UserId == socialUserId)
                .FirstOrDefaultAsync();
            if (action != default) {
                if (!(action.Actions.Count(a => a.action == actionStr) > 0)) {
                    action.Actions.Add(new EntityAction(EntityActionType.UserActionWithComment, actionStr));
                    if (await __DBContext.SaveChangesAsync() > 0) {
                        return ErrorCodes.NO_ERROR;
                    }
                }
                return ErrorCodes.NO_ERROR;
            } else {
                await __DBContext.SocialUserActionWithComments
                    .AddAsync(new SocialUserActionWithComment(){
                        UserId = socialUserId,
                        CommentId = commentId,
                        Actions = new List<EntityAction>(){
                            new EntityAction(EntityActionType.UserActionWithComment, actionStr)
                        }
                    });
                if (await __DBContext.SaveChangesAsync() > 0) {
                    return ErrorCodes.NO_ERROR;
                }
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        protected async Task<ErrorCodes> RemoveAction(long commentId, Guid socialUserId, string actionStr)
        {
            var action = await __DBContext.SocialUserActionWithComments
                .Where(e => e.CommentId == commentId && e.UserId == socialUserId)
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
        public async Task<ErrorCodes> UnLike(long commentId, Guid socialUserId)
        {
            return await RemoveAction(commentId, socialUserId, EntityAction.ActionTypeToString(ActionType.Like));
        }
        public async Task<ErrorCodes> Like(long commentId, Guid socialUserId)
        {
            await RemoveAction(commentId, socialUserId, EntityAction.ActionTypeToString(ActionType.Dislike));
            return await AddAction(commentId, socialUserId, EntityAction.ActionTypeToString(ActionType.Like));
        }
        public async Task<ErrorCodes> UnDisLike(long commentId, Guid socialUserId)
        {
            return await RemoveAction(commentId, socialUserId, EntityAction.ActionTypeToString(ActionType.Dislike));
        }
        public async Task<ErrorCodes> DisLike(long commentId, Guid socialUserId)
        {
            await RemoveAction(commentId, socialUserId, EntityAction.ActionTypeToString(ActionType.Like));
            return await AddAction(commentId, socialUserId, EntityAction.ActionTypeToString(ActionType.Dislike));
        }
        public async Task<ErrorCodes> Reply(long commentId, Guid socialUserId)
        {
            return await AddAction(commentId, socialUserId, EntityAction.ActionTypeToString(ActionType.Reply));
        }
        public async Task<ErrorCodes> RemoveReply(long postId, Guid socialUserId)
        {
            return await RemoveAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Reply));
        }
        public async Task<ErrorCodes> Report(long postId, Guid socialUserId)
        {
            return await AddAction(postId, socialUserId, EntityAction.ActionTypeToString(ActionType.Report));
        }
        #endregion

        #region Comment handle
        public async Task<ErrorCodes> AddComment(SocialComment Comment)
        {
            await __DBContext.SocialComments.AddAsync(Comment);
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region Add action comment post
                using (var scope = __ServiceProvider.CreateScope())
                {
                    await scope.ServiceProvider.GetRequiredService<SocialPostManagement>()
                            .Comment(Comment.PostId, Comment.Owner);
                    if (Comment.ParentId != default) {
                        await scope.ServiceProvider.GetRequiredService<SocialCommentManagement>()
                            .Reply(Comment.Id, Comment.Owner);
                    }

                    #region Write log
                    var __SocialUserAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialUserAuditLogManagement>();
                    await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                        Comment.GetModelName(),
                        Comment.Id.ToString(),
                        LOG_ACTIONS.CREATE,
                        Comment.Owner,
                        new JObject(),
                        Comment.GetJsonObjectForLog()
                    );
                    #endregion
                }
                #endregion
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        public async Task<ErrorCodes> DeleteComment(SocialComment Comment)
        {
            Comment.Status.ChangeStatus(StatusType.Deleted);
            if (await __DBContext.SaveChangesAsync() > 0) {
                #region Remove action comment post
                using (var scope = __ServiceProvider.CreateScope())
                {
                    await scope.ServiceProvider.GetRequiredService<SocialPostManagement>()
                            .UnComment(Comment.PostId, Comment.Owner);

                    #region Write log
                    var __SocialUserAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialUserAuditLogManagement>();
                    await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                        Comment.GetModelName(),
                        Comment.Id.ToString(),
                        LOG_ACTIONS.DELETE,
                        Comment.Owner,
                        new JObject(),
                        new JObject()
                    );
                    #endregion
                }
                #endregion
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        public async Task<ErrorCodes> ModifyComment(SocialComment Comment, SocialCommentModifyModel NewData)
        {
            if (Comment.Content == NewData.content) {
                return ErrorCodes.NO_CHANGE_DETECTED;
            }
            var oldComment = Utils.DeepClone(Comment.GetJsonObjectForLog());
            Comment.Status.ChangeStatus(StatusType.Edited);
            Comment.Content = NewData.content;
            Comment.LastModifiedTimestamp = DateTime.UtcNow;
            if (await __DBContext.SaveChangesAsync() > 0) {
                using (var scope = __ServiceProvider.CreateScope())
                {
                    var __SocialUserAuditLogManagement = scope.ServiceProvider.GetRequiredService<SocialUserAuditLogManagement>();
                    var (oldVal, newVal) = Utils.GetDataChanges(oldComment, Comment.GetJsonObjectForLog());
                    await __SocialUserAuditLogManagement.AddNewUserAuditLog(
                        Comment.GetModelName(),
                        Comment.Id.ToString(),
                        LOG_ACTIONS.MODIFY,
                        Comment.Owner,
                        oldVal,
                        newVal
                    );
                }
                return ErrorCodes.NO_ERROR;
            }
            return ErrorCodes.INTERNAL_SERVER_ERROR;
        }
        #endregion
    }
}