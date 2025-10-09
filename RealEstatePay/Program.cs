var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.Configure<RealEstatePay.Models.AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddHttpClient<RealEstatePay.Services.Interface.IHttpClientService, RealEstatePay.Services.Implementation.HttpClientService>();
builder.Services.AddScoped<RealEstatePay.Services.Interface.IDateTimeService, RealEstatePay.Services.Implementation.DateTimeService>();
builder.Services.AddScoped<RealEstatePay.Services.Interface.IPaymentSmsService, RealEstatePay.Services.Implementation.PaymentSmsService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/PaymentSms/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PaymentSms}/{action=Login}/{id?}");

app.Run();
