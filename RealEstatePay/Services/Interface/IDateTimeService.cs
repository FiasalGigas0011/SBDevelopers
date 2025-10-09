namespace RealEstatePay.Services.Interface
{
    public interface IDateTimeService
    {
        DateTime ConvertToIst(DateTime utcDateTime);
        string GetDateDisplay(DateTime dateTime);
        DateTime Now { get; }
    }
}