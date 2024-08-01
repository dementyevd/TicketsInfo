using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TicketsInfo.Models
{
    public class RZDClient : IDisposable
    {
        private static HttpClient? client;

        public RZDClient()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            client = new HttpClient(clientHandler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public void Dispose()
        {
            client?.Dispose();
        }
       
        public async Task<NewRIDResponse> GetRIDTest(string number, DateTime date, string fio, string Period)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://www.rzd.ru/legitimacy/status/");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36");
            string s = "{\"ticketNumber\":\"" + $"{number}\"," + "\"date\":\"" + $"{date.ToString("dd.MM.yyyy")}\"" + "}";
            var content = new StringContent(s, Encoding.UTF8, "application/json");
            request.Content = content;
            try
            {
                var response = await client.SendAsync(request);
                var t = await response.Content.ReadFromJsonAsync<RIDResponse>();
                return new NewRIDResponse { Number = number, Date = date, result = t.result, RID = t.RID, FIO = fio, Period = Period };
            }
            catch (Exception ex)
            {
                return new NewRIDResponse { FIO = fio, Number = number, Date = date, Period = Period };
            }
        }

        public async Task<TicketTotalInfo> GetDataTest(string number, DateTime date, long RID, string fio, string Period)
        {
            Uri newUri = null;
            Uri.TryCreate($"https://www.rzd.ru/legitimacy/status/?rid={RID}", UriKind.Absolute, out newUri);
            var request = new HttpRequestMessage(HttpMethod.Post, newUri);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36");
            string s = "{\"ticketNumber\":\"" + $"{number}\"," + "\"date\":\"" + $"{date.ToString("dd.MM.yyyy")}\"" + "}";
            var content = new StringContent(s, Encoding.UTF8, "application/json");
            request.Content = content;
            try
            {
                var response = await client.SendAsync(request);
                var ft = await response.Content.ReadFromJsonAsync<ResponseData>();
                if (ft.data != null)
                {
                    var stat = string.Empty;
                    if (ft.data.ticketResponseData.refundResultSum != 0)
                        stat = "stolen";
                    else
                        stat = "used";
                    return new TicketTotalInfo
                    {
                        status = stat,
                        FIO = fio,
                        ticketNumber = ft.data.passengerResponseData.ticketNumber,
                        date = ft.data.passengerResponseData.date,
                        time = ft.data.passengerResponseData.time,
                        stdep = ft.data.ticketResponseData.stdep,
                        starr = ft.data.ticketResponseData.starr,
                        train = ft.data.ticketResponseData.train,
                        vag = ft.data.ticketResponseData.vag,
                        cartype = ft.data.ticketResponseData.cartype,
                        klobsl = ft.data.ticketResponseData.klobsl,
                        opertype = ft.data.ticketResponseData.opertype,
                        er = ft.data.ticketResponseData.er,
                        dateoper = ft.data.ticketResponseData.dateoper,
                        timeoper = ft.data.ticketResponseData.timeoper,
                        datestop = ft.data.ticketResponseData.datestop,
                        timestop = ft.data.ticketResponseData.timestop,
                        oldTicket = ft.data.ticketResponseData.oldTicket,
                        kanpr = ft.data.ticketResponseData.kanpr,
                        hp = ft.data.ticketResponseData.hp,
                        db = ft.data.ticketResponseData.db,
                        ht = ft.data.ticketResponseData.ht,
                        vidr = ft.data.ticketResponseData.vidr,
                        valut = ft.data.ticketResponseData.valut,
                        tf = ft.data.ticketResponseData.tf,
                        tf4 = ft.data.ticketResponseData.tf4,
                        tf9 = ft.data.ticketResponseData.tf9,
                        tf94 = ft.data.ticketResponseData.tf94,
                        tf3 = ft.data.ticketResponseData.tf3,
                        tf5 = ft.data.ticketResponseData.tf5,
                        tf93 = ft.data.ticketResponseData.tf93,
                        tf95 = ft.data.ticketResponseData.tf95,
                        etf = ft.data.ticketResponseData.etf,
                        etf4 = ft.data.ticketResponseData.etf4,
                        etf9 = ft.data.ticketResponseData.etf9,
                        etf94 = ft.data.ticketResponseData.etf94,
                        tfa = ft.data.ticketResponseData.tfa,
                        tfb = ft.data.ticketResponseData.tfb,
                        tf9A = ft.data.ticketResponseData.tf9A,
                        tf9B = ft.data.ticketResponseData.tf9B,
                        eskv = ft.data.ticketResponseData.eskv,
                        etfb = ft.data.ticketResponseData.etfb,
                        eskvv = ft.data.ticketResponseData.eskvv,
                        etfbv = ft.data.ticketResponseData.etfbv,
                        ess = ft.data.ticketResponseData.ess,
                        etf5 = ft.data.ticketResponseData.etf5,
                        essv = ft.data.ticketResponseData.essv,
                        etf95 = ft.data.ticketResponseData.etf95,
                        esk = ft.data.ticketResponseData.esk,
                        etfc = ft.data.ticketResponseData.etfc,
                        eskv1 = ft.data.ticketResponseData.eskv1,
                        etfcv = ft.data.ticketResponseData.etfcv,
                        stv1 = ft.data.ticketResponseData.stv1,
                        stv2 = ft.data.ticketResponseData.stv2,
                        stv3 = ft.data.ticketResponseData.stv3,
                        resultSum = ft.data.ticketResponseData.resultSum,
                        resultNdsSum = ft.data.ticketResponseData.resultNdsSum,
                        refundResultSum = ft.data.ticketResponseData.refundResultSum,
                        refundResultNdsSum = ft.data.ticketResponseData.refundResultNdsSum,
                        refundWithholdNdsSum = ft.data.ticketResponseData.refundWithholdNdsSum,
                        result = ft.result,
                        month = DateTime.ParseExact(ft.data.passengerResponseData.date, "yyyy-MM-dd", CultureInfo.GetCultureInfo("ru-RU")).ToString("MM"),
                        year = DateTime.ParseExact(ft.data.passengerResponseData.date, "yyyy-MM-dd", CultureInfo.GetCultureInfo("ru-RU")).ToString("yyyy"),
                        period = Period
                    };
                }
                else
                {
                    return new TicketTotalInfo { ticketNumber = number, date = date.ToString("dd.MM.yyyy"), FIO = fio, error = ft.error, period = Period };
                }
            }
            catch (Exception ex)
            {
                return new TicketTotalInfo { ticketNumber = number, date = date.ToString("dd.MM.yyyy"), FIO = fio, error = ex.Message, period = Period };
            }
        }     
    }
}
