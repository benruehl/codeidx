using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeIDX.ViewModels
{
    public class HighlightInfo
    {

        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public int Length
        {
            get
            {
                return EndIndex - StartIndex;
            }
        }

        public HighlightInfo()
        { }

        public HighlightInfo(int startIndex, int length)
        {
            StartIndex = startIndex;
            EndIndex = startIndex + length;
        }

    }
}
