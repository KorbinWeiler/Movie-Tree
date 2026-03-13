using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public UserRole Role { get; set; } = UserRole.User;
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<WatchLaterItem> WatchLaterItems { get; set; } = [];
    public ICollection<Friendship> SentFriendRequests { get; set; } = [];
    public ICollection<Friendship> ReceivedFriendRequests { get; set; } = [];
}