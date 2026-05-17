namespace EventBookingSystem.ViewModels
{
    public class BookingItemDetailsViewModel
    {
        public string TicketTypeName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
