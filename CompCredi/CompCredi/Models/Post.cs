using System;
using System.Collections.Generic;

namespace CompCredi.Models {
    public class Post {public int Id { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; }
    public string MediaUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? OriginalPostId { get; set; } // ID do post original (se for repostagem)
    public Post OriginalPost { get; set; } // Referência ao post original
    public int? ReposterId { get; set; } // ID do usuário que fez a repostagem

    public User User { get; set; } // Autor do post original
    public ICollection<Interaction> Interactions { get; set; }
    }
}
