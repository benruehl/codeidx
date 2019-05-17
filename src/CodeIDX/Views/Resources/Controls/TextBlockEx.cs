using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CodeIDX.Views.Resources.Controls
{
    public static class TextBlockEx
    {
        public static Inline GetFormattedText(DependencyObject obj)
        {
            return (Inline)obj.GetValue(FormattedTextProperty);
        }

        public static void SetFormattedText(DependencyObject obj, Inline value)
        {
            obj.SetValue(FormattedTextProperty, value);
        }

        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached(
                "FormattedText",
                typeof(Inline),
                typeof(TextBlockEx),
                new PropertyMetadata(null, OnFormattedTextChanged));

        public static void OnFormattedTextChanged(
            DependencyObject o,
            DependencyPropertyChangedEventArgs e)
        {
            var textBlock = o as TextBlock;
            if (textBlock == null)
                return;

            textBlock.Inlines.Clear();
            var newInline = e.NewValue as Inline;
            if (newInline != null)
                textBlock.Inlines.Add(newInline);
        }
    }
}
