using System.ComponentModel.DataAnnotations.Schema;

namespace EventBookingSystem.Models
{
    public class BookingItem
    {
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public int Quantity { get; set; }

        [ForeignKey("Booking")]
        public int BookingId { get; set; }
        [ForeignKey("TicketType")]
        public int TicketTypeId { get; set; }
        public Booking Booking { get; set; }
        public TicketType TicketType { get; set; }
    }
}
