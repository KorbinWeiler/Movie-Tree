public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<MovieGenre> MovieGenres { get; set; } = [];
    public ICollection<AiPickList> AiPickLists { get; set; } = [];
}
