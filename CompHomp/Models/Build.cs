using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompHomp.Models
{
    public enum BuildStatus
    {
        [Display(Name = "На рассмотрении")]
        Pending,
        [Display(Name = "Одобрено")]
        Approved,
        [Display(Name = "Отклонено")]
        Rejected
    }

    public enum BuildCategory
    {
        Gaming,
        WorkStation,
        HomeOffice,
        VideoEditing,
        GraphicDesign,
        Streaming,
        Budget,
        Enthusiast,
        ServerBuild,
        UltraPerformance
    }

    public class Build
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public DateTime CreatedDate { get; set; }
        public BuildStatus Status { get; set; } = BuildStatus.Pending;
        public DateTime? ApprovedDate { get; set; }
       
        public string? Version { get; set; }
        public string? FilePath { get; set; }
        
        public bool IsCustom { get; set; }
        public decimal BasePrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }

        // Компоненты сборки
        public int? CpuId { get; set; }
        public Cpu Cpu { get; set; }
        
        public int? GpuId { get; set; }
        public Gpu Gpu { get; set; }
        
        public int? MotherboardId { get; set; }
        public Motherboard Motherboard { get; set; }
        
        public int? RamId { get; set; }
        public Ram Ram { get; set; }
        
        public int? StorageId { get; set; }
        public Storage Storage { get; set; }
        
        public int? PsuId { get; set; }
        public Psu Psu { get; set; }
        
        public int? CaseId { get; set; }
        public Case Case { get; set; }

        // Добавляем навигационное свойство для связи с покупками
        public List<PurchaseHistoryItem> PurchaseHistoryItems { get; set; } = new List<PurchaseHistoryItem>();

        public virtual ICollection<BuildRating> BuildRatings { get; set; }
        public virtual ICollection<BuildComment> BuildComments { get; set; }

        public void CalculateTotalPrice()
        {
            decimal componentsSum = 0;

            if (Cpu != null) componentsSum += Cpu.Price;
            if (Gpu != null) componentsSum += Gpu.Price;
            if (Motherboard != null) componentsSum += Motherboard.Price;
            if (Ram != null) componentsSum += Ram.Price;
            if (Storage != null) componentsSum += Storage.Price;
            if (Psu != null) componentsSum += Psu.Price;
            if (Case != null) componentsSum += Case.Price;

            // BasePrice теперь содержит только наценку (5% от суммы компонентов)
            BasePrice = componentsSum * 0.05m;
            // TotalPrice - полная стоимость (компоненты + наценка)
            TotalPrice = componentsSum + BasePrice;
        }

        public Build()
        {
            CreatedDate = DateTime.Now;
            BuildRatings = new List<BuildRating>();
            BuildComments = new List<BuildComment>();
        }
    }
}
