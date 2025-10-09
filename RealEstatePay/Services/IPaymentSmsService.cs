using RealEstatePay.Models;

namespace RealEstatePay.Services
{
    public interface IPaymentSmsService
    {
        Task<bool> SendSmsAsync(PaymentModel model);
        Task<(List<ContactModel> contacts, int totalPages, bool hasPrevious, bool hasNext)> GetContactsAsync(int page);
    }
}