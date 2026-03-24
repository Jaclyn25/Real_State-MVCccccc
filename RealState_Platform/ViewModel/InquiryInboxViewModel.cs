namespace RealState_Platform.ViewModel
{
    public class InquiryInboxViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PropertyTitle { get; set; }
        public int PropertyId { get; set; }
        public string? UserEmail { get; set; }
    }
}
