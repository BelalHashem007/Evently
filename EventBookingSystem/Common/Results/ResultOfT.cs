namespace EventBookingSystem.Common.Results
{
    public sealed class Result<T>
    {
        public bool Succeeded { get; init; }
        public T? Value { get; init; }
        public string? ErrorMessage { get; init; }
        public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

        public static Result<T> Success(T value) => new()
        {
            Succeeded = true,
            Value = value
        };

        public static Result<T> Failure(string errorMessage) => new()
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };

        public static Result<T> Failure(IReadOnlyDictionary<string, string[]> validationErrors) => new()
        {
            Succeeded = false,
            ValidationErrors = validationErrors
        };
    }
}
