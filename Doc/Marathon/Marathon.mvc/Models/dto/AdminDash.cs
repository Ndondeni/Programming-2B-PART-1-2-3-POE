namespace Marathon.mvc.Models.dto
{
    public class AdminDash
    {
        public class DashboardStatsViewModel
        {
            public int TotalEvents { get; set; }
            public int TotalCategories { get; set; }
            public int TotalUsers { get; set; }
            public int TotalAccounts { get; set; }
            public int TotalTickets { get; set; }
            public int TotalParticipants { get; set; }
            public int TotalPayments { get; set; }

            public int TicketsSold { get; set; }
            public decimal TotalRevenue { get; set; }
            public decimal RevenueThisMonth { get; set; }
        }

    }
}
