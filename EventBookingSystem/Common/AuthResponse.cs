namespace EventBookingSystem.Common
{
    public sealed class AuthResponse
    {
        public bool Succeeded { get; init; }

        public string? RedirectUrl { get; init; }

        public Dictionary<string, string[]> Errors { get; init; } = [];

        public static AuthResponse Success(string redirectUrl)
        {
            return new AuthResponse
            {
                Succeeded = true,
                RedirectUrl = redirectUrl
            };
        }

        public static AuthResponse Failed(Dictionary<string, string[]> errors)
        {
            return new AuthResponse
            {
                Succeeded = false,
                Errors = errors
            };
        }
    }
}
