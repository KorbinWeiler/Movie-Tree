public class Friendship
{
    public int Id { get; set; }
    public string RequesterId { get; set; } = null!;
    public string AddresseeId { get; set; } = null!;
    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser Requester { get; set; } = null!;
    public ApplicationUser Addressee { get; set; } = null!;
}
