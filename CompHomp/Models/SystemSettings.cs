namespace CompHomp.Models
{
    public class SystemSettings
    {
        public int Id { get; set; }
        public int MaxLoginAttempts { get; set; }
        public int LockoutDurationMinutes { get; set; }
        public int MinPasswordLength { get; set; }
        public bool RequireComplexPassword { get; set; }
        public int AuditLogRetentionDays { get; set; }
    }
}
