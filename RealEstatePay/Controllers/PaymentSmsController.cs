using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RealEstatePay.Models;
using RealEstatePay.Services.Interface;
using System.Diagnostics;

namespace RealEstatePay.Controllers
{
    public class PaymentSmsController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly IPaymentSmsService _paymentSmsService;

        public PaymentSmsController(IOptionsMonitor<AppSettings> appSettings, IPaymentSmsService paymentSmsService)
        {
            _appSettings = appSettings.CurrentValue;
            _paymentSmsService = paymentSmsService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username == _appSettings.AdminUsername && password == _appSettings.AdminPassword)
            {
                HttpContext.Session.SetString("IsLoggedIn", "true");
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = _appSettings.LoginErrorMessage;
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
                ViewBag.Error = _appSettings.ValidationErrorMessage;
                return View(model);
            }

            var success = await _paymentSmsService.SendSmsAsync(model);

            if (success)
            {
                ViewBag.Message = _appSettings.SmsSuccessMessage;
            }
            else
            {
                ViewBag.Error = _appSettings.SmsFailureMessage;
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Contacts(int page = 1)
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToAction("Login");

            try
            {
                var (contacts, totalPages, hasPrevious, hasNext) = await _paymentSmsService.GetContactsAsync(page);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.HasPrevious = hasPrevious;
                ViewBag.HasNext = hasNext;

                return View(contacts);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failed to load contacts: " + ex.Message;
                return View(new List<ContactModel>());
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
