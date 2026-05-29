using EventBookingSystem.Models;

namespace EventBookingSystem.Areas.Admin.ViewModels
{
    public class AdminBookingListItemViewModel
    {
        public int Id { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public BookingStatus Status { get; set; }
        public DateTime ExpiresAt { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
