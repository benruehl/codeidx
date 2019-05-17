using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.Settings
{
    public sealed class IndexSettings : CodeIDXSettingsBase
    {

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DisableOptimizeIndex
        {
            get
            {
                return (bool)(this["DisableOptimizeIndex"]);
            }
            set
            {
                SetValue("DisableOptimizeIndex", value);
            }
        }

    }
}
