namespace EventBookingSystem.Areas.Admin.ViewModels
{
    public class DashboardViewModel
    {
        public IReadOnlyList<DashboardStatCardViewModel> Stats { get; set; } = [];
        public IReadOnlyList<DashboardQuickActionViewModel> QuickActions { get; set; } = [];
    }
}
