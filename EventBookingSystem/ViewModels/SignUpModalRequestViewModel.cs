namespace EventBookingSystem.ViewModels
{
    public class SignUpModalRequestViewModel
    {
        public SignUpViewModel SignUp { get; set; } = new();

        public string? ReturnUrl { get; set; }
    }
}
