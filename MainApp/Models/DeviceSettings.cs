using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainApp.Models
{
   public class DeviceSettings
    {
        public string DeviceId { get; set; } = "Fan1";
        public string ApiUrl { get; set; } = "http://localhost:5000";
    }
}
