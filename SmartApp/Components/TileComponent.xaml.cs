using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using SmartApp.MVVM.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace SmartApp.Components
{
    public partial class TileComponent : UserControl, INotifyPropertyChanged
    {
        //private readonly RegistryManager _registryManager = RegistryManager.CreateFromConnectionString("HostName=pierre-hub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=yj/6v90HFCyCEFhxC2vQR3GgVFnF4s5p2y5k85FhRGs=");

        public event PropertyChangedEventHandler? PropertyChanged;

        public TileComponent()
        {
            InitializeComponent();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public static readonly DependencyProperty DeviceNameProperty = DependencyProperty.Register("DeviceName", typeof(string), typeof(TileComponent));
        public string DeviceName
        {
            get { return (string)GetValue(DeviceNameProperty); }
            set { SetValue(DeviceNameProperty, value); }
        }

        public static readonly DependencyProperty DeviceTypeProperty = DependencyProperty.Register("DeviceType", typeof(string), typeof(TileComponent));
        public string DeviceType
        {
            get { return (string)GetValue(DeviceTypeProperty); }
            set { SetValue(DeviceTypeProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(TileComponent));
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }


        public static readonly DependencyProperty IconActiveProperty = DependencyProperty.Register("IconActive", typeof(string), typeof(TileComponent));

        public string IconActive
        {
            get { return (string)GetValue(IconActiveProperty); }
            set { SetValue(IconActiveProperty, value); }
        }

        public static readonly DependencyProperty IconInActiveProperty = DependencyProperty.Register("IconInActive", typeof(string), typeof(TileComponent));

        public string IconInActive
        {
            get { return (string)GetValue(IconInActiveProperty); }
            set { SetValue(IconInActiveProperty, value); }
        }

        public static readonly DependencyProperty StateActiveProperty = DependencyProperty.Register("StateActive", typeof(string), typeof(TileComponent));

        public string StateActive
        {
            get { return (string)GetValue(StateActiveProperty); }
            set { SetValue(StateActiveProperty, value); }
        }

        public static readonly DependencyProperty StateInActiveProperty = DependencyProperty.Register("StateInActive", typeof(string), typeof(TileComponent));

        public string StateInActive
        {
            get { return (string)GetValue(StateInActiveProperty); }
            set { SetValue(StateInActiveProperty, value); }
        }


        private bool _deviceState;

        public bool DeviceState
        {
            get { return _deviceState; }
            set
            {
                _deviceState = value;
                OnPropertyChanged();
            }
        }

        private async void onOffDirectMethod_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as CheckBox;
                var deviceItem = (DeviceItem)button!.DataContext;

                deviceItem.DeviceState = !deviceItem.DeviceState;

                using ServiceClient serviceClient = ServiceClient.CreateFromConnectionString("HostName=pierre-hub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=yj/6v90HFCyCEFhxC2vQR3GgVFnF4s5p2y5k85FhRGs=");

                var directMethod = new CloudToDeviceMethod("OnOff");
                directMethod.SetPayloadJson(JsonConvert.SerializeObject(new { deviceState = !deviceItem.DeviceState }));
                var result = await serviceClient.InvokeDeviceMethodAsync(deviceItem.DeviceId, directMethod);

                var data = JsonConvert.DeserializeObject<dynamic>(result.GetPayloadAsJson());
                IsChecked = deviceItem.DeviceState;
            }
            catch { }
        }
    }
}
