using FileIndexer.ViewModels;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Search.Spans;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FileIndexer.LuceneServices
{
    public class SourceCodeTokenizer : WhitespaceTokenizer
    {

        private const string _CodePunctuations = "[({!$%&?})]`^~#-_.;,:/\\<>|=-+\"'@";

        public SourceCodeTokenizer(TextReader reader)
            : base(reader)
        {
        }

        protected override bool IsTokenChar(char c)
        {
            return base.IsTokenChar(c) || !IsPunctuation(c);
        }

        private bool IsPunctuation(char c)
        {
            return Char.IsPunctuation(c) ||
                _CodePunctuations.Contains(c);
        }
    }
}
