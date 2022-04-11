using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseAccess.Common;

namespace DatabaseAccess.Common.Actions
{
    #region Entity Social Action
    public class UserActionWithCategory {
        public static readonly int Follow = 0;
    }
    public class UserActionWithComment {
        public static readonly int Like = 0;
        public static readonly int Dislike = 1;
        public static readonly int Report = 2;
        public static readonly int Reply = 3;
    }
    public class UserActionWithTag {
        public static readonly int Follow = 0;
        public static readonly int Used = 1;
        public static readonly int Visited = 2;
    }
    public class UserActionWithPost {
        public static readonly int Like = 0;
        public static readonly int Dislike = 1;
        public static readonly int Follow = 2;
        public static readonly int Comment = 3;
        public static readonly int Report = 4;
        public static readonly int Visited = 5;
        public static readonly int Saved = 6;
    }
    public class UserActionWithUser {
        public static readonly int Follow = 0;
        public static readonly int Report = 1;
        public static readonly int Friend = 2;
        public static readonly int Blocked = 3;
    }
    #endregion
    public class BaseAction
    {
        public readonly static int InvalidAction = -1;
        public readonly static String InvalidActionStr = "Invalid Action";
        #region Map Action
        protected readonly static Dictionary<int, string> MapUserActionWithCategory = new()
        {
            { InvalidAction, InvalidActionStr },
            { UserActionWithCategory.Follow, "Follow" }
        };
        protected readonly static Dictionary<int, string> MapUserActionWithComment = new()
        {
            { InvalidAction, "Invalid Action" },
            { UserActionWithComment.Like, "Enabled" },
            { UserActionWithComment.Dislike, "Disabled" },
            { UserActionWithComment.Report, "Report" },
            { UserActionWithComment.Reply, "Reply" }
        };
        protected readonly static Dictionary<int, string> MapUserActionWithTag = new()
        {
            { InvalidAction, "Invalid Action" },
            { UserActionWithTag.Follow, "Follow" },
            { UserActionWithTag.Used, "Used" },
            { UserActionWithTag.Visited, "Visited" },
        };
        protected readonly static Dictionary<int, string> MapUserActionWithPost = new()
        {
            { InvalidAction, "Invalid Action" },
            { UserActionWithPost.Like, "Like" },
            { UserActionWithPost.Dislike, "Dislike" },
            { UserActionWithPost.Follow, "Follow" },
            { UserActionWithPost.Comment, "Comment" },
            { UserActionWithPost.Report, "Report" },
            { UserActionWithPost.Visited, "Visited" },
            { UserActionWithPost.Saved, "Saved" },
        };
        protected readonly static Dictionary<int, string> MapUserActionWithUser = new()
        {
            { InvalidAction, "Invalid Action" },
            { UserActionWithUser.Follow, "Follow" },
            { UserActionWithUser.Report, "Report" },
            { UserActionWithUser.Friend, "Friend" },
            { UserActionWithUser.Blocked, "Blocked" }
        };
        #endregion

        public static string ActionToString(int action, EntityAction entity)
        {
            switch (entity) {
                case EntityAction.UserActionWithCategory: {
                    return MapUserActionWithCategory
                        .GetValueOrDefault(action, InvalidActionStr);
                }
                case EntityAction.UserActionWithComment: {
                    return MapUserActionWithComment
                        .GetValueOrDefault(action, InvalidActionStr);
                }
                case EntityAction.UserActionWithTag: {
                    return MapUserActionWithTag
                        .GetValueOrDefault(action, InvalidActionStr);
                }
                case EntityAction.UserActionWithPost: {
                    return MapUserActionWithPost
                        .GetValueOrDefault(action, InvalidActionStr);
                }
                case EntityAction.UserActionWithUser: {
                    return MapUserActionWithUser
                        .GetValueOrDefault(action, InvalidActionStr);
                }
                default: {
                    return InvalidActionStr;
                }
            }
        }

        public static int ActionFromString(string action, EntityAction entity)
        {
            switch (entity) {
                case EntityAction.UserActionWithCategory: {
                    return MapUserActionWithCategory
                        .FirstOrDefault(x => x.Value == action).Key;
                }
                case EntityAction.UserActionWithComment: {
                    return MapUserActionWithComment
                        .FirstOrDefault(x => x.Value == action).Key;
                }
                case EntityAction.UserActionWithTag: {
                    return MapUserActionWithTag
                        .FirstOrDefault(x => x.Value == action).Key;
                }
                case EntityAction.UserActionWithPost: {
                    return MapUserActionWithPost
                        .FirstOrDefault(x => x.Value == action).Key;
                }
                case EntityAction.UserActionWithUser: {
                    return MapUserActionWithUser
                        .FirstOrDefault(x => x.Value == action).Key;
                }
                default: {
                    return InvalidAction;
                }
            }
        }
    }
}
