using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace CoreApi.Common
{
    public class Utils
    {
        // Email Format: {64}@{255} ----------- RFC 3696 - Session 3
        // Total length: 320
        public static string EmailRegex = "^[a-z0-9_\\.]{1,64}@[a-z]+\\.[a-z]{2,3}$";
        public static bool IsEmail(string Input)
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

        public static bool GetIpAddress(out string Ip)
        {
            Ip = "";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    Ip = ip.ToString();
                    return true;
                }
            }
            return false;
        }
    }
}