using RealEstatePay.Models;
using RealEstatePay.Services.Interface;
using RealEstatePay.Services.Implementation;
using RealEstatePay.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddHttpClient<IHttpClientService, HttpClientService>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>();
builder.Services.AddScoped<IPaymentSmsService, PaymentSmsService>();
builder.Services.AddSingleton<ILoggerService, LoggerService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/PaymentSms/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseMiddleware<LoggingMiddleware>();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PaymentSms}/{action=Login}/{id?}");

app.Run();
