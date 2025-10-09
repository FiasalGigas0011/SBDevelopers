using Newtonsoft.Json;
using RealEstatePay.Services.Interface;
using System.Text;

namespace RealEstatePay.Services.Implementation
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _httpClient;

        public HttpClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("accept", "*/*");
        }

        public async Task<HttpResponseMessage> PostAsync(string url, object requestBody)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json");

            return await _httpClient.PostAsync(url, content);
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await _httpClient.GetAsync(url);
        }
    }
}