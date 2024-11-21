        using System.ComponentModel.DataAnnotations;

        namespace CompCredi.DTOs {
            public class PostCreateDto {
                [Required(ErrorMessage = "O conteúdo do post é obrigatório.")]
                public string Content { get; set; }

                public string MediaUrl { get; set; }
            }

        }