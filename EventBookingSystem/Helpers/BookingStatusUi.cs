using EventBookingSystem.Models;

namespace EventBookingSystem.Helpers
{
    public static class BookingStatusUi
    {
        public static string BadgeClass(BookingStatus status) => status switch
        {
            BookingStatus.Pending => "text-bg-warning",
            BookingStatus.Confirmed => "text-bg-success",
            BookingStatus.Cancelled => "text-bg-danger",
            BookingStatus.Expired => "text-bg-dark",
            _ => "text-bg-secondary"
        };
    }
}
