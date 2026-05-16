using System.ComponentModel.DataAnnotations;

namespace EventBookingSystem.Areas.Admin.ViewModels
{
    public class EventFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Event name is required.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; } = DateTime.Today.AddDays(7);

        [Required(ErrorMessage = "Venue is required.")]
        public string Venue { get; set; } = string.Empty;

        [Display(Name = "Image URL")]
        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        public string? ImageUrl { get; set; }

        public List<TicketTypeFormViewModel> TicketTypes { get; set; } = [];
    }
}
