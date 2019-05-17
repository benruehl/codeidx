using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.Settings
{
    public sealed class UserInterfaceSettings : CodeIDXSettingsBase
    {

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ShowResultFileCount
        {
            get
            {
                return (bool)(this["ShowResultFileCount"]);
            }
            set
            {
                SetValue("ShowResultFileCount", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool LoadLastSearches
        {
            get
            {
                return (bool)(this["LoadLastSearches"]);
            }
            set
            {
                SetValue("LoadLastSearches", value);
            }
        }

    }
}
