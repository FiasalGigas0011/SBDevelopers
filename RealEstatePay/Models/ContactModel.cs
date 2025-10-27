namespace RealEstatePay.Models
{
    public class ContactModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? TextColumn { get; set; }
        public bool Tag { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ContactModelVM
    {
        /*public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string TextColumn { get; set; }
        public bool Tag { get; set; }
        public DateTime CreatedAt { get; set; }*/

        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string TextColumn { get; set; }
        public bool Tag { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}