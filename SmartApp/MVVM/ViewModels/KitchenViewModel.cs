using SmartApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApp.MVVM.ViewModels
{
    internal class KitchenViewModel
    {
        private ObservableCollection<DeviceItem> _deviceItems;
        public KitchenViewModel()
        {
            _deviceItems = new ObservableCollection<DeviceItem>();
        }
        public string Title { get; set; } = "Kitchen";
        public string Temperature { get; set; } = "23 °C";
        public string Humidity { get; set; } = "34 %";
        public IEnumerable<DeviceItem> DeviceItems => _deviceItems;
    }
}
