using System.ComponentModel.DataAnnotations;

namespace EventBookingSystem.Models
{
    public class Notification
    {
        public int Id { get; set; }
        [MaxLength(200), Required]
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
