using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompHomp.Models
{
    [Table("UserAuthLogs")]
    public class UserAuthLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Login { get; set; }

        [Required]
        [MaxLength(100)]
        public string Status { get; set; }

        public bool Success { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
