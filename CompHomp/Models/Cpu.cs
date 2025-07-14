using System;

namespace CompHomp.Models
{
    public class Cpu
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Socket { get; set; }
        public int Cores { get; set; }
        public double BaseClockSpeed { get; set; }  // В ГГц
        public int TDP { get; set; }  // В ваттах
    }
}
