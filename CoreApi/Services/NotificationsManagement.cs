using Serilog;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using CoreApi.Common;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using Common;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using DatabaseAccess.Common.Actions;
using DatabaseAccess.Common.Status;

namespace CoreApi.Services
{
    #region Notification sender
    public enum NotificationSenderAction {
        INVALID_ACTION              = -1,
        NEW_POST                    = 0,
        LIKE_POST                   = 1,
        APPROVE_POST                = 2,
        REJECT_POST                 = 3,
        PRIVATE_POST                = 4,
        DELETE_POST                 = 5,
        NEW_COMMENT                 = 6,
        LIKE_COMMENT                = 7,
        REPLY_COMMENT               = 8,
        FOLLOW_USER                 = 9,
    }

    public enum NotificationType {
        ACTION_WITH_POST            = 0,
        ACTION_WITH_COMMENT         = 1,
        ACTION_WITH_USER            = 2,
    }

    public class BaseNotificationSenderModel {
        protected string __ModelName;
        public string ModelName { get => __ModelName; }
        public string TraceId { get; set; }
        public string ActionStr { get; protected set; }
        public NotificationSenderAction Action { get; protected set; }
        public DateTime DateTimeSend { get; protected set; }
        public BaseNotificationSenderModel()
        {
            __ModelName = "BaseNotificationSenderModel";
        }
        public BaseNotificationSenderModel(NotificationSenderAction action)
        {
            DateTimeSend = DateTime.UtcNow;
            Action = action;
        }
    }

    public class PostNotificationModel : BaseNotificationSenderModel {
        public long PostId { get; set; }
        public PostNotificationModel(NotificationSenderAction action)
            : base(action)
        {
            __ModelName = "PostNotificationModel";
            switch (action) {
                case NotificationSenderAction.NEW_POST:
                    ActionStr = "new-post";
                    break;
                case NotificationSenderAction.APPROVE_POST:
                    ActionStr = "approve-post";
                    break;
                case NotificationSenderAction.REJECT_POST:
                    ActionStr = "reject-post";
                    break;
                case NotificationSenderAction.PRIVATE_POST:
                    ActionStr = "private-post";
                    break;
                case NotificationSenderAction.DELETE_POST:
                    ActionStr = "delete-post";
                    break;
                case NotificationSenderAction.LIKE_POST:
                    ActionStr = "like-post";
                    break;
                default:
                    throw new Exception($"Invalid action with post: { action }");
            }
        }
    }
    public class CommentNotificationModel : BaseNotificationSenderModel {
        public long CommentId { get; set; }
        public CommentNotificationModel(NotificationSenderAction action)
            : base(action)
        {
            __ModelName = "CommentNotificationModel";
            switch (action) {
                case NotificationSenderAction.NEW_COMMENT:
                    ActionStr = "new-comment";
                    break;
                case NotificationSenderAction.LIKE_COMMENT:
                    ActionStr = "like-comment";
                    break;
                case NotificationSenderAction.REPLY_COMMENT:
                    ActionStr = "reply-comment";
                    break;
                default:
                    throw new Exception($"Invalid action with comment: { action }");
            }
        }
    }
    public class UserNotificationModel : BaseNotificationSenderModel {
        public Guid UserId { get; set; }
        public UserNotificationModel(NotificationSenderAction action)
            : base(action)
        {
            __ModelName = "UserNotificationModel";
            switch (action) {
                case NotificationSenderAction.FOLLOW_USER:
                    ActionStr = "follow-user";
                    break;
                default:
                    throw new Exception($"Invalid action with user: { action }");
            }
        }
    }
    #endregion

    public class NotificationsManagement : BaseSingletonService
    {
        public NotificationsManagement(IServiceProvider _IServiceProvider)
            : base(_IServiceProvider)
        {
            __ServiceName = "NotificationsManagement";
        }

        #region Send Notifications
        public async Task<JObject> GetValueToDB(NotificationType type, BaseNotificationSenderModel modelData)
        {
            var ret = new JObject();
            switch(type) {
                case NotificationType.ACTION_WITH_POST:
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var postId = (modelData as PostNotificationModel).PostId;
                        var __SocialPostManagement = scope.ServiceProvider.GetRequiredService<SocialPostManagement>();
                        __SocialPostManagement.SetTraceId(modelData.TraceId);

                        var (post, error) = await __SocialPostManagement.FindPostById(postId);
                        if (error != ErrorCodes.NO_ERROR) {
                            LogError($"Not found post, PostId: { postId }");
                            break;
                        }
                        ret.Add("action", modelData.ActionStr);
                        ret.Add("date_send", modelData.DateTimeSend);
                        ret.Add("post_owner", new JObject(){
                            { "id", post.Owner },
                            { "user_name", post.OwnerNavigation.UserName },
                            { "display_name", post.OwnerNavigation.DisplayName },
                            { "avartar", post.OwnerNavigation.Avatar },
                        });
                        ret.Add("post_detail", new JObject(){
                            { "id", post.Id },
                            { "slug", post.Slug },
                            { "title", post.Title },
                        });
                        break;
                    }
                case NotificationType.ACTION_WITH_COMMENT:
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var commentId = (modelData as CommentNotificationModel).CommentId;
                        var __SocialCommentManagement = scope.ServiceProvider.GetRequiredService<SocialCommentManagement>();
                        __SocialCommentManagement.SetTraceId(modelData.TraceId);

                        var (comment, error) = await __SocialCommentManagement.FindCommentById(commentId);
                        if (error != ErrorCodes.NO_ERROR) {
                            LogError($"Not found comment, CommentId: { commentId }");
                            break;
                        }
                        ret.Add("action", modelData.ActionStr);
                        ret.Add("date_send", modelData.DateTimeSend);
                        ret.Add("post_owner", new JObject(){
                            { "id", comment.Post.Owner },
                            { "user_name", comment.Post.OwnerNavigation.UserName },
                            { "display_name", comment.Post.OwnerNavigation.DisplayName },
                            { "avartar", comment.Post.OwnerNavigation.Avatar },
                        });
                        ret.Add("post_detail", new JObject(){
                            { "id", comment.Post.Id },
                            { "slug", comment.Post.Slug },
                            { "title", comment.Post.Title },
                        });
                        ret.Add("comment_owner", new JObject(){
                            { "id", comment.Owner },
                            { "user_name", comment.OwnerNavigation.UserName },
                            { "display_name", comment.OwnerNavigation.DisplayName },
                            { "avartar", comment.OwnerNavigation.Avatar },
                        });
                        ret.Add("commment_content", comment.Content.Substring(0, comment.Content.Length > 50 ? 50 : comment.Content.Length));
                        break;
                    }
                case NotificationType.ACTION_WITH_USER:
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var userId = (modelData as UserNotificationModel).UserId;
                        var __SocialUserManagement = scope.ServiceProvider.GetRequiredService<SocialUserManagement>();
                        __SocialUserManagement.SetTraceId(modelData.TraceId);

                        var (user, error) = await __SocialUserManagement.FindUserById(userId);
                        if (error != ErrorCodes.NO_ERROR) {
                            LogError($"Not found user, UserId: { userId }");
                            break;
                        }
                        ret.Add("action", modelData.ActionStr);
                        ret.Add("date_send", modelData.DateTimeSend);
                        ret.Add("user_des", new JObject(){
                            { "id", user.Id },
                            { "user_name", user.UserName },
                            { "display_name", user.DisplayName },
                            { "avartar", user.Avatar },
                        });
                        break;
                    }
                default:
                    throw new Exception($"Invalid type when send notification: { type }");
            }

            return ret;
        }

        protected async Task AddNotification(SocialNotification notification, string traceId)
        {
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __DBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                await __DBContext.SocialNotifications.AddAsync(notification);
                if (await __DBContext.SaveChangesAsync() <= 0) {
                    LogError($"TraceId: { traceId }, AddNotification failed, owner: { notification.UserId }, type: { notification.Type }, conent: { notification.ContentStr }");
                } else {
                    LogDebug($"TraceId: { traceId }, AddNotification success, owner: { notification.UserId }, type: { notification.Type }, conent: { notification.ContentStr }");
                }
            }
        }

        protected async Task AddRangeNotification(SocialNotification[] notification, string traceId)
        {
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __DBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                await __DBContext.SocialNotifications.AddRangeAsync(notification);
                if (await __DBContext.SaveChangesAsync() <= 0) {
                    LogError($"TraceId: { traceId }, AddRangeNotification failed.");
                } else {
                    LogDebug($"TraceId: { traceId }, AddRangeNotification success.");
                }
            }
        }

        protected async Task SendNotificationTypeActionWithPost(PostNotificationModel modelData)
        {
            var type = NotificationType.ACTION_WITH_POST;
            var dataToDB = await GetValueToDB(type, modelData);
            if (dataToDB.Count == 0) {
                LogError($"TraceId: { modelData.TraceId }, Invalid notification data to save to DB. type: { type }");
                return;
            }
            switch (modelData.Action) {
                case NotificationSenderAction.APPROVE_POST:
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialPostManagement = scope.ServiceProvider.GetRequiredService<SocialPostManagement>();
                        __SocialPostManagement.SetTraceId(modelData.TraceId);
                        var (post, error) = await __SocialPostManagement.FindPostById(modelData.PostId);
                        if (error != ErrorCodes.NO_ERROR) {
                            throw new Exception($"TraceId: { modelData.TraceId }, Not found post, PostId: { modelData.PostId }");
                        }

                        List<SocialNotification> notifications = new List<SocialNotification>();
                        var userIds = post.OwnerNavigation.SocialUserActionWithUserUsers
                            .Where(
                                e => e.UserIdDes == post.Owner
                                && EF.Functions.JsonExists(e.ActionsStr,
                                    BaseAction.ActionToString(UserActionWithUser.Follow, EntityAction.UserActionWithUser))
                            )
                            .Select(e => e.UserId)
                            .ToArray();
                        foreach (var userId in userIds) {
                            notifications.Add(new SocialNotification(){
                                Content = dataToDB,
                                UserId = userId,
                                Type = modelData.ActionStr,
                            });
                        }
                        if (!userIds.Contains(post.Owner)) {
                            notifications.Add(new SocialNotification(){
                                Content = dataToDB,
                                UserId = post.Owner,
                                Type = modelData.ActionStr,
                            });
                        }
                        await AddRangeNotification(notifications.ToArray(), modelData.TraceId);
                        break;
                    }
                case NotificationSenderAction.NEW_POST:
                case NotificationSenderAction.REJECT_POST:
                case NotificationSenderAction.PRIVATE_POST:
                case NotificationSenderAction.DELETE_POST:
                case NotificationSenderAction.LIKE_POST:
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialPostManagement = scope.ServiceProvider.GetRequiredService<SocialPostManagement>();
                        __SocialPostManagement.SetTraceId(modelData.TraceId);
                        var (post, error) = await __SocialPostManagement.FindPostById(modelData.PostId);
                        if (error != ErrorCodes.NO_ERROR) {
                            throw new Exception($"TraceId: { modelData.TraceId }, Not found post, PostId: { modelData.PostId }");
                        }

                        List<SocialNotification> notifications = new List<SocialNotification>();
                        notifications.Add(new SocialNotification(){
                            Content = dataToDB,
                            UserId = post.Owner,
                            Type = modelData.ActionStr,
                        });
                        await AddRangeNotification(notifications.ToArray(), modelData.TraceId);
                        break;
                    }
                default:
                    throw new Exception($"TraceId: { modelData.TraceId }, Invalid action with post: { modelData.Action }");
            }
        }

        protected async Task SendNotificationTypeActionWithComment(CommentNotificationModel modelData)
        {
            var type = NotificationType.ACTION_WITH_COMMENT;
            var dataToDB = await GetValueToDB(type, modelData);
            if (dataToDB.Count == 0) {
                LogError($"TraceId: { modelData.TraceId }, Invalid notification data to save to DB. type: { type }");
                return;
            }
            switch (modelData.Action) {
                case NotificationSenderAction.LIKE_COMMENT:
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialCommentManagement = scope.ServiceProvider.GetRequiredService<SocialCommentManagement>();
                        __SocialCommentManagement.SetTraceId(modelData.TraceId);
                        var (comment, error) = await __SocialCommentManagement.FindCommentById(modelData.CommentId);
                        if (error != ErrorCodes.NO_ERROR) {
                            throw new Exception($"TraceId: { modelData.TraceId }, Not found comment, CommentId: { modelData.CommentId }");
                        }

                        List<SocialNotification> notifications = new List<SocialNotification>();
                        notifications.Add(new SocialNotification(){
                            Content = dataToDB,
                            UserId = comment.Owner,
                            Type = modelData.ActionStr,
                        });
                        await AddRangeNotification(notifications.ToArray(), modelData.TraceId);
                        break;
                    }
                case NotificationSenderAction.NEW_COMMENT:
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialCommentManagement = scope.ServiceProvider.GetRequiredService<SocialCommentManagement>();
                        __SocialCommentManagement.SetTraceId(modelData.TraceId);
                        var (comment, error) = await __SocialCommentManagement.FindCommentById(modelData.CommentId);
                        if (error != ErrorCodes.NO_ERROR) {
                            throw new Exception($"TraceId: { modelData.TraceId }, Not found comment, CommentId: { modelData.CommentId }");
                        }

                        List<SocialNotification> notifications = new List<SocialNotification>();
                        var userIds = comment.Post.SocialUserActionWithPosts
                            .Where(
                                e => e.PostId == comment.PostId
                                && e.UserId != comment.Parent.OwnerNavigation.Id
                                && EF.Functions.JsonExists(e.ActionsStr,
                                    BaseAction.ActionToString(UserActionWithPost.Follow, EntityAction.UserActionWithPost))
                            )
                            .Select(e => e.UserId)
                            .ToArray();
                        foreach (var userId in userIds) {
                            notifications.Add(new SocialNotification(){
                                Content = dataToDB,
                                UserId = userId,
                                Type = modelData.ActionStr,
                            });
                        }
                        if (!userIds.Contains(comment.Post.Owner)) {
                            notifications.Add(new SocialNotification(){
                                Content = dataToDB,
                                UserId = comment.Post.Owner,
                                Type = modelData.ActionStr,
                            });
                        }
                        await AddRangeNotification(notifications.ToArray(), modelData.TraceId);
                        break;
                    }
                case NotificationSenderAction.REPLY_COMMENT:
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialCommentManagement = scope.ServiceProvider.GetRequiredService<SocialCommentManagement>();
                        __SocialCommentManagement.SetTraceId(modelData.TraceId);
                        var (comment, error) = await __SocialCommentManagement.FindCommentById(modelData.CommentId);
                        if (error != ErrorCodes.NO_ERROR) {
                            throw new Exception($"TraceId: { modelData.TraceId }, Not found comment, CommentId: { modelData.CommentId }");
                        }

                        List<SocialNotification> notifications = new List<SocialNotification>();
                        notifications.Add(new SocialNotification(){
                            Content = dataToDB,
                            UserId = comment.Parent.Owner,
                            Type = modelData.ActionStr,
                        });
                        await AddRangeNotification(notifications.ToArray(), modelData.TraceId);
                        await SendNotificationTypeActionWithComment(
                            new CommentNotificationModel(NotificationSenderAction.NEW_COMMENT){
                                CommentId = comment.Id,
                                TraceId = modelData.TraceId
                            }
                        );
                        break;
                    }
                default:
                    throw new Exception($"TraceId: { modelData.TraceId }, Invalid action with comment: { modelData.Action }");
            }
        }

        protected async Task SendNotificationTypeActionWithUser(UserNotificationModel modelData)
        {
            var type = NotificationType.ACTION_WITH_COMMENT;
            var dataToDB = await GetValueToDB(type, modelData);
            if (dataToDB.Count == 0) {
                LogError($"TraceId: { modelData.TraceId }, Invalid notification data to save to DB. type: { type }");
                return;
            }
            switch (modelData.Action) {
                case NotificationSenderAction.FOLLOW_USER:
                    using (var scope = __ServiceProvider.CreateScope())
                    {
                        var __SocialUserManagement = scope.ServiceProvider.GetRequiredService<SocialUserManagement>();
                        __SocialUserManagement.SetTraceId(modelData.TraceId);
                        var (user, error) = await __SocialUserManagement.FindUserById(modelData.UserId);
                        if (error != ErrorCodes.NO_ERROR) {
                            throw new Exception($"TraceId: { modelData.TraceId }, Not found user, UserId: {modelData.UserId }");
                        }

                        List<SocialNotification> notifications = new List<SocialNotification>();
                        notifications.Add(new SocialNotification(){
                            Content = dataToDB,
                            UserId = user.Id,
                            Type = modelData.ActionStr,
                        });
                        await AddRangeNotification(notifications.ToArray(), modelData.TraceId);
                        break;
                    }
                default:
                    throw new Exception($"TraceId: { modelData.TraceId }, Invalid action with user: { modelData.Action }");
            }
        }

        public async Task SendNotification(NotificationType type, BaseNotificationSenderModel modelData)
        {
            switch(type) {
                case NotificationType.ACTION_WITH_POST:
                    await SendNotificationTypeActionWithPost(modelData as PostNotificationModel);
                    break;
                case NotificationType.ACTION_WITH_COMMENT:
                    await SendNotificationTypeActionWithComment(modelData as CommentNotificationModel);
                    break;
                case NotificationType.ACTION_WITH_USER:
                    await SendNotificationTypeActionWithUser(modelData as UserNotificationModel);
                    break;
                default:
                    throw new Exception($"Invalid type when send notification: { type }");
            }
        }

        public async Task SendNotifications((NotificationType type, BaseNotificationSenderModel modelData)[] notifications)
        {
            foreach (var it in notifications) {
                await SendNotification(it.type, it.modelData);
            }
        }
        #endregion

        #region Handle notifications
        public async Task<ErrorCodes> DeleteNotification(Guid UserId, long NotificatinId)
        {
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __DBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                var notification = await __DBContext.SocialNotifications
                                        .Where(e => e.Id == NotificatinId && e.UserId == UserId)
                                        .FirstOrDefaultAsync();
                if (notification == default) {
                    return ErrorCodes.NOT_FOUND;
                }
                notification.Status = SocialNotificationStatus.Deleted;
                if (await __DBContext.SaveChangesAsync() <= 0) {
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
            }
            return ErrorCodes.NO_ERROR;
        }
        public async Task<ErrorCodes> MarkNotificationAsRead(Guid UserId, long NotificatinId)
        {
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __DBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                var notification = await __DBContext.SocialNotifications
                                        .Where(e => e.Id == NotificatinId && e.UserId == UserId)
                                        .FirstOrDefaultAsync();
                if (notification == default) {
                    return ErrorCodes.NOT_FOUND;
                }
                notification.Status = SocialNotificationStatus.Read;
                if (await __DBContext.SaveChangesAsync() <= 0) {
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
            }
            return ErrorCodes.NO_ERROR;
        }
        public async Task<ErrorCodes> MarkNotificationsAsRead(Guid UserId)
        {
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __DBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                await __DBContext.SocialNotifications
                                        .ForEachAsync(e => e.Status = SocialNotificationStatus.Read);

                if (await __DBContext.SaveChangesAsync() < 0) {
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
            }
            return ErrorCodes.NO_ERROR;
        }
        public async Task<(List<SocialNotification>, int)> GetNotifications(Guid socialUserId,
                                                                   int start = 0,
                                                                   int size = 20,
                                                                   string search_term = default,
                                                                   string[] status = default)
        {
            List<SocialNotification> notifications = default;
            int totalCount = default;
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __DBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                var query = (from notification in __DBContext.SocialNotifications
                                .Where(e => e.UserId == socialUserId
                                    && ((status.Count() == 0
                                        && e.StatusStr != BaseStatus
                                            .StatusToString(SocialPostStatus.Deleted, EntityStatus.SocialPostStatus)
                                        ) || status.Contains(e.StatusStr)
                                    )
                                    && (search_term == default || (search_term != default && e.ContentStr.Contains(search_term)))
                                )
                            select notification)
                            .OrderByDescending(e => e.CreatedTimestamp)
                            .Skip(start).Take(size);

                notifications = await query.ToListAsync();
                totalCount = await __DBContext.SocialNotifications
                                .CountAsync(e => e.UserId == socialUserId
                                    && ((status.Count() == 0
                                        && e.StatusStr != BaseStatus
                                            .StatusToString(SocialPostStatus.Deleted, EntityStatus.SocialPostStatus)
                                        ) || status.Contains(e.StatusStr)
                                    )
                                    && (search_term == default || (search_term != default && e.ContentStr.Contains(search_term)))
                                );
            }
            

            return (notifications, totalCount);
        }
        #endregion
    }
}