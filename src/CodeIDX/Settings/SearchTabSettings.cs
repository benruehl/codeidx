using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeIDX.Settings
{
    public class SearchTabSettings
    {

        public string SearchText { get; set; }
        public bool MatchCase { get; set; }
        public bool EnableWildcards { get; set; }
        public bool MatchWholeWord { get; set; }
        public List<string> FileFilters { get; set; }

    }
}
