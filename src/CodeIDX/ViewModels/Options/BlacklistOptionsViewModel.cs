using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeIDX.ViewModels.Options
{
    public class BlacklistOptionsViewModel : ViewModel
    {
        private List<string> _Directories;
        public List<string> Directories
        {
            get
            {
                return _Directories;
            }
            set
            {
                if (_Directories != value)
                {
                    _Directories = value;
                    FirePropertyChanged("Directories");
                }
            }
        }

        public BlacklistOptionsViewModel()
        {
            Directories = new List<string>();
        }

        internal void AddDirectory(string directoryPath)
        {
            if (!Directories.Contains(directoryPath))
            {
                Directories.Add(directoryPath);
                FirePropertyChanged("Directories");
            }
        }

    }
}
