namespace EventBookingSystem.Areas.Admin.ViewModels
{
    public class AdminBookingListItemViewModel
    {
        public int Id { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
