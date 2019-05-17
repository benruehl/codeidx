using CodeIDX.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CodeIDX.Resources.Selectors
{
    public class FileFilterItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SelectedTemplate { get; set; }
        public DataTemplate DropDownTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var comboBox = TreeHelper.FindVisualAncestor<ComboBox>(container);
            if (comboBox == null)
                return DropDownTemplate;

            return SelectedTemplate;
        }
    }
}
