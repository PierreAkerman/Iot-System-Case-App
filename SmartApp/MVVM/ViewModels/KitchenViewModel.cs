using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using SmartApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using SmartApp.MVVM.Cores;

namespace SmartApp.MVVM.ViewModels
{
    internal class KitchenViewModel : ObservableObject
    {
        private DispatcherTimer _timer;
        private ObservableCollection<DeviceItem> _deviceItems;
        private List<DeviceItem> _tempList;
        private readonly RegistryManager _registryManager = RegistryManager.CreateFromConnectionString("HostName=pierre-hub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=yj/6v90HFCyCEFhxC2vQR3GgVFnF4s5p2y5k85FhRGs=");

        
        public KitchenViewModel()
        {
            _tempList = new List<DeviceItem>();
            _deviceItems = new ObservableCollection<DeviceItem>();
            PopulateDeviceItemsAsync().ConfigureAwait(false);
            SetInterval(TimeSpan.FromSeconds(3));
        }

        public string Title { get; set; } = "Kitchen & Dining";
        


        private ThermostatDevice _thermostatDevice = new ThermostatDevice();
        public ThermostatDevice ThermostatDevice
        {
            get => _thermostatDevice;
            set
            {
                _thermostatDevice = value;
                OnPropertyChanged();
            }
        }

        private string _temperature;
        public string Temperature
        {
            get { return _temperature; }
            set
            {
                _temperature = value;
                OnPropertyChanged();
            }
        }

        private string _humidity;

        public string Humidity
        {
            get { return _humidity; }
            set
            {
                _humidity = value;
                OnPropertyChanged();
            }
        }

        private void SetInterval(TimeSpan interval)
        {
            _timer = new DispatcherTimer()
            {
                Interval = interval
            };

            _timer.Tick += new EventHandler(timer_tick);
            _timer.Start();
        }

        private async void timer_tick(object sender, EventArgs e)
        {
            await PopulateDeviceItemsAsync();
            PopulateThermostatSensor();
            await UpdateDeviceItemsAsync();
        }

        public IEnumerable<DeviceItem> DeviceItems => _deviceItems;
        private async Task UpdateDeviceItemsAsync()
        {
            _tempList.Clear();

            foreach (var item in _deviceItems)
            {
                var device = await _registryManager.GetDeviceAsync(item.DeviceId);
                if (device == null)
                    _tempList.Add(item);
            }

            foreach (var item in _tempList)
            {
                _deviceItems.Remove(item);
            }
        }

        private async void PopulateThermostatSensor()
        {
            var result = _registryManager.CreateQuery("SELECT * FROM Devices WHERE properties.reported.deviceName = 'kitchen_thermostat'");
            
            foreach (var twin in await result.GetNextAsTwinAsync())
            {
                try { _thermostatDevice.Temperature = twin.Properties.Reported["temperature"]; }
                catch { }
                try { _thermostatDevice.Humidity = twin.Properties.Reported["humidity"]; }
                catch { }
            }

            Temperature = _thermostatDevice.Temperature;
            Humidity = _thermostatDevice.Humidity;
        }

        private async Task PopulateDeviceItemsAsync()
        {
            var result = _registryManager.CreateQuery("SELECT * FROM devices WHERE properties.reported.location = 'kitchen'");
            //var result = registryManager.CreateQuery("select * from devices");

            if (result.HasMoreResults)
            {
                foreach (Twin twin in await result.GetNextAsTwinAsync())
                {
                    var device = _deviceItems.FirstOrDefault(x => x.DeviceId == twin.DeviceId);

                    if (device == null)
                    {
                        device = new DeviceItem
                        {
                            DeviceId = twin.DeviceId,
                        };

                        try { device.DeviceName = twin.Properties.Reported["deviceName"]; }
                        catch { device.DeviceName = device.DeviceId; }
                        try { device.DeviceType = twin.Properties.Reported["deviceType"]; }
                        catch { }

                        switch (device.DeviceType.ToLower())
                        {
                            case "fan":
                                device.IconActive = "\uf863";
                                device.IconInActive = "\uf863";
                                device.StateActive = "ON";
                                device.StateInActive = "OFF";
                                break;

                            case "light":
                                device.IconActive = "\uf672";
                                device.IconInActive = "\uf0eb";
                                device.StateActive = "ON";
                                device.StateInActive = "OFF";
                                break;

                            default:
                                device.IconActive = "\uf2db";
                                device.IconInActive = "\uf2db";
                                device.StateActive = "ENABLE";
                                device.StateInActive = "DISABLE";
                                break;
                        }
                        _deviceItems.Add(device);
                    }
                    else { }
                }
            }
            else
            {
                _deviceItems.Clear();
            }
        }
    }
}
