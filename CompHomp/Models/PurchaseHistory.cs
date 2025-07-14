using System;
using System.Collections.Generic;

namespace CompHomp.Models
{
    public class PurchaseHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public List<PurchaseHistoryItem> Items { get; set; }

        public PurchaseHistory()
        {
            Items = new List<PurchaseHistoryItem>();
            PurchaseDate = DateTime.Now;
        }
    }

    public class PurchaseHistoryItem
    {
        public int Id { get; set; }
        public int PurchaseHistoryId { get; set; }
        public PurchaseHistory PurchaseHistory { get; set; }  
        public int BuildId { get; set; }  
        public Build Build { get; set; }  
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
