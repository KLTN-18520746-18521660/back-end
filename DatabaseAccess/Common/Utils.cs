using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess.Common
{
    public class Utils
    {
        #region Session
        public static readonly int SeesionTokenLength = 30;
        public static readonly string SessionTokenRegex = "^[a-z-0-9]{30}$";
        public static string RandomString(int StringLen)
        {
            string possibleChar = "abcdefghijklmnopqrstuvwxyz0123456789";
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            StringBuilder output = new StringBuilder("");
            for (int i = 0; i < StringLen; i++) {
                int pos = rand.Next(0, possibleChar.Length);
                output.Append(possibleChar[pos].ToString());
            }
            return output.ToString();
        }
        public static string GenerateSessionToken()
        {
            return RandomString(SeesionTokenLength);
        }
        #endregion
        #region Slug
        public static string GenerateSlug(string Original)
        {
            return "";
        }
        // [TODO] -- wait from fe
        public static bool ValidateSlug(string Slug, string Original)
        {
            return true;
        }
        #endregion
        #region Post
        public static string TakeContentForSearchFromRawContent(string RawContent)
        {
            return "";
        }
        #endregion
    }
}
