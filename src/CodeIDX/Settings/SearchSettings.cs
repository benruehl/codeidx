using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.Settings
{
    public sealed class SearchSettings : CodeIDXSettingsBase
    {
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool EnableFileFilter
        {
            get
            {
                return (bool)(this["EnableFileFilter"]);
            }
            set
            {
                SetValue("EnableFileFilter", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool EnableDirectoryFilter
        {
            get
            {
                return (bool)(this["EnableDirectoryFilter"]);
            }
            set
            {
                SetValue("EnableDirectoryFilter", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool EnableSearchInResults
        {
            get
            {
                return (bool)(this["EnableSearchInResults"]);
            }
            set
            {
                SetValue("EnableSearchInResults", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool EnableFilterByDefault
        {
            get
            {
                return (bool)(this["EnableFilterByDefault"]);
            }
            set
            {
                SetValue("EnableFilterByDefault", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("500")]
        public int PageSize
        {
            get
            {
                return (int)(this["PageSize"]);
            }
            set
            {
                SetValue("PageSize", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool LoadRemainingLazyResults
        {
            get
            {
                return (bool)(this["LoadRemainingLazyResults"]);
            }
            set
            {
                SetValue("LoadRemainingLazyResults", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool InsertTextFromClipBoard
        {
            get
            {
                return (bool)(this["InsertTextFromClipBoard"]);
            }
            set
            {
                SetValue("InsertTextFromClipBoard", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool EnableSearchHistory
        {
            get
            {
                return (bool)(this["EnableSearchHistory"]);
            }
            set
            {
                SetValue("EnableSearchHistory", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DelToRemoveFileFromResults
        {
            get
            {
                return (bool)(this["DelToRemoveFileFromResults"]);
            }
            set
            {
                SetValue("DelToRemoveFileFromResults", value);
            }
        }

    }
}
