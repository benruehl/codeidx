using CodeIDX.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeIDX.Services.Lucene
{
    public class LineMatch
    {
        public string Line { get; set; }
        public int LineNumber { get; set; }

        public IEnumerable<HighlightInfo> Highlights { get; set; }

    }
}
