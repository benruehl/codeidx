using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CodeIDX.Views.Resources.Converters
{
    public class ListToStringConverter : IValueConverter, IMultiValueConverter
    {

        public static ListToStringConverter Instance = new ListToStringConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConvertInternal(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<string> lineList = new List<string>();

            string text = value as string;
            if (string.IsNullOrEmpty(text))
                return lineList;

            var lines = text.Split('\n');
            foreach (var curLine in lines)
                lineList.Add(curLine.Trim());

            return lineList;
        }

        private static object ConvertInternal(object value, object parameter)
        {
            var list = value as IEnumerable<string>;
            if (list == null || list.Count() == 0)
                return null;

            string separator = "\n";
            string stringParameter = parameter as string;
            if (stringParameter != null && !string.IsNullOrEmpty(stringParameter))
                separator = stringParameter;

            return list.Aggregate((sum, next) => sum + separator + next);
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ConvertInternal(values.FirstOrDefault(), parameter);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
