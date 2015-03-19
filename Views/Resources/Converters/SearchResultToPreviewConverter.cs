using CodeIDX.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CodeIDX.Views.Resources.Converters
{
    public class SearchResultToPreviewConverter : IValueConverter
    {
        public static SearchResultToPreviewConverter Instance = new SearchResultToPreviewConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var searchResult = value as SearchResultViewModel;
            if (searchResult == null)
                return null;

            string file = searchResult.GetFilePath();
            if (!File.Exists(file))
                return null;

            using (FileStream fs = new FileStream(file, FileMode.Open))
            using (StreamReader reader = new StreamReader(fs))
            {
                return reader.ReadToEnd();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
