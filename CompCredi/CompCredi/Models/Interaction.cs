    namespace CompCredi.Models {
        public class Interaction {
            public int Id { get; set; }
            public int UserId { get; set; }
            public int PostId { get; set; }
            public string Type { get; set; } // Tipo da interação: "like" ou "comment"
            public string Content { get; set; } // Para comentários
            public int? ParentInteractionId { get; set; }
            public Interaction? ParentInteraction { get; set; }
            public ICollection<Interaction> Replies { get; set; } = new List<Interaction>();
            public DateTime CreatedAt { get; set; }

            public User User { get; set; }
            public Post Post { get; set; }
        }
    }
