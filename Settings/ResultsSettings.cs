using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.Settings
{
    public sealed class ResultsSettings : CodeIDXSettingsBase
    {

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool FilterFileOnEnter
        {
            get
            {
                return (bool)(this["FilterFileOnEnter"]);
            }
            set
            {
                SetValue("FilterFileOnEnter", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool EnableEditMatchOnDoubleClick
        {
            get
            {
                return (bool)(this["EnableEditMatchOnDoubleClick"]);
            }
            set
            {
                SetValue("EnableEditMatchOnDoubleClick", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool SelectMatchInPreview
        {
            get
            {
                return (bool)(this["SelectMatchInPreview"]);
            }
            set
            {
                SetValue("SelectMatchInPreview", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UseVisualStudioAsDefault
        {
            get
            {
                return (bool)(this["UseVisualStudioAsDefault"]);
            }
            set
            {
                SetValue("UseVisualStudioAsDefault", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UseNotepadAsDefault
        {
            get
            {
                return (bool)(this["UseNotepadAsDefault"]);
            }
            set
            {
                SetValue("UseNotepadAsDefault", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UseCustomEditorAsDefault
        {
            get
            {
                return (bool)(this["UseCustomEditorAsDefault"]);
            }
            set
            {
                SetValue("UseCustomEditorAsDefault", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UseDefaultEditorAsDefault
        {
            get
            {
                return (bool)(this["UseDefaultEditorAsDefault"]);
            }
            set
            {
                SetValue("UseDefaultEditorAsDefault", value);
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string DefaultEditorCommandLineOptions
        {
            get
            {
                return (string)(this["DefaultEditorCommandLineOptions"]);
            }
            set
            {
                SetValue("DefaultEditorCommandLineOptions", value);
            }
        }

    }
}
