using System;

namespace CompHomp.Models
{
    public class LogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string LogType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;

        public LogEntry()
        {
            Timestamp = DateTime.Now;
        }
    }
}
