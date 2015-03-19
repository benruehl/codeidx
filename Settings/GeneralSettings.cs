using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.Settings
{
    public sealed class GeneralSettings : CodeIDXSettingsBase
    {

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Never")]
        public RefreshAtStartupKind RefreshIndexAtStartup
        {
            get
            {
                return (RefreshAtStartupKind)(this["RefreshIndexAtStartup"]);
            }
            set
            {
                SetValue("RefreshIndexAtStartup", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ShowTrayIcon
        {
            get
            {
                return (bool)(this["ShowTrayIcon"]);
            }
            set
            {
                SetValue("ShowTrayIcon", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ExitToTray
        {
            get
            {
                return (bool)(this["ExitToTray"]);
            }
            set
            {
                SetValue("ExitToTray", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool MinimizeToTray
        {
            get
            {
                return (bool)(this["MinimizeToTray"]);
            }
            set
            {
                SetValue("MinimizeToTray", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool SingleClickTray
        {
            get
            {
                return (bool)(this["SingleClickTray"]);
            }
            set
            {
                SetValue("SingleClickTray", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool StartOnSystemStartup
        {
            get
            {
                return (bool)(this["StartOnSystemStartup"]);
            }
            set
            {
                SetValue("StartOnSystemStartup", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool LoadLastIndexOnStartup
        {
            get
            {
                return (bool)(this["LoadLastIndexOnStartup"]);
            }
            set
            {
                SetValue("LoadLastIndexOnStartup", value);
            }
        }


    }
}
