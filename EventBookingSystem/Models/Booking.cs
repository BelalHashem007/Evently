using System.ComponentModel.DataAnnotations.Schema;

namespace EventBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public DateTime ExpiresAt { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("Event")]
        public int EventId { get; set; }
        public ApplicationUser User { get; set; } = null!;
        public Event Event { get; set; } = null!;
        public ICollection<BookingItem> BookingItems { get; set; } = [];
    }
}
