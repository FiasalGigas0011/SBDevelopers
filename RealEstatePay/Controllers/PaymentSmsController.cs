using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RealEstatePay.Models;
using System.Diagnostics;
using System.Text;

namespace RealEstatePay.Controllers
{
    public class PaymentSmsController : Controller
    {
        private readonly IConfiguration _configuration;

        public PaymentSmsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {

            if (username == _configuration["AppSettings:AdminUsername"] && password == _configuration["AppSettings:AdminPassword"])
            {
                HttpContext.Session.SetString("IsLoggedIn", "true");
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = _configuration["AppSettings:LoginErrorMessage"];
            return View();
        }

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Dashboard(PaymentModel model)
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToAction("Login");
                
            if (!ModelState.IsValid)
            {
                ViewBag.Error = _configuration["AppSettings:ValidationErrorMessage"];
                return View(model);
            }

            try
            {

                var apiResponse = await SendSmsToApi(model.ContactNumber, $"Hello {model.CustomerName}, you have paid {model.AmountPaid} Rs. for Plot {model.PlotNumber} {model.SiteOrLayoutName} {model.LayoutNumber} on {DateTime.Now:dd-MM-yyyy HH:mm} to {_configuration["AppSettings:CompanyName"]} successfully.");


                if (apiResponse.IsSuccessStatusCode)
                {
                    ViewBag.Message = _configuration["AppSettings:SmsSuccessMessage"];
                }
                else
                {
                    ViewBag.Error = _configuration["AppSettings:SmsFailureMessage"];
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = _configuration["AppSettings:ApiErrorMessage"] + ex.Message;
            }

            return View(model);
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

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Contacts()
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToAction("Login");

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("accept", "*/*");
                    var response = await client.GetAsync("https://api.mybitproperty.com/api/Payment/get-contacts");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = await response.Content.ReadAsStringAsync();
                        var contacts = JsonConvert.DeserializeObject<List<ContactModel>>(jsonContent);
                        return View(contacts);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failed to load contacts: " + ex.Message;
            }

            return View(new List<ContactModel>());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
