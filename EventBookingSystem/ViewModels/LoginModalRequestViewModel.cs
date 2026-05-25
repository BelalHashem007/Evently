namespace EventBookingSystem.ViewModels
{
    public class LoginModalRequestViewModel
    {
        public LoginViewModel Login { get; set; } = new();

        public string? ReturnUrl { get; set; }
    }
}
