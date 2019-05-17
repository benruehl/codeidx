using CodeIDX.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CodeIDX.Settings
{
    public sealed class CodeIDXSettings : CodeIDXSettingsBase
    {

        public static CodeIDXSettings Default { get; private set; }
        public static GeneralSettings General { get; private set; }
        public static IndexSettings Index { get; private set; }
        public static SearchSettings Search { get; private set; }
        public static ResultsSettings Results { get; private set; }
        public static UserInterfaceSettings UserInterface { get; private set; }
        public static BlacklistSettings Blacklist { get; private set; }

        static CodeIDXSettings()
        {
            Default = new CodeIDXSettings();
            General = new GeneralSettings();
            Index = new IndexSettings();
            Search = new SearchSettings();
            Results = new ResultsSettings();
            UserInterface = new UserInterfaceSettings();
            Blacklist = new BlacklistSettings();
        }

        public static void SaveAll()
        {
            Default.Save();
            General.Save();
            Index.Save();
            Search.Save();
            Results.Save();
            UserInterface.Save();
            Blacklist.Save();
        }

        internal static void UpgradeAll()
        {
            if (!Default.UpgradeSettings)
                return;

            Default.Upgrade();
            General.Upgrade();
            Index.Upgrade();
            Search.Upgrade();
            Results.Upgrade();
            UserInterface.Upgrade();
            Blacklist.Upgrade();

            Default.UpgradeSettings = false;
            Default.Save();
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UpgradeSettings
        {
            get
            {
                return (bool)(this["UpgradeSettings"]);
            }
            set
            {
                SetValue("UpgradeSettings", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public double LastPreviewHeight
        {
            get
            {
                return (double)(this["LastPreviewHeight"]);
            }
            set
            {
                SetValue("LastPreviewHeight", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsBlacklistEnabled
        {
            get
            {
                return (bool)(this["IsBlacklistEnabled"]);
            }
            set
            {
                SetValue("IsBlacklistEnabled", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsEditEnabled
        {
            get
            {
                return (bool)(this["IsEditEnabled"]);
            }
            set
            {
                SetValue("IsEditEnabled", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public List<RecentIndexSetting> RecentIndices
        {
            get
            {
                return ((List<RecentIndexSetting>)(this["RecentIndices"]));
            }
            set
            {
                SetValue("RecentIndices", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public List<string> SearchHistory
        {
            get
            {
                return ((List<string>)(this["SearchHistory"]));
            }
            set
            {
                SetValue("SearchHistory", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public Point? WindowLocation
        {
            get
            {
                return (Point?)(this["WindowLocation"]);
            }
            set
            {
                if (value.HasValue && (value.Value.X < 0 || value.Value.Y < 0))
                    return;

                SetValue("WindowLocation", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public Size WindowSize
        {
            get
            {
                return (Size)(this["WindowSize"]);
            }
            set
            {
                if (value.Width < 0 || value.Height < 0)
                    return;

                SetValue("WindowSize", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsWindowMaximized
        {
            get
            {
                return (bool)(this["IsWindowMaximized"]);
            }
            set
            {
                SetValue("IsWindowMaximized", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool IsPreviewVisible
        {
            get
            {
                return (bool)(this["IsPreviewVisible"]);
            }
            set
            {
                SetValue("IsPreviewVisible", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastIndexLocationPath
        {
            get
            {
                return (string)(this["LastIndexLocationPath"]);
            }
            set
            {
                SetValue("LastIndexLocationPath", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastIndexSourcePath
        {
            get
            {
                return (string)(this["LastIndexSourcePath"]);
            }
            set
            {
                SetValue("LastIndexSourcePath", value);
            }
        }
    }
}
