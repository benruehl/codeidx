using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<ViewModel> _Children;
        public ReadOnlyObservableCollection<ViewModel> Children { get; private set; }

        private ViewModel _Parent;
        public ViewModel Parent
        {
            get
            {
                return _Parent;
            }
            protected set
            {
                if (_Parent != value)
                {
                    //remove from old parent
                    if (_Parent != null)
                        _Parent.RemoveChild(this);

                    //add to new parent
                    _Parent = value;
                    if (_Parent != null)
                        _Parent.AddChild(value);

                    FirePropertyChanged("Parent");
                }
            }
        }

        public ViewModel()
        {
            _Children = new ObservableCollection<ViewModel>();
            Children = new ReadOnlyObservableCollection<ViewModel>(_Children);
        }

        public void RemoveChild(ViewModel child)
        {
            if (child.Parent == this)
                child.Parent = null;

            _Children.Remove(child);
        }

        public void AddChild(ViewModel viewModel)
        {
            if (viewModel == null)
                return;

            if (viewModel.Parent != this)
                viewModel.Parent = this;

            if (!Children.Contains(viewModel))
                _Children.Add(viewModel);
        }

        protected void FirePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }

    public class ViewModel<ParentType> : ViewModel
        where ParentType : ViewModel
    {
        public new ParentType Parent
        {
            get
            {
                return base.Parent as ParentType;
            }
            set
            {
                base.Parent = value;
            }
        }

    }
}
