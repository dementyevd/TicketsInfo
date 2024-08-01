namespace TicketsInfo.Models
{
    public partial class ResponseData
    {
        public class Data
        {
            public string result { get; set; }
            public string requestDate { get; set; }
            public string requestTime { get; set; }
            public string requestId { get; set; }
            public Passengerresponsedata passengerResponseData { get; set; }
            public Ticketresponsedata ticketResponseData { get; set; }
        }

    }
}
