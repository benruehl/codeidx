using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeIDX.ViewModels.Options
{
    public class ResultOptionsViewModel : ViewModel
    {

        public bool SelectMatchInPreview { get; set; }
        public bool UseVisualStudioAsDefault { get; set; }
        public bool UseNotepadAsDefault { get; set; }
        public bool UseCustomEditorAsDefault { get; set; }
        public bool UseDefaultEditorAsDefault { get; set; }
        public string CustomDefaultEditorPath { get; set; }
        public string DefaultEditorCommandLineOptions { get; set; }
        public bool EnableEditMatchOnDoubleClick { get; set; }
        public bool FilterFileOnEnter { get; set; }

    }
}
