namespace EventBookingSystem.ViewModels
{
    public class PaginatedViewModel<T>
    {
        public required IReadOnlyList<T> Items { get; set; }
        public required int Page { get; set; }
        public required int TotalItems { get; set; }
        public required int MaxItemsPerPage { get; set; }
    }
}
