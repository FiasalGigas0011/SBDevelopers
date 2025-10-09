namespace RealEstatePay.Models
{
    public class AppSettings
    {
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string SmsApiUrl { get; set; }
        public string CompanyName { get; set; }
        public string ValidationErrorMessage { get; set; }
        public string SmsSuccessMessage { get; set; }
        public string SmsFailureMessage { get; set; }
        public string ApiErrorMessage { get; set; }
        public string LoginErrorMessage { get; set; }
    }
}