using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeIDX.ViewModels.Options
{
    public class UIOptionsViewModel : ViewModel
    {
        private bool _ShowResultFileCount;
        public bool ShowResultFileCount
        {
            get
            {
                return _ShowResultFileCount;
            }
            set
            {
                if (_ShowResultFileCount != value)
                {
                    _ShowResultFileCount = value;
                    FirePropertyChanged("ShowResultFileCount");
                }
            }
        }

        private bool _LoadLastSearches;
        public bool LoadLastSearches
        {
            get
            {
                return _LoadLastSearches;
            }
            set
            {
                if (_LoadLastSearches != value)
                {
                    _LoadLastSearches = value;
                    FirePropertyChanged("LoadLastSearches");
                }
            }
        }
    }
}
