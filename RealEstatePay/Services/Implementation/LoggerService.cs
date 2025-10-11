using RealEstatePay.Services.Interface;
namespace RealEstatePay.Services.Implementation
{
    public class LoggerService : ILoggerService
    {
        private readonly string _logPath;
        public LoggerService()
        {
            _logPath = Path.Combine(Directory.GetCurrentDirectory(), "SLog");
            if (!Directory.Exists(_logPath))
                Directory.CreateDirectory(_logPath);
            CleanupOldLogs();
        }
        public void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }
        public void LogError(string message, Exception ex = null)
        {
            var errorMessage = ex != null ? $"{message} - Exception: {ex.Message}" : message;
            WriteLog("ERROR", errorMessage);
        }
        public void LogWarning(string message)
        {
            WriteLog("WARNING", message);
        }
        private void WriteLog(string level, string message)
        {
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            var fileName = $"{today}_{level}.txt";
            var filePath = Path.Combine(_logPath, fileName);
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level}: {message}";
            File.AppendAllText(filePath, logEntry + Environment.NewLine);
        }
        private void CleanupOldLogs()
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-15);
                var files = Directory.GetFiles(_logPath, "*.txt");
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch
            {
            }
        }
    }
}