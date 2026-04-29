namespace HomeMedicineCabinet.Core.Entities
{
    public class IntakeSchedule
    {
        public int Id { get; set; }

        public int MedicineId { get; set; }

        public int UserId { get; set; }

        public string Dose { get; set; } = string.Empty;

        public string FrequencyType { get; set; } = string.Empty;

        public int? TimesPerDay { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public Medicine Medicine { get; set; } = null!;

        public User User { get; set; } = null!;

        public ICollection<IntakeTime> IntakeTimes { get; set; } = new List<IntakeTime>();

        public ICollection<IntakeLog> IntakeLogs { get; set; } = new List<IntakeLog>();
    }
}
