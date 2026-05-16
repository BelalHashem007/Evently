using System.ComponentModel.DataAnnotations;

namespace EventBookingSystem.Areas.Admin.ViewModels
{
    public class TicketTypeFormViewModel 
    {
        public int Id { get; set; }

        [MaxLength(100), Required(ErrorMessage = "Ticket Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ticket Price is required"), Range(0, double.MaxValue, ErrorMessage = "Price must be zero or greater.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Ticket Quantity is required"), Range(0, int.MaxValue, ErrorMessage = "Quantity must be zero or greater.")]
        public int Quantity { get; set; }

        public bool Remove { get; set; }

        public bool HasBookingItems { get; set; }
    }
}
