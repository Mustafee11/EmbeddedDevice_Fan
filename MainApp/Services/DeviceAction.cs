using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainApp.Services
{
    public static class DeviceAction
    {
        public static bool IsRunning { get; private set; } = false;

        public static void ToggleState()
        {
            IsRunning = !IsRunning;
        }
    }
}
