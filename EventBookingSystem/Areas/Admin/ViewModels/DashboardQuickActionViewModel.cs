namespace EventBookingSystem.Areas.Admin.ViewModels
{
    public class DashboardQuickActionViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string CssClass { get; set; } = "btn btn-outline-primary";
    }
}
