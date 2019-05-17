using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeIDX.Helpers
{
    public static class RegexHelper
    {

        public static bool IsValidRegex(string expression)
        {
            try
            {
                new Regex(expression);
                return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// taken from http://www.codeproject.com/Articles/11556/Converting-Wildcards-to-Regexes
        /// </summary>
        public static string WildcardToRegex(string pattern)
        {
            string regex = Regex.Escape(pattern)
                                .Replace(@"\*", ".*")
                                .Replace(@"\?", ".");

            if (pattern.StartsWith("\"") && regex.EndsWith("\""))
                return "^" + regex.Trim('\"') + "$";
            else
                return regex;
        }
    }
}
