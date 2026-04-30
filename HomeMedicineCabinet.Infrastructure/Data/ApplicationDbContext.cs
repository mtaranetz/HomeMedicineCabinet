using HomeMedicineCabinet.Core.Entities;
using Microsoft.EntityFrameworkCore;
namespace HomeMedicineCabinet.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<MedicineCategory> MedicineCategories => Set<MedicineCategory>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<MedicineStock> MedicineStocks => Set<MedicineStock>();
    public DbSet<IntakeSchedule> IntakeSchedules => Set<IntakeSchedule>();
    public DbSet<IntakeTime> IntakeTimes => Set<IntakeTime>();
    public DbSet<IntakeLog> IntakeLogs => Set<IntakeLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PushSubscription> PushSubscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureMedicineCategories(modelBuilder);
        ConfigureMedicines(modelBuilder);
        ConfigureMedicineStocks(modelBuilder);
        ConfigureIntakeSchedules(modelBuilder);
        ConfigureIntakeTimes(modelBuilder);
        ConfigureIntakeLogs(modelBuilder);
        ConfigureNotifications(modelBuilder);
        ConfigurePushSubscriptions(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureMedicineCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MedicineCategory>(entity =>
        {
            entity.ToTable("medicine_categories");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(255);
        });
    }

    private static void ConfigureMedicines(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.ToTable("medicines");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Form)
                .HasColumnName("form")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Dosage)
                .HasColumnName("dosage")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Manufacturer)
                .HasColumnName("manufacturer")
                .HasMaxLength(150);

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasColumnType("text");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(e => e.User)
                .WithMany(e => e.Medicines)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(e => e.Medicines)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureMedicineStocks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MedicineStock>(entity =>
        {
            entity.ToTable("medicine_stocks");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.MedicineId)
                .HasColumnName("medicine_id")
                .IsRequired();

            entity.Property(e => e.Quantity)
                .HasColumnName("quantity")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.Unit)
                .HasColumnName("unit")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.MinQuantity)
                .HasColumnName("min_quantity")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.ExpirationDate)
                .HasColumnName("expiration_date")
                .HasColumnType("date")
                .IsRequired();

            entity.Property(e => e.StoragePlace)
                .HasColumnName("storage_place")
                .HasMaxLength(150);

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Medicine)
                .WithMany(e => e.Stocks)
                .HasForeignKey(e => e.MedicineId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureIntakeSchedules(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IntakeSchedule>(entity =>
        {
            entity.ToTable("intake_schedules");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.MedicineId)
                .HasColumnName("medicine_id")
                .IsRequired();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.Dose)
                .HasColumnName("dose")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.FrequencyType)
                .HasColumnName("frequency_type")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.TimesPerDay)
                .HasColumnName("times_per_day");

            entity.Property(e => e.StartDate)
                .HasColumnName("start_date")
                .HasColumnType("date")
                .IsRequired();

            entity.Property(e => e.EndDate)
                .HasColumnName("end_date")
                .HasColumnType("date");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(e => e.Comment)
                .HasColumnName("comment")
                .HasMaxLength(255);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Medicine)
                .WithMany(e => e.IntakeSchedules)
                .HasForeignKey(e => e.MedicineId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(e => e.IntakeSchedules)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureIntakeTimes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IntakeTime>(entity =>
        {
            entity.ToTable("intake_times");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.IntakeScheduleId)
                .HasColumnName("intake_schedule_id")
                .IsRequired();

            entity.Property(e => e.IntakeTimeValue)
                .HasColumnName("intake_time")
                .HasColumnType("time")
                .IsRequired();

            entity.HasOne(e => e.IntakeSchedule)
                .WithMany(e => e.IntakeTimes)
                .HasForeignKey(e => e.IntakeScheduleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureIntakeLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IntakeLog>(entity =>
        {
            entity.ToTable("intake_logs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.IntakeScheduleId)
                .HasColumnName("intake_schedule_id")
                .IsRequired();

            entity.Property(e => e.PlannedDateTime)
                .HasColumnName("planned_datetime")
                .IsRequired();

            entity.Property(e => e.ActualDateTime)
                .HasColumnName("actual_datetime");

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .HasDefaultValue("Planned")
                .IsRequired();

            entity.Property(e => e.Comment)
                .HasColumnName("comment")
                .HasMaxLength(255);

            entity.HasOne(e => e.IntakeSchedule)
                .WithMany(e => e.IntakeLogs)
                .HasForeignKey(e => e.IntakeScheduleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureNotifications(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.MedicineId)
                .HasColumnName("medicine_id");

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.Message)
                .HasColumnName("message")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.ScheduledAt)
                .HasColumnName("scheduled_at")
                .IsRequired();

            entity.Property(e => e.SentAt)
                .HasColumnName("sent_at");

            entity.Property(e => e.IsRead)
                .HasColumnName("is_read")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithMany(e => e.Notifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Medicine)
                .WithMany(e => e.Notifications)
                .HasForeignKey(e => e.MedicineId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.IntakeLogId)
                .HasColumnName("intake_log_id");

            entity.HasOne(e => e.IntakeLog)
                .WithMany()
                .HasForeignKey(e => e.IntakeLogId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.IntakeLogId)
                .IsUnique();
        });
    }

    private static void ConfigurePushSubscriptions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PushSubscription>(entity =>
        {
            entity.ToTable("push_subscriptions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Endpoint)
                .HasColumnName("endpoint")
                .IsRequired();

            entity.Property(e => e.P256dh)
                .HasColumnName("p256dh")
                .IsRequired();

            entity.Property(e => e.Auth)
                .HasColumnName("auth")
                .IsRequired();

            entity.Property(e => e.UserAgent)
                .HasColumnName("user_agent");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.HasIndex(e => e.Endpoint)
                .IsUnique();
        });
    }
}