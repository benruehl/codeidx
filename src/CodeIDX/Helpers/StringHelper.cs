using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.Helpers
{
    public static class StringHelper
    {

        public static bool IsInteger(string text)
        {
            int result;
            return (int.TryParse(text, out result));
        }

        public static string Trim(string text, string trimText)
        {
            if (text.StartsWith(trimText))
                text = text.Substring(trimText.Length);
            if (text.EndsWith(trimText))
                text = text.Substring(0, text.Length - trimText.Length);

            return text;
        }

    }
}
