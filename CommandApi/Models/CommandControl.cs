using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandApi.Models
{
   public class CommandControl
    {
        public string Action { get; set; } = "";
        public double? Value {  get; set; }
    }
}
