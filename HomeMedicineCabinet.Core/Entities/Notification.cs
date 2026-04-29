namespace HomeMedicineCabinet.Core.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int? MedicineId { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime ScheduledAt { get; set; }

        public DateTime? SentAt { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;

        public Medicine? Medicine { get; set; }
    }
}
