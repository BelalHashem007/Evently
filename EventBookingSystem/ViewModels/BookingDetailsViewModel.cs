using EventBookingSystem.Models;

namespace EventBookingSystem.ViewModels
{
    public class BookingDetailsViewModel
    {
        public int Id { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Venue { get; set; } = string.Empty;
        public BookingStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public decimal TotalPrice { get; set; }
        public IReadOnlyList<BookingItemDetailsViewModel> Items { get; set; } = [];
    }
}
