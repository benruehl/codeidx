using CodeIDX.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.Services
{
    public sealed class ApplicationService
    {

        private static readonly ApplicationViewModel _ApplicationView = new ApplicationViewModel();
        
        public static ApplicationViewModel ApplicationView
        {
            get
            {
                return _ApplicationView;
            }
        }
        
        private ApplicationService()
        {
        }

    }
}
