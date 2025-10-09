using Microsoft.AspNetCore.Mvc;
using RealEstatePay.Models;
using RealEstatePay.Services;
using System.Diagnostics;

namespace RealEstatePay.Controllers
{
    public class PaymentSmsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IPaymentSmsService _paymentSmsService;

        public PaymentSmsController(IConfiguration configuration, IPaymentSmsService paymentSmsService)
        {
            _configuration = configuration;
            _paymentSmsService = paymentSmsService;
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

            var success = await _paymentSmsService.SendSmsAsync(model);
            
            if (success)
            {
                ViewBag.Message = _configuration["AppSettings:SmsSuccessMessage"];
            }
            else
            {
                ViewBag.Error = _configuration["AppSettings:SmsFailureMessage"];
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
