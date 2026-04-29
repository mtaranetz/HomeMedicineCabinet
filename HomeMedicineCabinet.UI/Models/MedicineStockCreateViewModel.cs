using System.ComponentModel.DataAnnotations;

namespace HomeMedicineCabinet.UI.Models;

public class MedicineStockCreateViewModel
{
    public int MedicineId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public string Unit { get; set; } = string.Empty;

    public int MinQuantity { get; set; }

    [Required]
    public DateTime ExpirationDate { get; set; }

    public string? StoragePlace { get; set; }
}