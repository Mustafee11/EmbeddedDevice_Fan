
using MainApp.Models;
using MainApp.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
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

        
        public MainWindow()
        {
            InitializeComponent();
            InitailizeFeatures();

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

            //_= SendFanStatus();

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
    }
}