using System.ComponentModel.DataAnnotations;

namespace HomeMedicineCabinet.UI.Models;

public class ScheduleCreateViewModel
{
    [Required]
    public int MedicineId { get; set; }

    [Required]
    public string Dose { get; set; } = string.Empty;

    [Required]
    public string FrequencyType { get; set; } = "Daily";

    public int? TimesPerDay { get; set; }

    [Required]
    public DateTime StartDate { get; set; } = DateTime.Today;

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Comment { get; set; }

    [Required]
    public string IntakeTimesText { get; set; } = string.Empty;
}