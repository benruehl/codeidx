using CodeIDX.ViewModels.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CodeIDX.Views.Resources.Converters
{
    public class ItemTypeEqualsTypeConverter : IMultiValueConverter
    {

        public static ItemTypeEqualsTypeConverter Instance = new ItemTypeEqualsTypeConverter();

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object item = values.FirstOrDefault();
            Type typeMatch = values.ElementAtOrDefault(1) as Type;

            if (item == null && typeMatch == null)
                return false;

            //GeneralOptions as default
            if (typeMatch == null && item is GeneralOptionsViewModel)
                return true;

            return item.GetType() == typeMatch;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
