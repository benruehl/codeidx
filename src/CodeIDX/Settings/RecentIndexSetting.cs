using CodeIDX.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CodeIDX.Settings
{
    
    public class RecentIndexSetting
    {

        public string Name { get; set; }
        public string IndexFile { get; set; }

        public List<SearchTabSettings> SearchTabs { get; set; }
    }
}
