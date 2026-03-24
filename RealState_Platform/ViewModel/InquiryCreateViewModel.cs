namespace RealState_Platform.ViewModel
{
    public class InquiryCreateViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 500 characters")]
        public string Message { get; set; }

        public int PropertyId { get; set; }
    }
}
