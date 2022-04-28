using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DatabaseAccess.Common.Actions
{
    public enum ActionType {
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

    public enum EntityActionType {
        InvalidEntity           = -1,
        UserActionWithCategory  = 0,
        UserActionWithComment   = 1,
        UserActionWithTag       = 2,
        UserActionWithPost      = 3,
        UserActionWithUser      = 4,
    }

    public class EntityAction
    {
        private EntityActionType type;
        private string __action;
        public string action { get => __action;  set {
            if (!ValidateAction(value, type)) {
                throw new Exception($"Invalid action: { value } , action entity type: { type }.");
            }
            __action = value;
        }}
        public DateTime date { get; set; }

        public EntityAction(EntityActionType _type, string _action)
        {
            type = _type;
            action = _action;
            date = DateTime.UtcNow;
        }

        public EntityAction()
        {
            type = EntityActionType.InvalidEntity;
            action = default;
            date = DateTime.UtcNow;
        }

        public EntityAction(EntityActionType _type, ActionType _action)
        {
            type = _type;
            action = ActionTypeToString(_action);
            date = DateTime.UtcNow;
        }

        public static string[] GetAllowActionsByType(EntityActionType type)
        {
            switch (type) {
                case EntityActionType.UserActionWithCategory:
                    return new string[]{
                        "Follow",
                        "Visited",
                    };
                case EntityActionType.UserActionWithTag:
                    return new string[]{
                        "Follow",
                        "Used",
                        "Visited",
                    };
                case EntityActionType.UserActionWithComment:
                    return new string[]{
                        "Like",
                        "Dislike",
                        "Report",
                        "Reply",
                    };
                case EntityActionType.UserActionWithPost:
                    return new string[]{
                        "Like",
                        "Dislike",
                        "Follow",
                        "Comment",
                        "Report",
                        "Visited",
                        "Saved",
                    };
                case EntityActionType.UserActionWithUser:
                    return new string[]{
                        "Follow",
                        "Report",
                    };
                default:
                    return default;
            }
        }

        public static bool ValidateAction(string action, EntityActionType type)
        {
            var allowActions = GetAllowActionsByType(type);
            if (allowActions == default) {
                return false;
            }
            return allowActions.Contains(action);
        }

        public static string ActionTypeToString(ActionType action)
        {
            switch (action) {
                case ActionType.Like:
                    return "Like";
                case ActionType.Dislike:
                    return "Dislike";
                case ActionType.Follow:
                    return "Follow";
                case ActionType.Report:
                    return "Report";
                case ActionType.Reply:
                    return "Reply";
                case ActionType.Used:
                    return "Used";
                case ActionType.Visited:
                    return "Visited";
                case ActionType.Saved:
                    return "Saved";
                case ActionType.Comment:
                    return "Comment";
            }
            return default;
        }

        public static string GenContainsJsonStatement(string actionStr)
        {
            return $"[{{\"action\":\"{ actionStr }\"}}]";
        }

        public static string GenContainsJsonStatement(ActionType action)
        {
            return $"[{{\"action\":\"{ ActionTypeToString(action) }\"}}]";
        }
    }
}