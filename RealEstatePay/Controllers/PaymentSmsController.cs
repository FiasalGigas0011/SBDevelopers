using Microsoft.AspNetCore.Mvc;
using RealEstatePay.Models;
using System.Diagnostics;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace RealEstatePay.Controllers
{
    public class PaymentSmsController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {

            if (username == "admin" && password == "admin")
                return RedirectToAction("Dashboard");

            ViewBag.Error = "Invalid username or password";
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Dashboard(PaymentModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please fill all required fields correctly.";
                return View(model);
            }

            try
            {
                string accountSid = "ACb834b71b683a6d176c41709649cb2042";
                string authToken = "7d505753987ad80e15ddef6199d8d9af";
                TwilioClient.Init(accountSid, authToken);

                var message = MessageResource.Create(
                    to: new PhoneNumber(model.ContactNumber),
                    from: new PhoneNumber("+19707167566"),
                    body: $"Hello {model.CustomerName}, you have paid ₹{model.AmountPaid} for Plot {model.PlotNumber} on {DateTime.Now:dd-MM-yyyy HH:mm} successfully."
                );

                ViewBag.Message = "SMS sent successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failed to send SMS: " + ex.Message;
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            return RedirectToAction("Login", "PaymentSms");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
