using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CompCredi.Data;
using CompCredi.Models;
using CompCredi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CompCredi.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(AppDbContext context, IConfiguration configuration) {
            _context = context;
            _configuration = configuration;
        }

        // Registro de usuário
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userDto) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);
            if (existingUser != null) return Conflict(new { message = "Username já existe." });

            var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
            if (existingEmail != null) return Conflict(new { message = "E-mail já está em uso." });

            var user = new User {
                Email = userDto.Email,
                Username = userDto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuário registrado com sucesso!" });
        }

        // Login de usuário
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userDto) {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(userDto.Password, user.Password))
                return Unauthorized(new { message = "Invalid username or password" });

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        private string GenerateJwtToken(User user) {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Obter perfil do usuário
        [HttpGet("{userId?}")]
        public async Task<IActionResult> GetProfile([FromRoute] int? userId) {
            var loggedUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (loggedUserIdClaim == null)
                return Unauthorized(new { message = "UserId não encontrado no token" });

            if (!int.TryParse(loggedUserIdClaim.Value, out int loggedUserId))
                return BadRequest(new { message = "UserId inválido" });

            var targetUserId = userId ?? loggedUserId;

            var user = await _context.Users
                .Where(u => u.Id == targetUserId)
                .Select(u => new {
                    u.Username,
                    u.Email,
                    u.Biography, // Incluindo a biografia no retorno
                    Followers = _context.UserFollowers.Count(f => f.FollowingId == u.Id),
                    Following = _context.UserFollowers.Count(f => f.FollowerId == u.Id),
                    u.ProfilePic // Inclui a imagem de perfil
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Usuário não encontrado" });

            return Ok(user);
        }


        // Atualizar biografia
        [HttpPut("update-bio")]
        public async Task<IActionResult> UpdateBio([FromBody] UpdateBioRequest request) {
            if (request == null || string.IsNullOrWhiteSpace(request.Biography))
                return BadRequest(new { message = "A biografia não pode estar vazia." });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "Usuário não autorizado." });

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest(new { message = "ID de usuário inválido." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "Usuário não encontrado." });

            user.Biography = request.Biography;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Biografia atualizada com sucesso." });
        }

        public class UpdateBioRequest {
            public string Biography { get; set; }
        }


        // Atualizar perfil do usuário
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateProfileDto profileDto) {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new { message = "UserId não encontrado no token" });

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest(new { message = "UserId inválido" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "Usuário não encontrado" });

            // Atualizando campos do usuário
            user.Username = profileDto.Username ?? user.Username;
            user.Email = profileDto.Email ?? user.Email;

            // Verifica se uma nova senha foi fornecida e a atualiza
            if (!string.IsNullOrEmpty(profileDto.Password)) {
                user.Password = BCrypt.Net.BCrypt.HashPassword(profileDto.Password);
            }

            // Atualizando a biografia, se fornecida
            if (!string.IsNullOrWhiteSpace(profileDto.Biography)) {
                if (profileDto.Biography.Length > 280) {
                    return BadRequest(new { message = "A biografia não pode exceder 280 caracteres" });
                }
                user.Biography = profileDto.Biography;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Perfil atualizado com sucesso" });
        }


        // Buscar usuários e postagens
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query) {
            var users = await _context.Users
                .Where(u => u.Username.Contains(query))
                .Select(u => new { u.Id, u.Username, u.ProfilePic })
                .ToListAsync();

            var posts = await _context.Posts
                .Where(p => p.Content.Contains(query))
                .Select(p => new { p.Id, p.Content, p.CreatedAt, Username = p.User.Username })
                .ToListAsync();

            return Ok(new { users, posts });
        }

        // Verificar sessão
        [HttpGet("verify-session")]
        public IActionResult VerifySession() {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new { message = "Sessão inválida" });

            return Ok(new { message = "Sessão válida" });
        }

        // Seguir usuário
        [HttpPost("follow/{userId}")]
        public async Task<IActionResult> FollowUser(int userId) {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new { message = "UserId não encontrado no token" });

            if (!int.TryParse(userIdClaim.Value, out int currentUserId))
                return BadRequest(new { message = "UserId inválido" });

            var userToFollow = await _context.Users.FindAsync(userId);
            if (userToFollow == null) return NotFound(new { message = "Usuário não encontrado" });

            var existingFollow = await _context.UserFollowers
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == userId);
            if (existingFollow != null)
                return BadRequest(new { message = "Você já está seguindo este usuário" });

            var follow = new UserFollower { FollowerId = currentUserId, FollowingId = userId };
            _context.UserFollowers.Add(follow);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuário seguido com sucesso" });
        }

        // Deixar de seguir usuário
        [HttpPost("unfollow/{userId}")]
        public async Task<IActionResult> UnfollowUser(int userId) {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized(new { message = "UserId não encontrado no token" });

            if (!int.TryParse(userIdClaim.Value, out int currentUserId))
                return BadRequest(new { message = "UserId inválido" });

            var existingFollow = await _context.UserFollowers
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == userId);
            if (existingFollow == null)
                return NotFound(new { message = "Você não está seguindo este usuário" });

            _context.UserFollowers.Remove(existingFollow);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Você deixou de seguir o usuário com sucesso" });
        }

        [HttpPost("upload-profile-pic")]
        public async Task<IActionResult> UploadProfilePic([FromForm] ProfilePicUploadRequest request) {
            try {
                var profilePic = request.File;

                if (profilePic == null || profilePic.Length == 0)
                    return BadRequest(new { message = "Nenhuma foto enviada." });

                // Validações
                var maxFileSize = int.Parse(_configuration["FileUpload:MaxFileSize"]);
                var allowedExtensions = _configuration["FileUpload:AllowedExtensions"].Split(',');

                if (profilePic.Length > maxFileSize)
                    return BadRequest(new { message = "O arquivo excede o tamanho máximo permitido." });

                var fileExtension = Path.GetExtension(profilePic.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "Extensão de arquivo não permitida." });

                // Caminho e nome do arquivo
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadDirectory = _configuration["FileUpload:UploadDirectory"];
                var filePath = Path.Combine(uploadDirectory, fileName);

                Directory.CreateDirectory(uploadDirectory);
                using (var stream = new FileStream(filePath, FileMode.Create)) {
                    await profilePic.CopyToAsync(stream);
                }

                // Atualiza o caminho da nova imagem no banco de dados
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized(new { message = "Usuário não autenticado." });

                if (!int.TryParse(userIdClaim.Value, out int userId))
                    return BadRequest(new { message = "ID do usuário inválido." });

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound(new { message = "Usuário não encontrado." });

                user.ProfilePic = $"/uploads/profile-pics/{fileName}"; // Salva o novo caminho
                await _context.SaveChangesAsync();

                return Ok(new {
                    message = "Foto de perfil atualizada com sucesso.",
                    profilePicUrl = $"/uploads/profile-pics/{fileName}"
                });
            }
            catch (Exception ex) {
                Console.Error.WriteLine($"Erro ao processar o upload: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro interno ao processar o upload." });
            }
        }






    }
}
