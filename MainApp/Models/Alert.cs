using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainApp.Models
{
    public class Alert
    {
        public string Type { get; set; } = null!;

        public string Severity { get; set; } = null!;

        public string Message { get; set; } = null!;

        public DateTime TimeStamp { get; set; }

        public string? Site { get; set; }

        public string? Machine { get; set; }

        public string? Source { get; set; }
    }
}
