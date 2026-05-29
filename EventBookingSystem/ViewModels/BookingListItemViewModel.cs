using EventBookingSystem.Models;

namespace EventBookingSystem.ViewModels
{
    public class BookingListItemViewModel
    {
        public int Id { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiresAt { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
