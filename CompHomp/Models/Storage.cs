using System;

namespace CompHomp.Models
{
    public class Storage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Type { get; set; }  // SSD, HDD, NVMe
        public int Capacity { get; set; }  // В ГБ
        public string FormFactor { get; set; }  // 2.5", 3.5", M.2
        public int ReadSpeed { get; set; }  // В МБ/с
    }
}
