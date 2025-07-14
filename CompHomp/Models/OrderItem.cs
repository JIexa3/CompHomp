using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompHomp.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }
        
        public int? BuildId { get; set; }
        [ForeignKey("BuildId")]
        public Build Build { get; set; }
        
        public string ComponentType { get; set; } // "Процессор", "Видеокарта" и т.д.
        public int ComponentId { get; set; }
        public string ComponentName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
