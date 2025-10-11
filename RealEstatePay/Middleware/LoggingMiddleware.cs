using RealEstatePay.Services.Interface;
namespace RealEstatePay.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerService _logger;
        public LoggingMiddleware(RequestDelegate next, ILoggerService logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var request = await FormatRequest(context.Request);
            _logger.LogInfo($"Request: {request}");
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            try
            {
                await _next(context);
                var response = await FormatResponse(context.Response);
                _logger.LogInfo($"Response: {response}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}", ex);
                throw;
            }
            finally
            {
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
        private async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            request.Body.Position = 0;
            return $"{request.Method} {request.Path} - Body: {body}";
        }
        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return $"Status: {response.StatusCode} - Body: {text.Substring(0, Math.Min(text.Length, 500))}";
        }
    }
}
