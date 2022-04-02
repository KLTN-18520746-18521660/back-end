

namespace CoreApi.Common
{
    public enum ErrorCodes {
        NO_ERROR = 1,
        INTERNAL_SERVER_ERROR = 2,
        NOT_FOUND = 3,
        SESSION_HAS_EXPIRED = 4,
        USER_HAVE_BEEN_LOCKED = 5,
        USER_DOES_NOT_HAVE_PERMISSION = 6,
        INVALID_USER = 7,
        DELETED = 8,
        USER_IS_NOT_OWNER = 9,
    }
}