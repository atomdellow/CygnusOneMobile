using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Maui.Storage;

namespace CygnusOneMobile.Services
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Exception,
        Debug
    }
    
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }

    public interface IDebugLogger
    {
        ObservableCollection<LogEntry> LogEntries { get; }
        void LogInfo(string message, string? details = null);
        void LogWarning(string message, string? details = null);
        void LogError(string message, string? details = null);
        void LogException(Exception ex, string? message = null);
        void Log(string message, LogLevel level = LogLevel.Info, string? details = null);
        void LogRequest(HttpRequestMessage request, string? additionalInfo = null);
        Task<string?> ExportLogs();
        void ClearLogs();
    }

    public class DebugLogger : IDebugLogger
    {
        private const int MaxLogEntries = 1000;
        public ObservableCollection<LogEntry> LogEntries { get; private set; }

        public DebugLogger()
        {
            LogEntries = new ObservableCollection<LogEntry>();
        }

        public void LogInfo(string message, string? details = null)
        {
            AddLogEntry("INFO", message, details);
        }

        public void LogWarning(string message, string? details = null)
        {
            AddLogEntry("WARNING", message, details);
        }

        public void LogError(string message, string? details = null)
        {
            AddLogEntry("ERROR", message, details);
        }

        public void LogException(Exception ex, string? message = null)
        {
            string exceptionMessage = message ?? "Exception occurred";
            string details = $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            
            if (ex.InnerException != null)
            {
                details += $"\nInner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }
            
            AddLogEntry("EXCEPTION", exceptionMessage, details);
        }
        
        public void Log(string message, LogLevel level = LogLevel.Info, string? details = null)
        {
            string levelString = level.ToString().ToUpper();
            AddLogEntry(levelString, message, details);
        }
        
        public void LogRequest(HttpRequestMessage request, string? additionalInfo = null)
        {
            if (request == null)
            {
                LogWarning("Attempted to log a null request");
                return;
            }
            
            string details = $"Method: {request.Method}\nUri: {request.RequestUri}";
            
            if (request.Content != null)
            {
                // Don't actually try to read the content as it may have already been read
                details += "\nContent: [Content present but not logged]";
            }
            
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                details += $"\nAdditional Info: {additionalInfo}";
            }
            
            AddLogEntry("REQUEST", $"HTTP Request: {request.Method} {request.RequestUri}", details);
        }

        public async Task<string?> ExportLogs()
        {
            try
            {
                string fileName = $"CygnusOne_Logs_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string logsFolderPath = Path.Combine(FileSystem.CacheDirectory, "Logs");
                
                if (!Directory.Exists(logsFolderPath))
                {
                    Directory.CreateDirectory(logsFolderPath);
                }

                string filePath = Path.Combine(logsFolderPath, fileName);
                
                // Use asynchronous file operations
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    await writer.WriteLineAsync($"CygnusOne Mobile App Logs - {DateTime.Now}");
                    await writer.WriteLineAsync("=========================================");
                    await writer.WriteLineAsync();

                    foreach (var log in LogEntries)
                    {
                        await writer.WriteLineAsync($"[{log.Timestamp}] [{log.Level}] {log.Message}");
                        if (!string.IsNullOrEmpty(log.Details))
                        {
                            await writer.WriteLineAsync($"Details: {log.Details}");
                            await writer.WriteLineAsync();
                        }
                    }
                }

                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to export logs: {ex.Message}");
                return null;
            }
        }

        public void ClearLogs()
        {
            LogEntries.Clear();
        }

        private void AddLogEntry(string level, string message, string? details)
        {
            // Enforce maximum log entries by removing oldest entries
            while (LogEntries.Count >= MaxLogEntries)
            {
                LogEntries.RemoveAt(0);
            }

            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Details = details ?? string.Empty
            };

            // Add to system debug output as well
            System.Diagnostics.Debug.WriteLine($"[{entry.Timestamp}] [{level}] {message} {(details != null ? $"- {details}" : "")}");

            // Add to our collection
            LogEntries.Add(entry);
        }
    }
}