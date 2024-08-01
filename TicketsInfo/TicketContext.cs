using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TicketsInfo.Models;

namespace TicketsInfo
{
    public class TicketContext : DbContext
    {
        public TicketContext()
        {
            try
            {
                Database.EnsureCreated();
            }
           catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(config.GetConnectionString("DefaultConnection"));
            }
        }
        public virtual DbSet<TicketTotalInfo> BaseTickets { get; set; }
    }
}
