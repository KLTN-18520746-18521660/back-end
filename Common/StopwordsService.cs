using System.Collections.Generic;
using System.Collections;
using System.Collections.Immutable;

namespace Common
{
    public static class StopwordsService
    {
        public static string PATH_FILE          = "vietnamese-stopwords.txt";
        public static List<string> STOP_WORDS   { get; private set; }
        public static void init() {
            STOP_WORDS = new List<string>();
        }
    }
}