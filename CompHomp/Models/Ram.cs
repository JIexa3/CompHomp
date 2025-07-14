using System;

namespace CompHomp.Models
{
    public class Ram
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }  // В ГБ
        public string Type { get; set; }  // DDR4, DDR5
        public int Speed { get; set; }  // В МГц
        public int ModulesCount { get; set; }  // Количество модулей в наборе
    }
}
