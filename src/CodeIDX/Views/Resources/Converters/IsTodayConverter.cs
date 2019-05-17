using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CodeIDX.Views.Resources.Converters
{
    public class IsTodayConverter : IValueConverter
    {

        public static IsTodayConverter Instance = new IsTodayConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is DateTime))
                return false;

            return ((DateTime)value).ToShortDateString() == DateTime.Now.ToShortDateString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
