using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeIDX.ViewModels.Commands
{
    public abstract class ViewModelCommand<ContextViewModelType>
        where ContextViewModelType : class
    {

        public void ExecuteHandler(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            Execute(e.Parameter as ContextViewModelType);
        }

        public void CanExecuteHandler(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CanExecute(e.Parameter as ContextViewModelType);
        }

        protected abstract void Execute(ContextViewModelType contextViewModel);

        protected virtual bool CanExecute(ContextViewModelType contextViewModel)
        {
            return contextViewModel != null;
        }

    }
}
