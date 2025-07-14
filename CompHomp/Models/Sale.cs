using System;

namespace CompHomp.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BuildId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = "Оплачен";

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Build Build { get; set; }
    }
}
