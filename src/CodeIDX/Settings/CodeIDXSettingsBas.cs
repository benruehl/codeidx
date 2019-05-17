using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.Settings
{
    public class CodeIDXSettingsBase : ApplicationSettingsBase
    {

        private bool _SettingsChanged = false;

        protected void SetValue(string settingKey, object value)
        {
            if (this[settingKey] != value)
            {
                this[settingKey] = value;
                _SettingsChanged = true;
            }
        }

        protected override void OnSettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_SettingsChanged)
            {
                e.Cancel = true;
            }
            else
            {
                _SettingsChanged = false;
                base.OnSettingsSaving(sender, e);
            }
        }

    }
}
