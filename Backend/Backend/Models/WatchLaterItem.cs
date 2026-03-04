public class WatchLaterItem
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public int MovieId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}
