

namespace CoreApi.Common
{
    public enum ErrorCodes {
        NO_ERROR                                            = 1,
        INTERNAL_SERVER_ERROR                               = 2,
        NOT_FOUND                                           = 3,
        SESSION_HAS_EXPIRED                                 = 4,
        USER_HAVE_BEEN_LOCKED                               = 5,
        USER_DOES_NOT_HAVE_PERMISSION                       = 6,
        DELETED                                             = 7,
        USER_IS_NOT_OWNER                                   = 8,
        INVALID_ACTION                                      = 9,
        INVALID_PARAMS                                      = 10,
        NO_CHANGE_DETECTED                                  = 11,
        PASSWORD_IS_EXPIRED                                 = 12,
        NOT_IMPLEMENT_YET                                   = 13,
    }
}