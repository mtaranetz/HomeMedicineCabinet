namespace HomeMedicineCabinet.Core.Entities
{
    public class IntakeLog
    {
        public int Id { get; set; }

        public int IntakeScheduleId { get; set; }

        public DateTime PlannedDateTime { get; set; }

        public DateTime? ActualDateTime { get; set; }

        public string Status { get; set; } = "Planned";

        public string? Comment { get; set; }

        public IntakeSchedule IntakeSchedule { get; set; } = null!;
    }
}
