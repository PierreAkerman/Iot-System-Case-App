using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dapper;
using Device.Thermostat.Models;
using Microsoft.Azure.Devices.Shared;

namespace Device.Thermostat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _connect_url = "http://localhost:7108/api/devices/connect";
        private readonly string _connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\Min dator\\source\\repos\\Systemutveckling\\IotSystemCaseApp\\Device.Thermostat\\Data\\deviceThermostatDB.mdf\";Integrated Security=True;Connect Timeout=30";
        private DeviceClient _deviceClient;
        private DeviceInfo _deviceInfo;

        private bool _isConnected = false;
        private string _deviceId;
        private int _previousTemperature;
        private int _previousHumidity;

        private static Random _random = new Random();
        public static int GetRandomNumber(int min, int max)
        {
            lock (_random)
            {
                return _random.Next(min, max);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            RunSensor().ConfigureAwait(false);
        }

        private async Task RunSensor()
        {
            await Setup();
            await GenerateThermostatData();
        }
        private async Task Setup()
        {
            tbConnectingMessage.Text = "Initializing Device, Please wait....";

            await Task.Delay(5000);

            using IDbConnection connection = new SqlConnection(_connectionString);
            _deviceId = await connection.QueryFirstOrDefaultAsync<string>("SELECT DeviceId FROM DeviceInfo");
            if (string.IsNullOrEmpty(_deviceId))
            {
                tbConnectingMessage.Text = "Generating DeviceId";
                //_deviceId = "Kitchen-Thermometer";
                await connection.ExecuteAsync(
                    "INSERT INTO DeviceInfo (DeviceId, DeviceType, DeviceName, Location, Owner) " +
                    "VALUES (@DeviceId, @DeviceType, @DeviceName, @Location, @Owner)",
                    new
                    {
                        DeviceId = _deviceId,
                        DeviceType = "sensor",
                        DeviceName = "kitchen_thermostat",
                        Location = "kitchen",
                        Owner = "Pierre"
                    });
            }

            var deviceConnectionString =
                await connection.QueryFirstOrDefaultAsync<string>("SELECT ConnectionString FROM DeviceInfo WHERE DeviceId = @DeviceId",
                    new { DeviceId = _deviceId });
            if (string.IsNullOrEmpty(deviceConnectionString))
            {
                tbConnectingMessage.Text = "Initializing ConnectionString, Please Wait.....";
                using var http = new HttpClient();
                var result = await http.PostAsJsonAsync(_connect_url, new { DeviceId = _deviceId });
                deviceConnectionString = await result.Content.ReadAsStringAsync();
                await connection.ExecuteAsync(
                    "UPDATE DeviceInfo SET ConnectionString = @ConnectionString WHERE DeviceId = @DeviceId",
                    new { DeviceId = _deviceId, ConnectionString = deviceConnectionString });
            }
            _deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString);

            tbConnectingMessage.Text = "Updating Properties, Please Wait.....";

            _deviceInfo = await connection.QueryFirstOrDefaultAsync<DeviceInfo>(
                "SELECT * FROM DeviceInfo WHERE DeviceId = @DeviceId",
                new { DeviceId = _deviceId });
            var twinCollection = new TwinCollection();
            twinCollection["owner"] = _deviceInfo.Owner;
            twinCollection["deviceName"] = _deviceInfo.DeviceName;
            twinCollection["deviceType"] = _deviceInfo.DeviceType;
            twinCollection["location"] = _deviceInfo.Location;

            await _deviceClient.UpdateReportedPropertiesAsync(twinCollection);

            _isConnected = true;
            tbConnectingMessage.Text = "Device Connected.";
        }

        private async Task GenerateThermostatData()
        {
            while (true)
            {
                if (_isConnected)
                {
                    var currentTemp = GetRandomNumber(18, 23);
                    var currentHumidity = GetRandomNumber(35, 60);

                    var twinCollection = new TwinCollection();
                    if (_previousTemperature != currentTemp)
                    {
                        twinCollection["temperature"] = currentTemp + "°C";
                        thermostatData.Text += ($"{DateTime.Now}: Current temperature is {currentTemp}, Previously it was {_previousTemperature}\n");
                        _previousTemperature = currentTemp;
                    }

                    if (_previousHumidity != currentHumidity)
                    {
                        twinCollection["humidity"] = currentHumidity + "%";
                        thermostatData.Text += ($"{DateTime.Now}: Current Humidity is {currentHumidity}, Previously it was {_previousHumidity}\n");
                        _previousHumidity = currentHumidity;
                    }
                    await _deviceClient.UpdateReportedPropertiesAsync(twinCollection);
                }
                await Task.Delay(5000);
            }

        }
    }
}
