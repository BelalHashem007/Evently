using System.ComponentModel.DataAnnotations.Schema;

namespace EventBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("Event")]
        public int EventId { get; set; }
        public ApplicationUser User { get; set; }
        public Event Event { get; set; }
    }
}
