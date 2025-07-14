using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompHomp.Models
{
    [Table("Cases")]
    public class Case
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; } = string.Empty;

        [Required]
        public string? FormFactor { get; set; }  // Mid Tower, Full Tower, etc.
        = string.Empty;

        [Required]
        public string? MotherboardSupport { get; set; }  // ATX, mATX, ITX
        = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public int MaxGPULength { get; set; }  // В мм
        public string? Color { get; set; } = string.Empty;
    }
}
