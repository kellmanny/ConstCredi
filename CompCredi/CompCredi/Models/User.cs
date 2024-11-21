using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CompCredi.Models {
    public class User {
        public int Id { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "O e-mail deve estar em um formato válido.")]
        [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "O nome de usuário deve ter no máximo 50 caracteres.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
        public string Password { get; set; } = string.Empty;

        // Campo opcional para imagem de perfil
        [StringLength(255, ErrorMessage = "A URL da imagem de perfil deve ter no máximo 255 caracteres.")]
        public string? ProfilePic { get; set; }

        // Novo campo para biografia
        [StringLength(280, ErrorMessage = "A biografia deve ter no máximo 280 caracteres.")]
        public string? Biography { get; set; } = null;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
        public ICollection<UserFollower> Followers { get; set; } = new List<UserFollower>();
        public ICollection<UserFollower> Following { get; set; } = new List<UserFollower>();
    }
}
