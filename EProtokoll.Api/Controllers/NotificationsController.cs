using EProtokoll.Api.Dtos;
using EProtokoll.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EProtokoll.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications)
    {
        _notifications = notifications;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool unreadOnly = false)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }
        var items = await _notifications.GetForUserAsync(userId.Value, unreadOnly);
        var result = items.Select(x => new NotificationResponse(x.Id, x.Type, x.Message, x.IsRead, x.CreatedAt, x.LetterId));
        return Ok(result);
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }
        var ok = await _notifications.MarkReadAsync(id, userId.Value);
        if (!ok)
        {
            return NotFound();
        }
        return Ok();
    }

    private int? GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
        if (int.TryParse(id, out var userId))
        {
            return userId;
        }
        return null;
    }
}
