namespace CompCredi.Models {
    public class Notification {
        public int Id { get; set; }
        public int UserId { get; set; } // Usuário que receberá a notificação
        public string Message { get; set; } = string.Empty; // Mensagem da notificação
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Data e hora da notificação
        public bool IsRead { get; set; } = false; // Status da leitura da notificação

        public User? User { get; set; } // Referência ao usuário
    }
}
