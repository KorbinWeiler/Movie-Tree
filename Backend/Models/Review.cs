public class Review
{
    public int Id { get; set; }
    public byte Rating { get; set; }
    public string? ReviewText { get; set; }
    public ReviewVisibility Visibility { get; set; } = ReviewVisibility.Public;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public string UserId { get; set; } = null!;
    public int MovieId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}