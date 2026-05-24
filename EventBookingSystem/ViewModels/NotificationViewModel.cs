namespace EventBookingSystem.ViewModels
{
    //creation
    public class NotificationCreationViewModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }

    public class NotificationCreationViewModelValidator
    {
        public bool Validate(NotificationCreationViewModel model)
        {
            //emtpy string or whitespaces
            if (string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.Message))
                return false;

            //check length
            if (model.Title.Length > 50 || model.Message.Length > 200)
                return false;

            return true;
        }

    }

    //view
    public class NotificationViewModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class NotificationPageViewModel
    {
        public IEnumerable<NotificationViewModel> Notifications { get; set; } = [];
        public bool IsEnd { get; set; }
    }
}
