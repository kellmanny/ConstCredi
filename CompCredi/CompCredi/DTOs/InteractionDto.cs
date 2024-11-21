namespace CompCredi.DTOs
{
    public class InteractionDTO
    {
        public int PostId { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public int? ParentInteractionId { get; set; }
    }
}
