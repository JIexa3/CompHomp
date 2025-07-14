using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompHomp.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        
        public int? UserId { get; set; }  
        
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public string Username { get; set; }
        
        public string EventType { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
