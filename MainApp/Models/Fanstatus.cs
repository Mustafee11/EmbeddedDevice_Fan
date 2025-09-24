using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainApp.Models
{
   public class Fanstatus
    {
        public string Id { get; set; } =Guid.NewGuid().ToString();
        public bool IsRunning { get; set; } = false;

        public double Speed { get; set; }


    }
}
