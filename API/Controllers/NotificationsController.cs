using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sahty.API.Data;
using Sahty.API.DTOs;

namespace Sahty.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public NotificationsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetMy()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var items = await _db.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    LinkUrl = n.LinkUrl,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (notification is null) return NotFound();

            notification.IsRead = true;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (notifications.Count == 0) return NoContent();

            foreach (var n in notifications)
                n.IsRead = true;

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
