using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompCredi.Data;
using CompCredi.Models;
using CompCredi.DTOs;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace CompCredi.Controllers {
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase {
        private readonly AppDbContext _context;

        public PostController(AppDbContext context) {
            _context = context;
        }

        // Criar um novo post
        [HttpPost("create")]
        public async Task<IActionResult> CreatePost([FromBody] PostCreateDto postDto) {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new { message = "UserId não encontrado no token" });

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest(new { message = "UserId inválido" });

            var post = new Post {
                UserId = userId,
                Content = postDto.Content,
                MediaUrl = postDto.MediaUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post criado com sucesso" });
        }

        // Obter timeline com interações (padrão)
        [HttpGet("timeline-with-interactions")]
        public async Task<IActionResult> GetTimelineWithInteractions(int pageNumber = 1, int pageSize = 10) {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Interactions)
                .ThenInclude(i => i.Replies) // Inclui as respostas de cada interação
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new {
                    p.Id,
                    p.Content,
                    p.MediaUrl,
                    p.CreatedAt,
                    Username = p.User.Username,
                    UserId = p.UserId,
                    ProfilePic = p.User.ProfilePic,
                    Likes = p.Interactions.Count(i => i.Type == "like"),
                    Comments = p.Interactions
                        .Where(i => i.Type == "comment" && i.ParentInteractionId == null)
                        .Select(c => new {
                            c.Id,
                            c.Content,
                            c.CreatedAt,
                            Username = c.User.Username,
                            UserId = c.UserId,
                            Likes = c.Replies.Count(r => r.Type == "like"), // Curtidas no comentário
                            Replies = c.Replies
                                .Where(r => r.Type == "comment")
                                .Select(r => new {
                                    r.Id,
                                    r.Content,
                                    r.CreatedAt,
                                    Username = r.User.Username,
                                    UserId = r.UserId,
                                    Likes = r.Replies.Count(rr => rr.Type == "like") // Curtidas na resposta
                                })
                        })
                })
                .ToListAsync();

            return Ok(posts);
        }



        // Obter posts de um usuário específico
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPostsByUser(int userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10) {
            var posts = await _context.Posts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new {
                    p.Id,
                    p.Content,
                    p.MediaUrl,
                    p.CreatedAt,
                    p.UserId, // Incluindo o UserId
                    Username = p.User.Username, // Incluindo o Username explicitamente
                    ProfilePic = p.User.ProfilePic, // Incluindo o ProfilePic
                    Likes = p.Interactions.Count(i => i.Type == "like"),
                    Comments = p.Interactions
                        .Where(i => i.Type == "comment")
                        .Select(c => new {
                            c.Id,
                            c.Content,
                            c.CreatedAt,
                            Username = c.User.Username, // Certificando-se de obter o Username dos comentários
                            UserId = c.UserId,
                            ProfilePic = c.User.ProfilePic
                        })
                })

                .ToListAsync();

            if (posts == null || !posts.Any())
                return NotFound(new { message = "Nenhum post encontrado para este usuário" });

            return Ok(posts);
        }


        // Atualizar um post
        [HttpPut("update/{postId}")]
        public async Task<IActionResult> UpdatePost(int postId, [FromBody] PostCreateDto postDto) {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new { message = "UserId não encontrado no token" });

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest(new { message = "UserId inválido" });

            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.UserId != userId)
                return NotFound(new { message = "Post não encontrado ou acesso negado" });

            post.Content = postDto.Content;
            post.MediaUrl = postDto.MediaUrl;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Post atualizado com sucesso" });
        }

        // Deletar um post
        [HttpDelete("delete/{postId}")]
        public async Task<IActionResult> DeletePost(int postId) {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new { message = "UserId não encontrado no token" });

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest(new { message = "UserId inválido" });

            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.UserId != userId)
                return NotFound(new { message = "Post não encontrado ou acesso negado" });

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Post deletado com sucesso" });
        }

        // Timeline com cursor para paginação avançada
        [HttpGet("timeline-with-cursor")]
        public async Task<IActionResult> GetTimelineWithCursor(DateTime? cursor = null, int pageSize = 10) {
            var postsQuery = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Interactions)
                .AsQueryable();

            if (cursor.HasValue) {
                postsQuery = postsQuery.Where(p => p.CreatedAt < cursor.Value);
            }

            var posts = await postsQuery
                .OrderByDescending(p => p.CreatedAt)
                .Take(pageSize)
                .Select(p => new {
                    p.Id,
                    p.Content,
                    p.MediaUrl,
                    p.CreatedAt,
                    Username = p.User.Username,
                    ProfilePic = p.User.ProfilePic,
                    Interactions = p.Interactions.Select(i => new {
                        i.Id,
                        i.Type,
                        i.Content,
                        i.CreatedAt,
                        Username = i.User.Username
                    })
                })
                .ToListAsync();

            var nextCursor = posts.LastOrDefault()?.CreatedAt;

            return Ok(new { posts, nextCursor });
        }

        [HttpPost("repost/{postId}")]
        public async Task<IActionResult> Repost(int postId) {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "UserId não encontrado no token" });

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest(new { message = "UserId inválido" });

            // Verificar se o post existe
            var post = await _context.Posts.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
                return NotFound(new { message = "Post não encontrado." });

            // Impedir repost do próprio post
            if (post.UserId == userId)
                return BadRequest(new { message = "Você não pode repostar o seu próprio post." });

            // Criar repost
            var repost = new Post {
                UserId = userId,
                Content = post.Content,
                MediaUrl = post.MediaUrl,
                OriginalPostId = postId, // Vincula ao post original
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(repost);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post repostado com sucesso.", repostId = repost.Id });
        }

        [HttpGet("timeline-with-reposts")]
        public async Task<IActionResult> GetTimeline() {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.OriginalPost)
                .ThenInclude(op => op.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
                var response = posts.Select(p => new {
                    Id = p.Id,
                    UserId = p.UserId,
                    Username = p.User?.Username ?? "Usuário desconhecido",
                    Content = p.Content ?? "", // Garante que Content nunca será null
                    CreatedAt = p.CreatedAt,
                    OriginalPost = p.OriginalPost != null ? new {
                        Id = p.OriginalPost.Id,
                        Content = p.OriginalPost.Content ?? "Post original indisponível", // Garante conteúdo no repost
                        Username = p.OriginalPost.User?.Username ?? "Usuário desconhecido"
                    } : null,
                    Reposter = p.ReposterId.HasValue ? new {
                        Id = p.ReposterId,
                        Username = _context.Users.FirstOrDefault(u => u.Id == p.ReposterId)?.Username ?? "Usuário desconhecido"
                    } : null
            });

            return Ok(response);
        }





    }
}
