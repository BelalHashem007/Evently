namespace EventBookingSystem.ViewModels
{
    public class CreateBookingViewModel
    {
        public int EventId { get; set; }
        public List<BookingTicketSelectionViewModel> Tickets { get; set; } = [];
    }
}
