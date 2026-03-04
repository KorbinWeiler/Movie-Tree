using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class FeedController(AppDbContext db) : ControllerBase
{
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private static ReviewDto ToDto(Review r) => new(
        r.Id, r.UserId, r.User.UserName!, r.User.ProfilePictureUrl,
        r.MovieId, r.Movie.Title, r.Movie.PosterUrl,
        r.Rating, r.ReviewText, r.Visibility, r.CreatedAt, r.UpdatedAt
    );

    // GET /api/feed/public?page=1&pageSize=20
    [HttpGet("public")]
    public async Task<IActionResult> Public([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (pageSize > 50) pageSize = 50;

        var reviews = await db.Reviews
            .Include(r => r.User)
            .Include(r => r.Movie)
            .Where(r => r.Visibility == ReviewVisibility.Public)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(reviews.Select(ToDto));
    }

    // GET /api/feed/friends?page=1&pageSize=20
    [HttpGet("friends")]
    [Authorize]
    public async Task<IActionResult> Friends([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (pageSize > 50) pageSize = 50;
        var userId = CurrentUserId!;

        var friendIds = await db.Friendships
            .Where(f => f.Status == FriendshipStatus.Accepted &&
                        (f.RequesterId == userId || f.AddresseeId == userId))
            .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
            .ToListAsync();

        var reviews = await db.Reviews
            .Include(r => r.User)
            .Include(r => r.Movie)
            .Where(r => friendIds.Contains(r.UserId) &&
                        r.Visibility != ReviewVisibility.Private)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(reviews.Select(ToDto));
    }
}
