namespace RealEstatePay.Services.Interface
{
    public interface IHttpClientService
    {
        Task<HttpResponseMessage> PostAsync(string url, object requestBody);
        Task<HttpResponseMessage> GetAsync(string url);
    }
}