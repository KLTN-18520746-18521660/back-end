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
        NewPost = 0,
        NewComment = 1,
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

    public class NewPostNotificationModel : BaseNotificationSenderModel {
        public long PostId { get; set; }
        public string UserName { get; set; }
        public string PostTitle { get; set; }
        public DateTime DateTimeSend { get; }
        public NewPostNotificationModel()
        {
            __ModelName = "NewPostNotificationModel";
            DateTimeSend = DateTime.UtcNow;
        }
    }
    public class NewCommentNotificationModel : BaseNotificationSenderModel {
        public long CommentId { get; set; }
        public DateTime DateTimeSend { get; }
        public NewCommentNotificationModel()
        {
            __ModelName = "NewCommentNotificationModel";
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