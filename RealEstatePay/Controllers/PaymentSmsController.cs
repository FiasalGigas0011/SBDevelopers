using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RealEstatePay.Models;
using RealEstatePay.Services.Interface;
using System.Diagnostics;
using System.Text.Json;

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
            if (username == _appSettings.AdminUsername && (password == _appSettings.AdminPassword || password == _appSettings.SuperAdminPassword))
            {
                HttpContext.Session.SetString("IsLoggedIn", "true");
                if (password == _appSettings.SuperAdminPassword)
                {
                    HttpContext.Session.SetString("IsSuperAdmin", "true");
                }
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

        public IActionResult Settings()
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true" || HttpContext.Session.GetString("IsSuperAdmin") != "true")
                return RedirectToAction("Login");
            
            // Ensure SiteLayoutOptions is not null
            if (_appSettings.SiteLayoutOptions == null)
                _appSettings.SiteLayoutOptions = new List<string>();
            
            return View(_appSettings);
        }

        [HttpPost]
        public async Task<IActionResult> Settings(AppSettings settings)
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true" || HttpContext.Session.GetString("IsSuperAdmin") != "true")
                return RedirectToAction("Login");

            try
            {
                var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                var json = await System.IO.File.ReadAllTextAsync(configPath);
                var config = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                
                var appSettingsSection = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(config["AppSettings"].ToString());
                
                appSettingsSection["AdminUsername"] = settings.AdminUsername;
                appSettingsSection["AdminPassword"] = settings.AdminPassword;
                appSettingsSection["SmsApiUrl"] = settings.SmsApiUrl;
                appSettingsSection["CompanyName"] = settings.CompanyName;
                appSettingsSection["ValidationErrorMessage"] = settings.ValidationErrorMessage;
                appSettingsSection["SmsSuccessMessage"] = settings.SmsSuccessMessage;
                appSettingsSection["SmsFailureMessage"] = settings.SmsFailureMessage;
                appSettingsSection["LoginErrorMessage"] = settings.LoginErrorMessage;
                appSettingsSection["SiteLayoutOptions"] = settings.SiteLayoutOptions;
                
                config["AppSettings"] = appSettingsSection;
                
                var updatedJson = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(configPath, updatedJson);
                
                ViewBag.Message = "Settings updated successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failed to update settings: " + ex.Message;
            }
            
            return View(settings);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
