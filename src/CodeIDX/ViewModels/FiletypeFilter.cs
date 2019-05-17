using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeIDX.ViewModels
{
    public class FiletypeFilter : ViewModel
    {

        public string Extension
        {
            get
            {
                return Filter.TrimStart('*');
            }
        }

        public string Filter { get; set; }
        private bool _IsActive;
        public bool IsActive
        {
            get
            {
                return _IsActive;
            }
            set
            {
                if (_IsActive != value)
                {
                    _IsActive = value;
                    FirePropertyChanged("IsActive");
                }
            }
        }

    }
}
