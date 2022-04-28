using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DatabaseAccess.Common.Status
{
    public enum StatusType {
        Enabled             = 0,
        Disabled            = 1,
        Readonly            = 2,
        Activated           = 3,
        Deleted             = 4,
        Blocked             = 5,
        Created             = 6,
        Edited              = 7,
        Sent                = 8,
        Read                = 9,
        Pending             = 10,
        Approved            = 11,
        Rejected            = 12,
        Private             = 13,
        Ignored             = 14,
        Handled             = 15,
    }

    public enum EntityStatusType {
        Invalid                         = -1,
        AdminBaseConfig                 = 0,
        AdminUserRight                  = 1,
        AdminUserRole                   = 2,
        AdminUser                       = 3,
        SocialCategory                  = 4,
        SocialComment                   = 5,
        SocialNotification              = 6,
        SocialPost                      = 7,
        SocialReport                    = 8,
        SocialTag                       = 9,
        SocialUserRight                 = 10,
        SocialUserRole                  = 11,
        SocialUser                      = 12,
    }

    public class EntityStatus
    {
        private EntityStatusType type;
        private string __status;
        private string status { get => __status;  set {
            if (!ValidateStatus(value, type)) {
                throw new Exception($"Invalid status of status entity type: { type }.");
            }
            __status = value;
        }}

        public StatusType Type { get => StatusStringToType(status); }
        public new string ToString() { return status; }

        public EntityStatus()
        {
            type = EntityStatusType.Invalid;
            status = default;
        }

        public EntityStatus(EntityStatusType _type, string _status)
        {
            type = _type;
            status = _status;
        }

        public EntityStatus(EntityStatusType _type, StatusType _status)
        {
            type = _type;
            status = StatusTypeToString(_status);
        }

        public void ChangeStatus(StatusType _status)
        {
            status = StatusTypeToString(_status);
        }

        public void ChangeStatus(string _status)
        {
            status = _status;
        }

        public static string[] GetAllowStatusByType(EntityStatusType type)
        {
            switch (type) {
                case EntityStatusType.AdminBaseConfig:
                    return new string[]{
                        "Enabled",
                        "Disabled",
                        "Readonly",
                    };
                case EntityStatusType.AdminUserRight:
                    return new string[]{
                        "Enabled",
                        "Disabled",
                        "Readonly",
                    };
                case EntityStatusType.AdminUserRole:
                    return new string[]{
                        "Enabled",
                        "Disabled",
                        "Readonly",
                    };
                case EntityStatusType.AdminUser:
                    return new string[]{
                        "Activated",
                        "Deleted",
                        "Blocked",
                        "Readonly",
                    };
                case EntityStatusType.SocialCategory:
                    return new string[]{
                        "Enabled",
                        "Disabled",
                        "Readonly",
                    };
                case EntityStatusType.SocialComment:
                    return new string[]{
                        "Created",
                        "Deleted",
                        "Edited",
                    };
                case EntityStatusType.SocialNotification:
                    return new string[]{
                        "Sent",
                        "Deleted",
                        "Read",
                    };
                case EntityStatusType.SocialPost:
                    return new string[]{
                        "Pending",
                        "Deleted",
                        "Approved",
                        "Rejected",
                        "Private",
                    };
                case EntityStatusType.SocialReport:
                    return new string[]{
                        "Pending",
                        "Ignored",
                        "Handled",
                    };
                case EntityStatusType.SocialTag:
                    return new string[]{
                        "Enabled",
                        "Disabled",
                        "Readonly",
                    };
                case EntityStatusType.SocialUserRight:
                    return new string[]{
                        "Enabled",
                        "Disabled",
                        "Readonly",
                    };
                case EntityStatusType.SocialUserRole:
                    return new string[]{
                        "Enabled",
                        "Disabled",
                        "Readonly",
                    };
                case EntityStatusType.SocialUser:
                    return new string[]{
                        "Activated",
                        "Deleted",
                        "Blocked",
                    };
                default:
                    return default;
            }
        }

        public static bool ValidateStatus(string status, EntityStatusType type)
        {
            var allowStatuss = GetAllowStatusByType(type);
            if (allowStatuss == default) {
                return false;
            }
            return allowStatuss.Contains(status);
        }

        public static string StatusTypeToString(StatusType status)
        {
            switch (status) {
                case StatusType.Enabled:
                    return "Enabled";
                case StatusType.Disabled:
                    return "Disabled";
                case StatusType.Readonly:
                    return "Readonly";
                case StatusType.Activated:
                    return "Activated";
                case StatusType.Deleted:
                    return "Deleted";
                case StatusType.Blocked:
                    return "Blocked";
                case StatusType.Created:
                    return "Created";
                case StatusType.Edited:
                    return "Edited";
                case StatusType.Sent:
                    return "Sent";
                case StatusType.Read:
                    return "Read";
                case StatusType.Pending:
                    return "Pending";
                case StatusType.Approved:
                    return "Approved";
                case StatusType.Rejected:
                    return "Rejected";
                case StatusType.Private:
                    return "Private";
                case StatusType.Ignored:
                    return "Ignored";
                case StatusType.Handled:
                    return "Handled";
            }
            return default;
        }

        public static StatusType StatusStringToType(string status)
        {
            switch (status) {
                case "Enabled":
                    return StatusType.Enabled;
                case "Disabled":
                    return StatusType.Disabled;
                case "Readonly":
                    return StatusType.Readonly;
                case "Activated":
                    return StatusType.Activated;
                case "Deleted":
                    return StatusType.Deleted;
                case "Blocked":
                    return StatusType.Blocked;
                case "Created":
                    return StatusType.Created;
                case "Edited":
                    return StatusType.Edited;
                case "Sent":
                    return StatusType.Sent;
                case "Read":
                    return StatusType.Read;
                case "Pending":
                    return StatusType.Pending;
                case "Approved":
                    return StatusType.Approved;
                case "Rejected":
                    return StatusType.Rejected;
                case "Private":
                    return StatusType.Private;
                case "Ignored":
                    return StatusType.Ignored;
                case "Handled":
                    return StatusType.Handled;
            }
            return default;
        }
    }
}