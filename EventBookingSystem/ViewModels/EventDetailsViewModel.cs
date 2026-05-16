namespace EventBookingSystem.ViewModels
{
    public class EventDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Venue { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsCancelled { get; set; }
        public IReadOnlyList<TicketTypeCardViewModel> TicketTypes { get; set; } = [];
    }
}
