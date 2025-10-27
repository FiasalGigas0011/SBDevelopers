using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
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
        private readonly string _connectionString =
        "Server=31.97.232.52,1433;Database=SBDEV;User ID=SA;Password=Elizabetholson@123;TrustServerCertificate=True;MultipleActiveResultSets=true;";


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
                // Step 1??: Build message text
                var message = BuildSmsMessage(model);

                // Step 2??: Save SMS details in database using ADO.NET
                await SaveSmsToDatabaseAsync(model);

                return true; // Successfully saved to DB
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while saving SMS: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SaveSmsToDatabaseAsync(PaymentModel model)
        {
            try
            {
                // ? Build SMS message text
                string smsMessage = BuildSmsMessage(model);

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // ? Use parameterized query to prevent SQL Injection
                    string query = @"
                    INSERT INTO Contacts 
                    (CustomerName, PlotNumber, SiteOrLayoutName, LayoutNumber, ContactNumber, Message, AmountPaid, CreatedDate)
                    VALUES 
                    (@CustomerName, @PlotNumber, @SiteOrLayoutName, @LayoutNumber, @ContactNumber, @Message, @AmountPaid, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        // Add all parameters safely
                        cmd.Parameters.AddWithValue("@CustomerName", model.CustomerName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@PlotNumber", model.PlotNumber ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@SiteOrLayoutName", model.SiteOrLayoutName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@LayoutNumber", model.LayoutNumber ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactNumber", model.ContactNumber ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Message", model.Message ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@AmountPaid", model.AmountPaid);
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving payment: {ex.Message}");
                return false;
            }
        }

        public async Task<(List<ContactModelVM> contacts, int totalPages, bool hasPrevious, bool hasNext)> GetContactsAsync(int page)
         {
             var contacts = new List<ContactModelVM>();
             string connectionString = _connectionString;

             try
             {
                 using (SqlConnection connection = new SqlConnection(connectionString))
                 {
                     await connection.OpenAsync();

                     string query = "SELECT * FROM Contacts"; // ?? Change table name & columns as per your database
                     using (SqlCommand cmd = new SqlCommand(query, connection))
                     {
                         using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                         {
                             while (await reader.ReadAsync())
                             {
                                 contacts.Add(new ContactModelVM
                                 {
                                     Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                                     PhoneNumber = reader["ContactNumber"]?.ToString() ?? "",
                                     TextColumn = reader["Message"]?.ToString(),
                                     Tag = reader["Tag"] != DBNull.Value && Convert.ToBoolean(reader["Tag"]),
                                     CreatedAt = reader["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedDate"]) : DateTime.MinValue
                                 });
                             }
                         }
                     }
                 }

                 // Convert UTC to IST if needed
                 ConvertContactsToIst(contacts);

                 // Apply pagination
                 return PaginateContacts(contacts, page);
             }
             catch (Exception ex)
             {
                 // Optional: Log exception
                 Console.WriteLine($"Error fetching contacts: {ex.Message}");
                 return (new List<ContactModelVM>(), 0, false, false);
             }
         }

        /*public async Task<(List<ContactModelVM> contacts, int totalPages, bool hasPrevious, bool hasNext)> GetContactsAsync(int page)
        {
            try
            {
                var response = await _httpClientService.GetAsync("https://api.mybitproperty.com/api/Payment/get-contacts");
                if (!response.IsSuccessStatusCode)
                    return (new List<ContactModelVM>(), 0, false, false);

                var jsonContent = await response.Content.ReadAsStringAsync();
                var allContacts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ContactModelVM>>(jsonContent) ?? new List<ContactModelVM>();
                ConvertContactsToIst(allContacts);
                return PaginateContacts(allContacts, page);
            }
            catch
            {
                return (new List<ContactModelVM>(), 0, false, false);
            }
        }*/

        private string BuildSmsMessage(PaymentModel model)
        {
            return $"Hello {model.CustomerName}, you have paid {model.AmountPaid} Rs. for Plot {model.PlotNumber} {model.SiteOrLayoutName} {model.LayoutNumber} on {_dateTimeService.Now:dd-MM-yyyy HH:mm} to {_appSettings.CompanyName} successfully.";
        }

        private void ConvertContactsToIst(List<ContactModelVM> contacts)
        {
            foreach (var contact in contacts)
            {
                contact.CreatedAt = _dateTimeService.ConvertToIst(contact.CreatedAt);
            }
        }

        private (List<ContactModelVM> contacts, int totalPages, bool hasPrevious, bool hasNext) PaginateContacts(List<ContactModelVM> allContacts, int page)
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