using System;
namespace ManageGo.Models
{
    public class DashboardResponseItem
    {
        public decimal TotalPaymentsThisWeek { get; set; }
        public decimal TotalPaymentsThisMonth { get; set; }
        public int NumberOfOpenTickets { get; set; }
        public int NumberOfTicketsWithNoReplay { get; set; }
    }
}
