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
        public string GetMessage(params string[] Params)
        {
            var Ret = TEMPLATE_MESSAGE;
            for (int Index = 0; Index < Params.Length; Index++) {
                var Key = "{" + Index.ToString() + "}";
                Ret.Replace(Key, Params[Index]);
            }
            return Ret;
        }
    }
    public static class RESPONSE_MESSAGES
    {
        public static REST_MESSAGE INVALID_REST_MESSAGE                             = new REST_MESSAGE("Invalid message");
        public static REST_MESSAGE INTERNAL_SERVER_ERROR                            = new REST_MESSAGE("Internal Server Error");
        public static REST_MESSAGE INVALID_REQUEST_BODY                             = new REST_MESSAGE("Invalid request body");
        public static REST_MESSAGE NOT_FOUND_USER                                   = new REST_MESSAGE("Not found any user");
    }
}
