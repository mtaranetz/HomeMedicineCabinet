namespace HomeMedicineCabinet.Core.Entities
{
    public class IntakeTime
    {
        public int Id { get; set; }

        public int IntakeScheduleId { get; set; }

        public TimeSpan IntakeTimeValue { get; set; }

        public IntakeSchedule IntakeSchedule { get; set; } = null!;
    }
}
