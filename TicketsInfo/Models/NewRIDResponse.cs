using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketsInfo.Models
{
    public class NewRIDResponse
    {
        public string FIO { get; set; }
        public string result { get; set; }
        public long RID { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Period { get; set; }
    }
}
