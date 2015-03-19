using Lucene.Net.Index;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.Helpers
{
    public static class LuceneHelper
    {
        private const string CodePunctuationChars = "[({!$%&?})]`^~#-.;,:/\\<>|=-+\"\'@/*";

        public static bool IsPunctuation(char character)
        {
            return CodePunctuationChars.Contains(character);
        }

        /// <summary>
        /// Surrounds all punctuation characters with whitespaces.
        /// </summary>
        public static string ExpandTokenBreak(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            foreach (char punctuation in CodePunctuationChars)
                text = text.Replace(punctuation.ToString(), string.Format(" {0} ", punctuation));

            return text;
        }

        /// <summary>
        /// A token break charactor is anything other than a letter, number or whitespace.
        /// </summary>
        public static bool IsValidTokenBreakCharactor(char character)
        {
            if (character == default(char) || character == ' ')
                return true;

            return !char.IsLetter(character) && !char.IsNumber(character) && character != '_';
        }

        public static bool IsValidIndexDirectory(string indexPath)
        {
            if (string.IsNullOrEmpty(indexPath))
                return false;

            if (!System.IO.Directory.Exists(indexPath))
                return false;

            return IndexReader.IndexExists(FSDirectory.Open(indexPath));
        }

    }
}
