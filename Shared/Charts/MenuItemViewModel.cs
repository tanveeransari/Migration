using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Shared.Charts
{
    public class MenuItemViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }
        public ObservableCollection<MenuItemViewModel> Children { get; set; }
        public ICommand MenuCommand { get; set; }
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }

            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        public MenuItemViewModel(string name)
        {
            this.Name = name;
        }

        public MenuItemViewModel(string name, ObservableCollection<MenuItemViewModel> pChildren)
        {
            this.Name = name;
            this.Children = pChildren;
        }

        public MenuItemViewModel(string name, ObservableCollection<MenuItemViewModel> pChildren, ICommand pCommand, bool pIsChecked)
        {
            this.Name = name;
            this.Children = pChildren;
            this.MenuCommand = pCommand;
            this.IsChecked = pIsChecked;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
