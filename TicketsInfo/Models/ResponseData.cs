using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketsInfo.Models
{
    public partial class ResponseData
    {
        public string result { get; set; }
        public Data data { get; set; }
        public string error { get; set; }
    }
}
