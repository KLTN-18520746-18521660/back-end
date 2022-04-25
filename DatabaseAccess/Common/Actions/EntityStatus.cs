using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DatabaseAccess.Common.Statuss
{
    public enum StatusType {
        Like        = 0,
        Dislike     = 1,
        Report      = 2,
        Reply       = 3,
        Follow      = 4,
        Used        = 5,
        Visited     = 6,
        Saved       = 7,
        Comment     = 8,
    }

    public enum EntityStatusType {
        InvalidEntity           = -1,
        UserStatusWithCategory  = 0,
        UserStatusWithComment   = 1,
        UserStatusWithTag       = 2,
        UserStatusWithPost      = 3,
        UserStatusWithUser      = 4,
    }

    public class EntityStatus
    {
        private EntityStatusType type;
        private string __status;
        public string status { get => __status;  set {
            if (!ValidateStatus(value, type)) {
                throw new Exception($"Invalid action of action entity type: { type }.");
            }
            __status = value;
        }}
        public DateTime date { get; set; }

        public EntityStatus(EntityStatusType _type, string _status)
        {
            type = _type;
            status = _status;
            date = DateTime.UtcNow;
        }

        public EntityStatus(EntityStatusType _type, StatusType _action)
        {
            type = _type;
            status = StatusTypeToString(_action);
            date = DateTime.UtcNow;
        }

        public static string[] GetAllowStatusByType(EntityStatusType type)
        {
            switch (type) {
                case EntityStatusType.UserStatusWithCategory:
                    return new string[]{
                        "Follow",
                        "Visited",
                    };
                case EntityStatusType.UserStatusWithTag:
                    return new string[]{
                        "Follow",
                        "Used",
                        "Visited",
                    };
                case EntityStatusType.UserStatusWithComment:
                    return new string[]{
                        "Like",
                        "Dislike",
                        "Report",
                        "Reply",
                    };
                case EntityStatusType.UserStatusWithPost:
                    return new string[]{
                        "Like",
                        "Dislike",
                        "Follow",
                        "Comment",
                        "Report",
                        "Visited",
                        "Saved",
                    };
                case EntityStatusType.UserStatusWithUser:
                    return new string[]{
                        "Follow",
                        "Report",
                    };
                default:
                    return default;
            }
        }

        public static bool ValidateStatus(string action, EntityStatusType type)
        {
            var allowStatuss = GetAllowStatusByType(type);
            if (allowStatuss == default) {
                return false;
            }
            return allowStatuss.Contains(action);
        }

        public static string StatusTypeToString(StatusType action)
        {
            switch (action) {
                case StatusType.Like:
                    return "Like";
                case StatusType.Dislike:
                    return "Dislike";
                case StatusType.Follow:
                    return "Follow";
                case StatusType.Report:
                    return "Report";
                case StatusType.Reply:
                    return "Reply";
                case StatusType.Used:
                    return "Used";
                case StatusType.Visited:
                    return "Visited";
                case StatusType.Saved:
                    return "Saved";
            }
            return default;
        }
    }
}