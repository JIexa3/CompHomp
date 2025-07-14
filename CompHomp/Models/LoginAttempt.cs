using System;
using System.ComponentModel.DataAnnotations;

namespace CompHomp.Models
{
    public class LoginAttempt
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Login { get; set; } = string.Empty;
        
        public int AttemptsCount { get; set; }
        public DateTime? LastAttemptTime { get; set; }
    }
}
