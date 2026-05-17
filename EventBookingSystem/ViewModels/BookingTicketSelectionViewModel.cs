using System.ComponentModel.DataAnnotations;

namespace EventBookingSystem.ViewModels
{
    public class BookingTicketSelectionViewModel
    {
        [Required]
        public int TicketTypeId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }
    }
}
