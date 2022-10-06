using SmartApp.MVVM.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApp.MVVM.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        public RelayCommand KitchenViewCommand { get; set; }
        public KitchenViewModel KitchenViewModel { get; set; }
        private object _currentView;

        public MainViewModel()
        {
            KitchenViewModel = new KitchenViewModel();
            KitchenViewCommand = new RelayCommand(x => { CurrentView = KitchenViewModel; });
        }
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }
    }
}
