using RealEstatePay.Services.Interface;

namespace RealEstatePay.Services.Implementation
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime Now => DateTime.Now;

        public DateTime ConvertToIst(DateTime utcDateTime)
        {
            return utcDateTime.AddHours(12).AddMinutes(30);
        }

        public string GetDateDisplay(DateTime dateTime)
        {
            var istToday = ConvertToIst(Now).Date;
            var contactDate = dateTime.Date;

            return contactDate switch
            {
                var date when date == istToday => "Today",
                var date when date == istToday.AddDays(-1) => "Yesterday",
                _ => contactDate.ToString("dd-MM-yyyy")
            };
        }
    }
}