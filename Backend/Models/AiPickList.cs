public class AiPickList
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public AiGenerationMode GenerationMode { get; set; }
    public int? GenreId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
    public Genre? Genre { get; set; }
    public ICollection<AiPickListItem> Items { get; set; } = [];
}
