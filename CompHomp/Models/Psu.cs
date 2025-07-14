using System;

namespace CompHomp.Models
{
    public class Psu
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Power { get; set; }  // В ваттах
        public string FormFactor { get; set; }  // ATX, SFX
        public string Efficiency { get; set; }  // 80+ Bronze, Gold, Platinum
        public bool IsModular { get; set; }
    }
}
