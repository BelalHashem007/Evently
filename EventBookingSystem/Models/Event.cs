using System.ComponentModel.DataAnnotations;

namespace EventBookingSystem.Models
{
    public class Event
    {
        public int Id { get; set; }
        [MaxLength(100), Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Venue { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsCancelled { get; set; }

        public List<TicketType> TicketTypes { get; set; } = [];
    }
}
