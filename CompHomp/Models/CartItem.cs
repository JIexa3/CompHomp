using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompHomp.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        
        public int BuildId { get; set; }
        [ForeignKey("BuildId")]
        public Build Build { get; set; }
        
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public DateTime DateAdded { get; set; }
        
        public CartItem()
        {
            DateAdded = DateTime.Now;
            Quantity = 1;
        }
    }
}
