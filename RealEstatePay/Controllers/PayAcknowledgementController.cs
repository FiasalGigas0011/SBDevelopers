using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RealEstatePay.Models;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace RealEstatePay.Controllers
{
    public class PayAcknowledgementController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Simple hardcoded login logic
            if (username == "admin" && password == "admin")
                return RedirectToAction("Dashboard");

            ViewBag.Error = "Invalid login";
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Dashboard(PaymentModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please fill all required fields correctly.";
                return View(model);
            }

            try
            {
                // Call external API (await the Task<HttpResponseMessage>)
                var apiResponse = await SendSmsToApi(model.ContactNumber, $"Hello {model.CustomerName}, you have paid {model.AmountPaid} Rs. for Plot {model.PlotNumber} {model.SiteOrLayoutName} {model.LayoutNumber} on {DateTime.Now:dd-MM-yyyy HH:mm} to Shakil Babu Developers successfully.");

                // Check the response status after awaiting the task
                if (apiResponse.IsSuccessStatusCode)
                {
                    ViewBag.Message = "SMS sent successfully via external API!";
                }
                else
                {
                    ViewBag.Error = "Failed to send SMS through external API.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred while calling the external API: " + ex.Message;
            }

            return View(model);
        }

        // Helper method to send SMS using the external API
        private async Task<HttpResponseMessage> SendSmsToApi(string phoneNumber, string message)
        {
            using (var client = new HttpClient())
            {
                // Set up the request headers (no Content-Type here)
                client.DefaultRequestHeaders.Add("accept", "*/*");

                // Create the JSON request body
                var requestBody = new
                {
                    phoneNumber = phoneNumber,
                    message = message
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json" // This sets the Content-Type properly
                );

                // Send the POST request
                var response = await client.PostAsync("https://api.mybitproperty.com/api/Payment/send-sms", content);

                return response;
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            //HttpContext.Session.Clear(); // if using session
                                         // Or sign out if using authentication schemes

            return RedirectToAction("Login", "PayAcknowledgement"); // Redirect to login page
        }

    }
}
