namespace HomeMedicineCabinet.Core.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();

        public ICollection<IntakeSchedule> IntakeSchedules { get; set; } = new List<IntakeSchedule>();

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
