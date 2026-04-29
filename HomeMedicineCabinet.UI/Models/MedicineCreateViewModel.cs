using System.ComponentModel.DataAnnotations;

namespace HomeMedicineCabinet.UI.Models;

public class MedicineCreateViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    [Required]
    public string Form { get; set; } = string.Empty;

    [Required]
    public string Dosage { get; set; } = string.Empty;

    public string? Manufacturer { get; set; }

    public string? Description { get; set; }
}