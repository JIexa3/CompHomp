using System;

namespace CompHomp.Models
{
    public class Motherboard
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Socket { get; set; }
        public string Chipset { get; set; }
        public string FormFactor { get; set; }  // ATX, mATX, ITX
        public string MemoryType { get; set; }  // DDR4, DDR5
    }
}
