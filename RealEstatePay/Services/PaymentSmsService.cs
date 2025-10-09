using Newtonsoft.Json;
using RealEstatePay.Models;
using System.Text;

namespace RealEstatePay.Services
{
    public class PaymentSmsService : IPaymentSmsService
    {
        private readonly IConfiguration _configuration;

        public PaymentSmsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendSmsAsync(PaymentModel model)
        {
            try
            {
                var apiResponse = await SendSmsToApi(model.ContactNumber, 
                    $"Hello {model.CustomerName}, you have paid {model.AmountPaid} Rs. for Plot {model.PlotNumber} {model.SiteOrLayoutName} {model.LayoutNumber} on {DateTime.Now:dd-MM-yyyy HH:mm} to {_configuration["AppSettings:CompanyName"]} successfully.");

                return apiResponse.IsSuccessStatusCode;
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
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("accept", "*/*");
                    var response = await client.GetAsync("https://api.mybitproperty.com/api/Payment/get-contacts");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = await response.Content.ReadAsStringAsync();
                        var allContacts = JsonConvert.DeserializeObject<List<ContactModel>>(jsonContent);
                        
                        foreach (var contact in allContacts)
                        {
                            contact.CreatedAt = contact.CreatedAt.AddHours(12).AddMinutes(30);
                        }
                        
                        const int pageSize = 10;
                        var totalContacts = allContacts.Count;
                        var totalPages = (int)Math.Ceiling((double)totalContacts / pageSize);
                        
                        var contacts = allContacts
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
                        
                        return (contacts, totalPages, page > 1, page < totalPages);
                    }
                }
            }
            catch
            {
                // Return empty result on error
            }

            return (new List<ContactModel>(), 0, false, false);
        }

        private async Task<HttpResponseMessage> SendSmsToApi(string phoneNumber, string message)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("accept", "*/*");

                var requestBody = new
                {
                    phoneNumber = phoneNumber,
                    message = message
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(_configuration["AppSettings:SmsApiUrl"], content);
                return response;
            }
        }
    }
}