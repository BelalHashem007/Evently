namespace EventBookingSystem.Areas.Admin.ViewModels
{
    public class AdminEventListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Venue { get; set; } = string.Empty;
        public bool IsCancelled { get; set; }
        public int TicketTypeCount { get; set; }
    }
}
