namespace RealEstatePay.Models
{
    public class PaymentModel
    {
        public string? PlotNumber { get; set; }      // Optional
        public decimal AmountPaid { get; set; }
        public string ContactNumber { get; set; }    // Required
        public string? SiteOrLayoutName { get; set; } // Optional
        public string? LayoutNumber { get; set; }    // Optional
       // public string? Revenue { get; set; }         // Optional (BK to SB Dev)
        public string? Message { get; set; }         // Optional message
        public string? CustomerName { get; set; }    // Optional
    }
}
