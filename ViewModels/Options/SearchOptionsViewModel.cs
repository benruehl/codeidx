using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeIDX.ViewModels.Options
{
    public class SearchOptionsViewModel : ViewModel
    {
    
        public bool EnableFilterByDefault { get; set; }
        public bool InsertTextFromClipBoard { get; set; }
        public bool EnableSearchHistory { get; set; }
        public bool EnableSearchInResults { get; set; }
        public bool EnableDirectoryFilter { get; set; }
        public bool EnableFileFilter { get; set; }

    }
}
