namespace HomeMedicineCabinet.Core.Entities
{
    public class Medicine
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int? CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Form { get; set; } = string.Empty;

        public string Dosage { get; set; } = string.Empty;

        public string? Manufacturer { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public User User { get; set; } = null!;

        public MedicineCategory? Category { get; set; }

        public ICollection<MedicineStock> Stocks { get; set; } = new List<MedicineStock>();

        public ICollection<IntakeSchedule> IntakeSchedules { get; set; } = new List<IntakeSchedule>();

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
