    using HomeMedicineCabinet.Infrastructure.Services;

    namespace HomeMedicineCabinet.UI.Services;

    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public NotificationBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var notificationService = scope.ServiceProvider
                    .GetRequiredService<NotificationService>();

                await notificationService.CheckIntakeReminders();

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }