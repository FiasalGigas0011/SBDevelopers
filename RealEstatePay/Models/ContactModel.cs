namespace RealEstatePay.Models
{
    public class ContactModel
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string TextColumn { get; set; }
        public bool Tag { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}