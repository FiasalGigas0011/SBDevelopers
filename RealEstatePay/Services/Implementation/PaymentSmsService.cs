using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RealEstatePay.Models;
using RealEstatePay.Services.Interface;

namespace RealEstatePay.Services.Implementation
{
    public class PaymentSmsService : IPaymentSmsService
    {
        private readonly AppSettings _appSettings;
        private readonly IHttpClientService _httpClientService;
        private readonly IDateTimeService _dateTimeService;
        private const int PageSize = 10;

        public PaymentSmsService(
            IOptionsMonitor<AppSettings> appSettings, 
            IHttpClientService httpClientService,
            IDateTimeService dateTimeService)
        {
            _appSettings = appSettings.CurrentValue;
            _httpClientService = httpClientService;
            _dateTimeService = dateTimeService;
        }

        public async Task<bool> SendSmsAsync(PaymentModel model)
        {
            try
            {
                var message = BuildSmsMessage(model);
                var requestBody = new { phoneNumber = model.ContactNumber, message };
                var response = await _httpClientService.PostAsync(_appSettings.SmsApiUrl, requestBody);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(List<ContactModel> contacts, int totalPages, bool hasPrevious, bool hasNext)> GetContactsAsync(int page)
        {
            try
            {
                var response = await _httpClientService.GetAsync("https://api.mybitproperty.com/api/Payment/get-contacts");
                if (!response.IsSuccessStatusCode)
                    return (new List<ContactModel>(), 0, false, false);

                var jsonContent = await response.Content.ReadAsStringAsync();
                var allContacts = JsonConvert.DeserializeObject<List<ContactModel>>(jsonContent) ?? new List<ContactModel>();
                ConvertContactsToIst(allContacts);
                return PaginateContacts(allContacts, page);
            }
            catch
            {
                return (new List<ContactModel>(), 0, false, false);
            }
        }

        private string BuildSmsMessage(PaymentModel model)
        {
            return $"Hello {model.CustomerName}, you have paid {model.AmountPaid} Rs. for Plot {model.PlotNumber} {model.SiteOrLayoutName} {model.LayoutNumber} on {_dateTimeService.Now:dd-MM-yyyy HH:mm} to {_appSettings.CompanyName} successfully.";
        }

        private void ConvertContactsToIst(List<ContactModel> contacts)
        {
            foreach (var contact in contacts)
            {
                contact.CreatedAt = _dateTimeService.ConvertToIst(contact.CreatedAt);
            }
        }

        private (List<ContactModel> contacts, int totalPages, bool hasPrevious, bool hasNext) PaginateContacts(List<ContactModel> allContacts, int page)
        {
            var totalContacts = allContacts.Count;
            var totalPages = (int)Math.Ceiling((double)totalContacts / PageSize);
            var contacts = allContacts
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            return (contacts, totalPages, page > 1, page < totalPages);
        }
    }
}