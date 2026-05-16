namespace EventBookingSystem.Common.Results
{
    public sealed class Result
    {
        public bool Succeeded { get; init; }
        public string? ErrorMessage { get; init; }
        public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

        public static Result Success() => new() { Succeeded = true };

        public static Result Failure(string errorMessage) => new()
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };

        public static Result Failure(IReadOnlyDictionary<string, string[]> validationErrors) => new()
        {
            Succeeded = false,
            ValidationErrors = validationErrors
        };
    }
}
