using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CodeIDX
{
    public static class Constants
    {

        public const string EnvDTEConstantVsViewKindTextView = "{7651A703-06E5-11D1-8EBD-00A0C90F26EA}";

        public static readonly Brush HighlightBrush = new SolidColorBrush(Color.FromArgb(0x7E, 0xF0, 0x9E, 0x00));
        public static readonly Brush SubHighlightBrush = new SolidColorBrush(Color.FromArgb(0x7E, 0x7B, 0xC9, 0x00));

        public const string IndexDirectoryNameSuffix = "IDX";
        public const string IndexFileExtension = ".fi";

        public static class IndexFields
        {
            public const string Content = "Content";
            public const string ContentCaseInsensitive = "ContentCaseInsensitive";
            public const string Directory = "Directory";
            public const string AnalizedDirectory = "AnalizedDirectory";
            public const string Filename = "Filename";
            public const string Extension = "Extension";
            public const string Hash = "Hash";
            public const string LastModified = "LastModified";
        }

    }
}
