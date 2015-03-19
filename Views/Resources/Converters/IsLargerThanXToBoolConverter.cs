using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CodeIDX.Views.Resources.Converters
{
    public class IsLargerThanXToBoolConverter :IValueConverter
    {

        public static IsLargerThanXToBoolConverter Instance = new IsLargerThanXToBoolConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var intValue = System.Convert.ToInt32(value);
                var intParameter = System.Convert.ToInt32(parameter);

                return intValue > intParameter;
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
