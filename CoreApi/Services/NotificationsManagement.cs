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
        APPROVE_MODIFY_POST         = 3,
        REJECT_POST                 = 4,
        REJECT_MODIFY_POST          = 5,
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
        public Guid? ActionOfUserId { get; protected set; }
        public Guid? ActionOfAdminUserId { get; protected set; }
        public BaseNotificationSenderModel()
        {
            __ModelName = "BaseNotificationSenderModel";
        }
        public BaseNotificationSenderModel(NotificationSenderAction action, Guid? actionOfUserId, Guid? actionOfAdminUserId)
        {
            if (actionOfAdminUserId == default && actionOfUserId == default) {
                throw new Exception("Not exception null userId's action");
            }
            Action = action;
            ActionOfUserId = actionOfUserId;
            ActionOfAdminUserId = actionOfAdminUserId;
        }
    }

    public class PostNotificationModel : BaseNotificationSenderModel {
        public long PostId { get; set; }
        public PostNotificationModel(NotificationSenderAction action, Guid? actionOfUserId, Guid? actionOfAdminUserId)
            : base(action, actionOfUserId, actionOfAdminUserId)
        {
            __ModelName = "PostNotificationModel";
            switch (action) {
                case NotificationSenderAction.NEW_POST:
                    ActionStr = "new-post";
                    break;
                case NotificationSenderAction.APPROVE_POST:
                    ActionStr = "approve-post";
                    break;
                case NotificationSenderAction.APPROVE_MODIFY_POST:
                    ActionStr = "approve-modify-post";
                    break;
                case NotificationSenderAction.REJECT_POST:
                    ActionStr = "reject-post";
                    break;
                case NotificationSenderAction.REJECT_MODIFY_POST:
                    ActionStr = "reject-modify-post";
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
        public CommentNotificationModel(NotificationSenderAction action, Guid? actionOfUserId, Guid? actionOfAdminUserId)
            : base(action, actionOfUserId, actionOfAdminUserId)
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
        public UserNotificationModel(NotificationSenderAction action, Guid? actionOfUserId, Guid? actionOfAdminUserId)
            : base(action, actionOfUserId, actionOfAdminUserId)
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
                            ret.Add("error", $"ErrorCode: { error }");
                            LogError($"Not found post, PostId: { postId }");
                            break;
                        }
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
                            ret.Add("error", $"ErrorCode: { error }");
                            LogError($"Not found comment, CommentId: { commentId }");
                            break;
                        }
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
                            ret.Add("error", $"ErrorCode: { error }");
                            LogError($"Not found user, UserId: { userId }");
                            break;
                        }
                        break;
                    }
                default:
                    throw new Exception($"TraceId: { modelData.TraceId }, Invalid type when send notification: { type }");
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
                    LogError($"TraceId: { traceId }, AddNotification failed, owner: { notification.Owner }, type: { notification.Type }, conent: { notification.ContentStr }");
                } else {
                    LogDebug($"TraceId: { traceId }, AddNotification success, owner: { notification.Owner }, type: { notification.Type }, conent: { notification.ContentStr }");
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
            LogInformation($"TraceId: { modelData.TraceId }, Received new notification, type = { type }, Action = { modelData.ActionStr }, ActionOfUserId = { modelData.ActionOfUserId }, ActionOfAdminUserId = { modelData.ActionOfAdminUserId }");
            if (dataToDB.Count != 0) {
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
                                    EntityAction.GenContainsJsonStatement(ActionType.Follow)
                                )
                            )
                            .Select(e => e.UserId)
                            .ToArray();
                        notifications.Add(new SocialNotification(){
                            Content             = dataToDB,
                            Owner               = post.Owner,
                            ActionOfUserId      = modelData.ActionOfUserId,
                            ActionOfAdminUserId = modelData.ActionOfAdminUserId,
                            Type                = modelData.ActionStr,
                            PostId              = modelData.PostId,
                        });
                        await AddRangeNotification(notifications.ToArray(), modelData.TraceId);
                        await SendNotificationTypeActionWithPost(
                            new PostNotificationModel(NotificationSenderAction.NEW_POST,
                                                      post.Owner,
                                                      default){
                                PostId  = post.Id,
                                TraceId = modelData.TraceId
                            }
                        );
                        break;
                    }
                case NotificationSenderAction.NEW_POST:
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
                                    EntityAction.GenContainsJsonStatement(ActionType.Follow)
                                )
                            )
                            .Select(e => e.UserId)
                            .ToArray();
                        foreach (var userId in userIds) {
                            notifications.Add(new SocialNotification(){
                                Content             = dataToDB,
                                Owner               = userId,
                                ActionOfUserId      = modelData.ActionOfUserId,
                                ActionOfAdminUserId = modelData.ActionOfAdminUserId,
                                Type                = modelData.ActionStr,
                                PostId              = modelData.PostId,
                            });
                        }
                        await AddRangeNotification(notifications.ToArray(), modelData.TraceId);
                        break;
                    }
                case NotificationSenderAction.APPROVE_MODIFY_POST:
                case NotificationSenderAction.REJECT_MODIFY_POST:
                case NotificationSenderAction.REJECT_POST:
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
                            Content             = dataToDB,
                            Owner               = post.Owner,
                            ActionOfUserId      = modelData.ActionOfUserId,
                            ActionOfAdminUserId = modelData.ActionOfAdminUserId,
                            Type                = modelData.ActionStr,
                            PostId              = modelData.PostId,
                        });
                        await AddRangeNotification(notifications.ToArray(), modelData.TraceId);
                        break;
                    }
                default:
                    throw new Exception($"TraceId: { modelData.TraceId }, Invalid action with post: { modelData.Action }");
            }
            LogInformation($"TraceId: { modelData.TraceId }, Handle new notification successfully.");
        }

        protected async Task SendNotificationTypeActionWithComment(CommentNotificationModel modelData)
        {
            var type = NotificationType.ACTION_WITH_COMMENT;
            var dataToDB = await GetValueToDB(type, modelData);
            LogInformation($"TraceId: { modelData.TraceId }, Received new notification, type = { type }, Action = { modelData.ActionStr }, ActionOfUserId = { modelData.ActionOfUserId }, ActionOfAdminUserId = { modelData.ActionOfAdminUserId }");
            if (dataToDB.Count != 0) {
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
                            Content             = dataToDB,
                            Owner               = comment.Owner,
                            ActionOfUserId      = modelData.ActionOfUserId,
                            ActionOfAdminUserId = modelData.ActionOfAdminUserId,
                            Type                = modelData.ActionStr,
                            CommentId           = modelData.CommentId,
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
                                && (comment.ParentId == default || e.UserId != comment.Parent.OwnerNavigation.Id)
                                && e.ActionsStr.Contains(
                                    EntityAction.GenContainsJsonStatement(ActionType.Follow)
                                )
                            )
                            .Select(e => e.UserId)
                            .ToArray();
                        foreach (var userId in userIds) {
                            notifications.Add(new SocialNotification(){
                                Content             = dataToDB,
                                Owner               = userId,
                                ActionOfUserId      = modelData.ActionOfUserId,
                                ActionOfAdminUserId = modelData.ActionOfAdminUserId,
                                Type                = modelData.ActionStr,
                                CommentId           = modelData.CommentId,
                            });
                        }
                        if (!userIds.Contains(comment.Post.Owner)) {
                            notifications.Add(new SocialNotification(){
                                Content             = dataToDB,
                                Owner               = comment.Post.Owner,
                                ActionOfUserId      = modelData.ActionOfUserId,
                                ActionOfAdminUserId = modelData.ActionOfAdminUserId,
                                Type                = modelData.ActionStr,
                                CommentId           = modelData.CommentId,
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
                            Content             = dataToDB,
                            Owner               = comment.Parent.Owner,
                            ActionOfUserId      = modelData.ActionOfUserId,
                            ActionOfAdminUserId = modelData.ActionOfAdminUserId,
                            Type                = modelData.ActionStr,
                            CommentId           = modelData.CommentId,
                        });
                        await AddRangeNotification(notifications.ToArray(), modelData.TraceId);
                        await SendNotificationTypeActionWithComment(
                            new CommentNotificationModel(NotificationSenderAction.NEW_COMMENT,
                                                         modelData.ActionOfUserId,
                                                         modelData.ActionOfAdminUserId){
                                CommentId = comment.Id,
                                TraceId = modelData.TraceId
                            }
                        );
                        break;
                    }
                default:
                    throw new Exception($"TraceId: { modelData.TraceId }, Invalid action with comment: { modelData.Action }");
            }
            LogInformation($"TraceId: { modelData.TraceId }, Handle new notification successfully.");
        }

        protected async Task SendNotificationTypeActionWithUser(UserNotificationModel modelData)
        {
            var type = NotificationType.ACTION_WITH_USER;
            var dataToDB = await GetValueToDB(type, modelData);
            LogInformation($"TraceId: { modelData.TraceId }, Received new notification, type = { type }, Action = { modelData.ActionStr }, ActionOfUserId = { modelData.ActionOfUserId }, ActionOfAdminUserId = { modelData.ActionOfAdminUserId }");
            if (dataToDB.Count != 0) {
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
                            Content             = dataToDB,
                            Owner               = user.Id,
                            ActionOfUserId      = modelData.ActionOfUserId,
                            ActionOfAdminUserId = modelData.ActionOfAdminUserId,
                            Type                = modelData.ActionStr,
                            UserId              = modelData.UserId,
                        });
                        await AddRangeNotification(notifications.ToArray(), modelData.TraceId);
                        break;
                    }
                default:
                    throw new Exception($"TraceId: { modelData.TraceId }, Invalid action with user: { modelData.Action }");
            }
            LogInformation($"TraceId: { modelData.TraceId }, Handle new notification successfully.");
        }

        public async Task SendNotification(NotificationType type, BaseNotificationSenderModel modelData)
        {
            try {
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
                        throw new Exception($"TraceId: { modelData.TraceId }, Invalid type when send notification: { type }");
                }
            } catch (Exception e) {
                LogError(e.ToString());
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
        public async Task<ErrorCodes> DeleteNotification(Guid UserId, long NotificationId)
        {
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __DBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                var notification = await __DBContext.SocialNotifications
                                        .Where(e => e.Id == NotificationId && e.Owner == UserId)
                                        .FirstOrDefaultAsync();
                if (notification == default) {
                    return ErrorCodes.NOT_FOUND;
                }
                notification.Status.ChangeStatus(StatusType.Deleted);
                if (await __DBContext.SaveChangesAsync() <= 0) {
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
            }
            return ErrorCodes.NO_ERROR;
        }
        public async Task<ErrorCodes> MarkNotificationAsRead(Guid UserId, long NotificationId)
        {
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __DBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                var notification = await __DBContext.SocialNotifications
                    .Where(e => e.Id == NotificationId && e.Owner == UserId)
                    .FirstOrDefaultAsync();
                if (notification == default) {
                    return ErrorCodes.NOT_FOUND;
                }
                notification.Status.ChangeStatus(StatusType.Read);
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
                    .Where(e => e.Owner == UserId)
                    .ForEachAsync(e => e.StatusStr = EntityStatus.StatusTypeToString(StatusType.Read));

                if (await __DBContext.SaveChangesAsync() < 0) {
                    return ErrorCodes.INTERNAL_SERVER_ERROR;
                }
            }
            return ErrorCodes.NO_ERROR;
        }
        public async Task<(List<SocialNotification>, int)> GetNotifications(Guid socialUserId,
                                                                            int start = 0,
                                                                            int size = 20,
                                                                            string[] status = default)
        {
            List<SocialNotification> notifications = default;
            int totalCount = default;
            if (status == default) {
                status = new string[]{};
            }
            using (var scope = __ServiceProvider.CreateScope())
            {
                var __DBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                var query = __DBContext.SocialNotifications
                    .Where(e => e.Owner == socialUserId
                        && ((status.Count() == 0
                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted))
                            || status.Contains(e.StatusStr)
                        )
                        && (e.PostId == default || e.Post.Owner == socialUserId
                            || e.Post.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved)
                        )
                        && (e.CommentId == default || e.Comment.Owner == socialUserId
                            || e.Comment.StatusStr == EntityStatus.StatusTypeToString(StatusType.Deleted)
                        )
                        && (e.UserId == default || e.UserId == socialUserId
                            || e.UserIdDesNavigation.StatusStr == EntityStatus.StatusTypeToString(StatusType.Deleted)
                        )
                    )
                    .OrderByDescending(e => e.CreatedTimestamp)
                    .Skip(start).Take(size)
                    .Include(e => e.ActionOfAdminUserIdNavigation)
                    .Include(e => e.ActionOfUserIdNavigation);

                notifications = await query.ToListAsync();
                totalCount = await __DBContext.SocialNotifications
                    .CountAsync(e => e.Owner == socialUserId
                        && ((status.Count() == 0
                            && e.StatusStr != EntityStatus.StatusTypeToString(StatusType.Deleted))
                            || status.Contains(e.StatusStr)
                        )
                        && (e.PostId == default || e.Post.Owner == socialUserId
                            || e.Post.StatusStr == EntityStatus.StatusTypeToString(StatusType.Approved)
                        )
                        && (e.CommentId == default || e.Comment.Owner == socialUserId
                            || e.Comment.StatusStr == EntityStatus.StatusTypeToString(StatusType.Deleted)
                        )
                        && (e.UserId == default || e.UserId == socialUserId
                            || e.UserIdDesNavigation.StatusStr == EntityStatus.StatusTypeToString(StatusType.Deleted)
                        )
                    );

                #region Update content notifications
                var __BaseConfig = (BaseConfig)__ServiceProvider.GetService(typeof(BaseConfig));
                var (intervalTime, error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.NOTIFICATION, SUB_CONFIG_KEY.INTERVAL_TIME);
                LogWarning(error);
                var now = DateTime.UtcNow;
                foreach (var notify in notifications) {
                    if (notify.LastUpdateContent.HasValue == false
                        || (now = notify.LastUpdateContent.Value.ToUniversalTime()).Minute > intervalTime
                    ) {
                        notify.LastUpdateContent = now;
                        notify.Content = ProcessingContent(notify);
                    }
                }
                await __DBContext.SaveChangesAsync();
                #endregion
            }

            return (notifications, totalCount);
        }

        protected JObject ProcessingContent(SocialNotification notify)
        {
            var count = notify.UserId != default ? 1 : 0;
            count += notify.CommentId != default ? 1 : 0;
            count += notify.PostId != default ? 1 : 0;
            if (count != 1) {
                return new JObject(){
                    { "error", "Invalid notification." }
                };
            }

            if (notify.UserId != default) {
                return new JObject(){
                    {
                        "user_des",
                        new JObject(){
                            { "user_name", notify.UserIdDesNavigation.UserName },
                            { "display_name", notify.UserIdDesNavigation.DisplayName },
                            { "avatar", notify.UserIdDesNavigation.Avatar },
                        }
                    }
                };
            } else if (notify.CommentId != default) {
                StringBuilder cmtContent = new StringBuilder(
                    notify.Comment.Content.Substring(0, notify.Comment.Content.Length > 47 ? 47 : notify.Comment.Content.Length)
                );
                cmtContent.Append("...");

                return new JObject(){
                    {
                        "post_owner",
                        new JObject(){
                            { "user_name", notify.Comment.Post.OwnerNavigation.UserName },
                            { "display_name", notify.Comment.Post.OwnerNavigation.DisplayName },
                            { "avatar", notify.Comment.Post.OwnerNavigation.Avatar },
                        }
                    },
                    {
                        "post_detail",
                        new JObject(){
                            { "slug", notify.Comment.Post.Slug },
                            { "title", notify.Comment.Post.Title },
                        }
                    },
                    {
                        "comment_owner",
                        new JObject(){
                            { "user_name", notify.Comment.OwnerNavigation.UserName },
                            { "display_name", notify.Comment.OwnerNavigation.DisplayName },
                            { "avatar", notify.Comment.OwnerNavigation.Avatar },
                        }
                    },
                    { "comment_content", cmtContent.ToString() },
                };
            } else if (notify.PostId != default) {
                return new JObject(){
                    {
                        "post_owner",
                        new JObject(){
                            { "user_name", notify.Post.OwnerNavigation.UserName },
                            { "display_name", notify.Post.OwnerNavigation.DisplayName },
                            { "avatar", notify.Post.OwnerNavigation.Avatar },
                        }
                    },
                    {
                        "post_detail",
                        new JObject(){
                            { "slug", notify.Post.Slug },
                            { "title", notify.Post.Title },
                        }
                    },
                };
            }
            return new JObject(){
                { "error", "Invalid notification." }
            };
        }
        #endregion
    }
}