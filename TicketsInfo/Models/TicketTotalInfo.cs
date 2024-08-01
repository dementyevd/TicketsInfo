using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketsInfo.Models
{
    public class TicketTotalInfo
    {
        public string? FIO { get; set; }
        [Key]
        public string? ticketNumber { get; set; }
        public string? date { get; set; }
        public string? time { get; set; }
        public string? month { get; set; }
        public string? year { get; set; }
        public string? period { get; set; }
        public string? status { get; set; }
        public string? stdep { get; set; }
        public string? starr { get; set; }
        public string? train { get; set; }
        public string? vag { get; set; }
        public string? cartype { get; set; }
        public string? klobsl { get; set; }
        public string? opertype { get; set; }
        public string? er { get; set; }
        public string? dateoper { get; set; }
        public string? timeoper { get; set; }
        public string? datestop { get; set; }
        public string? timestop { get; set; }
        public string? oldTicket { get; set; }
        public string? kanpr { get; set; }
        public string? hp { get; set; }
        public string? db { get; set; }
        public string? ht { get; set; }
        public string? vidr { get; set; }
        public string? valut { get; set; }
        public float tf { get; set; }
        public float tf4 { get; set; }
        public float tf9 { get; set; }
        public float tf94 { get; set; }
        public float tf3 { get; set; }
        public float tf5 { get; set; }
        public float tf93 { get; set; }
        public float tf95 { get; set; }
        public float etf { get; set; }
        public float etf4 { get; set; }
        public float etf9 { get; set; }
        public float etf94 { get; set; }
        public float tfa { get; set; }
        public float tfb { get; set; }
        public float tf9A { get; set; }
        public float tf9B { get; set; }
        public float eskv { get; set; }
        public float etfb { get; set; }
        public float eskvv { get; set; }
        public float etfbv { get; set; }
        public float ess { get; set; }
        public float etf5 { get; set; }
        public float essv { get; set; }
        public float etf95 { get; set; }
        public float esk { get; set; }
        public float etfc { get; set; }
        public float eskv1 { get; set; }
        public float etfcv { get; set; }
        public float stv1 { get; set; }
        public float stv2 { get; set; }
        public float stv3 { get; set; }
        public float resultSum { get; set; }
        public float resultNdsSum { get; set; }
        public float refundResultSum { get; set; }
        public float refundResultNdsSum { get; set; }
        public float refundWithholdNdsSum { get; set; }
        public string? error { get; set; }
        public string? result { get; set; }
    }
}
