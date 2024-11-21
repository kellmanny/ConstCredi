using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompCredi.Data;
using CompCredi.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using CompCredi.DTOs;


namespace CompCredi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InteractionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InteractionController(AppDbContext context)
        {
            _context = context;
        }

        // Endpoint para registrar curtidas
        [HttpPost("like/{targetId}")]
        public async Task<IActionResult> LikeInteraction(int targetId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "UserId não encontrado no token." });

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest(new { message = "UserId inválido." });

            try
            {
                // Verifica se é uma interação ou um post
                var interaction = await _context.Interactions.FindAsync(targetId);
                var isPost = interaction == null;

                if (isPost)
                {
                    var post = await _context.Posts.FindAsync(targetId);
                    if (post == null)
                        return NotFound(new { message = "Post não encontrado." });

                    var existingLike = await _context.Interactions.FirstOrDefaultAsync(i =>
                        i.UserId == userId && i.PostId == targetId && i.Type == "like");

                    if (existingLike != null)
                    {
                        _context.Interactions.Remove(existingLike);
                    }
                    else
                    {
                        var like = new Interaction
                        {
                            UserId = userId,
                            PostId = targetId,
                            Type = "like",
                            Content = "", // Garantir que o campo Content seja preenchido
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Interactions.Add(like);
                    }
                }
                else
                {
                    var existingInteractionLike = await _context.Interactions.FirstOrDefaultAsync(i =>
                        i.UserId == userId && i.ParentInteractionId == targetId && i.Type == "like");

                    if (existingInteractionLike != null)
                    {
                        _context.Interactions.Remove(existingInteractionLike);
                    }
                    else
                    {
                        var like = new Interaction
                        {
                            UserId = userId,
                            PostId = interaction.PostId,
                            ParentInteractionId = targetId,
                            Type = "like",
                            Content = "", // Garantir que o campo Content seja preenchido
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Interactions.Add(like);
                    }
                }

                await _context.SaveChangesAsync();

                var likeCount = isPost
                    ? await _context.Interactions.CountAsync(i => i.PostId == targetId && i.Type == "like")
                    : await _context.Interactions.CountAsync(i => i.ParentInteractionId == targetId && i.Type == "like");

                return Ok(new { likes = likeCount });
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Erro ao salvar a interação no banco de dados: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(500, new
                {
                    message = "Erro interno ao processar a interação.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro geral: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Erro interno inesperado.",
                    error = ex.Message
                });
            }
        }

        // Novo endpoint para comentários e interações
        [HttpPost("interact")]
        public async Task<IActionResult> CreateInteraction([FromBody] InteractionDTO interactionDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "UserId não encontrado no token" });

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest(new { message = "UserId inválido" });

            try
            {
                var post = await _context.Posts.FindAsync(interactionDto.PostId);
                if (post == null)
                    return NotFound(new { message = "Post não encontrado." });

                var interaction = new Interaction
                {
                    UserId = userId,
                    PostId = interactionDto.PostId,
                    Type = interactionDto.Type,
                    Content = interactionDto.Content,
                    ParentInteractionId = interactionDto.ParentInteractionId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Interactions.Add(interaction);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Interação registrada com sucesso." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar a interação: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Erro interno ao processar a interação.",
                    error = ex.Message
                });
            }
        }


        // Endpoint para carregar timeline com interações
        [HttpGet("timeline-with-interactions")]
        public async Task<IActionResult> GetTimelineWithInteractions()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Interactions.Where(i => i.Type == "comment"))
                .ThenInclude(i => i.User) // Inclui dados do usuário nos comentários
                .Select(p => new
                {
                    p.Id,
                    p.Content,
                    p.MediaUrl,
                    p.CreatedAt,
                    p.UserId,
                    Username = p.User.Username,
                    ProfilePic = p.User.ProfilePic,
                    Likes = p.Interactions.Count(i => i.Type == "like"),
                    Comments = p.Interactions.Where(i => i.Type == "comment").Select(c => new
                    {
                        c.Id,
                        c.Content,
                        c.CreatedAt,
                        c.UserId,
                        Username = c.User.Username,
                        ProfilePic = c.User.ProfilePic
                    })
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(posts);
        }
    }
}
