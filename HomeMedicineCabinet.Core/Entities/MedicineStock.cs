namespace HomeMedicineCabinet.Core.Entities
{
    public class MedicineStock
    {
        public int Id { get; set; }

        public int MedicineId { get; set; }

        public decimal Quantity { get; set; }

        public string Unit { get; set; } = string.Empty;

        public decimal MinQuantity { get; set; }

        public DateTime ExpirationDate { get; set; }

        public string? StoragePlace { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Medicine Medicine { get; set; } = null!;
    }
}
