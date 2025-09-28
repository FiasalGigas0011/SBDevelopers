namespace RealEstatePay.Models
{
    public class PaymentModel
    {
        public string PlotNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal RemainingAmount { get; set; }
        public string ContactNumber { get; set; }
    }
}
