using System.ComponentModel.DataAnnotations;

namespace CompCredi.DTOs {
    public class UserRegisterDto {
        [Required(ErrorMessage = "O campo E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do E-mail é inválido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O campo Username é obrigatório.")]
        [StringLength(50, ErrorMessage = "O Username deve ter no máximo 50 caracteres.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        [StringLength(100, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma letra minúscula, um número e um símbolo.")]
        public string Password { get; set; }
    }
}
