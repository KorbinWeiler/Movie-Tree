public class AiPickListItem
{
    public int Id { get; set; }
    public int AiPickListId { get; set; }
    public int MovieId { get; set; }
    public byte Position { get; set; }

    public AiPickList AiPickList { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}
