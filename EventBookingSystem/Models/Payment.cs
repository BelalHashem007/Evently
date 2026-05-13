using System.ComponentModel.DataAnnotations.Schema;

namespace EventBookingSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public DateTime PaidAt { get; set; }
        public string StripeSessionId { get; set; }

        [ForeignKey("Booking")]
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
    }
}
