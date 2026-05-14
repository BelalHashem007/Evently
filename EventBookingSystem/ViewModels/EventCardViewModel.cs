namespace EventBookingSystem.ViewModels
{
    public class EventCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Venue { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string PriceRange { get; set; } = string.Empty;
    }
}
