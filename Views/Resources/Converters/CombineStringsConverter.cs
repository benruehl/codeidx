using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CodeIDX.Views.Resources.Converters
{
    public class CombineStringsConverter : IMultiValueConverter
    {

        public static CombineStringsConverter Instance = new CombineStringsConverter();

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var stringValues = values.OfType<string>();

            string result = string.Empty;
            foreach (var item in stringValues)
                result += item;

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
