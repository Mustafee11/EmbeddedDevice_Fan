
using MainApp.Models;
using MainApp.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _logFilePath = "";
        private double _pendingSpeed;
        private bool _IsRunning = false;
        private Storyboard? _rotatingFAN;
        private DispatcherTimer _Speedtimer;
        private List<string> _eventLog;

        public static readonly HttpClient http = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

        private HubConnection? _hub;

        private string _settingsFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EmbbededDevice",
            "settings.json"

            );

        private DeviceSettings _settings = new DeviceSettings();


        public MainWindow()
        {
            InitializeComponent();

            LoadSettings();

            InitailizeFeatures();
            SetSignalR();
            

        }

        public void InitailizeFeatures()
        {
            _rotatingFAN = ((BeginStoryboard)FindResource(("sb-rotate-fan"))).Storyboard;
            _Speedtimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(500),
            };
            _Speedtimer.Tick += (_, _) =>
            {
                _Speedtimer.Stop();
                LogMessage($"Speed set to {_pendingSpeed:0.00}");
                _ = SendFanStatus();

                if (_pendingSpeed > 2)
                {
                    _ = SendAlert();
                }

            };
            _eventLog = [];

            var AppDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EmbbededDevice");
            Directory.CreateDirectory(AppDir);
            _logFilePath = System.IO.Path.Combine(AppDir, "eventLog.log");

        }

      

        private void Btn_OnOff_Click(object sender, RoutedEventArgs e)
        {

            ToggleRunningState();


        }

        private  void ToggleRunningState()
        {

            DeviceAction.ToggleState();


            if (DeviceAction.IsRunning)
            {


                Btn_OnOff.Content = "STOP";
                _rotatingFAN!.Begin();
                _rotatingFAN.SetSpeedRatio(Slider_Speed.Value);
                LogMessage("Fan Started");

                 

            }
            else
            {
                Btn_OnOff.Content = "START";
                _rotatingFAN!.Pause();
                LogMessage("Fan Stopped");
            }

            _= SendFanStatus();

            //if (!_IsRunning)
            //{

            //    _IsRunning = true;
            //    Btn_OnOff.Content = "STOP";
            //}
            //else
            //{

            //    _IsRunning = false;
            //    Btn_OnOff.Content = "START";
            //}

        }

        private  void Slider_Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DeviceAction.IsRunning && _rotatingFAN is not null)
            {
                _rotatingFAN.SetSpeedRatio(e.NewValue);
                _pendingSpeed = e.NewValue;
                _Speedtimer?.Stop();
                _Speedtimer.Start();

               

                //_ = SendFanStatus();


            }

           


        }

        public async Task SendFanStatus()
        {
            try
            {
                var Status = new Fanstatus
                {
                    IsRunning = DeviceAction.IsRunning,
                    Speed = _pendingSpeed

                };
                var response = await http.PostAsJsonAsync("/fanStatus", Status);
                if (!response.IsSuccessStatusCode)
                {
                    LogMessage($"failed to send status{response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending status{ex.Message}");
            }
        }

        public async Task SendAlert()
        {
            try
            {
                var alert = new Alert
                {
                    TimeStamp = DateTime.Now,
                    Severity = "High",
                    Type = "Overheating",
                    Machine = "EmbeddedFan",
                    Message = "Temp: 100 °C"

                };

                var response = await http.PostAsJsonAsync<Alert>("/alerts", alert);
                if (!response.IsSuccessStatusCode)
                {
                    LogMessage($"failed to send status{response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending status{ex.Message}");
            }
        }

        public async void SetSignalR()
        {
            try
            {
                var clientId = $"hmi-{Environment.MachineName}";

                _hub = new HubConnectionBuilder()
                    .WithUrl($"{_settings.ApiUrl}/hmi?clientId={clientId}")
                    .WithAutomaticReconnect()
                    .Build();

                _hub.On<CommandControl>("CommandsReviced", (cmds) =>
                {
                    App.Current.Dispatcher.Invoke(() => HandleCommands(cmds));
                });

                await _hub.StartAsync();
            }
            catch (Exception ex)
            {
                LogMessage($"Error connecting to server {ex.Message}");

            }
        }

        private void HandleCommands(CommandControl cmds)
        {
            switch (cmds.Action)
            {
                case "TurnOn":
                    if (!DeviceAction.IsRunning)
                     ToggleRunningState();
                    break;
                case "TurnOff":
                    if (DeviceAction.IsRunning)
                        ToggleRunningState();
                    break;
                case "SetSpeed":
                    if(DeviceAction.IsRunning && cmds.Value.HasValue)
                    {
                        Slider_Speed.Value =  cmds.Value.Value;
                        _rotatingFAN?.SetSpeedRatio(cmds.Value.Value);
                        _pendingSpeed = cmds.Value.Value;
                       

                    }
                    break;

            }
        }

        private void LogMessage(string message)
        {

            var line = @$"{DateTime.Now:yyyy-MM-dd HH:mm:ss}:  {message}";
            _eventLog?.Add(line);

            try
            {
                File.AppendAllText(_logFilePath, line + Environment.NewLine);

            }
            catch (Exception ex)
            {
                


            }
        }

        private void LoadSettings()
        {
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_settingsFilePath)!);

                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<DeviceSettings>(json);
                    if (settings != null)
                    {
                        _settings = settings;



                    }
                    else
                    {
                        SaveSettings();
                    }
                    
                    

                    http.BaseAddress = new Uri(_settings.ApiUrl);

                }
                else
                {
                    SaveSettings() ;
                }


            }
            catch
            {
                http.BaseAddress = new Uri(_settings.ApiUrl);
            }
        }

        private void SaveSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_settingsFilePath)!);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch
            {

            }
        }

       
    }
}