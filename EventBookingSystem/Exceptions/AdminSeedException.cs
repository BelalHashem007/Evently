namespace EventBookingSystem.Exceptions
{
    public class AdminSeedException : Exception
    {
        public AdminSeedException(string message) : base(message)
        {
        }

        public AdminSeedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
