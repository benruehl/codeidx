using CodeIDX.Services.Lucene;
using CodeIDX.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.ViewModels
{
    public class SearchResultViewModel : ViewModel<SearchViewModel>
    {
        /// <summary>
        /// True, if the current line is the first match of matching lines in this file.
        /// </summary>
        public bool IsFirstOfFile { get; private set; }
        public string Directory { get; private set; }
        public string Filename { get; private set; }
        public string Extension { get; private set; }
        public string MatchingLine { get; private set; }
        public int LineNumber { get; private set; }
        public IEnumerable<HighlightInfo> LineHighlights { get; private set; }

        public SearchResultViewModel(bool isFirstOfFile, string directory, string filename, string extension, int lineNumber, string matchingLine, IEnumerable<HighlightInfo> lineHighlights)
        {
            IsFirstOfFile = isFirstOfFile;
            Directory = directory;
            Filename = filename;
            Extension = extension;
            LineNumber = lineNumber;
            MatchingLine = matchingLine;
            LineHighlights = lineHighlights;
        }

        internal string GetFilePath()
        {
            return Path.Combine(Directory, Filename + Extension);
        }
    }
}
