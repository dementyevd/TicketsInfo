using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketsInfo.Models
{
    public class Ticket
    {
        public string FIO { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public long Rid { get; set; }
        public string Error { get; set; }
        public string Period { get; set; }
    }
}
