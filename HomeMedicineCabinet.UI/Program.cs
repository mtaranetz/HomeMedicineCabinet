using HomeMedicineCabinet.Core.Interfaces;
using HomeMedicineCabinet.Infrastructure.Data;
using HomeMedicineCabinet.UI.Hubs;
using HomeMedicineCabinet.UI.Services;
using Microsoft.EntityFrameworkCore;
using HomeMedicineCabinet.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

using WebPush;

var keys = VapidHelper.GenerateVapidKeys();

Console.WriteLine(keys.PublicKey);
Console.WriteLine(keys.PrivateKey);

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );

            mySqlOptions.CommandTimeout(60);
        }
    );
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<INotificationSender, SignalRNotificationSender>();
builder.Services.AddHostedService<NotificationBackgroundService>();
builder.Services.AddScoped<PushNotificationService>();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Medicines}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
