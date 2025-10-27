using RealEstatePay.Models;

namespace RealEstatePay.Services.Interface
{
    public interface IPaymentSmsService
    {
        Task<bool> SendSmsAsync(PaymentModel model);
        Task<(List<ContactModelVM> contacts, int totalPages, bool hasPrevious, bool hasNext)> GetContactsAsync(int page);
    }
}