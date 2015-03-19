using Lucene.Net.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileIndexer.LuceneServices
{
    public class SourceCodeAnalyzer : Analyzer
    {

        public override TokenStream TokenStream(string fieldName, System.IO.TextReader reader)
        {
            return new SourceCodeTokenizer(reader);
        }

        public override TokenStream ReusableTokenStream(string fieldName, System.IO.TextReader reader)
        {
            Tokenizer value = (Tokenizer)PreviousTokenStream;
            if (value == null)
            {
                value = new SourceCodeTokenizer(reader);
                PreviousTokenStream = value;
            }
            else
                value.Reset(reader);

            return value;

        }
    }
}
