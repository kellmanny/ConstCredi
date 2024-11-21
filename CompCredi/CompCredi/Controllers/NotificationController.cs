using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompCredi.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using CompCredi.Models;

namespace CompCredi.Controllers {
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context) {
            _context = context;
        }

        // Obter notificações
        [HttpGet]
        public async Task<IActionResult> GetNotifications(int pageNumber = 1, int pageSize = 10) {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new { message = "UserId não encontrado no token" });

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest(new { message = "UserId inválido" });

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new {
                    n.Id,
                    n.Message,
                    n.CreatedAt,
                    n.IsRead
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // Marcar uma notificação como lida
        [HttpPut("{notificationId}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int notificationId) {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new { message = "UserId não encontrado no token" });

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest(new { message = "UserId inválido" });

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null) return NotFound(new { message = "Notificação não encontrada" });

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Notificação marcada como lida" });
        }

        // Marcar todas as notificações como lidas
        [HttpPut("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead() {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new { message = "UserId não encontrado no token" });

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest(new { message = "UserId inválido" });

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            notifications.ForEach(n => n.IsRead = true);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Todas as notificações foram marcadas como lidas" });
        }

        // Criar uma notificação ao curtir um post
        [HttpPost("like-notification")]
        public async Task<IActionResult> CreateLikeNotification(int postId) {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new { message = "UserId não encontrado no token" });

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest(new { message = "UserId inválido" });

            var post = await _context.Posts.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null) return NotFound(new { message = "Post não encontrado" });

            if (post.UserId != userId) {
                var notification = new Notification {
                    UserId = post.UserId,
                    Message = $"Seu post foi curtido por {User.Identity.Name}.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Notificação de curtida criada com sucesso" });
        }

        // Criar uma notificação ao comentar em um post
        [HttpPost("comment-notification")]
        public async Task<IActionResult> CreateCommentNotification(int postId, [FromBody] string commentContent) {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new { message = "UserId não encontrado no token" });

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest(new { message = "UserId inválido" });

            var post = await _context.Posts.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null) return NotFound(new { message = "Post não encontrado" });

            if (post.UserId != userId) {
                var notification = new Notification {
                    UserId = post.UserId,
                    Message = $"Seu post recebeu um comentário de {User.Identity.Name}: \"{commentContent}\".",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Notificação de comentário criada com sucesso" });
        }
    }
}
