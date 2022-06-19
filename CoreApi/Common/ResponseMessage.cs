using System.Text.RegularExpressions;

namespace CoreApi.Common
{
    public class REST_MESSAGE
    {
        // Example: "Invalid value of field {0}: {1}"
        public string TEMPLATE_MESSAGE;
        public string CODE { get; private set; }

        public REST_MESSAGE(string TEMPLATE, string MESSAGE_CODE = default)
        {
            TEMPLATE_MESSAGE = TEMPLATE;
            CODE = MESSAGE_CODE == default ? GetCodeFromTemplate() : MESSAGE_CODE;
        }
        private string GetCodeFromTemplate()
        {
            return Regex.Replace(TEMPLATE_MESSAGE.ToUpper(), "(\\s+)", "_");
        }
        // "Not allow action: {0}" --> Not allow action: ABC
        public string GetMessage(params string[] Params)
        {
            var Ret = TEMPLATE_MESSAGE;
            if (Params != default) {
                for (int Index = 0; Index < Params.Length; Index++) {
                    var Key = "{" + Index.ToString() + "}";
                    Ret = Ret.Replace(Key, Params[Index]);
                }
            }
            Ret = Regex.Replace(Ret, "{\\d}", string.Empty);
            return Ret.Trim();
        }
    }
    public static class RESPONSE_MESSAGES
    {
        public readonly static REST_MESSAGE INVALID_REST_MESSAGE                    = new REST_MESSAGE("Invalid message",
                                                                                                       nameof(INVALID_REST_MESSAGE));
        public readonly static REST_MESSAGE OK                                      = new REST_MESSAGE("OK",
                                                                                                       nameof(OK));
        public readonly static REST_MESSAGE NOT_FOUND                               = new REST_MESSAGE("Not found {0}",
                                                                                                       nameof(NOT_FOUND));
        public readonly static REST_MESSAGE ALREADY_EXIST                           = new REST_MESSAGE("{0} already exist",
                                                                                                       nameof(ALREADY_EXIST));
        public readonly static REST_MESSAGE INTERNAL_SERVER_ERROR                   = new REST_MESSAGE("Internal server error",
                                                                                                       nameof(INTERNAL_SERVER_ERROR));
        public readonly static REST_MESSAGE INVALID_REQUEST_BODY                    = new REST_MESSAGE("Invalid request body",
                                                                                                       nameof(INVALID_REQUEST_BODY));
        public readonly static REST_MESSAGE INVALID_ORDER_PARAMS                    = new REST_MESSAGE("Invalid order params",
                                                                                                       nameof(INVALID_ORDER_PARAMS));
        public readonly static REST_MESSAGE INVALID_STATUS_PARAMS                   = new REST_MESSAGE("Invalid status params",
                                                                                                       nameof(INVALID_STATUS_PARAMS));
        public readonly static REST_MESSAGE NOT_ALLOW_SORT_BY                       = new REST_MESSAGE("Not allow sort by field: '{0}'",
                                                                                                       nameof(NOT_ALLOW_SORT_BY));
        public readonly static REST_MESSAGE PASSWORD_IS_EXPIRED                     = new REST_MESSAGE("Password is expired, you must change password",
                                                                                                       nameof(PASSWORD_IS_EXPIRED));
        public readonly static REST_MESSAGE USER_HAS_BEEN_LOCKED                    = new REST_MESSAGE("User has been locked",
                                                                                                       nameof(USER_HAS_BEEN_LOCKED));
        public readonly static REST_MESSAGE USER_HAS_BEEN_DELETED                   = new REST_MESSAGE("User has been deleted",
                                                                                                       nameof(USER_HAS_BEEN_DELETED));
        public readonly static REST_MESSAGE USERNAME_HAS_BEEN_USED                  = new REST_MESSAGE("UserName has been used",
                                                                                                       nameof(USERNAME_HAS_BEEN_USED));
        public readonly static REST_MESSAGE EMAIL_HAS_BEEN_USED                     = new REST_MESSAGE("Email has been used",
                                                                                                       nameof(EMAIL_HAS_BEEN_USED));
        public readonly static REST_MESSAGE SESSION_HAS_EXPIRED                     = new REST_MESSAGE("Session has expired",
                                                                                                       nameof(SESSION_HAS_EXPIRED));
        public readonly static REST_MESSAGE MISSING_HEADER_AUTHORIZE                = new REST_MESSAGE("Missing header authorization",
                                                                                                       nameof(MISSING_HEADER_AUTHORIZE));
        public readonly static REST_MESSAGE INVALID_HEADER_AUTHORIZE                = new REST_MESSAGE("Invalid header authorization",
                                                                                                       nameof(INVALID_HEADER_AUTHORIZE));
        public readonly static REST_MESSAGE INVALID_SESSION_TOKEN                   = new REST_MESSAGE("Invalid session token",
                                                                                                       nameof(INVALID_SESSION_TOKEN));
        public readonly static REST_MESSAGE USER_NOT_FOUND_OR_INCORRECT_PASSWORD    = new REST_MESSAGE("User not found or incorrect password",
                                                                                                       nameof(USER_NOT_FOUND_OR_INCORRECT_PASSWORD));
        public readonly static REST_MESSAGE INCORRECT_PASSWORD                      = new REST_MESSAGE("Incorrect password",
                                                                                                       nameof(INCORRECT_PASSWORD));
        public readonly static REST_MESSAGE INVALID_REQUEST_UPLOAD_FILE             = new REST_MESSAGE("Invalid request upload file",
                                                                                                       nameof(INVALID_REQUEST_UPLOAD_FILE));
        public readonly static REST_MESSAGE EXCEED_MAX_SIZE_OF_FILES                = new REST_MESSAGE("Exceed max size of files to upload",
                                                                                                       nameof(EXCEED_MAX_SIZE_OF_FILES));
        public readonly static REST_MESSAGE NOT_ALLOW_UPLOAD_FILE_TYPE              = new REST_MESSAGE("Not allow upload file type: {0}",
                                                                                                       nameof(NOT_ALLOW_UPLOAD_FILE_TYPE));
        public readonly static REST_MESSAGE NOT_ALLOW_UPLOAD_FILE_WITH_PATH         = new REST_MESSAGE("Not allow upload file with path: '{0}'",
                                                                                                       nameof(NOT_ALLOW_UPLOAD_FILE_WITH_PATH));
        public readonly static REST_MESSAGE USER_DOES_NOT_HAVE_PERMISSION           = new REST_MESSAGE("User doesn't have permission to {0}",
                                                                                                       nameof(USER_DOES_NOT_HAVE_PERMISSION));
        public readonly static REST_MESSAGE BAD_REQUEST_PARAMS                      = new REST_MESSAGE("Bad request params",
                                                                                                       nameof(BAD_REQUEST_PARAMS));
        public readonly static REST_MESSAGE BAD_REQUEST_BODY                        = new REST_MESSAGE("Bad request body",
                                                                                                       nameof(BAD_REQUEST_BODY));
        public readonly static REST_MESSAGE INVALID_REQUEST_PARAMS_START_SIZE       = new REST_MESSAGE("Invalid request params start: {0}. Total size is {1}",
                                                                                                       nameof(INVALID_REQUEST_PARAMS_START_SIZE));
        public readonly static REST_MESSAGE INVALID_CONFIG_KEY                      = new REST_MESSAGE("Invalid config_key: {0}",
                                                                                                       nameof(INVALID_CONFIG_KEY));
        public readonly static REST_MESSAGE NOT_ALLOW_TO_DO                         = new REST_MESSAGE("Not allow to {0}",
                                                                                                       nameof(NOT_ALLOW_TO_DO));
        public readonly static REST_MESSAGE NOT_ACCEPT                              = new REST_MESSAGE("Not accept {0}",
                                                                                                       nameof(NOT_ACCEPT));
        public readonly static REST_MESSAGE EMAIL_VERIFIED                          = new REST_MESSAGE("Email verified",
                                                                                                       nameof(EMAIL_VERIFIED));
        public readonly static REST_MESSAGE NO_CHANGES_DETECTED                     = new REST_MESSAGE("No changes detected",
                                                                                                       nameof(NO_CHANGES_DETECTED));
        public readonly static REST_MESSAGE INVALID_ACTION                          = new REST_MESSAGE("Invalid action: {0}",
                                                                                                       nameof(INVALID_ACTION));
        public readonly static REST_MESSAGE POLICY_PASSWORD_VIOLATION               = new REST_MESSAGE("Violate policy password: {0}",
                                                                                                       nameof(POLICY_PASSWORD_VIOLATION));
        public readonly static REST_MESSAGE ACTION_HAS_BEEN_TAKEN                   = new REST_MESSAGE("Action '{0}' has been taken",
                                                                                                       nameof(ACTION_HAS_BEEN_TAKEN));
        public readonly static REST_MESSAGE INCORRECT_OLD_PASSWORD                   = new REST_MESSAGE("Incorrect old password",
                                                                                                       nameof(INCORRECT_OLD_PASSWORD));
        public readonly static REST_MESSAGE EMAIL_IS_SENDING                        = new REST_MESSAGE("Email is sending",
                                                                                                       nameof(EMAIL_IS_SENDING));
        public readonly static REST_MESSAGE EMAIL_IS_SENT_SUCCESSFULLY              = new REST_MESSAGE("Email is sent successfully",
                                                                                                       nameof(EMAIL_IS_SENT_SUCCESSFULLY));
        public readonly static REST_MESSAGE REQUEST_HAS_EXPIRED                     = new REST_MESSAGE("Request has expired",
                                                                                                       nameof(REQUEST_HAS_EXPIRED));
    }
}
