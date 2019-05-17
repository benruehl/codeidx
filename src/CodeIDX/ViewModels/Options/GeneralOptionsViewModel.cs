using CodeIDX.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeIDX.ViewModels.Options
{
    public class GeneralOptionsViewModel : ViewModel
    {

        public bool ShowTrayIcon { get; set; }
        public bool StartOnSystemStartup { get; set; }
        public bool MinimizeToTray { get; set; }
        public bool ExitToTray { get; set; }
        public bool SingleClickTray { get; set; }
        public bool LoadLastIndexOnStartup { get; set; }
        public RefreshAtStartupKind RefreshIndexAtStartup { get; set; }
    }
}
