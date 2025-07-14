using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompHomp.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        
        public DateTime OrderDate { get; set; }
        
        public decimal TotalPrice { get; set; }
        
        public string Status { get; set; } // "В обработке", "Подтвержден", "Отменен", "Завершен"
        
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        
        public Order()
        {
            OrderItems = new List<OrderItem>();
            OrderDate = DateTime.Now;
            Status = "В обработке";
        }
    }
}
