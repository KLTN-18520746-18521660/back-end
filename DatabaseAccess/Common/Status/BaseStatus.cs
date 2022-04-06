using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseAccess.Common.Interface;

namespace DatabaseAccess.Common.Status
{
    #region EntityStatus Status
    public static class AdminBaseConfigStatus {
        public static readonly int Enabled = 0;
        public static readonly int Disabled = 1;
        public static readonly int Readonly = 2;
    }
    public static class AdminUserRightStatus {
        public static readonly int Enabled = 0;
        public static readonly int Disabled = 1;
        public static readonly int Readonly = 2;
    }
    public static class AdminUserRoleStatus {
        public static readonly int Enabled = 0;
        public static readonly int Disabled = 1;
        public static readonly int Readonly = 2;
    }
    public static class AdminUserStatus {
        public static readonly int Activated = 0;
        public static readonly int Deleted = 1;
        public static readonly int Blocked = 2;
        public static readonly int Readonly = 3;
    }
    public static class SocialCategoryStatus {
        public static readonly int Enabled = 0;
        public static readonly int Disabled = 1;
        public static readonly int Readonly = 2;
    }
    public static class SocialCommentStatus {
        public static readonly int Created = 0;
        public static readonly int Deleted = 1;
        public static readonly int Edited = 2;
    }
    public static class SocialNotificationStatus {
        public static readonly int Sent = 0;
        public static readonly int Deleted = 1;
        public static readonly int Read = 2;
    }
    public static class SocialPostStatus {
        public static readonly int Pending = 0;
        public static readonly int Deleted = 1;
        public static readonly int Approved = 2;
        public static readonly int Rejected = 3;
        public static readonly int Private = 4;
    }
    public static class SocialReportStatus {
        public static readonly int Pending = 0;
        public static readonly int Ignored = 1;
        public static readonly int Handled = 2;
    }
    public static class SocialTagStatus {
        public static readonly int Enabled = 0;
        public static readonly int Disabled = 1;
        public static readonly int Readonly = 2;
    }
    public static class SocialUserRightStatus {
        public static readonly int Enabled = 0;
        public static readonly int Disabled = 1;
        public static readonly int Readonly = 2;
    }
    public static class SocialUserRoleStatus {
        public static readonly int Enabled = 0;
        public static readonly int Disabled = 1;
        public static readonly int Readonly = 2;
    }
    public static class SocialUserStatus {
        public static readonly int Activated = 0;
        public static readonly int Deleted = 1;
        public static readonly int Blocked = 2;
    }
    #endregion
    public class BaseStatus
    {
        public readonly static int InvalidStatus = -1;
        public readonly static String InvalidStatusStr = "Invalid Status";
        #region Map Status
        protected readonly static Dictionary<int, string> MapAdminBaseConfigStatus = new()
        {
            { InvalidStatus, InvalidStatusStr },
            { AdminBaseConfigStatus.Enabled, "Enabled" },
            { AdminBaseConfigStatus.Disabled, "Disabled" },
            { AdminBaseConfigStatus.Readonly, "Readonly" }
        };
        protected readonly static Dictionary<int, string> MapAdminUserRightStatus = new()
        {
            { InvalidStatus, "Invalid Status" },
            { AdminUserRightStatus.Enabled, "Enabled" },
            { AdminUserRightStatus.Disabled, "Disabled" },
            { AdminUserRightStatus.Readonly, "Readonly" }
        };
        protected readonly static Dictionary<int, string> MapAdminUserRoleStatus = new()
        {
            { InvalidStatus, "Invalid Status" },
            { AdminUserRoleStatus.Enabled, "Enabled" },
            { AdminUserRoleStatus.Disabled, "Disabled" },
            { AdminUserRoleStatus.Readonly, "Readonly" }
        };
        protected readonly static Dictionary<int, string> MapAdminUserStatus = new()
        {
            { InvalidStatus, "Invalid Status" },
            { AdminUserStatus.Activated, "Activated" },
            { AdminUserStatus.Deleted, "Deleted" },
            { AdminUserStatus.Blocked, "Blocked" },
            { AdminUserStatus.Readonly, "Readonly" }
        };
        protected readonly static Dictionary<int, string> MapSocialCategoryStatus = new()
        {
            { InvalidStatus, "Invalid Status" },
            { SocialCategoryStatus.Enabled, "Enabled" },
            { SocialCategoryStatus.Disabled, "Disabled" },
            { SocialCategoryStatus.Readonly, "Readonly" }
        };
        protected readonly static Dictionary<int, string> MapSocialCommentStatus = new()
        {
            { InvalidStatus, "Invalid Status" },
            { SocialCommentStatus.Created, "Created" },
            { SocialCommentStatus.Deleted, "Deleted" },
            { SocialCommentStatus.Edited, "Edited" }
        };
        protected readonly static Dictionary<int, string> MapSocialNotificationStatus = new()
        {
            { InvalidStatus, "Invalid Status" },
            { SocialNotificationStatus.Sent, "Sent" },
            { SocialNotificationStatus.Deleted, "Deleted" },
            { SocialNotificationStatus.Read, "Read" }
        };
        protected readonly static Dictionary<int, string> MapSocialPostStatus = new()
        {
            { InvalidStatus, "Invalid Status" },
            { SocialPostStatus.Pending, "Pending" },
            { SocialPostStatus.Deleted, "Deleted" },
            { SocialPostStatus.Approved, "Approved" },
            { SocialPostStatus.Rejected, "Rejected" },
            { SocialPostStatus.Private, "Private" }
        };
        protected readonly static Dictionary<int, string> MapSocialReportStatus = new()
        {
            { InvalidStatus, "Invalid Status" },
            { SocialReportStatus.Pending, "Pending" },
            { SocialReportStatus.Ignored, "Ignored" },
            { SocialReportStatus.Handled, "Handled" }
        };
        protected readonly static Dictionary<int, string> MapSocialTagStatus = new()
        {
            { InvalidStatus, "Invalid Status" },
            { SocialTagStatus.Enabled, "Enabled" },
            { SocialTagStatus.Disabled, "Disabled" },
            { SocialTagStatus.Readonly, "Readonly" }
        };
        protected readonly static Dictionary<int, string> MapSocialUserRightStatus = new()
        {
            { InvalidStatus, "Invalid Status" },
            { SocialUserRightStatus.Enabled, "Enabled" },
            { SocialUserRightStatus.Disabled, "Disabled" },
            { SocialUserRightStatus.Readonly, "Readonly" }
        };
        protected readonly static Dictionary<int, string> MapSocialUserRoleStatus = new()
        {
            { InvalidStatus, "Invalid Status" },
            { SocialUserRoleStatus.Enabled, "Enabled" },
            { SocialUserRoleStatus.Disabled, "Disabled" },
            { SocialUserRoleStatus.Readonly, "Readonly" }
        };
        protected readonly static Dictionary<int, string> MapSocialUserStatus = new()
        {
            { InvalidStatus, "Invalid Status" },
            { SocialUserStatus.Activated, "Activated" },
            { SocialUserStatus.Deleted, "Deleted" },
            { SocialUserStatus.Blocked, "Blocked" }
        };
        #endregion

        public static string StatusToString(int status, EntityStatus entity)
        {
            switch (entity) {
                case EntityStatus.AdminBaseConfigStatus: {
                    return MapAdminBaseConfigStatus
                        .GetValueOrDefault(status, InvalidStatusStr);
                }
                case EntityStatus.AdminUserRightStatus: {
                    return MapAdminUserRightStatus
                        .GetValueOrDefault(status, InvalidStatusStr);
                }
                case EntityStatus.AdminUserRoleStatus: {
                    return MapAdminUserRoleStatus
                        .GetValueOrDefault(status, InvalidStatusStr);
                }
                case EntityStatus.AdminUserStatus: {
                    return MapAdminUserStatus
                        .GetValueOrDefault(status, InvalidStatusStr);
                }
                case EntityStatus.SocialCategoryStatus: {
                    return MapSocialCategoryStatus
                        .GetValueOrDefault(status, InvalidStatusStr);
                }
                case EntityStatus.SocialCommentStatus: {
                    return MapSocialCommentStatus
                        .GetValueOrDefault(status, InvalidStatusStr);
                }
                case EntityStatus.SocialNotificationStatus: {
                    return MapSocialNotificationStatus
                        .GetValueOrDefault(status, InvalidStatusStr);
                }
                case EntityStatus.SocialPostStatus: {
                    return MapSocialPostStatus
                        .GetValueOrDefault(status, InvalidStatusStr);
                }
                case EntityStatus.SocialReportStatus: {
                    return MapSocialReportStatus
                        .GetValueOrDefault(status, InvalidStatusStr);
                }
                case EntityStatus.SocialTagStatus: {
                    return MapSocialTagStatus
                        .GetValueOrDefault(status, InvalidStatusStr);
                }
                case EntityStatus.SocialUserRightStatus: {
                    return MapSocialUserRightStatus
                        .GetValueOrDefault(status, InvalidStatusStr);
                }
                case EntityStatus.SocialUserRoleStatus: {
                    return MapSocialUserRoleStatus
                        .GetValueOrDefault(status, InvalidStatusStr);
                }
                case EntityStatus.SocialUserStatus: {
                    return MapSocialUserStatus
                        .GetValueOrDefault(status, InvalidStatusStr);
                }
                default: {
                    return InvalidStatusStr;
                }
            }
        }

        public static int StatusFromString(string status, EntityStatus entity)
        {
            switch (entity) {
                case EntityStatus.AdminBaseConfigStatus: {
                    return MapAdminBaseConfigStatus
                        .FirstOrDefault(x => x.Value == status).Key;
                }
                case EntityStatus.AdminUserRightStatus: {
                    return MapAdminUserRightStatus
                        .FirstOrDefault(x => x.Value == status).Key;
                }
                case EntityStatus.AdminUserRoleStatus: {
                    return MapAdminUserRoleStatus
                        .FirstOrDefault(x => x.Value == status).Key;
                }
                case EntityStatus.AdminUserStatus: {
                    return MapAdminUserStatus
                        .FirstOrDefault(x => x.Value == status).Key;
                }
                case EntityStatus.SocialCategoryStatus: {
                    return MapSocialCategoryStatus
                        .FirstOrDefault(x => x.Value == status).Key;
                }
                case EntityStatus.SocialCommentStatus: {
                    return MapSocialCommentStatus
                        .FirstOrDefault(x => x.Value == status).Key;
                }
                case EntityStatus.SocialNotificationStatus: {
                    return MapSocialNotificationStatus
                        .FirstOrDefault(x => x.Value == status).Key;
                }
                case EntityStatus.SocialPostStatus: {
                    return MapSocialPostStatus
                        .FirstOrDefault(x => x.Value == status).Key;
                }
                case EntityStatus.SocialReportStatus: {
                    return MapSocialReportStatus
                        .FirstOrDefault(x => x.Value == status).Key;
                }
                case EntityStatus.SocialTagStatus: {
                    return MapSocialTagStatus
                        .FirstOrDefault(x => x.Value == status).Key;
                }
                case EntityStatus.SocialUserRightStatus: {
                    return MapSocialUserRightStatus
                        .FirstOrDefault(x => x.Value == status).Key;
                }
                case EntityStatus.SocialUserRoleStatus: {
                    return MapSocialUserRoleStatus
                        .FirstOrDefault(x => x.Value == status).Key;
                }
                case EntityStatus.SocialUserStatus: {
                    return MapSocialUserStatus
                        .FirstOrDefault(x => x.Value == status).Key;
                }
                default: {
                    return InvalidStatus;
                }
            }
        }
    }
}
