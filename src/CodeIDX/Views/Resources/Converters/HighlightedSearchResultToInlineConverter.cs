using CodeIDX.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace CodeIDX.Views.Resources.Converters
{
    /// <summary>
    /// TODO refactor
    /// </summary>
    public class HighlightedSearchResultToInlineConverter : IValueConverter
    {

        public static HighlightedSearchResultToInlineConverter Instance = new HighlightedSearchResultToInlineConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SearchResultViewModel searchResult = value as SearchResultViewModel;
            if (searchResult == null)
                return null;

            Span resultSpan = new Span();

            string line = searchResult.MatchingLine;
            int lastHighlightEndIndex = 0;
            foreach (var highlight in searchResult.LineHighlights.OrderBy(cur => cur.StartIndex))
            {
                resultSpan.Inlines.Add(new Run(line.Substring(lastHighlightEndIndex, highlight.StartIndex - lastHighlightEndIndex)));

                if (highlight is WildcardHighlightInfo)
                {
                    int lastPartHighlightEndIndex = highlight.StartIndex;
                    foreach (var partHighlight in ((WildcardHighlightInfo)highlight).PartHighlights)
                    {
                        resultSpan.Inlines.Add(new Run(line.Substring(lastPartHighlightEndIndex, partHighlight.StartIndex - lastPartHighlightEndIndex))
                        {
                            Background = Constants.HighlightBrush
                        });

                        //add highlighted part text
                        resultSpan.Inlines.Add(new Run(line.Substring(partHighlight.StartIndex, partHighlight.Length))
                        {
                            Background = Constants.SubHighlightBrush
                        });

                        lastPartHighlightEndIndex = partHighlight.EndIndex;
                    }

                    //add remaining highlighted text
                    resultSpan.Inlines.Add(new Run(line.Substring(lastPartHighlightEndIndex, highlight.EndIndex - lastPartHighlightEndIndex))
                    {
                        Background = Constants.HighlightBrush
                    });
                }
                else
                {
                    //add highlighted text
                    resultSpan.Inlines.Add(new Run(line.Substring(highlight.StartIndex, highlight.Length))
                    {
                        Background = Constants.HighlightBrush
                    });
                }

                lastHighlightEndIndex = highlight.EndIndex;
            }

            //add remaining normal text
            resultSpan.Inlines.Add(new Run(line.Substring(lastHighlightEndIndex)));

            return resultSpan;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
