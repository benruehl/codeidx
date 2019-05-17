using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.Settings
{
    public sealed class BlacklistSettings : ApplicationSettingsBase
    {

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public List<string> BlacklistDirectories
        {
            get
            {
                return (List<string>)(this["BlacklistDirectories"]);
            }
            set
            {
                this["BlacklistDirectories"] = value;
            }
        }

    }
}
