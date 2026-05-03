using System.ComponentModel.DataAnnotations;

namespace HomeMedicineCabinet.UI.Models;

public class MedicineStockCreateViewModel
{
    public int MedicineId { get; set; }

    [Required]
    public decimal Quantity { get; set; }

    //[Required]
    //public string Unit { get; set; } = string.Empty;

    public decimal MinQuantity { get; set; }

    [Required]
    public DateTime ExpirationDate { get; set; }

    public string? StoragePlace { get; set; }

    public bool IsPackage { get; set; }

    public int? ItemsPerPackage { get; set; }
}