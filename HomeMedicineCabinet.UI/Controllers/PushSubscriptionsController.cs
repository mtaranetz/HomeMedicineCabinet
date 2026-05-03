using HomeMedicineCabinet.Core.Entities;
using HomeMedicineCabinet.Infrastructure.Data;
using HomeMedicineCabinet.UI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HomeMedicineCabinet.UI.Controllers;

[Authorize]
public class PushSubscriptionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public PushSubscriptionsController(
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult PublicKey()
    {
        var publicKey = _configuration["Vapid:PublicKey"];

        return Json(new
        {
            publicKey
        });
    }

    [HttpPost]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionDto dto)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(dto.Endpoint))
        {
            return BadRequest();
        }

        var existingSubscription = await _context.PushSubscriptions
            .FirstOrDefaultAsync(s =>
                s.Endpoint == dto.Endpoint &&
                s.UserId == userId);

        if (existingSubscription == null)
        {
            var subscription = new PushSubscription
            {
                UserId = userId,
                Endpoint = dto.Endpoint,
                P256dh = dto.Keys.P256dh,
                Auth = dto.Keys.Auth,
                UserAgent = Request.Headers.UserAgent.ToString(),
                CreatedAt = DateTime.Now
            };

            _context.PushSubscriptions.Add(subscription);
        }
        else
        {
            existingSubscription.P256dh = dto.Keys.P256dh;
            existingSubscription.Auth = dto.Keys.Auth;
            existingSubscription.UserAgent = Request.Headers.UserAgent.ToString();
        }

        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribePushDto dto)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(dto.Endpoint))
        {
            return BadRequest();
        }

        var subscription = await _context.PushSubscriptions
            .FirstOrDefaultAsync(s =>
                s.Endpoint == dto.Endpoint &&
                s.UserId == userId);

        if (subscription != null)
        {
            _context.PushSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
        }

        return Ok();
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}