using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Text.RegularExpressions;

namespace CoreApi.Common
{
    public class Utils
    {
        public static string EmailRegex = "^(([^<>()[\\]\\\\.,;:\\s@\\\"]+(\\.[^<>()[\\]\\\\.,;:\\s@\\\"]+)*)|(\\\".+\\\"))@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\])|(([a-zA-Z\\-0-9]+\\.)+[a-zA-Z]{2,}))$";
        public static bool isEmail(string Input)
        {
            return Regex.IsMatch(Input, EmailRegex);
        }
        public static bool IsValidSessionToken(string session_token)
        {
            if (session_token.Length != DatabaseAccess.Common.Utils.SeesionTokenLength ||
                !Regex.IsMatch(session_token, DatabaseAccess.Common.Utils.SessionTokenRegex)) {
                return false;
            }
            return true;
        }
    }
}