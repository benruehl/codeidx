using CodeIDX.Helpers;
using CodeIDX.Services;
using CodeIDX.Settings;
using CodeIDX.ViewModels;
using CodeIDX.Views;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Remotion.Linq.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Xceed.Wpf.DataGrid;

namespace CodeIDX.Views.Resources.Controls
{
    /// <summary>
    /// taken from http://stackoverflow.com/questions/9223674/highlight-all-occurrences-of-selected-word-in-avalonedit
    /// </summary>
    public class ColorizeAvalonEdit : DocumentColorizingTransformer
    {

        private SearchViewModel CurrentSearch
        {
            get { return ApplicationService.ApplicationView.CurrentSearch; }
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (CurrentSearch == null)
                return;

            string lineText = CurrentContext.Document.GetText(line);
            var searchResult = CurrentSearch.SearchResults.FirstOrDefault(cur => cur.LineNumber == line.LineNumber && lineText == cur.MatchingLine);
            if (searchResult == null)
                return;

            //highlight whole line
            if (CurrentSearch.SelectedResult != null && line.LineNumber == CurrentSearch.SelectedResult.LineNumber)
                ChangeLinePart(line.Offset, line.EndOffset, cur => cur.TextRunProperties.SetBackgroundBrush(new SolidColorBrush(Color.FromArgb(0x7E, 0xAD, 0xD8, 0xE6))));

            //highlight words line
            int lineStartOffset = line.Offset;
            foreach (var highlight in searchResult.LineHighlights)
            {
                ChangeLinePart(
                    lineStartOffset + highlight.StartIndex,
                    lineStartOffset + highlight.EndIndex,
                    (VisualLineElement element) =>
                    {
                        element.TextRunProperties.SetBackgroundBrush(Constants.HighlightBrush);
                    });

                if (highlight is WildcardHighlightInfo)
                {
                    //highlight wildcard parts
                    foreach (var partHighlight in ((WildcardHighlightInfo)highlight).PartHighlights)
                    {
                        ChangeLinePart(
                            lineStartOffset + partHighlight.StartIndex,
                            lineStartOffset + partHighlight.EndIndex,
                            (VisualLineElement element) =>
                            {
                                element.TextRunProperties.SetBackgroundBrush(Constants.SubHighlightBrush);
                            });
                    }
                }
            }
        }
    }
}
