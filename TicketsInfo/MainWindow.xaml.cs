using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TicketsInfo.Models;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Org.BouncyCastle.Bcpg;
using System.Data;
using System.Runtime.InteropServices;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;
using System.Threading;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Text.Json;
using System.Net.Http;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace TicketsInfo
{
    public partial class MainWindow : Window
    {
        private TicketContext db = new TicketContext();
        private string errorText = "Для данного сочетания параметров запроса не найден ни один билет";
        public List<Ticket> Tickets { get; set; }
        public List<Ticket> ReturnedTickets { get; set; }
        public List<Ticket> ErrorTickets { get; set; }
        public ObservableCollection<Ticket> ErrorsFinal { get; set; }
        public List<Ticket> ReloadTickets { get; set; }
        public List<Ticket> ReloadErrorTickets { get; set; }
        public ObservableCollection<TicketTotalInfo> TicketTotals { get; set; }
        public ObservableCollection<TicketTotalInfo> SearchTickets { get; set; }
        public List<TicketTotalInfo> HistoryToDowloadTickets { get; set; }
        public string[] Monthes { get; set; } = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
        public IEnumerable<int> YearsArray { get; set; } = Enumerable.Range(2021, DateTime.Now.Year - 2019);
        public List<string> Years { get; set; } = new List<string>();
        public string SelectedMonth { get; set; }
        public string SelectedYear { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            ReturnedTickets = new List<Ticket>();
            Tickets = new List<Ticket>();
            ErrorTickets = new List<Ticket>();
            ReloadTickets = new List<Ticket>();
            TicketTotals = new ObservableCollection<TicketTotalInfo>();
            SearchTickets = new ObservableCollection<TicketTotalInfo>();
            HistoryToDowloadTickets = new List<TicketTotalInfo>();
            ErrorsFinal = new ObservableCollection<Ticket>();
            ReloadErrorTickets = new List<Ticket>();
            myGrid.ItemsSource = TicketTotals;
            myGridError.ItemsSource = ErrorsFinal;
            myGridSearch.ItemsSource = SearchTickets;
            SelectedMonth = DateTime.Now.ToString("MM");
            SelectedYear = DateTime.Now.ToString("yyyy");
            BeginDate = DateTime.Now;
            EndDate = DateTime.Now;
            foreach (var year in YearsArray)
            {
                Years.Add(year.ToString());
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            tbTicketsToDownload.Text = string.Empty;
            tbDownloadedTickets.Text = string.Empty;
            tbTicketsWithErors.Text = string.Empty;
            tbWarning.Text = string.Empty;
            if (ExcelBtn.IsChecked == true)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Excel files (.xlsx)|*.xlsx";
                if (openFileDialog.ShowDialog() == true)
                {
                    ImportExcel(openFileDialog);
                }
            }
            if (PdfBtn.IsChecked == true)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "PDF files (.pdf)|*.pdf";
                if (openFileDialog.ShowDialog() == true)
                {
                    ImportPdf(openFileDialog);
                }
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            if (saveFileDialog.ShowDialog() == true)
            {
                var TicketType = typeof(TicketTotalInfo);
                var ErrorType = typeof(Ticket);
                var wb = new XLWorkbook();
                var sh = wb.Worksheets.Add("Tickets");
                var sh1 = wb.Worksheets.Add("Errors");
                int col = 1;
                foreach (var prop in TicketType.GetProperties())
                {
                    sh.Cell(1, col).SetValue(prop.Name);
                    col++;
                }
                col = 1;
                foreach (var prop in ErrorType.GetProperties())
                {
                    sh1.Cell(1, col).SetValue(prop.Name);
                    col++;
                }
                for (int i = 0; i < TicketTotals.Count; i++)
                {
                    for (int j = 0; j < TicketType.GetProperties().Length; j++)
                    {
                        sh.Cell(i + 2, j + 1).SetValue(TicketType.GetProperties()[j].GetValue(TicketTotals[i])?.ToString());
                    }
                }
                for (int i = 0; i < ErrorsFinal.Count; i++)
                {
                    for (int j = 0; j < ErrorType.GetProperties().Length; j++)
                    {
                        if (ErrorType.GetProperties()[j].GetValue(ErrorsFinal[i]) != null)
                            sh1.Cell(i + 2, j + 1).SetValue(ErrorType.GetProperties()[j].GetValue(ErrorsFinal[i])?.ToString());
                        else
                            sh1.Cell(i + 2, j + 1).SetValue(string.Empty);
                    }
                }
                try
                {
                    wb.SaveAs(saveFileDialog.FileName);
                    MessageBox.Show("Данные выгружены", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

        }
        private async void ImportExcel(OpenFileDialog openFileDialog)
        {
            HistoryToDowloadTickets.Clear();
            Tickets.Clear();
            TicketTotals.Clear();
            ErrorTickets.Clear();
            ErrorsFinal.Clear();
            try
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                using (XLWorkbook workBook = new XLWorkbook(openFileDialog.FileName))
                {
                    IXLWorksheet workSheet = workBook.Worksheet(1);
                    int lrow = workSheet.RowsUsed().Count();
                    bool firstRow = true;
                    foreach (var row in workSheet.RowsUsed())
                    {
                        if (firstRow)
                        {
                            foreach (IXLCell cell in row.Cells())
                            {
                                dt.Columns.Add(cell.Value.ToString());
                            }
                            firstRow = false;
                        }
                        else
                        {
                            dt.Rows.Add();
                            int i = 0;
                            foreach (IXLCell cell in row.Cells("1:3"))
                            {
                                dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                                i++;
                            }
                        }
                    }
                }

                string tmp = string.Empty;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string FIO = dt.Rows[i][0].ToString();
                    string number = dt.Rows[i][1].ToString();
                    DateTime date = DateTime.Parse(dt.Rows[i][2].ToString());
                    tmp = date.ToString("dd.MM.yy");
                    Tickets.Add(new Ticket { FIO = FIO, Number = number, Date = date, Period = tmp });
                }

                //проверка на наличие официальных возвратов
                var returns = db.BaseTickets.Where(x => x.status == "returned").Select(x => x.ticketNumber).ToList();
                Tickets.RemoveAll(x => returns.Contains(x.Number));

                await GetInfoFromRZD(Tickets);
                await ReloadTicketTask();
                await ReloadErrorTicketTask();
                await AddTicketsToDb(TicketTotals);
                await AddErrorTicketsToDb(ErrorTickets);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                myPbar.Value = 0;
                myTblock.Text = string.Empty;
                return;
            }
        }
        private async void ImportPdf(OpenFileDialog openFileDialog)
        {
            try
            {
                HistoryToDowloadTickets.Clear();
                SearchTickets.Clear();
                ReturnedTickets.Clear();
                Tickets.Clear();
                TicketTotals.Clear();
                ErrorTickets.Clear();
                ErrorsFinal.Clear();
                string marker = "Железнодорожные билеты";
                string str = string.Empty;
                string thePage = string.Empty;
                string separator = "----- -----";
                string tmp = string.Empty;
                List<string> ticketString = new List<string>();
                List<string> ticketStringToLoad = new List<string>();
                List<string> returnedStringTickets = new List<string>();

                foreach (string filename in openFileDialog.FileNames)
                {
                    ITextExtractionStrategy its = new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy();
                    StringBuilder text = new StringBuilder();
                    using (PdfReader reader = new PdfReader(filename))
                    {
                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        {
                            thePage = PdfTextExtractor.GetTextFromPage(reader, i, its);
                        }
                        string[] theLines = thePage.Split('\n');
                        foreach (var theLine in theLines)
                        {
                            text.AppendLine(theLine);
                        }
                    }
                    if (text.ToString().IndexOf(marker) != -1)
                    {
                        str = text.ToString().Substring(text.ToString().IndexOf(marker) + marker.Length);
                        if (str.IndexOf("Прочие услуги") != -1)
                        {
                            str = str.Substring(0, str.LastIndexOf("Прочие услуги"));
                        }
                        while (str.IndexOf(separator) != -1)
                        {
                            tmp = str.Substring(0, str.IndexOf(separator) + separator.Length).Replace(Environment.NewLine, "");
                            ticketString.Add(tmp);
                            str = str.Substring(str.IndexOf(separator) + separator.Length);
                        }
                    }
                }

                foreach (var ticket in ticketString)
                {
                    var splitted = ticket.Split('/');
                    switch (splitted.Length)
                    {
                        case 7:
                            if (splitted[3].Trim() == "Продажа")
                            {
                                ticketStringToLoad.Add(ticket);
                            }
                            if (splitted[3].Trim() == "Возврат")
                            {
                                returnedStringTickets.Add(ticket);
                            }
                            break;
                        case 10:
                            if (splitted[4].Trim() == "Продажа")
                            {
                                ticketStringToLoad.Add(ticket);
                            }
                            if (splitted[4].Trim() == "Возврат")
                            {
                                returnedStringTickets.Add(ticket);
                            }
                            break;
                    }
                }

                Tickets.AddRange(GetTicketsFromString(ticketStringToLoad));
                ReturnedTickets.AddRange(GetTicketsFromString(returnedStringTickets));
                UpdateCurrentReturns(Tickets, ReturnedTickets);
                GetCurrentMonthTickets();
                GetHistoryTicketsWithCurrentPeriod();

                await GetInfoFromRZD(Tickets);
                await ReloadTicketTask();
                await ReloadErrorTicketTask();
                await AddTicketsToDb(TicketTotals);
                await AddErrorTicketsToDb(ErrorTickets);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                myPbar.Value = 0;
                myTblock.Text = string.Empty;
                return;
            }
        }
        private string getData(string text)
        {
            Regex regex = new Regex(@"(\d\d).(\d\d).(\d\d)");
            MatchCollection matches = regex.Matches(text);
            if (matches.Count > 0)
            {
                return matches[0].Value.ToString();
            }
            else { return "fail"; }
        }
        private void GetCurrentMonthTickets()
        {
            var stringDate = DateTime.DaysInMonth(int.Parse(SelectedYear), int.Parse(SelectedMonth)).ToString() + "." + SelectedMonth + "." + SelectedYear;
            var reportingDate = DateTime.ParseExact(stringDate, "dd.MM.yyyy", CultureInfo.GetCultureInfo("ru-RU"));
            List<Ticket> tmp = new List<Ticket>();
            foreach (var ticket in Tickets)
            {
                //var ticketMonth = DateTime.ParseExact(ticket.Period, "dd.MM.yy", CultureInfo.GetCultureInfo("ru-RU")).ToString("MM");
                //var ticketYear = DateTime.ParseExact(ticket.Period, "dd.MM.yy", CultureInfo.GetCultureInfo("ru-RU")).ToString("yyyy");
                //if (ticketMonth != ticket.Date.ToString("MM") || ticketYear != ticket.Date.ToString("yyyy"))
                //if (int.Parse(ticket.Date.ToString("MM")) > int.Parse(SelectedMonth) || int.Parse(ticket.Date.ToString("yyyy")) > int.Parse(SelectedYear))
                if (ticket.Date > reportingDate)
                {
                    TicketTotals.Add(new TicketTotalInfo
                    {
                        FIO = ticket.FIO,
                        ticketNumber = ticket.Number,
                        date = ticket.Date.ToString("yyyy-MM-dd"),
                        month = ticket.Date.ToString("MM"),
                        year = ticket.Date.ToString("yyyy"),
                        period = ticket.Period,
                        status = "notUsed"
                    });
                    tmp.Add(ticket);
                }
            }
            Tickets.RemoveAll(x => tmp.Contains(x));
        }
        private void GetHistoryTicketsWithCurrentPeriod()
        {            
            var tmp = db.BaseTickets.Where(x => x.month == SelectedMonth && x.year == SelectedYear && x.status == "notUsed").ToList();
            HistoryToDowloadTickets.AddRange(tmp);
            foreach (var ticket in HistoryToDowloadTickets)
            {
                if (!Tickets.Exists(x => x.Number == ticket.ticketNumber))
                {
                    var tmpTicket = new Ticket { FIO = ticket.FIO, Date = DateTime.ParseExact(ticket.date, "yyyy-MM-dd", CultureInfo.GetCultureInfo("ru-RU")), Number = ticket.ticketNumber, Period = ticket.period };
                    Tickets.Add(tmpTicket);
                }
            }
        }
        private List<Ticket> GetTicketsFromString(List<string> ticketString)
        {
            List<Ticket> tmpTickets = new List<Ticket>();
            foreach (var ticket in ticketString)
            {
                string FIO, number, period, tmpStr = string.Empty;
                DateTime date;
                var splitted = ticket.Split('/');
                switch (splitted.Length)
                {
                    case 7:
                        tmpStr = splitted[0].Trim();
                        period = tmpStr.Substring(tmpStr.Length - 8);
                        //period = DateTime.ParseExact(tmpStr.Substring(tmpStr.Length - 8), "dd.MM.yy", CultureInfo.GetCultureInfo("ru-RU")).ToString("MM");
                        FIO = splitted[2].Trim();
                        number = splitted[4].Trim();
                        date = DateTime.ParseExact(getData(splitted[6]), "dd.MM.yy", CultureInfo.GetCultureInfo("ru-RU"));
                        tmpTickets.Add(new Ticket { FIO = FIO, Number = number, Date = date, Period = period });
                        break;
                    case 10:
                        tmpStr = splitted[0].Trim();
                        period = tmpStr.Substring(tmpStr.Length - 8);
                        //period = DateTime.ParseExact(tmpStr.Substring(tmpStr.Length - 8), "dd.MM.yy", CultureInfo.GetCultureInfo("ru-RU")).ToString("MM");
                        FIO = splitted[2].Trim();
                        number = splitted[5].Trim();
                        date = DateTime.ParseExact(getData(splitted[8]), "dd.MM.yy", CultureInfo.GetCultureInfo("ru-RU"));
                        tmpTickets.Add(new Ticket { FIO = FIO, Number = number, Date = date, Period = period });
                        break;
                }
            }
            return tmpTickets;
        }
        private void UpdateCurrentReturns(List<Ticket> tickets, List<Ticket> returnedTickets)
        {
            List<Ticket> RemainReturns = new List<Ticket>();
            List<TicketTotalInfo> FoundedReturns = new List<TicketTotalInfo>();
            foreach (var ticket in returnedTickets)
            {
                if (tickets.Exists(x => x.Number == ticket.Number))
                {
                    TicketTotals.Add(new TicketTotalInfo
                    {
                        FIO = ticket.FIO,
                        ticketNumber = ticket.Number,
                        date = ticket.Date.ToString("yyyy-MM-dd"),
                        month = ticket.Date.ToString("MM"),
                        year = ticket.Date.ToString("yyyy"),
                        period = ticket.Period,
                        status = "returned"
                    });
                    tickets.RemoveAll(x => x.Number == ticket.Number);
                }
                else
                    RemainReturns.Add(ticket);
            }            
            var tmp = db.BaseTickets.Where(x => x.status == "notUsed" || x.status == "stolen").ToList();
            foreach (var ticket in RemainReturns)
            {
                if (tmp.Exists(x => x.ticketNumber == ticket.Number))
                    FoundedReturns.Add(tmp.Find(x => x.ticketNumber == ticket.Number));
                else
                    TicketTotals.Add(new TicketTotalInfo
                    {
                        FIO = ticket.FIO,
                        ticketNumber = ticket.Number,
                        date = ticket.Date.ToString("yyyy-MM-dd"),
                        month = ticket.Date.ToString("MM"),
                        year = ticket.Date.ToString("yyyy"),
                        period = ticket.Period,
                        status = "returned"
                    });
            }
            if (FoundedReturns.Count > 0)
                db.BaseTickets.Where(x => FoundedReturns.Contains(x)).ExecuteUpdate(s => s.SetProperty(u => u.status, "returned").
                                                                                           SetProperty(u => u.refundResultNdsSum, 0).
                                                                                           SetProperty(u => u.refundResultSum, 0).
                                                                                           SetProperty(u => u.refundWithholdNdsSum, 0));
        }
        public async Task ReloadTicketTask()
        {
            int step = 0;
            while (ErrorTickets.Where(x => x.Error != errorText).Count() > 0 && step < 5)
            {
                step++;
                ReloadTickets.Clear();
                var tmp = ErrorTickets.Where(x => x.Error != errorText).ToList();
                foreach (var t in tmp)
                {
                    ReloadTickets.Add(t);
                    ErrorTickets.Remove(t);
                }
                await GetInfoFromRZD(ReloadTickets);
            }
        }
        public async Task ReloadErrorTicketTask()
        {
            int step = 0;
            while (ErrorTickets.Where(x => x.Error == errorText).Count() > 0 && step < 2)
            {
                step++;
                ReloadErrorTickets.Clear();
                var tmp = ErrorTickets.Where(x => x.Error == errorText).ToList();
                foreach (var t in tmp)
                {
                    t.Date = t.Date.AddDays(-1);
                    ReloadErrorTickets.Add(t);
                    ErrorTickets.Remove(t);
                }
                await GetInfoFromRZD(ReloadErrorTickets);

            }
            var temp = ErrorTickets.Where(x => x.Error != errorText).ToList();
            if (temp.Any())
                await ReloadTicketTask();
        }
        private async Task GetInfoFromRZD(List<Ticket> tickets)
        {
            using var cl = new RZDClient();
            tbTicketsToDownload.Text = tickets.Count.ToString();
            tbWarning.Text = "Ожидайте...";
            tbWarning.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Red);
            myPbar.Minimum = 0;
            myPbar.Maximum = tickets.Count;
            myPbar.Value = 0;
            tbDownloadedTickets.Text = myPbar.Value.ToString();
            myTblock.Text = "Загрузка RID...";

            var rand = new Random();
            var responses = new List<NewRIDResponse>();
            var loadedTickets = new List<TicketTotalInfo>();
            var batchSize = 15;
            int numberOfBatches = (int)Math.Ceiling((double)tickets.Count() / batchSize);
            for (int i = 0; i < numberOfBatches; i++)
            {
                var currentIds = tickets.Skip(i * batchSize).Take(batchSize);
                var tasks = currentIds.Select(resp => cl.GetRIDTest(resp.Number, resp.Date, resp.FIO, resp.Period));
                responses.AddRange(await Task.WhenAll(tasks));
                myPbar.Value = responses.Count;
                await Task.Delay(TimeSpan.FromSeconds(rand.Next(1, 4)));
            }
            myPbar.Minimum = 0;
            myPbar.Maximum = responses.Count;
            myPbar.Value = 0;
            myTblock.Text = "Загрузка билетов...";
            for (int i = 0; i < numberOfBatches; i++)
            {
                var currentIds = responses.Skip(i * batchSize).Take(batchSize);
                var tasks = currentIds.Select(resp => cl.GetDataTest(resp.Number, resp.Date, resp.RID, resp.FIO, resp.Period));
                loadedTickets.AddRange(await Task.WhenAll(tasks));
                myPbar.Value = loadedTickets.Count;
                tbDownloadedTickets.Text = loadedTickets.Count.ToString();
                await Task.Delay(TimeSpan.FromSeconds(rand.Next(1, 4)));
            }

            foreach (var ticket in loadedTickets)
            {
                if (ticket.result != "OK")
                {
                    ErrorTickets.Add(new Ticket { FIO = ticket.FIO, Number = ticket.ticketNumber, Date = DateTime.ParseExact(ticket.date, "dd.MM.yyyy", CultureInfo.GetCultureInfo("ru-RU")), Error = ticket.error, Period = ticket.period });
                }
                else
                {
                    TicketTotals.Add(ticket);
                }
            }
        }
        private async Task AddTicketsToDb(ObservableCollection<TicketTotalInfo> ticketTotals)
        {
            if (ticketTotals.Count > 0)
            {
                var ticketsToUpdate = new List<TicketTotalInfo>();
                var ticketsToAdd = new List<TicketTotalInfo>();
                foreach (var ticket in ticketTotals)
                {
                    if (HistoryToDowloadTickets.FirstOrDefault(x => x.ticketNumber == ticket.ticketNumber) != null)
                        ticketsToUpdate.Add(ticket);
                    else
                        ticketsToAdd.Add(ticket);
                }
                //using var db = new TicketContext();
                foreach (var ticket in ticketsToUpdate)
                {
                    db.BaseTickets.Where(x => x.ticketNumber == ticket.ticketNumber).ExecuteUpdate(s => s.SetProperty(u => u.status, ticket.status).
                                                                                                        SetProperty(u => u.essv, ticket.essv).
                                                                                                        SetProperty(u => u.ess, ticket.ess).
                                                                                                        SetProperty(u => u.cartype, ticket.cartype).
                                                                                                        SetProperty(u => u.dateoper, ticket.dateoper).
                                                                                                        SetProperty(u => u.datestop, ticket.datestop).
                                                                                                        SetProperty(u => u.er, ticket.er).
                                                                                                        SetProperty(u => u.esk, ticket.esk).
                                                                                                        SetProperty(u => u.eskv, ticket.eskv).
                                                                                                        SetProperty(u => u.eskv1, ticket.eskv1).
                                                                                                        SetProperty(u => u.eskvv, ticket.eskvv).
                                                                                                        SetProperty(u => u.error, ticket.error).
                                                                                                        SetProperty(u => u.etf, ticket.etf).
                                                                                                        SetProperty(u => u.etf4, ticket.etf4).
                                                                                                        SetProperty(u => u.etf5, ticket.etf5).
                                                                                                        SetProperty(u => u.etf9, ticket.etf9).
                                                                                                        SetProperty(u => u.etf94, ticket.etf94).
                                                                                                        SetProperty(u => u.etf95, ticket.etf95).
                                                                                                        SetProperty(u => u.etfb, ticket.etfb).
                                                                                                        SetProperty(u => u.etfbv, ticket.etfbv).
                                                                                                        SetProperty(u => u.etfc, ticket.etfc).
                                                                                                        SetProperty(u => u.etfcv, ticket.etfcv).
                                                                                                        SetProperty(u => u.hp, ticket.hp).
                                                                                                        SetProperty(u => u.ht, ticket.ht).
                                                                                                        SetProperty(u => u.kanpr, ticket.kanpr).
                                                                                                        SetProperty(u => u.klobsl, ticket.klobsl).
                                                                                                        SetProperty(u => u.oldTicket, ticket.oldTicket).
                                                                                                        SetProperty(u => u.opertype, ticket.opertype).
                                                                                                        SetProperty(u => u.refundResultNdsSum, ticket.refundResultNdsSum).
                                                                                                        SetProperty(u => u.refundResultSum, ticket.refundResultSum).
                                                                                                        SetProperty(u => u.refundWithholdNdsSum, ticket.refundWithholdNdsSum).
                                                                                                        SetProperty(u => u.result, ticket.result).
                                                                                                        SetProperty(u => u.resultNdsSum, ticket.resultNdsSum).
                                                                                                        SetProperty(u => u.resultSum, ticket.resultSum).
                                                                                                        SetProperty(u => u.starr, ticket.starr).
                                                                                                        SetProperty(u => u.stdep, ticket.stdep).
                                                                                                        SetProperty(u => u.stv1, ticket.stv1).
                                                                                                        SetProperty(u => u.stv2, ticket.stv2).
                                                                                                        SetProperty(u => u.stv3, ticket.stv3).
                                                                                                        SetProperty(u => u.tf, ticket.tf).
                                                                                                        SetProperty(u => u.tf3, ticket.tf3).
                                                                                                        SetProperty(u => u.tf4, ticket.tf4).
                                                                                                        SetProperty(u => u.tf5, ticket.tf5).
                                                                                                        SetProperty(u => u.tf9, ticket.tf9).
                                                                                                        SetProperty(u => u.tf93, ticket.tf93).
                                                                                                        SetProperty(u => u.tf94, ticket.tf94).
                                                                                                        SetProperty(u => u.tf95, ticket.tf95).
                                                                                                        SetProperty(u => u.tf9A, ticket.tf9A).
                                                                                                        SetProperty(u => u.tf9B, ticket.tf9B).
                                                                                                        SetProperty(u => u.tfa, ticket.tfa).
                                                                                                        SetProperty(u => u.tfb, ticket.tfb).
                                                                                                        SetProperty(u => u.vidr, ticket.vidr).
                                                                                                        SetProperty(u => u.year, ticket.year).
                                                                                                        SetProperty(u => u.month, ticket.month).
                                                                                                        SetProperty(u => u.timeoper, ticket.timeoper).
                                                                                                        SetProperty(u => u.timestop, ticket.timestop).
                                                                                                        SetProperty(u => u.train, ticket.train).
                                                                                                        SetProperty(u => u.vag, ticket.vag).
                                                                                                        SetProperty(u => u.valut, ticket.valut).
                                                                                                        SetProperty(u => u.time, ticket.time));
                }

                var numbers = ticketsToAdd.Select(x => x.ticketNumber).ToList();
                var existsTickets = db.BaseTickets.Where(x => numbers.Contains(x.ticketNumber)).ToList();
                var existsToUpdate = new List<TicketTotalInfo>();
                if (existsTickets.Any())
                {
                    foreach (var ticket in existsTickets)
                    {
                        existsToUpdate.Add(ticketTotals.First(x => x.ticketNumber == ticket.ticketNumber));
                        ticketsToAdd.RemoveAll(x => x.ticketNumber == ticket.ticketNumber);
                    }
                }
                foreach (var ticket in existsToUpdate)
                {
                    db.BaseTickets.Where(x => x.ticketNumber == ticket.ticketNumber).ExecuteUpdate(s => s.SetProperty(u => u.status, ticket.status).
                                                                                                        SetProperty(u => u.essv, ticket.essv).
                                                                                                        SetProperty(u => u.ess, ticket.ess).
                                                                                                        SetProperty(u => u.cartype, ticket.cartype).
                                                                                                        SetProperty(u => u.dateoper, ticket.dateoper).
                                                                                                        SetProperty(u => u.datestop, ticket.datestop).
                                                                                                        SetProperty(u => u.er, ticket.er).
                                                                                                        SetProperty(u => u.esk, ticket.esk).
                                                                                                        SetProperty(u => u.eskv, ticket.eskv).
                                                                                                        SetProperty(u => u.eskv1, ticket.eskv1).
                                                                                                        SetProperty(u => u.eskvv, ticket.eskvv).
                                                                                                        SetProperty(u => u.error, ticket.error).
                                                                                                        SetProperty(u => u.etf, ticket.etf).
                                                                                                        SetProperty(u => u.etf4, ticket.etf4).
                                                                                                        SetProperty(u => u.etf5, ticket.etf5).
                                                                                                        SetProperty(u => u.etf9, ticket.etf9).
                                                                                                        SetProperty(u => u.etf94, ticket.etf94).
                                                                                                        SetProperty(u => u.etf95, ticket.etf95).
                                                                                                        SetProperty(u => u.etfb, ticket.etfb).
                                                                                                        SetProperty(u => u.etfbv, ticket.etfbv).
                                                                                                        SetProperty(u => u.etfc, ticket.etfc).
                                                                                                        SetProperty(u => u.etfcv, ticket.etfcv).
                                                                                                        SetProperty(u => u.hp, ticket.hp).
                                                                                                        SetProperty(u => u.ht, ticket.ht).
                                                                                                        SetProperty(u => u.kanpr, ticket.kanpr).
                                                                                                        SetProperty(u => u.klobsl, ticket.klobsl).
                                                                                                        SetProperty(u => u.oldTicket, ticket.oldTicket).
                                                                                                        SetProperty(u => u.opertype, ticket.opertype).
                                                                                                        SetProperty(u => u.refundResultNdsSum, ticket.refundResultNdsSum).
                                                                                                        SetProperty(u => u.refundResultSum, ticket.refundResultSum).
                                                                                                        SetProperty(u => u.refundWithholdNdsSum, ticket.refundWithholdNdsSum).
                                                                                                        SetProperty(u => u.result, ticket.result).
                                                                                                        SetProperty(u => u.resultNdsSum, ticket.resultNdsSum).
                                                                                                        SetProperty(u => u.resultSum, ticket.resultSum).
                                                                                                        SetProperty(u => u.starr, ticket.starr).
                                                                                                        SetProperty(u => u.stdep, ticket.stdep).
                                                                                                        SetProperty(u => u.stv1, ticket.stv1).
                                                                                                        SetProperty(u => u.stv2, ticket.stv2).
                                                                                                        SetProperty(u => u.stv3, ticket.stv3).
                                                                                                        SetProperty(u => u.tf, ticket.tf).
                                                                                                        SetProperty(u => u.tf3, ticket.tf3).
                                                                                                        SetProperty(u => u.tf4, ticket.tf4).
                                                                                                        SetProperty(u => u.tf5, ticket.tf5).
                                                                                                        SetProperty(u => u.tf9, ticket.tf9).
                                                                                                        SetProperty(u => u.tf93, ticket.tf93).
                                                                                                        SetProperty(u => u.tf94, ticket.tf94).
                                                                                                        SetProperty(u => u.tf95, ticket.tf95).
                                                                                                        SetProperty(u => u.tf9A, ticket.tf9A).
                                                                                                        SetProperty(u => u.tf9B, ticket.tf9B).
                                                                                                        SetProperty(u => u.tfa, ticket.tfa).
                                                                                                        SetProperty(u => u.tfb, ticket.tfb).
                                                                                                        SetProperty(u => u.vidr, ticket.vidr).
                                                                                                        SetProperty(u => u.year, ticket.year).
                                                                                                        SetProperty(u => u.month, ticket.month).
                                                                                                        SetProperty(u => u.timeoper, ticket.timeoper).
                                                                                                        SetProperty(u => u.timestop, ticket.timestop).
                                                                                                        SetProperty(u => u.train, ticket.train).
                                                                                                        SetProperty(u => u.vag, ticket.vag).
                                                                                                        SetProperty(u => u.valut, ticket.valut).
                                                                                                        SetProperty(u => u.time, ticket.time));
                    var item1 = db.BaseTickets.Where(x => x.ticketNumber == ticket.ticketNumber);
                }

                db.BaseTickets.AddRange(ticketsToAdd);
                await db.SaveChangesAsync();
            }
        }
        private async Task AddErrorTicketsToDb(List<Ticket> errorTickets)
        {
            if (errorTickets.Count != 0)
            {
                foreach (var ticket in errorTickets)
                {
                    ErrorsFinal.Add(ticket);
                }

                var tmp = new List<TicketTotalInfo>();
                foreach (var ticket in errorTickets)
                {
                    tmp.Add(new TicketTotalInfo
                    {
                        FIO = ticket.FIO,
                        ticketNumber = ticket.Number,
                        date = ticket.Date.ToString("yyyy-MM-dd"),
                        month = ticket.Date.ToString("MM"),
                        year = ticket.Date.ToString("yyyy"),
                        period = ticket.Period,
                        status = "error",
                        error = ticket.Error
                    });
                }
                var numbers = tmp.Select(x => x.ticketNumber).ToList();
                var existsTickets = db.BaseTickets.Where(x => numbers.Contains(x.ticketNumber)).ToList();
                if (existsTickets.Any())
                {
                    foreach (var ticket in existsTickets)
                    {
                        tmp.RemoveAll(x => x.ticketNumber == ticket.ticketNumber);
                    }
                }
                db.BaseTickets.AddRange(tmp);
                await db.SaveChangesAsync();
            }
            tbTicketsWithErors.Text = ErrorsFinal.Count.ToString();
            tbDownloadedTickets.Text = (Tickets.Count - ErrorsFinal.Count).ToString();
            tbTicketsToDownload.Text = Tickets.Count.ToString();
            tbWarning.Text = "Процесс завершен";
            tbWarning.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Green);
            myPbar.Value = 0;
            myTblock.Text = string.Empty;
            MessageBox.Show("Данные загружены", "Загрузка", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SearchTickets.Clear();
            List<TicketTotalInfo> tmp = new List<TicketTotalInfo>();
            if (tboxMyNumber.Text.Length > 0)
            {
                var searchString = "%"+tboxMyNumber.Text + "%";
                try
                {
                    tmp = db.BaseTickets.FromSqlInterpolated($"SELECT * FROM BaseTickets WHERE ticketNumber like {searchString}").ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                if (BeginDate == null || EndDate == null)
                {
                    MessageBox.Show("Не указан период поиска!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (BeginDate > EndDate)
                {
                    MessageBox.Show("Окончание периода не может быть меньше его начала!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                try
                {
                    using var db = new TicketContext();
                    if (rbtnAll.IsChecked == true)
                        tmp = db.BaseTickets.FromSqlInterpolated($"SELECT * FROM BaseTickets WHERE date BETWEEN {BeginDate.ToString("yyyy-MM-dd")} AND {EndDate.ToString("yyyy-MM-dd")}").ToList();
                    string searchStatus;
                    if (rbtnNotUsed.IsChecked == true)
                    {
                        searchStatus = "notUsed";
                        tmp = db.BaseTickets.FromSqlInterpolated($"SELECT * FROM BaseTickets WHERE status = {searchStatus} AND date BETWEEN {BeginDate.ToString("yyyy-MM-dd")} AND {EndDate.ToString("yyyy-MM-dd")}").ToList();
                    }
                    if (rbtnUsed.IsChecked == true)
                    {
                        searchStatus = "used";
                        tmp = db.BaseTickets.FromSqlInterpolated($"SELECT * FROM BaseTickets WHERE status = {searchStatus} AND date BETWEEN {BeginDate.ToString("yyyy-MM-dd")} AND {EndDate.ToString("yyyy-MM-dd")}").ToList();
                    }
                    if (rbtnReturned.IsChecked == true)
                    {
                        searchStatus = "returned";
                        tmp = db.BaseTickets.FromSqlInterpolated($"SELECT * FROM BaseTickets WHERE status = {searchStatus} AND date BETWEEN {BeginDate.ToString("yyyy-MM-dd")} AND {EndDate.ToString("yyyy-MM-dd")}").ToList();
                    }
                    if (rbtnStolen.IsChecked == true)
                    {
                        searchStatus = "stolen";
                        tmp = db.BaseTickets.FromSqlInterpolated($"SELECT * FROM BaseTickets WHERE status = {searchStatus} AND date BETWEEN {BeginDate.ToString("yyyy-MM-dd")} AND {EndDate.ToString("yyyy-MM-dd")}").ToList();
                    }

                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            foreach (var t in tmp)
            {
                SearchTickets.Add(t);
            }
            tboxMyNumber.Text = tboxMyNumber.Text.Trim();
        }
        private void SearchExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            if (saveFileDialog.ShowDialog() == true)
            {
                var TicketType = typeof(TicketTotalInfo);
                var wb = new XLWorkbook();
                var sh = wb.Worksheets.Add("Tickets");
                int col = 1;
                foreach (var prop in TicketType.GetProperties())
                {
                    sh.Cell(1, col).SetValue(prop.Name);
                    col++;
                }

                for (int i = 0; i < SearchTickets.Count; i++)
                {
                    for (int j = 0; j < TicketType.GetProperties().Length; j++)
                    {
                        sh.Cell(i + 2, j + 1).SetValue(TicketType.GetProperties()[j].GetValue(SearchTickets[i])?.ToString());
                    }
                }
                try
                {
                    wb.SaveAs(saveFileDialog.FileName);
                    MessageBox.Show("Данные выгружены", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
        private async void ErrorFinalsReload_Click(object sender, RoutedEventArgs e)
        {
            if (ErrorsFinal.Count > 0)
            {
                HistoryToDowloadTickets.Clear();
                TicketTotals.Clear();
                ErrorTickets.Clear();
                Tickets.Clear();

                foreach (var ticket in ErrorsFinal)
                {
                    Tickets.Add(ticket);
                }
                ErrorsFinal.Clear();

                await GetInfoFromRZD(Tickets);
                await ReloadTicketTask();
                await ReloadErrorTicketTask();
                await AddTicketsToDb(TicketTotals);
                await AddErrorTicketsToDb(ErrorTickets);
            }
        }

    }
}

