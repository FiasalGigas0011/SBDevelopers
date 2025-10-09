namespace RealEstatePay.Models
{
    public class PaymentModel
    {
        public string? PlotNumber { get; set; }
        public decimal AmountPaid { get; set; }
        public string ContactNumber { get; set; }
        public string? SiteOrLayoutName { get; set; }
        public string? LayoutNumber { get; set; }

        public string? Message { get; set; }
        public string? CustomerName { get; set; }
    }
}
