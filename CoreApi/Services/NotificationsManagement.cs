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

namespace CoreApi.Services
{
    #region Notification sender
    public enum NotificationSenderAction {
        NEW_POST = 0,
        COMENT_POST = 1,
        OTHER_ACTION_WITH_POST = 3,
        NEW_COMMENT = 4,
        LIKE_COMMENT = 5,
        REPLY_COMMENT = 6,
        ORTHER_ACTION_WITH_COMMENT = 7,
        FOLLOW_USER = 8,
        ORTHER_ACTION_WITH_USER = 9,
    }

    public class BaseNotificationSenderModel {
        protected string __ModelName;
        public string ModelName { get => __ModelName; }
        public BaseNotificationSenderModel()
        {
            __ModelName = "BaseNotificationSenderModel";
        }
        virtual public JArray GetAttributes()
        {
            JArray attrs = new JArray();
            foreach(var prop in this.GetType().GetProperties()) {
                if (!prop.Name.Contains("ModelName", StringComparison.OrdinalIgnoreCase)) {
                    attrs.Add(prop.Name);
                }
            }
            return attrs;
        }
    }

    public class PostNotificationModel : BaseNotificationSenderModel {
        public long PostId { get; set; }
        public string Action { get; set; }
        public DateTime DateTimeSend { get; }
        public PostNotificationModel(NotificationSenderAction action)
        {
            __ModelName = "PostNotificationModel";
            DateTimeSend = DateTime.UtcNow;
            switch (action) {
                case NotificationSenderAction.NEW_POST:
                    Action = "new-post";
                    break;
                case NotificationSenderAction.COMENT_POST:
                    Action = "comment-post";
                    break;
                default:
                    throw new Exception($"Invalid action with post: { action }");
            }
        }
    }
    public class CommentNotificationModel : BaseNotificationSenderModel {
        public long CommentId { get; set; }
        public DateTime DateTimeSend { get; }
        public CommentNotificationModel()
        {
            __ModelName = "CommentNotificationModel";
            DateTimeSend = DateTime.UtcNow;
        }
    }
    public class UserNotificationModel : BaseNotificationSenderModel {
        public long CommentId { get; set; }
        public DateTime DateTimeSend { get; }
        public UserNotificationModel()
        {
            __ModelName = "UserNotificationModel";
            DateTimeSend = DateTime.UtcNow;
        }
    }
    #endregion

    public class NotificationsManagement : BaseSingletonService
    {
        private Dictionary<string, string> __NotificationTemplates = new Dictionary<string, string>()
        {
            {
                "NewPostNotificationModel",
                "<b>@Model.UserName</b> posted a new post: @Model.PostTitle"
            },
            {
                "NewPostNotificationModel",
                "<b>@Model.UserName</b> posted a new post: @Model.PostTitle"
            }
        };
        public NotificationsManagement(IServiceProvider _IServiceProvider)
            : base(_IServiceProvider)
        {
            __ServiceName = "NotificationsManagement";
            using (var scope = __ServiceProvider.CreateScope())
            {
                // Configs = scope.ServiceProvider.GetRequiredService<DBContext>().AdminBaseConfigs.ToList();
            }
            LogInformation("Init load all config successfully.");
        }

    }
}