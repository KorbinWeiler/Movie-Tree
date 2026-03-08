public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public int? RuntimeMinutes { get; set; }
    public string? PosterUrl { get; set; }

    public ICollection<MovieGenre> MovieGenres { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<WatchLaterItem> WatchLaterItems { get; set; } = [];
}