using System;

namespace CompHomp.Models
{
    public class Gpu
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int MemorySize { get; set; }  // В ГБ
        public string MemoryType { get; set; }  // GDDR6, GDDR6X и т.д.
        public int CoreClockSpeed { get; set; }  // В МГц
        public int TDP { get; set; }  // В ваттах
    }
}
